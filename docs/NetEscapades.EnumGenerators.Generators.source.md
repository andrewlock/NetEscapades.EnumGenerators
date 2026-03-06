# ![](https://raw.githubusercontent.com/andrewlock/NetEscapades.EnumGenerators/refs/heads/main/docs/images/icon_32.png) NetEscapades.EnumGenerators.Generators

![Build status](https://github.com/andrewlock/NetEscapades.EnumGenerators/actions/workflows/BuildAndPack.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/NetEscapades.EnumGenerators.svg)](https://www.nuget.org/packages/NetEscapades.EnumGenerators/)

[NetEscapades.EnumGenerators.Generators](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Generators) is a source generator package that generates extension methods for enums, to allow fast "reflection".

In general, we recommend installing the [NetEscapades.EnumGenerators](https://www.nuget.org/packages/NetEscapades.EnumGenerators) metapackage. [See below](#package-referencing-options) for details on choosing between these two packages.

> [NetEscapades.EnumGenerators](https://www.nuget.org/packages/NetEscapades.EnumGenerators) requires the .NET 7 SDK or higher. [NetEscapades.EnumGenerators.Interceptors](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Interceptors) requires the .NET 8.0.400 SDK or higher. You can still target earlier frameworks like .NET Core 3.1 etc, the version requirement only applies to the version of the .NET SDK installed.

toc

## Why use these packages?

include: benchmark

## Adding NetEscapades.EnumGenerators.Generators to your project

Add the package to your application using

```bash
dotnet add package NetEscapades.EnumGenerators.Generators
```

This adds a `<PackageReference>` to your project. You can additionally mark the package as `PrivateAssets="all"` and `ExcludeAssets="runtime"`.

> Setting `PrivateAssets="all"` means any projects referencing this one won't get a reference to the _NetEscapades.EnumGenerators.Generators_ package. Setting `ExcludeAssets="runtime"` ensures the _NetEscapades.EnumGenerators.Attributes.dll_ file is not copied to your build output (it is not required at runtime).

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="NetEscapades.EnumGenerators.Generators" Version="1.0.0-beta20"
    PrivateAssets="all" ExcludeAssets="runtime" />
  <!-- -->

</Project>
```

include: enum-usage

include: usage-analyzers

## Enabling automatic interception (experimental)

include: interceptor-intro

Interceptors were introduced as an experimental feature in C#12 with .NET 8. They allow a source generator to "intercept" certain method calls, and replace the call with a different one. _NetEscapades.EnumGenerators_ has support for intercepting `ToString()` and `HasFlag()` method calls.

> To use interceptors, you must be using at least version 8.0.400 of the .NET SDK. [This ships with Visual Studio version 17.11](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs), so you will need at least that version or higher.

To use interception, add the additional NuGet package [NetEscapades.EnumGenerators.Interceptors](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Interceptors) to your project using:

```bash
dotnet add package NetEscapades.EnumGenerators.Interceptors
```

This adds a `<PackageReference>` to your project. You can additionally mark the package as `PrivateAssets="all"` and `ExcludeAssets="runtime"`, similarly to _NetEscapades.EnumGenerators_.

include: interception-config

include: package-referencing

include: preserving-usages
