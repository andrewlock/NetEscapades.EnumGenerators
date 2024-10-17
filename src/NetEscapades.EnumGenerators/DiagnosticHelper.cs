using Microsoft.CodeAnalysis;

namespace NetEscapades.EnumGenerators;

public static class DiagnosticHelper
{
    public static readonly DiagnosticDescriptor CsharpVersionLooLow = new(
        id: "NEEG001",
        title: "Language version too low",
        messageFormat: "Interceptors require at least C# version 11",
        category: "Requirements",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SdkVersionTooLow = new(
        id: "NEEG002",
        title: ".NET SDK version too low",
        messageFormat: "Interceptors require a .NET SDK version of at least 8.0.400",
        category: "Requirements",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

}