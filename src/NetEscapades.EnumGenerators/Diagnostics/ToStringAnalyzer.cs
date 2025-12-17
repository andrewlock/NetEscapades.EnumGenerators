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
        messageFormat: "Use ToStringFast() instead of ToString() for better performance on enum '{0}'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(ctx =>
        {
            var enumExtensionsAttr =
                ctx.Compilation.GetTypeByMetadataName(Attributes.EnumExtensionsAttribute);

            if (enumExtensionsAttr is null)
            {
                return;
            }

            ctx.RegisterSyntaxNodeAction(
                c => AnalyzeInvocation(c, enumExtensionsAttr),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumExtensionsAttr)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a member access expression (e.g., value.ToString())
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Check if there are too many arguments)
        if (invocation.ArgumentList.Arguments.Count > 1)
        {
            return;
        }

        // Check if the method name is "ToString"
        if (memberAccess.Name.Identifier.Text != "ToString")
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
        // We handle format specifiers, so accept ToString() with 0 or 1 parameters
        if (methodSymbol.Name != "ToString" ||
            methodSymbol.Parameters.Length > 1 ||
            (methodSymbol.ContainingType.SpecialType != SpecialType.System_Object &&
             methodSymbol.ContainingType.SpecialType != SpecialType.System_Enum))
        {
            return;
        }

        // If there's a format parameter, check if it's compatible with ToStringFast()
        // ToStringFast() is equivalent to ToString() with no args or ToString("G")/ToString("g")/ToString("")
        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            var argument = invocation.ArgumentList.Arguments[0];
            var constantValue = context.SemanticModel.GetConstantValue(argument.Expression);
            
            // If we can't determine the value at compile time, don't suggest replacement
            // If it's not a string (e.g., it's an IFormatProvider), don't suggest replacement
            if (!constantValue.HasValue || constantValue.Value is not string formatString)
            {
                return;
            }

            // Check if the format string is compatible with ToStringFast()
            // Only "", "G", and "g" are compatible
            if (!string.IsNullOrEmpty(formatString) && 
                !string.Equals(formatString, "G", StringComparison.Ordinal) &&
                !string.Equals(formatString, "g", StringComparison.Ordinal))
            {
                return;
            }
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
            if (SymbolEqualityComparer.Default.Equals(
                    attributeData.AttributeClass,
                    enumExtensionsAttr))
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
