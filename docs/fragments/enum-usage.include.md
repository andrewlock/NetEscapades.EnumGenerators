Adding the package will automatically add a marker attribute, `[EnumExtensions]`, to your project.

To use the generator, add the `[EnumExtensions]` attribute to an enum. For example:

```csharp
[EnumExtensions]
public enum MyEnum
{
    First,

    [EnumMember(Value = "2nd")]
    Second,
}
```

This will generate a class called `MyEnumExtensions` (by default), which contains a number of helper methods. For example:

```csharp
public static partial class MyEnumExtensions
{
    public const int Length = 2;

    public static string ToStringFast(this MyEnum value, bool useMetadataAttributes)
        => useMetadataAttributes ? value.ToStringFastWithMetadata() : value.ToStringFast();

    public static string ToStringFast(this MyEnum value)
        => value switch
        {
            MyEnum.First => nameof(MyEnum.First),
            MyEnum.Second => nameof(MyEnum.Second),
            _ => value.ToString(),
        };

    private static string ToStringFastWithMetadata(this MyEnum value)
        => value switch
        {
            MyEnum.First => nameof(MyEnum.First),
            MyEnum.Second => "2nd",
            _ => value.ToString(),
        };

    public static bool IsDefined(MyEnum value)
        => value switch
        {
            MyEnum.First => true,
            MyEnum.Second => true,
            _ => false,
        };

    public static bool IsDefined(string name) => IsDefined(name, allowMatchingMetadataAttribute: false);

    public static bool IsDefined(string name, bool allowMatchingMetadataAttribute)
    {
        var isDefinedInDisplayAttribute = false;
        if (allowMatchingMetadataAttribute)
        {
            isDefinedInDisplayAttribute = name switch
            {
                "2nd" => true,
                _ => false,
            };
        }

        if (isDefinedInDisplayAttribute)
        {
            return true;
        }


        return name switch
        {
            nameof(MyEnum.First) => true,
            nameof(MyEnum.Second) => true,
            _ => false,
        };
    }

    public static MyEnum Parse(string? name)
        => TryParse(name, out var value, false, false) ? value : ThrowValueNotFound(name);

    public static MyEnum Parse(string? name, bool ignoreCase)
        => TryParse(name, out var value, ignoreCase, false) ? value : ThrowValueNotFound(name);

    public static MyEnum Parse(string? name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => TryParse(name, out var value, ignoreCase, allowMatchingMetadataAttribute) ? value : throw new ArgumentException($"Requested value '{name}' was not found.");

    public static bool TryParse(string? name, out MyEnum value)
        => TryParse(name, out value, false, false);

    public static bool TryParse(string? name, out MyEnum value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    public static bool TryParse(string? name, out MyEnum value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => ignoreCase
            ? TryParseIgnoreCase(name, out value, allowMatchingMetadataAttribute)
            : TryParseWithCase(name, out value, allowMatchingMetadataAttribute);

    private static bool TryParseIgnoreCase(string? name, out MyEnum value, bool allowMatchingMetadataAttribute)
    {
        if (allowMatchingMetadataAttribute)
        {
            switch (name)
            {
                case string s when s.Equals("2nd", System.StringComparison.OrdinalIgnoreCase):
                    value = MyEnum.Second;
                    return true;
                default:
                    break;
            };
        }

        switch (name)
        {
            case string s when s.Equals(nameof(MyEnum.First), System.StringComparison.OrdinalIgnoreCase):
                value = MyEnum.First;
                return true;
            case string s when s.Equals(nameof(MyEnum.Second), System.StringComparison.OrdinalIgnoreCase):
                value = MyEnum.Second;
                return true;
            case string s when int.TryParse(name, out var val):
                value = (MyEnum)val;
                return true;
            default:
                value = default;
                return false;
        }
    }

    private static bool TryParseWithCase(string? name, out MyEnum value, bool allowMatchingMetadataAttribute)
    {
        if (allowMatchingMetadataAttribute)
        {
            switch (name)
            {
                case "2nd":
                    value = MyEnum.Second;
                    return true;
                default:
                    break;
            };
        }

        switch (name)
        {
            case nameof(MyEnum.First):
                value = MyEnum.First;
                return true;
            case nameof(MyEnum.Second):
                value = MyEnum.Second;
                return true;
            case string s when int.TryParse(name, out var val):
                value = (MyEnum)val;
                return true;
            default:
                value = default;
                return false;
        }
    }

    public static MyEnum[] GetValues()
    {
        return new[]
        {
            MyEnum.First,
            MyEnum.Second,
        };
    }

    public static string[] GetNames()
    {
        return new[]
        {
            nameof(MyEnum.First),
            nameof(MyEnum.Second),
        };
    }
}
```

If you create a "Flags" `enum` by decorating it with the `[Flags]` attribute, an additional method is created, which provides a bitwise alternative to the `Enum.HasFlag(flag)` method:

```csharp
public static bool HasFlagFast(this MyEnum value, MyEnum flag)
    => flag == 0 ? true : (value & flag) == flag;
```

Note that if you provide a `[EnumMember]` attribute, the value you provide for this attribute can be used by methods like `ToStringFast()` and `TryParse()` by passing the argument `useMetadataAttributes: true`. Alternatively, you can use the `[Display]` or `[Description]` attributes, and set  the `MetadataSource` property on the `[EnumExtensions]` attribute e.g.

```csharp
[EnumExtensions(MetadataSource = MetadataSource.DisplayAttribute)]
public enum EnumWithDisplayNameInNamespace
{
    First = 0,
    [Display(Name = "2nd")]
    Second = 1,
    Third = 2,
}
```

Alternatively, you can use `MetadataSource.None` to choose none of the metadata attributes. In this case, the overloads that take a `useMetadataAttributes` parameter will not be emitted.

You can set the default metadata source to use for a whole project by setting the `EnumGenerator_EnumMetadataSource` property in your project:

```xml
<PropertyGroup>
  <EnumGenerator_EnumMetadataSource>EnumMemberAttribute</EnumGenerator_EnumMetadataSource>
</PropertyGroup>
```

You can override the name of the extension class by setting `ExtensionClassName` in the attribute and/or the namespace of the class by setting `ExtensionClassNamespace`.

### Controlling extension accessibility

By default, the generated extension class is `public` if the enum is `public`, and `internal` if the enum is `internal`. You can control the accessibility of the generated extension classes in two ways:

**Global Configuration (MSBuild Property):**

Set the `EnumGenerator_ForceInternal` property in your project file to force all generated extension classes to be `internal`:

```xml
<PropertyGroup>
  <!-- Make all generated extension classes internal -->
  <EnumGenerator_ForceInternal>true</EnumGenerator_ForceInternal>
</PropertyGroup>
```

Valid values are:
- `false` (default): Generated extensions follow the enum's accessibility (`public` for `public` enums, `internal` for `internal` enums)
- `true`: Forces all generated extensions to be `internal`, even for `public` enums

**Per-Enum Configuration:**

You can also set the `IsInternal` property on individual enums using the `[EnumExtensions]` attribute:

```csharp
// Force this enum's extensions to be internal
[EnumExtensions(IsInternal = true)]
public enum MyEnum { ... }
```

or for external enums:

```csharp
// Force ExternalEnum's extensions to be internal
[EnumExtensions<ExternalEnum>(IsInternal = true)]
```
