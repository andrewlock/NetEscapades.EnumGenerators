namespace NetEscapades.EnumGenerators;

public readonly struct EnumValueOption
{
    /// <summary>
    /// Custom name setted by the <c>[Display(Name)]</c> attribute.
    /// </summary>
    public string? DisplayName { get; }
    public bool IsDisplayNameTheFirstPresence { get; }

    public EnumValueOption(string? displayName, bool isDisplayNameTheFirstPresence)
    {
        DisplayName = displayName;
        IsDisplayNameTheFirstPresence = isDisplayNameTheFirstPresence;
    }
}
