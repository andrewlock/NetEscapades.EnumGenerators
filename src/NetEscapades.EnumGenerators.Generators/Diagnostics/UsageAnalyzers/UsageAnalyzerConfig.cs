using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

public static class UsageAnalyzerConfig
{
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Warning;
    public const string EnableKey = "build_property.EnumGenerator_EnableUsageAnalyzers";

    internal static bool IsEnabled(AnalyzerOptions context)
        => context.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(EnableKey, out var value)
           && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
}
