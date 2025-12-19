using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics;

public class AnalyzerHelpers
{
    public static (INamedTypeSymbol? enumExtensionsAttr, HashSet<INamedTypeSymbol>? externalEnumTypes) GetEnumExtensionAttributes(
        CompilationStartAnalysisContext ctx)
    {
        var enumExtensionsAttr =
            ctx.Compilation.GetTypeByMetadataName(Attributes.EnumExtensionsAttribute);
        var externalEnumExtensionsAttr =
            ctx.Compilation.GetTypeByMetadataName(Attributes.ExternalEnumExtensionsAttribute);

        if (enumExtensionsAttr is null)
        {
            return (enumExtensionsAttr, null);
        }

        // Collect all enum types that have EnumExtensions<T> attributes
        var externalEnumTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        if (externalEnumExtensionsAttr is not null)
        {
            foreach (var attribute in ctx.Compilation.Assembly.GetAttributes())
            {
                if (attribute.AttributeClass is { IsGenericType: true } attrClass &&
                    SymbolEqualityComparer.Default.Equals(attrClass.ConstructedFrom, externalEnumExtensionsAttr) &&
                    attrClass.TypeArguments is [INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType])
                {
                    externalEnumTypes.Add(enumType);
                }
            }
        }

        return (enumExtensionsAttr, externalEnumTypes);
    }

    public static bool IsEnumWithExtensions(
        ITypeSymbol receiverType,
        INamedTypeSymbol enumExtensionsAttr,
        HashSet<INamedTypeSymbol> externalEnumTypes)
    {
        // Check if the enum has the [EnumExtensions] attribute or is referenced in EnumExtensions<T>
        // First check if the enum itself has the attribute
        foreach (var attributeData in receiverType.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(
                    attributeData.AttributeClass,
                    enumExtensionsAttr))
            {
                return true;
            }
        }

        // If not, check if it's in the external enum types (EnumExtensions<T>)
        return receiverType is INamedTypeSymbol namedType
               && externalEnumTypes.Contains(namedType);
    }
}