using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace NetEscapades.EnumGenerators.Diagnostics;

public static class AnalyzerHelpers
{
    public static (INamedTypeSymbol? enumExtensionsAttr, ExternalEnumDictionary? externalEnumTypes) GetEnumExtensionAttributes(Compilation compilation)
    {
        var enumExtensionsAttr =
            compilation.GetTypeByMetadataName(Attributes.EnumExtensionsAttribute);
        var externalEnumExtensionsAttr =
            compilation.GetTypeByMetadataName(Attributes.ExternalEnumExtensionsAttribute);

        if (enumExtensionsAttr is null)
        {
            return (enumExtensionsAttr, null);
        }

        // Collect all enum types that have EnumExtensions<T> attributes
        var externalEnumTypes = new ExternalEnumDictionary(SymbolEqualityComparer.Default);
        if (externalEnumExtensionsAttr is not null)
        {
            foreach (var attribute in compilation.Assembly.GetAttributes())
            {
                if (attribute.AttributeClass is { IsGenericType: true } attrClass &&
                    SymbolEqualityComparer.Default.Equals(attrClass.ConstructedFrom, externalEnumExtensionsAttr) &&
                    attrClass.TypeArguments is [INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType])
                {
                    var details = ExtractExtensionClassDetails(enumType, attribute);
                    externalEnumTypes.Add(enumType, details);
                }
            }
        }

        return (enumExtensionsAttr, externalEnumTypes);
    }

    public static bool IsEnumWithExtensions(
        ITypeSymbol receiverType,
        INamedTypeSymbol enumExtensionsAttr,
        ExternalEnumDictionary externalEnumTypes,
        [NotNullWhen(true)] out string? extensionNamespace,
        [NotNullWhen(true)] out string? extensionClass)
    {
        // Check if the enum has the [EnumExtensions] attribute or is referenced in EnumExtensions<T>
        // First check if the enum itself has the attribute
        foreach (var attributeData in receiverType.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(
                    attributeData.AttributeClass,
                    enumExtensionsAttr))
            {
                (extensionNamespace, extensionClass) = ExtractExtensionClassDetails(receiverType, attributeData);
                return true;
            }
        }

        // If not, check if it's in the external enum types (EnumExtensions<T>)
        if(receiverType is INamedTypeSymbol namedType
               && externalEnumTypes.TryGetValue(namedType, out var details))
        {
            extensionNamespace = details.Namespace;
            extensionClass = details.Class;
            return true;
        }

        extensionNamespace = null;
        extensionClass = null;
        return false;
    }

    private static (string Namespace, string Class) ExtractExtensionClassDetails(
        ITypeSymbol receiverType,
        AttributeData attributeData)
    {
        string? nameSpace = null;
        string? className = null;

        if (!attributeData.NamedArguments.IsDefaultOrEmpty)
        {
            // extract the extension namespace
            foreach (var (key, value) in attributeData.NamedArguments)
            {
                if (key == nameof(EnumExtensionsAttribute.ExtensionClassNamespace) &&
                    value is { Kind: TypedConstantKind.Primitive, Value: string ns } &&
                    !string.IsNullOrWhiteSpace(ns))
                {
                    nameSpace = ns;
                }
                if (key == nameof(EnumExtensionsAttribute.ExtensionClassName) &&
                    value is { Kind: TypedConstantKind.Primitive, Value: string name } &&
                    !string.IsNullOrWhiteSpace(name))
                {
                    className = name;
                }
            }
        }

        return (Namespace: nameSpace ?? EnumGenerator.GetEnumExtensionNamespace(receiverType),
            Class: className ?? EnumGenerator.GetEnumExtensionName(receiverType));
    }
}