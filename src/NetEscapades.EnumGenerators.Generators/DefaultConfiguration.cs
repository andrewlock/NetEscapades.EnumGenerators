namespace NetEscapades.EnumGenerators;

/// <summary>
/// Represents the default configurations for enum generation
/// </summary>
internal readonly record struct DefaultConfiguration(
    MetadataSource MetadataSource,
    bool ForceExtensionMembers,
    bool ForceInternalAccessModifier);
