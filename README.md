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
}
```

This will generate a class called `MyEnumExtensions` (by default), which contains a number of helper methods. For example:

```csharp
public static partial class MyEnumExtensions
{
    public static string ToStringFast(this MyEnum value)
        => value switch
        {
            MyEnum.First => nameof(MyEnum.First),
            MyEnum.Second => nameof(MyEnum.Second),
            _ => value.ToString(),
        };

   public static bool IsDefined(MyEnum value)
        => value switch
        {
            MyEnum.First => true,
            MyEnum.Second => true,
            _ => false,
        };

    public static bool IsDefined(string name)
        => name switch
        {
            nameof(MyEnum.First) => true,
            nameof(MyEnum.Second) => true,
            _ => false,
        };

    public static bool TryParse(
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? name, 
        bool ignoreCase, 
        out MyEnum value)
        => ignoreCase ? TryParseIgnoreCase(name, out value) : TryParse(name, out value);

    private static bool TryParseIgnoreCase(
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? name, 
        out MyEnum value)
    {
        switch (name)
        {
            case { } s when s.Equals(nameof(MyEnum.First), System.StringComparison.OrdinalIgnoreCase):
                value = MyEnum.First;
                return true;
            case { } s when s.Equals(nameof(MyEnum.Second), System.StringComparison.OrdinalIgnoreCase):
                value = MyEnum.Second;
                return true;
            case { } s when int.TryParse(name, out var val):
                value = (MyEnum)val;
                return true;
            default:
                value = default;
                return false;
        }
    }

    public static bool TryParse(
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? name, 
        out MyEnum value)
    {
        switch (name)
        {
            case nameof(MyEnum.First):
                value = MyEnum.First;
                return true;
            case nameof(MyEnum.Second):
                value = MyEnum.Second;
                return true;
            case { } s when int.TryParse(name, out var val):
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

You can override the name of the extension class by setting `ExtensionClassName` in the attribute and/or the namespace of the class by setting `ExtensionClassNamespace`. By default, the class will be public if the enum is public, otherwise it will be internal.

## CS0436 and [InternalsVisibleTo]

The source generator automatically adds the `[EnumExtensions]` attributes to your compilation as an `internal` attribute. If you add the source generator package to multiple projects, and use the `[InternalsVisibleTo]` attribute, you may experience errors when you build:

```bash
warning CS0436: The type 'EnumExtensionsAttribute' in 'NetEscapades.EnumGenerators\EnumExtensionsAttribute.cs' conflicts with the imported type 'EnumExtensionsAttribute' in 'MyProject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'.
```

Removing the `[InternalsVisibleTo]` attribute will resolve the problem, but if this is not possible you can disable the auto-generation of the `[EnumExtensions]` marker attributes, and rely on the helper [`NetEscapades.EnumExtensions.Attributes` package instead](https://www.nuget.org/packages/NetEscapades.EnumExtensions.Attributes). This package contains the same attributes, but as they are in an external package, you can avoid the CS0436 error.

Add the package to your solution, ensuring you set `"PrivateAssets="All"` in the `<PackageReference>`. To disable the auto-generation of the marker attributes, define the constant `NETESCAPADES_ENUMGENERATORS_EXCLUDE_ATTRIBUTES` in your project file. Your project file should look something like the following:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <DefineConstants>NETESCAPADES_ENUMGENERATORS_EXCLUDE_ATTRIBUTES</DefineConstants>
  </PropertyGroup>
  
  <!-- Core package -->
  <PackageReference Include="NetEscapades.EnumExtensions" Version="1.0.0-beta03" />
  <PackageReference Include="NetEscapades.EnumExtensions.Attributes" Version="1.0.0-beta03">
    <PrivateAssets>All</PrivateAssets>
  </PackageReference>
  <!-- -->

</Project>
```

The attribute library is only required at compile time, so it won't appear in your build output. 
