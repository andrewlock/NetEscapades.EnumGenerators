using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HasFlagAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG005";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Use HasFlagFast() instead of HasFlag()",
        messageFormat: "Use HasFlagFast() instead of HasFlag() for better performance on enum '{0}'",
        category: "Usage",
        defaultSeverity: UsageAnalyzerConfig.DefaultSeverity,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(ctx =>
        {
            if (!UsageAnalyzerConfig.IsEnabled(ctx.Options))
            {
                return;
            }

            var (enumExtensionsAttr, externalEnumTypes) = AnalyzerHelpers.GetEnumExtensionAttributes(ctx.Compilation);
            if (enumExtensionsAttr is null || externalEnumTypes is null)
            {
                return;
            }

            ctx.RegisterSyntaxNodeAction(
                c => AnalyzeInvocation(c, enumExtensionsAttr, externalEnumTypes),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumExtensionsAttr, ExternalEnumDictionary externalEnumTypes)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a member access expression (e.g., value.HasFlag())
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Check if the method name is "HasFlag"
        if (memberAccess.Name.Identifier.Text != "HasFlag")
        {
            return;
        }

        // Check if there is exactly one argument
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        // Get the symbol information for the invocation
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Verify this is the HasFlag() method from System.Enum
        if (methodSymbol.Name != "HasFlag" ||
            methodSymbol.Parameters.Length != 1 ||
            methodSymbol.ContainingType.SpecialType != SpecialType.System_Enum)
        {
            return;
        }

        // Get the type of the receiver (the thing before .HasFlag())
        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (receiverType is null || receiverType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        if (!AnalyzerHelpers.IsEnumWithExtensions(receiverType, enumExtensionsAttr, externalEnumTypes, out var extensionType))
        {
            return;
        }

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(
            descriptor: Rule,
            location: memberAccess.Name.GetLocation(),
            messageArgs: receiverType.Name,
            properties: ImmutableDictionary.CreateRange<string, string?>([
                new(AnalyzerHelpers.ExtensionTypeNameProperty, extensionType),
            ]));

        context.ReportDiagnostic(diagnostic);
    }
}
