namespace NetEscapades.EnumGenerators;

internal static class AccessibilityHelper
{
    /// <summary>
    /// Determines the effective accessibility of generated extension classes based on:
    /// - Enum accessibility (internal enums => always internal extensions)
    /// - Per-enum configuration (IsInternal attribute property)
    /// - Global configuration (MSBuild property)
    /// </summary>
    internal static bool ShouldBeInternal(
        in EnumToGenerate enumToGenerate,
        ExtensionAccessibility defaultAccessibility)
    {
        if (!enumToGenerate.IsPublic)
        {
            return true;
        }
        
        if (enumToGenerate.ForceInternalExtensions.HasValue)
        {
            return enumToGenerate.ForceInternalExtensions.Value;
        }
        
        return defaultAccessibility == ExtensionAccessibility.Internal;
    }
}
