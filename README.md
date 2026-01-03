# ![](https://raw.githubusercontent.com/andrewlock/NetEscapades.EnumGenerators/refs/heads/main/icon_32.png) NetEscapades.EnumGenerators

![Build status](https://github.com/andrewlock/NetEscapades.EnumGenerators/actions/workflows/BuildAndPack.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/NetEscapades.EnumGenerators.svg)](https://www.nuget.org/packages/NetEscapades.EnumGenerators/)

A Source Generator package that generates extension methods for enums, to allow fast "reflection".

> This source generator requires the .NET 7 SDK. You can target earlier frameworks like .NET Core 3.1 etc, but the _SDK_ must be at least 7.0.100


## Why use this package?

Many methods that work with enums are surprisingly slow. Calling `ToString()` or `HasFlag()` on an enum seems like it _should_ be fast, but it often isn't. This package provides a set of extension methods, such as `ToStringFast()` or `HasFlagFast()` that are designed to be very fast, with fewer allocations.


For example, the following benchmark shows the advantage of calling `ToStringFast()` over `ToString()`:

``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1348 (20H2/October2020Update)
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
  DefaultJob : .NET Framework 4.8 (4.8.4420.0), X64 RyuJIT
.NET SDK=6.0.100
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
```

|       Method |     FX |       Mean |     Error |    StdDev | Ratio |  Gen 0 | Allocated |
|------------- |--------|-----------:|----------:|----------:|------:|-------:|----------:|
| ToString |`net48` | 578.276 ns | 3.3109 ns | 3.0970 ns | 1.000 | 0.0458 |      96 B |
| ToStringFast |`net48` |   3.091 ns | 0.0567 ns | 0.0443 ns | 0.005 |      - |         - |
| ToString |`net6.0`| 17.985 ns | 0.1230 ns | 0.1151 ns | 1.000 | 0.0115 |      24 B |
| ToStringFast |`net6.0`|  0.121 ns | 0.0225 ns | 0.0199 ns | 0.007 |      - |         - |

Enabling these additional extension methods is as simple as adding an attribute to your enum:

```csharp
[EnumExtensions] // 👈 Add this
public enum Color
{
    Red = 0,
    Blue = 1,
}
```

## Adding NetEscapades.EnumGenerators to your project

Add the package to your application using

```bash
dotnet add package NetEscapades.EnumGenerators
```


This adds a `<PackageReference>` to your project. You can additionally mark the package as `PrivateAssets="all"` and `ExcludeAssets="runtime"`.

> Setting `PrivateAssets="all"` means any projects referencing this one won't get a reference to the _NetEscapades.EnumGenerators_ package. Setting `ExcludeAssets="runtime"` ensures the _NetEscapades.EnumGenerators.Attributes.dll_ file is not copied to your build output (it is not required at runtime).

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta16" 
    PrivateAssets="all" ExcludeAssets="runtime" />
  <!-- -->

</Project>
```

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

You can set the default metadata source to use for a whole project by setting the EnumGenerator_EnumMetadataSource property in your project:

```xml
<PropertyGroup>
  <EnumGenerator_EnumMetadataSource>EnumMemberAttribute</EnumGenerator_EnumMetadataSource>
</PropertyGroup>
```

You can override the name of the extension class by setting `ExtensionClassName` in the attribute and/or the namespace of the class by setting `ExtensionClassNamespace`. By default, the class will be public if the enum is public, otherwise it will be internal.

## Enabling interception

Interceptors were introduced as an experimental feature in C#12 with .NET 8. They allow a source generator to "intercept" certain method calls, and replace the call with a different one. _NetEscapades.EnumGenerators_ has support for intercepting `ToString()` and `HasFlag()` method calls.

> To use interceptors, you must be using at least version 8.0.400 of the .NET SDK. [This ships with Visual Studio version 17.11](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs), so you will need at least that version or higher.

To enable interception for a project, add the interceptor package to your application using:

```bash
dotnet add package NetEscapades.EnumGenerators.Interceptors
```

This adds a `<PackageReference>` to your project. You can additionally mark the package as `PrivateAssets="all"` and `ExcludeAssets="runtime"`.

> Setting `PrivateAssets="all"` means any projects referencing this one won't get a reference to the _NetEscapades.EnumGenerators.Interceptors_ package. Setting `ExcludeAssets="runtime"` ensures the _NetEscapades.EnumGenerators.Interceptors.Attributes.dll_ file is _not_ copied to your build output (it is not required at runtime).

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators.Interceptors" Version="1.0.0-beta16" 
    PrivateAssets="all" ExcludeAssets="runtime" />
  <!-- -->

</Project>
```

By default, adding [NetEscapades.EnumGenerators.Interceptors](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Interceptors) to a project enables interception for all enums defined in the project that use the `[EnumExtensions]` or `[EnumExtensions<T>]` attributes. If you wish to intercept calls made to enums with extensions defined in _other_ projects, you must add the `[Interceptable<T>]` attribute in the project where you want the interception to happen, e.g.

```csharp
[assembly:Interceptable<DateTimeKind>]
[assembly:Interceptable<Color>]
```

If you don't want a specific enum to be intercepted, you can set the `IsInterceptable` property to `false`, e.g.

```csharp
[EnumExtensions(IsInterceptable = false)]
public enum Colour
{
    Red = 0,
    Blue = 1,
}
```

Interception only works when the target type is unambiguously an interceptable enum, so it won't work


- When `ToString()` is called in other source generated code.
- When `ToString()` is called in already-compiled code.
- If the `ToString()` call is _implicit_ (for example in `string` interpolation)
- If the `ToString()` call is made on a base type, such as `System.Enum` or `object`
- If the `ToString()` call is made on a generic type

## Usage Analyzers

_NetEscapades.EnumGenerators_ includes optional analyzers that encourage the use of the generated extension methods instead of the built-in `System.Enum` methods. These analyzers can help improve performance by suggesting the faster, generated, alternatives like `ToStringFast()`, `HasFlagFast()`, and `TryParse()`.

### Enabling the analyzers

The usage analyzers are disabled by default. To enable them, add a `.globalconfig` file to your project with the following content:

```ini
is_global = true
netescapades.enumgenerators.usage_analyzers.enable = true
```

The project should auto detect the analyzers, and enables all of the usage analyzers with the default severity.
### Configuring analyzer severity (optional)

Once enabled, you can optionally configure the severity of individual analyzer rules using an `.editorconfig` file. For example:

```ini
[*.{cs,vb}]

# NEEG004: Use ToStringFast() instead of ToString()
dotnet_diagnostic.NEEG004.severity = warning

# NEEG005: Use HasFlagFast() instead of HasFlag()
dotnet_diagnostic.NEEG005.severity = warning

# NEEG006: Use generated IsDefined() instead of Enum.IsDefined()
dotnet_diagnostic.NEEG006.severity = warning

# NEEG007: Use generated Parse() instead of Enum.Parse()
dotnet_diagnostic.NEEG007.severity = warning

# NEEG008: Use generated GetNames() instead of Enum.GetNames()
dotnet_diagnostic.NEEG008.severity = warning

# NEEG009: Use generated GetValues() instead of Enum.GetValues()
dotnet_diagnostic.NEEG009.severity = warning

# NEEG010: Use generated GetValuesAsUnderlyingType() instead of Enum.GetValuesAsUnderlyingType()
dotnet_diagnostic.NEEG010.severity = warning

# NEEG011: Use generated TryParse() instead of Enum.TryParse()
dotnet_diagnostic.NEEG011.severity = warning
```

Valid severity values include: `none`, `silent`, `suggestion`, `warning`, and `error`.

### Code fixes

All usage analyzers include automatic code fixes. When a diagnostic is triggered, you can use the quick fix functionality in your IDE to automatically replace the `System.Enum` method with the corresponding generated extension method.

## Embedding the attributes in your project

By default, the `[EnumExtensions]` attributes referenced in your application are contained in an external dll. It is also possible to embed the attributes directly in your project, so they appear in the dll when your project is built. If you wish to do this, you must do two things:

1. Define the MSBuild constant `NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES`. This ensures the attributes are embedded in your project
2. Add `compile` to the list of excluded assets in your `<PackageReference>` element. This ensures the attributes in your project are referenced, instead of the _NetEscapades.EnumGenerators.Attributes.dll_ library.

Your project file should look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <!--  Define the MSBuild constant    -->
    <DefineConstants>$(DefineConstants);NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta16" 
                    PrivateAssets="all"
                    ExcludeAssets="compile;runtime" />
<!--                               ☝ Add compile to the list of excluded assets. -->

</Project>
```

## Preserving usages of the `[EnumExtensions]` attribute

The `[EnumExtensions]` attribute is decorated with the `[Conditional]` attribute, [so their usage will not appear in the build output of your project](https://andrewlock.net/conditional-compilation-for-ignoring-method-calls-with-the-conditionalattribute/#applying-the-conditional-attribute-to-classes). If you use reflection at runtime on one of your `enum`s, you will not find `[EnumExtensions]` in the list of custom attributes.

If you wish to preserve these attributes in the build output, you can define the `NETESCAPADES_ENUMGENERATORS_USAGES` MSBuild variable. Note that this means your project will have a runtime-dependency on _NetEscapades.EnumGenerators.Attributes.dll_ so you need to ensure this is included in your build output.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <!--  Define the MSBuild constant to preserve usages   -->
    <DefineConstants>$(DefineConstants);NETESCAPADES_ENUMGENERATORS_USAGES</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta16" PrivateAssets="all" />
  <!--              ☝ You must not exclude the runtime assets in this case -->

</Project>
```

## Error CS0436 and [InternalsVisibleTo]

> In the latest version of _NetEscapades.EnumGenerators_, you should not experience error CS0436 by default.

In previous versions of the _NetEscapades.EnumGenerators_ generator, the `[EnumExtensions]` attributes were added to your compilation as `internal` attributes by default. If you added the source generator package to multiple projects, and used the `[InternalsVisibleTo]` attribute, you could experience errors when you build:

```bash
warning CS0436: The type 'EnumExtensionsAttribute' in 'NetEscapades.EnumGenerators\NetEscapades.EnumGenerators\EnumExtensionsAttribute.cs' conflicts with the imported type 'EnumExtensionsAttribute' in 'MyProject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'.
```

In the latest version of _NetEscapades.EnumGenerators_, the attributes are not embedded by default, so you should not experience this problem. If you see this error, compare your installation to the examples in the installation guide.
