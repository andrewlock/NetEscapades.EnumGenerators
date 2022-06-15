namespace NetEscapades.EnumGenerators;

public readonly struct EnumValueOption
{
    /// <summary>
    /// Custom name setted by the <c>[Display(Name)]</c> attribute.
    /// </summary>
    public string? DisplayName { get; }
    public bool IsDisplayNameTheFirstPresence { get; }

    /// <summary>
    /// Custom name setted by the <c>[Description(Name)]</c> attribute.
    /// </summary>
    public string? DescriptionName { get; }

    public EnumValueOption(string? displayName, bool isDisplayNameTheFirstPresence, string? descriptionName)
    {
        DisplayName = displayName;
        IsDisplayNameTheFirstPresence = isDisplayNameTheFirstPresence;
        DescriptionName = descriptionName;
    }
}
