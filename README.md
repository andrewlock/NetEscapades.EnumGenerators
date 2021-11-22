# NetEscapades.EnumGenerators

![Build status](https://github.com/andrewlock/NetEscapades.EnumGenerators/actions/workflows/BuildAndPack.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/NetEscapades.EnumGenerators.svg)](https://www.nuget.org/packages/NetEscapades.EnumGenerators/)

A Source Generator package that generates extension methods for enums, to allow fast "reflection".

> This source generator requires the .NET 6 SDK. You can target earlier frameworks like .NET Core 3.1 etc, but the _SDK_ must be at least 6.0.100

Add the package to your application using

```bash
dotnet add package NetEscapades.EnumGenerators
```

This will bring in the _NetEscapades.EnumGenerators_ package. This contains the source generator, which automatically adds a marker attribute, `[EnumExtensions]`, to your project. 

To use the generator, add the `[EnumExtensions]` attribute to an enum. For example:

```csharp
[EnumExtensions]
public MyEnum
{
    First,
    Second,
    Third,
}
```

This will generate a class called `MyEnumExtensions` (by default), which contains a number of helper methods. For example:

```csharp
public static partial class EnumInNamespaceExtensions
{
    public static bool IsDefined(this MyEnum value)
        => value switch
        {
            MyEnum.First => true,
            MyEnum.Second => true,
            MyEnum.Third => true,
            _ => false,
        };

    public static string ToStringFast(this MyEnum value)
        => value switch
        {
            MyEnum.First => nameof(MyEnum.First),
            MyEnum.Second => nameof(MyEnum.Second),
            MyEnum.Third => nameof(MyEnum.Third),
            _ => value.ToString(),
        };

    public static bool TryParse(string name, bool ignoreCase, out MyEnum value)
        => ignoreCase ? TryParseIgnoreCase(name, out value) : TryParse(name, out value);

    private static bool TryParseIgnoreCase(string name, out MyEnum value)
    {
        switch (name)
        {
            case { } s when s.Equals(nameof(MyEnum.First), System.StringComparison.OrdinalIgnoreCase):
                value = MyEnum.First;
                return true;
            case { } s when s.Equals(nameof(MyEnum.Second), System.StringComparison.OrdinalIgnoreCase):
                value = MyEnum.Second;
                return true;
            case { } s when s.Equals(nameof(MyEnum.Third), System.StringComparison.OrdinalIgnoreCase):
                value = MyEnum.Third;
                return true;
            default:
                value = default;
                return false;
        }
    }

    public static bool TryParse(string name, out MyEnum value)
    {
        switch (name)
        {
            case nameof(MyEnum.First):
                value = MyEnum.First;
                return true;
            case nameof(MyEnum.Second):
                value = MyEnum.Second;
                return true;
            case nameof(MyEnum.Third):
                value = MyEnum.Third;
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
            MyEnum.Third,
        };
    }

    public static string[] GetNames()
    {
        return new[]
        {
            nameof(MyEnum.First),
            nameof(MyEnum.Second),
            nameof(MyEnum.Third),
        };
    }
}
```

You can override the name of the extension class by setting `ExtensionClassName` in the attribute and/or the namespace of the class by setting `ExtensionClassNamespace`. By default, the class will be public if the enum is public, otherwise it will be internal.