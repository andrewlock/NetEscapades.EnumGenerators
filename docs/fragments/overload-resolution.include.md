
## Resolving overload ambiguity with `new()`

The generated extension methods include overloads that accept option types such as `EnumParseOptions` and `SerializationOptions`. When using target-typed `new()` with these overloads, the C# compiler may report a CS0121 ambiguity error because it cannot determine which overload to call:

```csharp
// CS0121: The call is ambiguous between 'Parse(string, StringComparison)' and 'Parse(string, EnumParseOptions)'
var result = MyEnumExtensions.Parse("First", new());
```

On **.NET 9+**, this is resolved automatically using the `[OverloadResolutionPriority]` attribute, which is included in the generated code gated behind a preprocessor directive.

On **older target frameworks**, you can opt in to the same behavior by defining the `NETESCAPADES_ENUMGENERATORS_OVERLOAD_PRIORITY` preprocessor symbol and providing a polyfill of the `OverloadResolutionPriorityAttribute` type.

The easiest way to add the polyfill is to use the [Polyfill](https://github.com/SimonCropp/Polyfill) NuGet package, which provides this and many other missing types for older frameworks.

Alternatively, you can add the attribute manually to your project:

```csharp
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property,
        AllowMultiple = false, Inherited = false)]
    sealed class OverloadResolutionPriorityAttribute : Attribute
    {
        public OverloadResolutionPriorityAttribute(int priority) => Priority = priority;
        public int Priority { get; }
    }
}
```

Then define the preprocessor symbol in your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <!-- Enable overload resolution priority for generated enum methods -->
    <DefineConstants>$(DefineConstants);NETESCAPADES_ENUMGENERATORS_OVERLOAD_PRIORITY</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta21" />
</Project>
```
