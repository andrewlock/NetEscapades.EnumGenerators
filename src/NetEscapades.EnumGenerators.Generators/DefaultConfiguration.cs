namespace NetEscapades.EnumGenerators;

/// <summary>
/// Represents the default configurations for enum generation
/// </summary>
/// <param name="MetadataSource">The default metadata source to use</param>
/// <param name="ForceExtensionMembers">Whether to force extension members</param>
/// <param name="ExtensionAccessibility">The default accessibility for generated extensions</param>
internal readonly record struct DefaultConfiguration(
    MetadataSource MetadataSource,
    bool ForceExtensionMembers,
    ExtensionAccessibility ExtensionAccessibility);
