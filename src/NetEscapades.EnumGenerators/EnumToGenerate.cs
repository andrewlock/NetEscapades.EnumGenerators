namespace NetEscapades.EnumGenerators;

public readonly struct EnumToGenerate
{
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;
    public readonly bool IsPublic;
    public readonly bool HasFlags;
    public readonly string UnderlyingType;

    /// <summary>
    /// Key is the enum name. Value is the custom name setted by the <c>[Display(Name)]</c> attribute.
    /// </summary>
    public readonly List<KeyValuePair<string, string?>> Names;

    public EnumToGenerate(
    string name,
    string ns,
    string fullyQualifiedName,
    string underlyingType,
    bool isPublic,
    List<KeyValuePair<string, string?>> names,
    bool hasFlags)
    {
        Name = name;
        Namespace = ns;
        UnderlyingType = underlyingType;
        Names = names;
        HasFlags = hasFlags;
        IsPublic = isPublic;
        FullyQualifiedName = fullyQualifiedName;
    }
}