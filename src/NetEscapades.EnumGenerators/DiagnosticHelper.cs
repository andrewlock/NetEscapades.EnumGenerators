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

}