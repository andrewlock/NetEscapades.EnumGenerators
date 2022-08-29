using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators;

[Generator]
public partial class EnumGenerator : IIncrementalGenerator
{
    private const string DisplayAttribute = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string EnumExtensionsAttribute = "NetEscapades.EnumGenerators.EnumExtensionsAttribute";
    private const string HasFlagsAttribute = "System.HasFlagsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValueProvider<CompilationInfo> compilationInfo = 
            context.CompilationProvider
                .Select((c, _) => new CompilationInfo(c.GetTypeByMetadataName(EnumExtensionsAttribute), c.GetTypeByMetadataName(DisplayAttribute), c.GetTypeByMetadataName(HasFlagsAttribute)));

        IncrementalValuesProvider<ISymbol> enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValuesProvider<EnumToGenerate?> enumToGenerate = enumDeclarations.Combine(compilationInfo)
            .Select((source, ct) => GetTypesToGenerate(source.Right, source.Left, ct))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(enumToGenerate,
                static (spc, source) => Execute(source, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is EnumDeclarationSyntax m && m.AttributeLists.Count > 0;

    static ISymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName == EnumExtensionsAttribute)
                {
                    // return the enum
                    return context.SemanticModel.GetDeclaredSymbol(enumDeclarationSyntax);
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    static void Execute(EnumToGenerate? enumToGenerate, SourceProductionContext context)
    {
        if (enumToGenerate is EnumToGenerate eg && enumToGenerate != null)
        {
            StringBuilder sb = new();
            var result = SourceGenerationHelper.GenerateExtensionClass(sb, eg);
            context.AddSource(eg.Name + "_EnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    static EnumToGenerate? GetTypesToGenerate(CompilationInfo compilationInfo, ISymbol declaredSymbol, CancellationToken ct)
    {
        INamedTypeSymbol? enumAttribute = compilationInfo.EnumAttribute;
        if (enumAttribute == null)
        {
            // nothing to do if this type isn't available
            return null;
        }

        INamedTypeSymbol? displayAttribute = compilationInfo.DisplayAttribute;
        INamedTypeSymbol? hasFlagsAttribute = compilationInfo.HasFlagsAttribute;
        // stop if we're asked to
        ct.ThrowIfCancellationRequested();

        if (declaredSymbol is not INamedTypeSymbol enumSymbol)
        {
            // report diagnostic, something went wrong
            return null;
        }

        string name = enumSymbol.Name + "Extensions";
        string nameSpace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();
        var hasFlags = false;

        foreach (AttributeData attributeData in enumSymbol.GetAttributes())
        {
            if (hasFlagsAttribute is not null && hasFlagsAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
            {
                hasFlags = true;
                continue;
            }

            if (!enumAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
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

        string fullyQualifiedName = enumSymbol.ToString();
        string underlyingType = enumSymbol.EnumUnderlyingType?.ToString() ?? "int";

        var enumMembers = enumSymbol.GetMembers();
        var members = new List<KeyValuePair<string, EnumValueOption>>(enumMembers.Length);
        var displayNames = new HashSet<string>();
        var isDisplayNameTheFirstPresence = false;

        foreach (var member in enumMembers)
        {
            if (member is not IFieldSymbol field
                || field.ConstantValue is null)
            {
                continue;
            }

            string? displayName = null;
            if (displayAttribute is not null)
            {
                foreach (var attribute in member.GetAttributes())
                {
                    if (!displayAttribute.Equals(attribute.AttributeClass, SymbolEqualityComparer.Default))
                    {
                        continue;
                    }

                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == "Name" && namedArgument.Value.Value?.ToString() is { } dn)
                        {
                            displayName = dn;
                            isDisplayNameTheFirstPresence = displayNames.Add(displayName);
                            break;
                        }
                    }
                }
            }

            members.Add(new KeyValuePair<string, EnumValueOption>(member.Name, new EnumValueOption(displayName, isDisplayNameTheFirstPresence)));
        }

        return new EnumToGenerate(
            name: name,
            fullyQualifiedName: fullyQualifiedName,
            ns: nameSpace,
            underlyingType: underlyingType,
            isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public,
            hasFlags: hasFlags,
            names: members,
            isDisplayAttributeUsed: displayNames.Count > 0);
    }
}
