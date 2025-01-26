using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators.Interceptors;

[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "InterceptableAttribute.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<EnumToIntercept> enumsToIntercept = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Attributes.EnumExtensionsAttribute,
                predicate: static (node, _) => node is EnumDeclarationSyntax,
                transform: GetInterceptableEnum)
            .WithTrackingName(TrackingNames.InitialExtraction)
            .Where(static m => m is not null)
            .Select((e, _) => e!.Value)
            .WithTrackingName(TrackingNames.RemovingNulls);

        IncrementalValuesProvider<EnumToIntercept> externalEnums = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                Attributes.ExternalEnumExtensionsAttribute,
                predicate: static (node, _) => node is CompilationUnitSyntax,
                transform: static (ctx, ct) => GetEnumToInterceptFromAssemblyAttribute(
                    ctx, ct, "EnumExtensionsAttribute", "EnumExtensions"))
            .Where(static m => m is not null)
            .SelectMany(static (m, _) => m!.Value)
            .WithTrackingName(TrackingNames.InitialExternalExtraction);

        var interceptionExplicitlyEnabled = context.AnalyzerConfigOptionsProvider
            .Select((x, _) =>
                x.GlobalOptions.TryGetValue($"build_property.{Constants.EnabledPropertyName}", out var enableSwitch)
                && enableSwitch.Equals("true", StringComparison.Ordinal));

        var csharpSufficient = context.CompilationProvider
            .Select((x,_) => x is CSharpCompilation { LanguageVersion: LanguageVersion.Default or >= LanguageVersion.CSharp11 });

        var settings = interceptionExplicitlyEnabled
            .Combine(csharpSufficient)
            .WithTrackingName(TrackingNames.Settings);

        var interceptableEnumsAndLocations = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.InterceptableAttribute,
                predicate: static (node, _) => node is CompilationUnitSyntax,
                transform: static (ctx, ct) =>
                {
                    var enumToIntercept = GetEnumToInterceptFromAssemblyAttribute(
                        ctx, ct, "InterceptableAttribute", "Interceptable");
                    var location = LocationInfo.CreateFrom(ctx.TargetNode.GetLocation());
                    return (enumToIntercept, location);
                })
            .WithTrackingName(TrackingNames.InitialInterceptable);

        var interceptableEnums = interceptableEnumsAndLocations
            .Where(static m => m.enumToIntercept is not null)
            .SelectMany(static (m, _) => m.enumToIntercept!.Value)
            .WithTrackingName(TrackingNames.InitialInterceptableOnly);

        var interceptionEnabled = settings
            .Select((x, _) => x.Left && x.Right);

        var locations = context.SyntaxProvider
            .CreateSyntaxProvider(InterceptorPredicate, InterceptorParser)
            .Combine(interceptionEnabled)
            .Where(x => x.Right && x.Left is not null)
            .Select((x, _) => x.Left!)
            .Collect()
            .WithTrackingName(TrackingNames.InterceptedLocations);

        var enumInterceptions = enumsToIntercept
            .Combine(locations)
            .Select(FilterInterceptorCandidates)
            .Where(x => x is not null)
            .WithTrackingName(TrackingNames.EnumInterceptions);

        var externalInterceptions = externalEnums
            .Combine(locations)
            .Select(FilterInterceptorCandidates!)
            .Where(x => x is not null)
            .WithTrackingName(TrackingNames.ExternalInterceptions);

        var additionalInterceptions = interceptableEnums
            .Combine(locations)
            .Select(FilterInterceptorCandidates!)
            .Where(x => x is not null)
            .WithTrackingName(TrackingNames.AdditionalInterceptions);

        context.RegisterSourceOutput(enumInterceptions,
            static (spc, toIntercept) => ExecuteInterceptors(toIntercept, spc));

        context.RegisterSourceOutput(externalInterceptions,
            static (spc, toIntercept) => ExecuteInterceptors(toIntercept, spc));

        context.RegisterSourceOutput(additionalInterceptions,
            static (spc, toIntercept) => ExecuteInterceptors(toIntercept, spc));

        context.RegisterImplementationSourceOutput(settings,
            static (spc, args) =>
            {
                var explicitlyEnabled = args.Left;
                var csharpSufficient = args.Right;
                if (explicitlyEnabled && !csharpSufficient)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(DiagnosticHelper.CsharpVersionLooLow, location: null));
                }
            });
    }

    static EquatableArray<EnumToIntercept>? GetEnumToInterceptFromAssemblyAttribute(
        GeneratorAttributeSyntaxContext context, CancellationToken ct, string fullAttributeName, string shortAttributeName)
    {
        List<EnumToIntercept>? enums = null;
        foreach (AttributeData attribute in context.Attributes)
        {
            if (!((attribute.AttributeClass?.Name == fullAttributeName ||
                   attribute.AttributeClass?.Name == shortAttributeName) &&
                  attribute.AttributeClass.IsGenericType &&
                  attribute.AttributeClass.TypeArguments.Length == 1))
            {
                // wrong attribute
                continue;
            }

            var isInterceptable = true;
            string? name = null;
            string? nameSpace = null;

            foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key == "IsInterceptable"
                    && namedArgument.Value.Value is false)
                {
                    isInterceptable = false;
                    break;
                }

                if (namedArgument.Key == "ExtensionClassNamespace"
                    && namedArgument.Value.Value?.ToString() is { } ns)
                {
                    nameSpace = ns;
                }
                else if (namedArgument.Key == "ExtensionClassName"
                         && namedArgument.Value.Value?.ToString() is { } n)
                {
                    name = n;
                }
            }
            if(isInterceptable && attribute.AttributeClass.TypeArguments[0] is INamedTypeSymbol enumSymbol)
            {
                enums ??= new();
                enums.Add(ToEnumToIntercept(enumSymbol, name, nameSpace));
            }
        }

        return enums is not null
            ? new EquatableArray<EnumToIntercept>(enums.ToArray())
            : null;
    }

    static EnumToIntercept? GetInterceptableEnum(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        var enumSymbol = context.TargetSymbol as INamedTypeSymbol;
        if (enumSymbol is null)
        {
            // nothing to do if this type isn't available
            return null;
        }

        ct.ThrowIfCancellationRequested();

        string? nameSpace = null;
        string? name = null;

        foreach (AttributeData attributeData in enumSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass?.Name != "EnumExtensionsAttribute" ||
                attributeData.AttributeClass.ToDisplayString() != Attributes.EnumExtensionsAttribute)
            {
                continue;
            }

            foreach (KeyValuePair<string, TypedConstant> namedArgument in attributeData.NamedArguments)
            {

                if (namedArgument is { Key: "IsInterceptable", Value.Value: false })
                {
                    // Not interceptable, can bail out
                    return null;
                }

                if (namedArgument.Key == "ExtensionClassNamespace"
                    && namedArgument.Value.Value?.ToString() is { } ns)
                {
                    nameSpace = ns;
                }
                else if (namedArgument.Key == "ExtensionClassName"
                         && namedArgument.Value.Value?.ToString() is { } n)
                {
                    name = n;
                }
            }
        }

        return ToEnumToIntercept(enumSymbol, name, nameSpace);
    }

    static EnumToIntercept ToEnumToIntercept(INamedTypeSymbol enumSymbol, string? name, string? nameSpace)
    {
        name ??= enumSymbol.Name + "Extensions";
        nameSpace ??= enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();

        return new(name, enumSymbol.ToString(), nameSpace);
    }

    private static bool InterceptorPredicate(SyntaxNode node, CancellationToken ct) =>
        node is InvocationExpressionSyntax {Expression: MemberAccessExpressionSyntax {Name.Identifier.ValueText: "ToString" or "HasFlag"}};

    private static CandidateInvocation? InterceptorParser(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.Node is InvocationExpressionSyntax {Expression: MemberAccessExpressionSyntax {Name: { } nameSyntax}} invocation
            && ctx.SemanticModel.GetOperation(ctx.Node, ct) is IInvocationOperation targetOperation
            && targetOperation.TargetMethod is {Name : "ToString" or "HasFlag", ContainingType: {Name: "Enum", ContainingNamespace: {Name: "System", ContainingNamespace.IsGlobalNamespace: true}}}
            && targetOperation.Instance?.Type is { } type
#pragma warning disable RSEXPERIMENTAL002 // / Experimental interceptable location API
            && ctx.SemanticModel.GetInterceptableLocation(invocation) is { } location)
#pragma warning restore RSEXPERIMENTAL002
        {
            var targetToIntercept = targetOperation.TargetMethod.Name switch
            {
                "ToString" => InterceptorTarget.ToString,
                "HasFlag" => InterceptorTarget.HasFlag,
                _ => default, // can't be reached
            };
            return new CandidateInvocation(location, type.ToString(), targetToIntercept);
        }

        return null;
    }

    private MethodToIntercept? FilterInterceptorCandidates(
        (EnumToIntercept Enum, ImmutableArray<CandidateInvocation> Candidates) arg1, 
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
        var (result, filename) = SourceGenerationHelper.GenerateInterceptorsClass(toIntercept!);
        spc.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }
}
