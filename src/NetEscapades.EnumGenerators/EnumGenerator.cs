using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators;

[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    private const string DisplayAttribute = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string DescriptionAttribute = "System.ComponentModel.DescriptionAttribute";
    private const string EnumExtensionsAttribute = "NetEscapades.EnumGenerators.EnumExtensionsAttribute";
    private const string ExternalEnumExtensionsAttribute = "NetEscapades.EnumGenerators.EnumExtensionsAttribute`1";
    private const string FlagsAttribute = "System.FlagsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<EnumToGenerate> enumsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                EnumExtensionsAttribute,
                predicate: (node, _) => node is EnumDeclarationSyntax,
                transform: GetTypeToGenerate)
            .WithTrackingName(TrackingNames.InitialExtraction)
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value)
            .WithTrackingName(TrackingNames.RemovingNulls);

        IncrementalValuesProvider<EnumToGenerate> externalEnums = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                ExternalEnumExtensionsAttribute,
                predicate: (node, _) => node is CompilationUnitSyntax,
                transform: GetExternalTypeToGenerate)
            .Where(static m => m is not null)
            .SelectMany(static (m, _) => m!.Value)
            .WithTrackingName(TrackingNames.InitialExternalExtraction);
        
        context.RegisterSourceOutput(enumsToGenerate,
            static (spc, enumToGenerate) => Execute(in enumToGenerate, spc));

        context.RegisterSourceOutput(externalEnums,
            static (spc, enumToGenerate) => Execute(in enumToGenerate, spc));

        // Interceptor!
        var interceptionExplicitlyEnabled = context.AnalyzerConfigOptionsProvider
            .Select((x, _) =>
                x.GlobalOptions.TryGetValue($"build_property.{Constants.EnabledPropertyName}", out var enableSwitch)
                && enableSwitch.Equals("true", StringComparison.Ordinal));

        var csharpSufficient = context.CompilationProvider
            .Select((x,_) => x is CSharpCompilation { LanguageVersion: LanguageVersion.Default or >= LanguageVersion.CSharp11 });

        var settings = interceptionExplicitlyEnabled
            .Combine(csharpSufficient)
            .WithTrackingName(TrackingNames.Settings);

        var interceptionEnabled = settings
            .Select((x, _) => x.Left && x.Right);

        var locations = context.SyntaxProvider
            .CreateSyntaxProvider(InterceptorPredicate, InterceptorParser)
            .Combine(interceptionEnabled)
            .Where(x => x.Right && x.Left is not null)
            .Select((x, _) => x.Left!)
            .Collect()
            .WithTrackingName(TrackingNames.InterceptedLocations);

        var enumInterceptions = enumsToGenerate
            .Combine(locations)
            .Select(FilterInterceptorCandidates!)
            .Where(x => x is not null)
            .WithTrackingName(TrackingNames.EnumInterceptions);

        var externalInterceptions = externalEnums
            .Combine(locations)
            .Select(FilterInterceptorCandidates!)
            .Where(x => x is not null)
            .WithTrackingName(TrackingNames.ExternalInterceptions);

        context.RegisterImplementationSourceOutput(enumInterceptions,
            static (spc, toIntercept) => ExecuteInterceptors(toIntercept, spc));

        context.RegisterImplementationSourceOutput(externalInterceptions,
            static (spc, toIntercept) => ExecuteInterceptors(toIntercept, spc));
        context.RegisterImplementationSourceOutput(settings,
            static (spc, args) =>
            {
                if (args.Left && !args.Right)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(DiagnosticHelper.CsharpVersionLooLow, location: null));
                }
            });
    }

    static void Execute(in EnumToGenerate enumToGenerate, SourceProductionContext context)
    {
        StringBuilder sb = new StringBuilder();
        var result = SourceGenerationHelper.GenerateExtensionClass(sb, in enumToGenerate);
        context.AddSource(enumToGenerate.Name + "_EnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));    
    }

    static EquatableArray<EnumToGenerate>? GetExternalTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        List<EnumToGenerate>? enums = null;
        foreach (AttributeData attribute in context.Attributes)
        {
            if (!((attribute.AttributeClass?.Name == "EnumExtensionsAttribute" ||
                   attribute.AttributeClass?.Name == "EnumExtensions") &&
                  attribute.AttributeClass.IsGenericType &&
                  attribute.AttributeClass.TypeArguments.Length == 1))
            {
                // wrong attribute
                continue;
            }

            var enumSymbol = attribute.AttributeClass.TypeArguments[0] as INamedTypeSymbol;
            if (enumSymbol is null)
            {
                continue;
            }

            bool hasFlags = false;
            string? name = null;
            string? nameSpace = null;

            foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key == "ExtensionClassNamespace"
                    && namedArgument.Value.Value?.ToString() is { } ns)
                {
                    nameSpace = ns;
                    continue;
                }

                if (namedArgument.Key == "ExtensionClassName"
                    && namedArgument.Value.Value?.ToString() is { } n)
                {
                    name = n;
                }
            }

            foreach (var attrData in enumSymbol.GetAttributes())
            {
                if ((attrData.AttributeClass?.Name == "FlagsAttribute" ||
                     attrData.AttributeClass?.Name == "Flags") &&
                    attrData.AttributeClass.ToDisplayString() == FlagsAttribute)
                {
                    hasFlags = true;
                    break;
                }
            }

            var enumToGenerate = TryExtractEnumSymbol(enumSymbol, name, nameSpace, hasFlags);
            if (enumToGenerate is not null)
            {
                enums ??= new();
                enums.Add(enumToGenerate.Value);
            }
        }

        return enums is not null
            ? new EquatableArray<EnumToGenerate>(enums.ToArray())
            : null;
    }

    static EnumToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        INamedTypeSymbol? enumSymbol = context.TargetSymbol as INamedTypeSymbol;
        if (enumSymbol is null)
        {
            // nothing to do if this type isn't available
            return null;
        }

        ct.ThrowIfCancellationRequested();

        var hasFlags = false;
        string? nameSpace = null;
        string? name = null;

        foreach (AttributeData attributeData in enumSymbol.GetAttributes())
        {
            if ((attributeData.AttributeClass?.Name == "FlagsAttribute" ||
                 attributeData.AttributeClass?.Name == "Flags") &&
                attributeData.AttributeClass.ToDisplayString() == FlagsAttribute)
            {
                hasFlags = true;
                continue;
            }

            if (attributeData.AttributeClass?.Name != "EnumExtensionsAttribute" ||
                attributeData.AttributeClass.ToDisplayString() != EnumExtensionsAttribute)
            {
                continue;
            }

            foreach (KeyValuePair<string, TypedConstant> namedArgument in attributeData.NamedArguments)
            {
                if (namedArgument.Key == "ExtensionClassNamespace"
                    && namedArgument.Value.Value?.ToString() is { } ns)
                {
                    nameSpace = ns;
                    continue;
                }

                if (namedArgument.Key == "ExtensionClassName"
                    && namedArgument.Value.Value?.ToString() is { } n)
                {
                    name = n;
                }
            }
        }

        return TryExtractEnumSymbol(enumSymbol, name, nameSpace, hasFlags);
    }

    static EnumToGenerate? TryExtractEnumSymbol(INamedTypeSymbol enumSymbol, string? name, string? nameSpace, bool hasFlags)
    {
        name ??= enumSymbol.Name + "Extensions";
        nameSpace ??= enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();

        string fullyQualifiedName = enumSymbol.ToString();
        string underlyingType = enumSymbol.EnumUnderlyingType?.ToString() ?? "int";

        var enumMembers = enumSymbol.GetMembers();
        var members = new List<(string, EnumValueOption)>(enumMembers.Length);
        HashSet<string>? displayNames = null;
        var isDisplayNameTheFirstPresence = false;

        foreach (var member in enumMembers)
        {
            if (member is not IFieldSymbol field
                || field.ConstantValue is null)
            {
                continue;
            }

            string? displayName = null;
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == "DisplayAttribute" &&
                    attribute.AttributeClass.ToDisplayString() == DisplayAttribute)
                {
                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == "Name" && namedArgument.Value.Value?.ToString() is { } dn)
                        {
                            // found display attribute, all done
                            displayName = dn;
                            goto addDisplayName;
                        }
                    }
                }
                
                if (attribute.AttributeClass?.Name == "DescriptionAttribute" 
                    && attribute.AttributeClass.ToDisplayString() == DescriptionAttribute
                    && attribute.ConstructorArguments.Length == 1)
                {
                    if (attribute.ConstructorArguments[0].Value?.ToString() is { } dn)
                    {
                        // found display attribute, all done
                        // Handle cases where contains a quote or a backslash
                        displayName = dn
                            .Replace(@"\", @"\\")
                            .Replace("\"", "\\\"");
                        goto addDisplayName;
                    }
                }
            }

            addDisplayName:
            if (displayName is not null)
            {
                displayNames ??= new();
                isDisplayNameTheFirstPresence = displayNames.Add(displayName);    
            }
            
            members.Add((member.Name, new EnumValueOption(displayName, isDisplayNameTheFirstPresence)));
        }

        return new EnumToGenerate(
            name: name,
            fullyQualifiedName: fullyQualifiedName,
            ns: nameSpace,
            underlyingType: underlyingType,
            isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public,
            hasFlags: hasFlags,
            names: members,
            isDisplayAttributeUsed: displayNames?.Count > 0);
    }

    private static bool InterceptorPredicate(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax {Expression: MemberAccessExpressionSyntax {Name.Identifier.ValueText: "ToString"}};

    private static CandidateInvocation? InterceptorParser(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.Node is InvocationExpressionSyntax {Expression: MemberAccessExpressionSyntax {Name: { } nameSyntax}} invocation
            && ctx.SemanticModel.GetOperation(ctx.Node, ct) is IInvocationOperation targetOperation
            && targetOperation.TargetMethod is {Name : "ToString", ContainingType: {Name: "Enum", ContainingNamespace: {Name: "System", ContainingNamespace.IsGlobalNamespace: true}}}
            && targetOperation.Instance?.Type is { } type
#pragma warning disable RSEXPERIMENTAL002 // / Experimental interceptable location API
            && ctx.SemanticModel.GetInterceptableLocation(invocation) is { } location)
#pragma warning restore RSEXPERIMENTAL002
        {
            return new CandidateInvocation(location, type.ToString());
        }

        return null;
    }

    private MethodToIntercept? FilterInterceptorCandidates(
        (EnumToGenerate Enum, ImmutableArray<CandidateInvocation> Candidates) arg1, 
        CancellationToken ct)
    {
        if (arg1.Candidates.IsDefaultOrEmpty)
        {
            return default;
        }

        List<CandidateInvocation>? results = null;
        foreach (var candidate in arg1.Candidates)
        {
            if (arg1.Enum.FullyQualifiedName.Equals(candidate.EnumName, StringComparison.Ordinal))
            {
                results ??= new();
                results.Add(candidate);
            }
        }

        if (results is null)
        {
            return null;
        }

        return new(new(results.ToArray()), arg1.Enum);
    }

    private static void ExecuteInterceptors(MethodToIntercept? toIntercept, SourceProductionContext spc)
    {
        var result = SourceGenerationHelper.GenerateInterceptorsClass(toIntercept!);
        spc.AddSource(toIntercept!.ExtensionTypeName + "_Interceptors.g.cs", SourceText.From(result, Encoding.UTF8));
    }
}
