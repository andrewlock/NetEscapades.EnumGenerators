namespace NetEscapades.EnumGenerators;

/// <summary>
/// Add to enums to indicate that extension methods should be generated for the type
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class EnumExtensionsAttribute : Attribute
{
    /// <summary>
    /// The namespace to generate the extension class.
    /// If not provided the namespace of the enum will be used
    /// </summary>
    public string? ExtensionClassNamespace { get; set; }

    /// <summary>
    /// The name to use for the extension class.
    /// If not provided, the enum name with "Extensions" will be used.
    /// For example for an Enum called StatusCodes, the default name
    /// will be StatusCodesExtensions
    /// </summary>
    public string? ExtensionClassName { get; set; }
}
