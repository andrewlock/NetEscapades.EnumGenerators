using Microsoft.CodeAnalysis;

namespace NetEscapades.EnumGenerators;

public static class DiagnosticHelper
{
    public static readonly DiagnosticDescriptor EnumInGenericType = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: "NEEG002",
#pragma warning restore RS2008
        title: "Enum in generic type not supported",
        messageFormat: "The enum '{0}' is nested inside a generic type, which is not supported for enum extension generation",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}