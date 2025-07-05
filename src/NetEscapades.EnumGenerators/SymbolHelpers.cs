using Microsoft.CodeAnalysis;

namespace NetEscapades.EnumGenerators;

internal static class SymbolHelpers
{
    public static bool IsNestedInGenericType(INamedTypeSymbol enumSymbol)
    {
        var containingType = enumSymbol.ContainingType;
        while (containingType is not null)
        {
            if (containingType.IsGenericType)
            {
                return true;
            }
            containingType = containingType.ContainingType;
        }
        return false;
    }
}