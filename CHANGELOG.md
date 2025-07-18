Changelog
--- 

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
