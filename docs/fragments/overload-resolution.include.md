
## Resolving overload ambiguity with `new()`

The generated extension methods include overloads that accept option types such as `EnumParseOptions` and `SerializationOptions`. When using target-typed `new()` with these overloads, the C# compiler may report a CS0121 ambiguity error because it cannot determine which overload to call:

```csharp
// CS0121: The call is ambiguous between 'Parse(string, StringComparison)' and 'Parse(string, EnumParseOptions)'
var result = MyEnumExtensions.Parse("First", new());
```

On **.NET 9+**, this is resolved automatically using the `[OverloadResolutionPriority]` attribute. The generator detects that the attribute type is available in the compilation and includes it in the generated code.

On **older target frameworks** (with C# 13+ language version), you can get the same behavior by providing a polyfill of the `OverloadResolutionPriorityAttribute` type. The generator will automatically detect the polyfill and include the attribute — no additional configuration is needed.

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

If you need to disable the `[OverloadResolutionPriority]` attribute in the generated code, define the `NETESCAPADES_ENUMGENERATORS_OMIT_OVERLOAD_PRIORITY` preprocessor symbol:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);NETESCAPADES_ENUMGENERATORS_OMIT_OVERLOAD_PRIORITY</DefineConstants>
</PropertyGroup>
```
