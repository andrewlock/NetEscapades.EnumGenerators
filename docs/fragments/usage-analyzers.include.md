
## Usage Analyzers

_NetEscapades.EnumGenerators_ includes optional analyzers that encourage the use of the generated extension methods instead of the built-in `System.Enum` methods. These analyzers can help improve performance by suggesting the faster, generated, alternatives like `ToStringFast()`, `HasFlagFast()`, and `TryParse()`.

### Enabling the analyzers

The usage analyzers are disabled by default. To enable them, set the `EnumGenerator_EnableUsageAnalyzers` MSBuild property to `true` in your project:

```xml
<PropertyGroup>
  <EnumGenerator_EnableUsageAnalyzers>true</EnumGenerator_EnableUsageAnalyzers>
</PropertyGroup>
```

Alternatively, [add a `.globalconfig` file](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files#global-analyzerconfig) to your project with the following content:

```ini
is_global = true
build_property.EnumGenerator_EnableUsageAnalyzers = true
```

After using one of these configuration options, the analyzers in your project should be enabled with the default severity of `Warning`.

### Configuring analyzer severity (optional)

Once enabled, you can optionally configure the severity of individual analyzer rules using one or more [`.editorconfig` files](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files#editorconfig). For example, the following changes all the built-in analyzers to report usages as errors instead of warnings

```ini
[*.{cs,vb}]

# NEEG004: Use ToStringFast() instead of ToString()
dotnet_diagnostic.NEEG004.severity = error

# NEEG005: Use HasFlagFast() instead of HasFlag()
dotnet_diagnostic.NEEG005.severity = error

# NEEG006: Use generated IsDefined() instead of Enum.IsDefined()
dotnet_diagnostic.NEEG006.severity = error

# NEEG007: Use generated Parse() instead of Enum.Parse()
dotnet_diagnostic.NEEG007.severity = error

# NEEG008: Use generated GetNames() instead of Enum.GetNames()
dotnet_diagnostic.NEEG008.severity = error

# NEEG009: Use generated GetValues() instead of Enum.GetValues()
dotnet_diagnostic.NEEG009.severity = error

# NEEG010: Use generated GetValuesAsUnderlyingType() instead of Enum.GetValuesAsUnderlyingType()
dotnet_diagnostic.NEEG010.severity = error

# NEEG011: Use generated TryParse() instead of Enum.TryParse()
dotnet_diagnostic.NEEG011.severity = error

# NEEG012: Call ToStringFast() on enum in StringBuilder.Append() for better performance
dotnet_diagnostic.NEEG012.severity = error
```

These are reported in both your IDE and via the CLI as Roslyn errors:

![Demonstrating the Roslyn errors shown for using the non-generated methods](https://raw.githubusercontent.com/andrewlock/NetEscapades.EnumGenerators/refs/heads/main/docs/images//analyzer.png)

Valid severity values include: `none`, `silent`, `suggestion`, `warning`, and `error`.

### Code fixes

All usage analyzers include automatic code fixes. When a diagnostic is triggered, you can use the quick fix functionality in your IDE to automatically replace the `System.Enum` method with the corresponding generated extension method:

![Demonstrating the code-fix option available in your IDE for each of the analyzers](https://raw.githubusercontent.com/andrewlock/NetEscapades.EnumGenerators/refs/heads/main/docs/images//code_fix.png)
