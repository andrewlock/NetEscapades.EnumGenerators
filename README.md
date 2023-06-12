# NetEscapades.EnumGenerators

![Build status](https://github.com/andrewlock/NetEscapades.EnumGenerators/actions/workflows/BuildAndPack.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/NetEscapades.EnumGenerators.svg)](https://www.nuget.org/packages/NetEscapades.EnumGenerators/)

A Source Generator package that generates extension methods for enums, to allow fast "reflection".

> This source generator requires the .NET 7 SDK. You can target earlier frameworks like .NET Core 3.1 etc, but the _SDK_ must be at least 7.0.100

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
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta04" 
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

    [Display(Name = "2nd")]
    Second,
}
```

This will generate a class called `MyEnumExtensions` (by default), which contains a number of helper methods. For example:

```csharp
public static partial class MyEnumExtensions
{
    public const int Length = 2;

    public static string ToStringFast(this MyEnum value)
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

If you create a "Flags" `enum` by decorating it with the `[Flags]` attribute, an additional method is created, which provides a bitwise alternative to the `Enum.HasFlag(flag)` method:

```csharp
public static bool HasFlagFast(this MyEnum value, MyEnum flag)
    => flag == 0 ? true : (value & flag) == flag;
```

Note that if you provide a `[Display]` or `[Description]` attribute, the value you provide for this attribute can be used by methods like `ToStringFast()` and `TryParse()` by passing the argument `allowMatchingMetadataAttribute: true`. Adding both attributes to an enum member is not supported, though conventionally the "first" attribute will be used.

You can override the name of the extension class by setting `ExtensionClassName` in the attribute and/or the namespace of the class by setting `ExtensionClassNamespace`. By default, the class will be public if the enum is public, otherwise it will be internal.

If you want a `JsonConverter` that uses the generated extensions for efficient serialization and deserialization you can add the `EnumJsonConverter` and `JsonConverter` to the enum. For example:
```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[EnumExtensions]
[EnumJsonConverter(typeof(MyEnumConverter))]
[JsonConverter(typeof(MyEnumConverter))]
public enum MyEnum
{
    First,
    [Display(Name = "2nd")] Second
}
```

This will generate a class called `MyEnumConverter`. For example:
```csharp
/// <summary>
/// Converts a <see cref="global::MyEnum" /> to or from JSON.
/// </summary>
public sealed class MyEnumConverter : global::System.Text.Json.Serialization.JsonConverter<global::MyEnum>
{
    /// <inheritdoc />
    /// <summary>
    /// Read and convert the JSON to <see cref="global::MyEnum" />.
    /// </summary>
    /// <remarks>
    /// A converter may throw any Exception, but should throw <see cref="global::System.Text.Json.JsonException" /> when the JSON is invalid.
    /// </remarks>
    public override global::MyEnum Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
    {
#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
        char[]? rentedBuffer = null;
        var bufferLength = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;

        var charBuffer = bufferLength <= 128
            ? stackalloc char[128]
            : rentedBuffer = global::System.Buffers.ArrayPool<char>.Shared.Rent(bufferLength);

        var charsWritten = reader.CopyString(charBuffer);
        global::System.ReadOnlySpan<char> source = charBuffer[..charsWritten];
        try
        {
            if (global::MyEnumExtensions.TryParse(source, out var enumValue, true, false))
                return enumValue;

            throw new global::System.Text.Json.JsonException($"{source.ToString()} is not a valid value.", null, null, null);
        }
        finally
        {
            if (rentedBuffer is not null)
            {
                charBuffer[..charsWritten].Clear();
                global::System.Buffers.ArrayPool<char>.Shared.Return(rentedBuffer);
            }
        }
#else
        var source = reader.GetString();
        if (global::MyEnumExtensions.TryParse(source, out var enumValue, true, false))
            return enumValue;

        throw new global::System.Text.Json.JsonException($"{source} is not a valid value.", null, null, null);
#endif
    }

    /// <inheritdoc />
    public override void Write(global::System.Text.Json.Utf8JsonWriter writer, global::MyEnum value, global::System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(global::MyEnumExtensions.ToStringFast(value));
}
```

_Note: If you've added `JsonStringEnumConverter` to the `JsonSerializerOptions.Converters`, you must add the generated converters manually before adding the `JsonStringEnumConverter`._

You can customize the generated code for the converter by setting the following values:
- `CaseSensitive` - Indicates if the string representation is case sensitive when deserializing it as an enum.
- `CamelCase` - Indicates if the value of `PropertyName` should be camel cased.
- `AllowMatchingMetadataAttribute` - If `true`, considers the value of metadata attributes, otherwise ignores them.
- `PropertyName` - If set, this value will be used in messages when there are problems with validation and/or serialization/deserialization occurs.


## Embedding the attributes in your project

By default, the `[EnumExtensions]` attributes referenced in your application are contained in an external dll. It is also possible to embed the attributes directly in your project, so they appear in the dll when your project is built. If you wish to do this, you must do two things:

1. Define the MSBuild constant `NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES`. This ensures the attributes are embedded in your project
2. Add `compile` to the list of excluded assets in your `<PackageReference>` element. This ensures the attributes in your project are referenced, instead of the _NetEscapades.EnumGenerators.Attributes.dll_ library.

Your project file should look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <!--  Define the MSBuild constant    -->
    <DefineConstants>$(DefineConstants);NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta04" 
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
    <TargetFramework>net6.0</TargetFramework>
    <!--  Define the MSBuild constant to preserve usages   -->
    <DefineConstants>$(DefineConstants);NETESCAPADES_ENUMGENERATORS_USAGES</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta05" PrivateAssets="all" />
  <!--              ☝ You must not exclude the runtime assets in this case -->

</Project>
```

## Error CS0436 and [InternalsVisibleTo]

> In the latest version of _NetEscapades.EnumGenerators_, you should not experience error CS0436 by default.

In previous versions of the _NetEscapades.EnumGenerators_ generator, the `[EnumExtensions]` attributes were added to your compilation as `internal` attributes by default. If you added the source generator package to multiple projects, and used the `[InternalsVisibleTo]` attribute, you could experience errors when you build:

```bash
warning CS0436: The type 'EnumExtensionsAttribute' in 'NetEscapades.EnumGenerators\NetEscapades.EnumGenerators\EnumExtensionsAttribute.cs' conflicts with the imported type 'EnumExtensionsAttribute' in 'MyProject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'.
```

In the latest version of _StronglyTypedId_, the attributes are not embedded by default, so you should not experience this problem. If you see this error, compare your installation to the examples in the installation guide.
