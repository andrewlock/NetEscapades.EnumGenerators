Changelog
--- 

## [v1.0.0-beta20](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta19..v1.0.0-beta20) (2026-01-05)

### Breaking Changes

* Split NetEscapades.EnumGenerators package into 3 packages (#233, #234)
  * _NetEscapades.EnumGenerators_ is a meta package, and should not be added with `PrivateAssets="All"` or `ExcludeAssets="runtime"`
  * _NetEscapades.EnumGenerators.Generators_ contains the source generator, and _can_ be added with private assets
  * _NetEscapades.EnumGenerators.RuntimeDependencies_ contains optional runtime dependencies used during generation
  * Please see the readme for advice on choosing your packages: https://github.com/andrewlock/NetEscapades.EnumGenerators#package-referencing-options

### Features

* Add analyzer to detect `StringBuilder.Append(enum)` on enums with `[EnumExtensions]` or `EnumExtensions<T>` (#230)

### Fixes

* Fix generating invalid XML in System.Memory warnings (#228)

## [v1.0.0-beta19](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta18..v1.0.0-beta19) (2026-01-05)

### Fixes

* Fix README image links

## [v1.0.0-beta18](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta16..v1.0.0-beta18) (2026-01-05)

### Breaking Changes

* Add support for generating `ReadOnlySpan<char>` based methods when System.Memory is present (#182, #212)

### Features

* Add support for disabling number parsing using `EnumParseOptions` (#175)
* Add support for automatic `ToLowerInvariant` and `ToUpperInvariant` in `ToStringFast()`, using `SerializationOptions` (#177)
* Add Analyzers to encourage using the generated methods 
  * Add analyzer to detect ToString() on enums with [EnumExtensions] or EnumExtensions<T> (#196)
  * Extend ToStringAnalyzer to detect enum usage in string interpolation (#198)
  * Add analyzer to detect HasFlag() and suggest HasFlagFast() replacement (#199)
  * Extract the extension class and namespace in analyzers (#200)
  * Add using directive support to HasFlagAnalyzer and ToStringAnalyzer code fixes (#202)
  * Add analyzer for Enum.IsDefined() suggesting generated IsDefined method (#203)
  * Add analyzer for Enum.Parse() suggesting generated Parse method (NEEG007) (#204)
  * Add analyzer for Enum.GetNames() with generated alternative (#209)
  * Add analyzer for Enum.GetValues() with code fix to use generated method (#207)
  * Add analyzer for Enum.GetValuesAsUnderlyingType() usage (#208)
  * Add analyzer for Enum.TryParse() usage (#206)
  * Refactor analyzers to reduce some duplication (#205)
  * Disable usage analyzers by default, enable via editorconfig (#214)
  * Update default severity to warning (#218)
  * Change how the usage analyzers are enabled to use an `EnumGenerator_EnableUsageAnalyzers` MSBuild property instead (#222)
  * Add AnalyzerTests project to confirm that the analyzers are actually triggering warnings in a real project (#223)
  * Mention globalconfig in the docs, and tweak the detection for consistency (#224)

### Fixes

* Remove ToStringFastWithMetadata when it's not needed for perf reasons(#176)
* Minor performance fixes in generated code (#178)
* Use Collection expressions in generated code if possible (#179)

### Misc

* Build tweaks (#180)
* Add support for .NET 10.0 in Benchmark project (#184) Thanks [@HakamFostok](https://github.com/HakamFostok)!
* Add Usage Analyzers documentation to README (#216)
* Update Readme (#219)

## [v1.0.0-beta16](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta15..v1.0.0-beta16) (2025-11-04)

### Fixes
- Fix incorrect default value of `EnumGenerator_ForceExtensionMembers` incorrectly generating C#14 extension members (#172) Thanks [@sindrekroknes](sindrekroknes)

## [v1.0.0-beta15](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta14..v1.0.0-beta15) (2025-10-29)

### Breaking Changes:

- Embedding marker attributes in the target dll using `NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES` is no longer supported (#160)
- Only one source of metadata for enum member descriptions may be selected, `[Description]`, `[Display]`, or `[EnumMember]` (the default) (#163)

### Features
- Add analyzer warning when generated extension class would clash with other generated extension class (Dianostic NEEG001) (#158)
- Add analyzer warning that you can't generate extensions for enum nested in a generic class (Dianostic NEEG002) (#159)
- Add analyzer info when enum contains duplicate case labels that may give unexpected results from `ToStringFast()`  (Dianostic NEEG003) (#162)

### Fixes
- Don't generate C#14 extension members when using `LangVersion.Preview` unless MSBuild property `EnumGenerator_ForceExtensionMembers` is set (#165)
- Fix code generation for enum members that use C# reserved member names (#168)

## [v1.0.0-beta14](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta13..v1.0.0-beta14) (2025-06-15)

### Features
- Expose generated static methods as C#14 static extension members on enums, when building with C#14 (#154)

### Fixes
- Improve XML documentation (#142) Thanks [@paulomorgado](https://github.com/paulomorgado)!
- Add support for enums with duplicate values (#143) Thanks [@paulomorgado](https://github.com/paulomorgado)!

## [v1.0.0-beta13](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta12..v1.0.0-beta13) (2025-05-11)

### Features
- Added `AsUnderlyingType()` and `GetValuesAsUnderlyingType()` (#125) Thanks [@TheConstructor](https://github.com/TheConstructor)!

### Fixes
- Replaced multiline verbatim strings (`@""`) with raw string literals (#87) Thanks [@Guiorgy](https://github.com/Guiorgy)!
- Update `GetValues`, `GetNames` and `GetValuesAsUnderlyingType` to match System.Enum order (#134) Thanks [@TheConstructor](https://github.com/TheConstructor)!
- Directly call `ToString` on unnamed numeric values to reduce overhead (#135) [@TheConstructor](https://github.com/TheConstructor)!

## [v1.0.0-beta12](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta11..v1.0.0-beta12) (2025-01-26)

### Breaking Changes:
- By default, `ToStringFast()` no longer uses `[DisplayName]` and `[Description]` by default. The original behaviour can be restored by passing `allowMatchingMetadataAttribute:true` (#122)
- Split the experimental interceptor support into a separate project, NetEscapades.EnumGenerators.Interceptors (#125)
- Enable interception by default when NetEscapades.EnumGenerators.Interceptors is added (#127)

### Features
- Added a package logo (#125)

### Fixes
- Fixed indentation in generated code so it aligns properly with 4 spaces (#120) Thanks [@karl-sjogren](https://github.com/karl-sjogren)!
- Fix missing global on System namespace usages (#118) Thanks [@henrikwidlund](https://github.com/henrikwidlund)!
- Don't use `using`s in generated code (#129)

## [v1.0.0-beta11](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta09..v1.0.0-beta11) (2024-10-24)

### Features
- Add optional interceptor for ToString() and HasFlag() (#94, #101, #104, #105, #106, #108, #113)
- Ignore usages of obsolete enum members in generated code (#111)

### Fixes
- Fix escaping of strings in description and display attributes (#109)
- Ensure we can handle enums with the same name in different namespaces (#114)
- Fix naming conflicts in System namespace (#118) Thanks [@henrikwidlund](https://github.com/henrikwidlund)!

## [v1.0.0-beta09](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta08..v1.0.0-beta09) (2024-05-15)

### Features
- Add support for generating extensions for external enums that come from other assemblies (#82)
- Add support for new Parse overloads (#85)

### Fixes
- Add `[GeneratedCodeAttribute]` to the generated extensions (#83)
- Split TryParse code method to try to fix coverlet issue (#84)


## [v1.0.0-beta08](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta07..v1.0.0-beta08) (2023-06-05)

### Fixes

- Exclude embedded attribute from code coverage [#59](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/59) (Thanks [@erri120](https://github.com/erri120)!)
- Fix Error when a class with the same name as namespace [#62](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/62)
- Support quotes and slashes in description/displayname attribute [#63](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/63)

## [v1.0.0-beta07](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta06..v1.0.0-beta07) (2023-03-09)

### Fixes

* Add `global::` prefix to System namespace references [#55](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/55)

## [v1.0.0-beta06](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta05..v1.0.0-beta06) (2022-12-20)

### Fixes

* Fix XML comments for public generated members [#49](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/49)

## [v1.0.0-beta05](https://github.com/andrewlock/NetEscapades.EnumGenerators/compare/v1.0.0-beta04..v1.0.0-beta05) (2022-12-19)

### Features

* Add support for overriding `ToStringFast()` and related methods by adding `[Description]` attribute to members [#46](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/46) 
* Add support for overriding `ToStringFast()` and related methods by adding `[Display]` attribute to members [#13](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/13) (thanks [@adamradocz](https://github.com/adamradocz))
* Add a .NET 4.5.1 target to the attributes dll, to reduce dependencies introduced by .NET Standard [#45](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/45)
* Add parsing overloads for `ReadOnlySpan<T>` [#16](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/16) (thanks [@adamradocz](https://github.com/adamradocz))
* Add `Length` extension method  [#7](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/7) (thanks [@tothalexlaszlo](https://github.com/tothalexlaszlo)) 

### Fixes 
* Fix `HasFlagsFast()` implementation [#44](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/46)
* Add XML documentation for attributes [9a38580cdc9e](https://github.com/andrewlock/NetEscapades.EnumGenerators/commit/9a38580cdc9e51b113dcd08bff168e0151b87e2d)
* Add XML comments to public generated members and fix formatting [#42](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/42)
* Fixed spelling of `isDisplayAttributeUsed` property [#17](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/17) (thanks [@JasonLautzenheiser](https://github.com/JasonLautzenheiser))

### Refactoring

* Use DotNet.ReproducibleBuilds [#35](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/35) (thanks [@HavenDV](https://github.com/HavenDV))
* Switch to `ForAttributeWithMetadataName` [#41](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/41)
* Update README [#8](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/8) [#20](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/20) [#1](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/1) (thanks [@tothalexlaszlo](https://github.com/tothalexlaszlo), [@Rekkonnect](https://github.com/Rekkonnect))
* Fix typo [#5](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/5)
