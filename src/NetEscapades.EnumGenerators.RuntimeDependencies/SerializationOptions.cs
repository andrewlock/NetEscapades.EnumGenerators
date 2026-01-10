namespace NetEscapades.EnumGenerators;

/// <summary>
/// Options to apply when calling <c>ToStringFast</c> on an enum. 
/// </summary>
public readonly struct SerializationOptions
{
    /// <summary>
    /// Create an instance of <see cref="SerializationOptions"/>
    /// </summary>
    /// <param name="useMetadataAttributes">Sets whether the value of any metadata value attribute
    /// values applied to an enum should be used in the <c>ToStringFast</c> call.</param>
    /// <param name="transform">Sets the <see cref="SerializationTransform"/> to use when serializing the enum value.</param>
    public SerializationOptions(
        bool useMetadataAttributes = false,
        SerializationTransform transform = SerializationTransform.None)
    {
        UseMetadataAttributes = useMetadataAttributes;
        Transform = transform;
    }

    /// <summary>
    /// Gets whether the value of any metadata attributes applied to an enum member
    /// should be used as the <c>ToString()</c> value for the enum.
    /// </summary>
    /// <remarks>
    /// By default, it's set to <see langword="false"/>, so the value of metadata attributes on the
    /// enum values are ignored. 
    /// </remarks>
    public bool UseMetadataAttributes { get; }

    /// <summary>
    /// Gets the <see cref="SerializationTransform"/> to use during parsing.
    /// </summary>
    /// <remarks>
    /// By default, it's set to <see cref="SerializationTransform.None"/>, and the value is not transformed. 
    /// </remarks>
    public SerializationTransform Transform { get; }
}