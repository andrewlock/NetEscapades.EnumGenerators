
## Package referencing options

[NetEscapades.EnumGenerators](https://www.nuget.org/packages/NetEscapades.EnumGenerators) is a metapackage that references additional packages for functionality.

```
NetEscapades.EnumGenerators
  |____NetEscapades.EnumGenerators.Generators
  |____NetEscapades.EnumGenerators.RuntimeDependencies
```

These packages provide the following functionality:
- `NetEscapades.EnumGenerators` is a meta package for easy install.
- `NetEscapades.EnumGenerators.Generators` contains the source generator itself.
- `NetEscapades.EnumGenerators.RuntimeDependencies` contains dependencies that need to be referenced at runtime by the generated code.

The default approach is to reference the meta-package in your project. The runtime dependencies and generator packages will then flow transitively to any project that references yours, and the generator will run in those projects by default.

### Avoiding runtime dependencies

In some cases you may not want these dependencies to flow to other projects. This is common when you are using _NetEscapades.EnumGenerators_ internally in your own library, for example. In this scenario, we suggest you take the following approach:

- Reference  [NetEscapades.EnumGenerators.Generators](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Generators) directly, and set `PrivateAssets=All`
- _Optionally_ reference [NetEscapades.EnumGenerators.RuntimeDependencies](https://www.nuget.org/packages/NetEscapades.EnumGenerators.RuntimeDependencies) directly.


```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <!-- Add the generator package with PrivateAssets -->
  <PackageReference Include="NetEscapades.EnumGenerators.Generators" Version="1.0.0-beta20" PrivateAssets="All"/>

  <!-- Optionally add the runtime dependencies package -->
  <PackageReference Include="NetEscapades.EnumGenerators.RuntimeDependencies" Version="1.0.0-beta20" />
</Project>
```

The [NetEscapades.EnumGenerators.RuntimeDependencies](https://www.nuget.org/packages/NetEscapades.EnumGenerators.RuntimeDependencies) packages is a "normal" dependency, that contains types that are used by the generated code, such as `EnumParseOptions`, `SerializationOptions`, and `SerializationTransform`:

```csharp
namespace NetEscapades.EnumGenerators;

/// <summary>
/// Defines the options use when parsing enums using members provided by NetEscapades.EnumGenerator.
/// </summary>
public readonly struct EnumParseOptions { }

/// <summary>
/// Options to apply when calling <c>ToStringFast</c> on an enum.
/// </summary>
public readonly struct SerializationOptions

/// <summary>
/// Transform to apply when calling <c>ToStringFast</c>
/// </summary>
public enum SerializationTransform
```

If the [NetEscapades.EnumGenerators.RuntimeDependencies](https://www.nuget.org/packages/NetEscapades.EnumGenerators.RuntimeDependencies) package is not found, the generated code creates nested versions of the dependencies in each generated extension method instead:

```csharp
namespace SomeNameSpace;


public static partial class MyEnumExtensions
{
    // ... generated members

    // The runtime dependencies are generated as nested types instead
    public readonly struct EnumParseOptions { }
    public readonly struct SerializationOptions
    public enum SerializationTransform
}
```

Generating the runtime dependencies as nested types has both upsides and downsides:

- It avoids placing downstream dependency requirements on consumers of your library.
- If the generated extension methods are `internal`, the generated runtime dependencies are also `internal`, and so not exposed to downstream consumers.
- It makes consuming the APIs that use the runtime dependencies more verbose.


### Choosing the correct packages for your scenario

In general, for simplicity, we recommend referencing [NetEscapades.EnumGenerators](https://www.nuget.org/packages/NetEscapades.EnumGenerators), and thereby using [NetEscapades.EnumGenerators.RuntimeDependencies](https://www.nuget.org/packages/NetEscapades.EnumGenerators.RuntimeDependencies). This particularly makes sense when you are the primary consumer of the extension methods, or where you don't mind if consumers end up referencing the generator package.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <PackageReference Include="NetEscapades.EnumGenerators" Version="1.0.0-beta20" />
</Project>
```

In contrast, if you are producing a reusable library and don't want any runtime dependencies to be exposed to consumers, we recommend using [NetEscapades.EnumGenerators.Generators](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Generators) and setting `PrivateAssets=All` and `ExcludeAssets="runtime"`.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <PackageReference Include="NetEscapades.EnumGenerators.Generators" Version="1.0.0-beta20" PrivateAssets="All" ExcludeAssets="runtime" />
</Project>
```

The final option is to reference [NetEscapades.EnumGenerators.Generators](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Generators) and set `PrivateAssets=All` and `ExcludeAssets="runtime"` (to avoid it being referenced transitively), but then also reference [NetEscapades.EnumGenerators.RuntimeDependencies](https://www.nuget.org/packages/NetEscapades.EnumGenerators.RuntimeDependencies), to produce easier-to consume APIs.


```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <PackageReference Include="NetEscapades.EnumGenerators.Generators" Version="1.0.0-beta20" PrivateAssets="All" ExcludeAssets="runtime"/>
  <PackageReference Include="NetEscapades.EnumGenerators.RuntimeDependencies" Version="1.0.0-beta20" />
</Project>
```

> [!WARNING]
> When using the [NetEscapades.EnumGenerators](https://www.nuget.org/packages/NetEscapades.EnumGenerators) metapackage, it's important you _don't_ set `PrivateAssets=All`. If you want to use `PrivateAssets=All`, use [NetEscapades.EnumGenerators.Generators](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Generators) for this scenario.
