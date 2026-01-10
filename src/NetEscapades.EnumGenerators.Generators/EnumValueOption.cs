namespace NetEscapades.EnumGenerators;

public readonly struct EnumValueOption : IEquatable<EnumValueOption>
{
    /// <summary>
    /// Custom name set by the <c>[Display(Name)]</c> attribute.
    /// </summary>
    private readonly string? _displayName;
    private readonly string? _description;
    private readonly string? _enumMemberValue;
    public object ConstantValue { get; }

    public EnumValueOption(string? displayName, string? description, string? enumMemberValue, object constantValue)
    {
        _displayName = displayName;
        ConstantValue = constantValue;
        _description = description;
        _enumMemberValue = enumMemberValue;
    }
    
    public bool Equals(EnumValueOption other)
    {
        return _displayName == other._displayName &&
               _description == other._description &&
               _enumMemberValue == other._enumMemberValue &&
               Equals(ConstantValue, other.ConstantValue);
    }

    public string? GetMetadataName(MetadataSource metadataSource)
        => metadataSource switch
        {
            MetadataSource.DisplayAttribute => _displayName,
            MetadataSource.DescriptionAttribute => _description,
            MetadataSource.EnumMemberAttribute => _enumMemberValue,
            _ => null,
        };

    public static EnumValueOption CreateWithoutAttributes(object constantValue)
        => new(null, null, null, constantValue);
}
