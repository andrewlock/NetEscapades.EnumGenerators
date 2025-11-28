namespace NetEscapades.EnumGenerators;

/// <summary>
/// Transform to apply when calling <c>ToStringFast</c> 
/// </summary>
public enum SerializationTransform
{
    /// <summary>
    /// Don't apply a transform to the ToStringFast() result.
    /// </summary>
    None,

    /// <summary>
    /// Call <see cref="string.ToLowerInvariant"/> on the ToStringFast() result.
    /// </summary>
    LowerInvariant,

    /// <summary>
    /// Call <see cref="string.ToUpperInvariant"/> on the ToStringFast() result.
    /// </summary>
    UpperInvariant,
}