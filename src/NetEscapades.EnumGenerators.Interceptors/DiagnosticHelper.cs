using Microsoft.CodeAnalysis;

namespace NetEscapades.EnumGenerators.Interceptors;

public static class DiagnosticHelper
{
    public static readonly DiagnosticDescriptor CsharpVersionLooLow = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: "NEEG001",
#pragma warning restore RS2008
        title: "Language version too low",
        messageFormat: "Interceptors require at least C# version 11",
        category: "Requirements",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}