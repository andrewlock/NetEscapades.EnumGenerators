Changelog
--- 

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
* Add XML comments to public generated members and fix formatting [#42](Add XML comments to public members and fix formatting (#42))
* Fixed spelling of `isDisplayAttributeUsed` property [#17](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/17) (thanks [@JasonLautzenheiser](https://github.com/JasonLautzenheiser))

### Refactoring

* Use DotNet.ReproducibleBuilds [#35](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/35) (thanks [@HavenDV](https://github.com/HavenDV))
* Switch to `ForAttributeWithMetadataName` [#41](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/41)
* Update README [#8](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/8) [#20](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/20) [#1](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/1) (thanks [@tothalexlaszlo](https://github.com/tothalexlaszlo), [@Rekkonnect](https://github.com/Rekkonnect))
* Fix typo [#5](https://github.com/andrewlock/NetEscapades.EnumGenerators/pull/5)
