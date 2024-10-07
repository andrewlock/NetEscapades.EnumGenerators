using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators
{
    [Generator]
    public class JsonConverterGenerator : IIncrementalGenerator
    {
        private const string EnumJsonConverterAttribute = "NetEscapades.EnumGenerators.EnumJsonConverterAttribute";
        private const string EnumExtensionsAttribute = "NetEscapades.EnumGenerators.EnumExtensionsAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
                "EnumJsonConverterAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.JsonConverterAttribute, Encoding.UTF8)));

            var jsonConvertersToGenerate = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    EnumJsonConverterAttribute,
                    static (node, _) => node is EnumDeclarationSyntax,
                    GetTypeToGenerate)
                .Where(static m => m is not null);

            context.RegisterSourceOutput(jsonConvertersToGenerate,
                static (spc, source) => Execute(source, spc));
        }

        private static void Execute(JsonConverterToGenerate? jsonConverterToGenerate, SourceProductionContext context)
        {
            if (jsonConverterToGenerate is { } eg)
            {
                StringBuilder sb = new();
                var result = SourceGenerationHelper.GenerateJsonConverterClass(sb, eg);
                context.AddSource(eg.ConverterType + ".g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }

        private static JsonConverterToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context,
            CancellationToken ct)
        {
            if (context.TargetSymbol is not INamedTypeSymbol enumSymbol)
            {
                // nothing to do if this type isn't available
                return null;
            }

            ct.ThrowIfCancellationRequested();

            var extensionName = enumSymbol.Name + "Extensions";
            var extensionNamespace = enumSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : enumSymbol.ContainingNamespace.ToString();

            var attributes = enumSymbol.GetAttributes();
            var enumJsonConverterAttribute = attributes.FirstOrDefault(static ad =>
                ad.AttributeClass?.Name == "EnumJsonConverterAttribute" ||
                ad.AttributeClass?.ToDisplayString() == EnumJsonConverterAttribute);

            if (enumJsonConverterAttribute == null)
                return null;

            var enumExtensionsAttribute = attributes.FirstOrDefault(static ad =>
                ad.AttributeClass?.Name == "EnumExtensionsAttribute" ||
                ad.AttributeClass?.ToDisplayString() == EnumExtensionsAttribute);

            if (enumExtensionsAttribute == null)
                return null;

            foreach (var namedArgument in enumExtensionsAttribute.NamedArguments)
            {
                switch (namedArgument.Key)
                {
                    case "ExtensionClassNamespace" when namedArgument.Value.Value?.ToString() is { } ns:
                        extensionNamespace = ns;
                        continue;
                    case "ExtensionClassName" when namedArgument.Value.Value?.ToString() is { } n:
                        extensionName = n;
                        break;
                }
            }

            ProcessNamedArguments(enumJsonConverterAttribute,
                out var caseSensitive,
                out var camelCase,
                out var allowMatchingMetadataAttribute,
                out var propertyName);

            ProcessConstructorArguments(enumJsonConverterAttribute,
                out var converterNamespace,
                out var converterType);

            if (string.IsNullOrEmpty(converterType))
                return null;

            var fullyQualifiedName = enumSymbol.ToString();

            return new JsonConverterToGenerate
            (
                extensionName,
                fullyQualifiedName,
                extensionNamespace,
                converterType!,
                converterNamespace,
                enumSymbol.DeclaredAccessibility == Accessibility.Public,
                caseSensitive,
                camelCase,
                allowMatchingMetadataAttribute,
                propertyName
            );
        }

        private static void ProcessNamedArguments(AttributeData attributeData,
            out bool caseSensitive,
            out bool camelCase,
            out bool allowMatchingMetadataAttribute,
            out string? propertyName)
        {
            caseSensitive = false;
            camelCase = false;
            allowMatchingMetadataAttribute = false;
            propertyName = null;

            foreach (var namedArgument in attributeData.NamedArguments)
            {
                switch (namedArgument.Key)
                {
                    case "CaseSensitive" when namedArgument.Value.Value?.ToString() is { } cs:
                        caseSensitive = bool.Parse(cs);
                        continue;
                    case "CamelCase" when namedArgument.Value.Value?.ToString() is { } cc:
                        camelCase = bool.Parse(cc);
                        continue;
                    case "AllowMatchingMetadataAttribute" when namedArgument.Value.Value?.ToString() is { } amma:
                        allowMatchingMetadataAttribute = bool.Parse(amma);
                        continue;
                    case "PropertyName" when namedArgument.Value.Value?.ToString() is { } pn:
                        propertyName = pn;
                        continue;
                }
            }
        }

        private static void ProcessConstructorArguments(AttributeData attributeData,
            out string? converterNamespace,
            out string? converterType)
        {
            if (attributeData.ConstructorArguments[0].Value is ISymbol symbol)
            {
                converterNamespace = !symbol.ContainingNamespace.IsGlobalNamespace
                    ? symbol.ContainingNamespace.ToString()
                    : string.Empty;

                converterType = symbol.Name;
            }
            else
            {
                converterNamespace = null;
                converterType = null;
            }
        }
    }
}