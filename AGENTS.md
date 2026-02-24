# AGENTS.md

This file provides guidance to AI coding agents when working with code in this repository.

## Project Overview

NetEscapades.EnumGenerators is a C# source generator that creates high-performance extension methods for enums, replacing slow reflection-based `Enum.ToString()`, `Enum.IsDefined()`, `Enum.Parse()`, etc. with compile-time generated switch-based alternatives. It also includes Roslyn analyzers (with code fixes) that suggest using the generated methods, and an interceptor generator that automatically replaces standard enum method calls.

## Build Commands

This project uses [Nuke](https://nuke.build/) for builds. The solution file is `NetEscapades.EnumGenerators.sln`.

```bash
# Build
./build.cmd Compile          # Windows
./build.sh Compile           # Unix

# Run all tests (unit + integration)
./build.cmd Test
./build.sh Test

# Run a single test project directly
dotnet test tests/NetEscapades.EnumGenerators.Tests

# Run a single test by name
dotnet test tests/NetEscapades.EnumGenerators.Tests --filter "FullyQualifiedName~CanGenerateEnumExtensionsInGlobalNamespace"

# Pack NuGet packages
./build.cmd Pack

# Full CI pipeline (clean + test + package tests)
./build.cmd Clean Test TestPackage
```

## Architecture

### NuGet Package Structure (6 packages)

The project ships as multiple packages to avoid forcing runtime dependencies:

- **NetEscapades.EnumGenerators** — meta-package referencing Generators + RuntimeDependencies
- **NetEscapades.EnumGenerators.Generators** — the source generator + analyzers (netstandard2.0)
- **NetEscapades.EnumGenerators.Attributes** — `[EnumExtensions]` attribute (netstandard2.0, net451)
- **NetEscapades.EnumGenerators.RuntimeDependencies** — `EnumParseOptions`, `SerializationOptions`, `SerializationTransform` types (netstandard2.0, net451)
- **NetEscapades.EnumGenerators.Interceptors** — C# 12+ interceptor generator (netstandard2.0)
- **NetEscapades.EnumGenerators.Interceptors.Attributes** — `[Interceptable<T>]` attribute

### Source Generator Pipeline (`src/NetEscapades.EnumGenerators.Generators/`)

- **EnumGenerator.cs** — `IIncrementalGenerator` entry point. Uses `ForAttributeWithMetadataName` to find enums decorated with `[EnumExtensions]` or `[EnumExtensions<T>]`. Extracts `EnumToGenerate` model from syntax/semantic analysis.
- **SourceGenerationHelper.cs** (~2300 lines) — Generates the extension class source code via `StringBuilder`. Produces `ToStringFast()`, `IsDefined()`, `TryParse()`, `Parse()`, `GetValues()`, `GetNames()`, `HasFlagFast()`, etc. Handles multiple code paths: with/without System.Memory, with/without runtime dependencies, C# 14 field keyword support, collection expressions.
- **EnumToGenerate.cs** — Immutable model capturing everything needed to generate code for one enum (name, namespace, underlying type, members, flags, metadata source). Must implement `IEquatable<T>` correctly for incremental caching.
- **TrackingNames.cs** — String constants for incremental generator step tracking, used in tests to verify caching behavior.

### Analyzers (`src/NetEscapades.EnumGenerators.Generators/Diagnostics/`)

Two categories:

**Definition Analyzers** (always on) — validate `[EnumExtensions]` usage:
- `DuplicateEnumValueAnalyzer` — warns about duplicate enum values
- `DuplicateExtensionClassAnalyzer` — warns about conflicting extension class names
- `EnumInGenericTypeAnalyzer` — warns about enums nested in generic types

**Usage Analyzers** (NEEG004–NEEG012, opt-in via `EnumGenerator_EnableUsageAnalyzers`) — suggest using generated methods instead of framework methods:
- Each has a paired `CodeFixProvider` for automatic fixes
- Controlled by `build_property.EnumGenerator_EnableUsageAnalyzers` in `.editorconfig`
- Pattern: analyzer detects `Enum.Method()` call on a decorated enum, code fix replaces with `EnumExtensions.MethodFast()`

### Interceptors (`src/NetEscapades.EnumGenerators.Interceptors/`)

- **InterceptorGenerator.cs** — C# 12+ feature. Intercepts `ToString()` and `HasFlag()` calls at compile time and replaces them with generated fast versions.
- Requires .NET 8.0.400+ SDK, controlled by `EnumGenerator_EnableInterception` MSBuild property.

### Key Design Constraints

- **Generators target netstandard2.0** — must run inside any version of the Roslyn compiler. Cannot use modern .NET APIs directly; uses `Polyfill` package for compatibility.
- **Incremental generator caching** — `EnumToGenerate` and all pipeline outputs must correctly implement equality. Tests verify this by running the generator twice and asserting all outputs are `Cached` or `Unchanged` on the second run.
- **No Roslyn symbols in outputs** — the generator must not store `ISymbol`, `Compilation`, or `SyntaxNode` objects in pipeline outputs (would break caching). `TestHelpers.AssertObjectGraph` validates this.

## Testing

### Test Projects

- **NetEscapades.EnumGenerators.Tests** — main test project with:
  - **Snapshot tests** (Verify.Xunit) for generated source output, stored in `tests/.../Snapshots/`. Use `dotnet tool run dotnet-verify accept` or manually update `.verified.` files when output intentionally changes.
  - **Generator tests** (`EnumGeneratorTests.cs`) — feed source code strings through `TestHelpers.GetGeneratedOutput()`, assert no diagnostics, verify output with Verify snapshots.
  - **Analyzer tests** — use `Microsoft.CodeAnalysis.Testing` framework. Each inherits `AnalyzerTestsBase<TAnalyzer, TCodeFixer>` providing `VerifyAnalyzerAsync`/`VerifyCodeFixAsync` helpers.
  - **Incremental caching tests** — every generator test automatically runs twice to verify caching correctness.
- **Integration test projects** — test the actual NuGet packages work correctly in various configurations (NetStandard, System.Memory, Interceptors, PrivateAssets).
- **Benchmarks** — BenchmarkDotNet project comparing generated methods vs. framework methods.

### Test Patterns

- Generator tests use `TestHelpers.Options` record to configure source input, language version, analyzer options, and features.
- Snapshot files use parameter-based naming: `TestName_param1_param2.verified.txt`.
- Version strings in generated code are scrubbed via `ScrubExpectedChanges()` to avoid snapshot churn on version bumps.

## Version Management

- Version is defined in `version.props` (`VersionPrefix` + `VersionSuffix`).
- `Constants.cs` in the generators project also contains the version string used in `[GeneratedCode]` attributes.
- Both must be updated together when changing versions.

## MSBuild Properties (user-facing configuration)

- `EnumGenerator_EnumMetadataSource` — default metadata source attribute (EnumMemberAttribute, DisplayAttribute, DescriptionAttribute, None)
- `EnumGenerator_ForceInternal` — force all generated extension classes to be internal
- `EnumGenerator_EnableUsageAnalyzers` — enable usage analyzers (default: false)
- `EnableEnumGeneratorInterceptor` — enable interceptor generator
