namespace NetEscapades.EnumGenerators;

public readonly struct EnumValueOption : IEquatable<EnumValueOption>
{
    /// <summary>
    /// Custom name set by the <c>[Display(Name)]</c> attribute.
    /// </summary>
    public string? DisplayName { get; }
    public bool IsDisplayNameTheFirstPresence { get; }
    public object ConstantValue { get; }

    public EnumValueOption(string? displayName, bool isDisplayNameTheFirstPresence, object constantValue)
    {
        DisplayName = displayName;
        IsDisplayNameTheFirstPresence = isDisplayNameTheFirstPresence;
        ConstantValue = constantValue;
    }
    
    public bool Equals(EnumValueOption other)
    {
        return DisplayName == other.DisplayName &&
               IsDisplayNameTheFirstPresence == other.IsDisplayNameTheFirstPresence;
    }
}
