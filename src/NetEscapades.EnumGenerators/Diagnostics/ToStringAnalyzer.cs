using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ToStringAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG004";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Use ToStringFast() instead of ToString()",
        messageFormat: "Use ToStringFast() instead of ToString() for better performance on enum '{0}' marked with [EnumExtensions]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        
        // Check if this is a member access expression (e.g., value.ToString())
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Check if the method name is "ToString"
        if (memberAccess.Name.Identifier.Text != "ToString")
        {
            return;
        }

        // Check if there are no arguments (we only care about parameterless ToString())
        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            return;
        }

        // Get the symbol information for the invocation
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Verify this is the ToString() method from System.Object or System.Enum
        if (methodSymbol.Name != "ToString" ||
            methodSymbol.Parameters.Length != 0 ||
            (methodSymbol.ContainingType.SpecialType != SpecialType.System_Object &&
             methodSymbol.ContainingType.SpecialType != SpecialType.System_Enum))
        {
            return;
        }

        // Get the type of the receiver (the thing before .ToString())
        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (receiverType is null || receiverType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        // Check if the enum has the [EnumExtensions] attribute
        bool hasEnumExtensionsAttribute = false;
        foreach (var attributeData in receiverType.GetAttributes())
        {
            if (attributeData.AttributeClass?.ToDisplayString() == Attributes.EnumExtensionsAttribute)
            {
                hasEnumExtensionsAttribute = true;
                break;
            }
        }

        if (!hasEnumExtensionsAttribute)
        {
            return;
        }

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Name.GetLocation(),
            receiverType.Name);
        
        context.ReportDiagnostic(diagnostic);
    }
}
