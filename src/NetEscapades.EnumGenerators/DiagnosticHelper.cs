using Microsoft.CodeAnalysis;

namespace NetEscapades.EnumGenerators;

public static class DiagnosticHelper
{
    public static readonly DiagnosticDescriptor EnumInGenericType = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: "NEEG001",
#pragma warning restore RS2008
        title: "Enum in generic type not supported",
        messageFormat: "The enum '{0}' is nested inside a generic type. [EnumExtension] attribute is not supported.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}