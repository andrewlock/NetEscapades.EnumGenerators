using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

public static class UsageAnalyzerConfig
{
    public const string EnableKey = "netescapades.enumgenerators.usage_analyzers.enable";

    internal static readonly DiagnosticDescriptor ConfigDescriptor = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: "NEEGCONFIG001",
#pragma warning restore RS2008
        title: "Enable callsite analyzers for generated enum extensions",
        messageFormat:
        "Enable analyzers to encourage use of generated extension methods instead of System.Enum methods",
        category: "Configuration",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        customTags:
        [
            "EditorConfigOption",
            "EditorConfigOptionKey=netescapades.enumgenerators.usage_analyzers.enable",
            "EditorConfigOptionDescription=Enable callsite analyzers for generated enum extensions",
            "EditorConfigOptionAllowedValues=true,false",
            "EditorConfigOptionDefault=false",
            WellKnownDiagnosticTags.NotConfigurable
        ]);

    internal static bool IsEnabled(AnalyzerOptions context)
        => context.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(EnableKey, out var value) &&
           bool.TryParse(value, out var isEnabled)
           && isEnabled;
}
