using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators;

[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    private const string EnumExtensionsAttribute = "NetEscapades.EnumGenerators.EnumExtensionsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(enumDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is EnumDeclarationSyntax m && m.AttributeLists.Count > 0;

    static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
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

                // Is the attribute the [LoggerMessage] attribute?
                if (fullName == EnumExtensionsAttribute)
                {
                    // return the enum
                    return enumDeclarationSyntax;
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    static void Execute(Compilation compilation, ImmutableArray<EnumDeclarationSyntax> enums, SourceProductionContext context)
    {
        if (enums.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        IEnumerable<EnumDeclarationSyntax> distinctEnums = enums.Distinct();

        List<EnumToGenerate> enumsToGenerate = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);
        if (enumsToGenerate.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var enumToGenerate in enumsToGenerate)
            {
                sb.Clear();
                var result = SourceGenerationHelper.GenerateExtensionClass(sb, enumToGenerate);
                context.AddSource(enumToGenerate.Name + "_EnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }
    }

    static List<EnumToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<EnumDeclarationSyntax> enums, CancellationToken ct)
    {
        var enumsToGenerate = new List<EnumToGenerate>();
        INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName(EnumExtensionsAttribute);
        if (enumAttribute == null)
        {
            // nothing to do if this type isn't available
            return enumsToGenerate;
        }

        foreach (var enumDeclarationSyntax in enums)
        {
            // stop if we're asked to
            ct.ThrowIfCancellationRequested();

            SemanticModel semanticModel = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
            {
                // report diagnostic, something went wrong
                continue;
            }

            string name = enumSymbol.Name + "Extensions";
            string nameSpace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();

            foreach (AttributeData attributeData in enumSymbol.GetAttributes())
            {
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

                break;
            }

            string fullyQualifiedName = enumSymbol.ToString();
            string underlyingType = enumSymbol.EnumUnderlyingType?.ToString() ?? "int";

            var enumMembers = enumSymbol.GetMembers();
            var members = new List<KeyValuePair<string, object>>(enumMembers.Length);

            foreach (var member in enumMembers)
            {
                if (member is not IFieldSymbol field
                    || field.ConstantValue is null)
                {
                    continue;
                }

                members.Add(new KeyValuePair<string, object>(member.Name, field.ConstantValue));
            }

            enumsToGenerate.Add(new EnumToGenerate(
                name: name,
                fullyQualifiedName: fullyQualifiedName,
                ns: nameSpace,
                underlyingType: underlyingType,
                isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public,
                values: members));
        }

        return enumsToGenerate;
    }

    static string GetNamespace(EnumDeclarationSyntax enumDeclarationSyntax)
    {
        // determine the namespace the class is declared in, if any
        string nameSpace = string.Empty;
        SyntaxNode? potentialNamespaceParent = enumDeclarationSyntax.Parent;
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                namespaceParent = parent;
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
            }
        }

        return nameSpace;
    }
}