using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TryParseAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG008";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Use generated TryParse() instead of Enum.TryParse()",
        messageFormat: "Use generated TryParse() instead of Enum.TryParse() for better performance on enum '{0}'",
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

        if (invocation.ArgumentList.Arguments.Count is 0 or > 3
            || invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name.Identifier.Text != nameof(Enum.TryParse))
        {
            // can't be the one we want
            return;
        }

        // Get the symbol information for the invocation
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Verify this is the TryParse() method from System.Enum
        if (methodSymbol.Name != nameof(Enum.TryParse) ||
            methodSymbol.ContainingType.SpecialType != SpecialType.System_Enum)
        {
            return;
        }

        // Only handle generic TryParse methods
        // The non-generic methods use 'out object?' which is incompatible with generated 'out TEnum' signature
        if (!methodSymbol.IsGenericMethod || methodSymbol.TypeArguments.Length != 1)
        {
            return;
        }

        // Handle generic patterns only, value may be string or ReadOnlySpan<char>
        // 1. Enum.TryParse<TEnum>(value, out result) - has 2 parameters
        // 2. Enum.TryParse<TEnum>(value, ignoreCase, out result) - has 3 parameters
        // 3. Enum.TryParse<TEnum>(ReadOnlySpan<char> value, out result) - has 2 parameters (NET5+)
        // 4. Enum.TryParse<TEnum>(ReadOnlySpan<char> value, ignoreCase, out result) - has 3 parameters (NET5+)
        if (invocation.ArgumentList.Arguments.Count is not (2 or 3))
        {
            return;
        }

        var enumType = methodSymbol.TypeArguments[0];

        if (enumType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        if (!AnalyzerHelpers.IsEnumWithExtensions(enumType, enumExtensionsAttr, externalEnumTypes, out var extensionType))
        {
            return;
        }

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(
            descriptor: Rule,
            location: invocation.GetLocation(),
            messageArgs: enumType.Name,
            properties: ImmutableDictionary.CreateRange<string, string?>([
                new(AnalyzerHelpers.ExtensionTypeNameProperty, extensionType),
            ]));

        context.ReportDiagnostic(diagnostic);
    }
}
