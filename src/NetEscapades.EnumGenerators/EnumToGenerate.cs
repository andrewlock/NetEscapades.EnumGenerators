namespace NetEscapades.EnumGenerators;

public readonly struct EnumToGenerate : IEquatable<EnumToGenerate>
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

    public readonly bool IsDisplayAttributeUsed;

    public EnumToGenerate(
    string name,
    string ns,
    string fullyQualifiedName,
    string underlyingType,
    bool isPublic,
    List<KeyValuePair<string, EnumValueOption>> names,
    bool hasFlags,
    bool isDisplayAttributeUsed)
    {
        Name = name;
        Namespace = ns;
        UnderlyingType = underlyingType;
        Names = names;
        HasFlags = hasFlags;
        IsPublic = isPublic;
        FullyQualifiedName = fullyQualifiedName;
        IsDisplayAttributeUsed = isDisplayAttributeUsed;
    }

    public bool Equals(EnumToGenerate other)
    {
        return Name == other.Name &&
            Namespace == other.Namespace &&
            UnderlyingType == other.UnderlyingType &&
            Names.All(other.Names.Contains) &&
            HasFlags == other.HasFlags &&
            IsPublic == other.IsPublic &&
            FullyQualifiedName == other.FullyQualifiedName &&
            IsDisplayAttributeUsed == other.IsDisplayAttributeUsed;
    }
}