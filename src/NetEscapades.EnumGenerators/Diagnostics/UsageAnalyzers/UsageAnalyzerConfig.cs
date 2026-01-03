using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

internal static class UsageAnalyzerConfig
{
    private const string EnableKey = "netescapades_enumgenerators_usage_analyzers_enable";

    public static readonly DiagnosticDescriptor ConfigDescriptor = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: "NEEG_CONFIG001",
#pragma warning restore RS2008
        title: "Enable or disable usage analyzers",
        messageFormat: "This is a configuration option and should not appear as a diagnostic",
        category: "Configuration",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: false,
        customTags: new[] { WellKnownDiagnosticTags.NotConfigurable });

    public static bool IsEnabled(CompilationStartAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (options.TryGetValue(EnableKey, out var value) && 
            bool.TryParse(value, out var isEnabled))
        {
            return isEnabled;
        }

        return false; // Disabled by default
    }
}
