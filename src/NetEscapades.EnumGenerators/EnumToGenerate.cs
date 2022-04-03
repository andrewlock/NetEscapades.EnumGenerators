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
    /// Key is the enum name.
    /// </summary>
    public readonly List<KeyValuePair<string, EnumValueOption>> Names;

    public readonly bool IsDisplaAttributeUsed;

    public EnumToGenerate(
    string name,
    string ns,
    string fullyQualifiedName,
    string underlyingType,
    bool isPublic,
    List<KeyValuePair<string, EnumValueOption>> names,
    bool hasFlags,
    bool isDisplaAttributeUsed)
    {
        Name = name;
        Namespace = ns;
        UnderlyingType = underlyingType;
        Names = names;
        HasFlags = hasFlags;
        IsPublic = isPublic;
        FullyQualifiedName = fullyQualifiedName;
        IsDisplaAttributeUsed = isDisplaAttributeUsed;
    }
}