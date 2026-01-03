using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GetNamesAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG008";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Use generated GetNames() instead of Enum.GetNames()",
        messageFormat: "Use generated GetNames() instead of Enum.GetNames() for better performance on enum '{0}'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule, UsageAnalyzerConfig.ConfigDescriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(ctx =>
        {
            if (!UsageAnalyzerConfig.IsEnabled(ctx))
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

        if (invocation.ArgumentList.Arguments.Count is not (0 or 1)
            || invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name.Identifier.Text != nameof(Enum.GetNames))
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

        // Verify this is the GetNames() method from System.Enum
        if (methodSymbol.Name != nameof(Enum.GetNames) ||
            methodSymbol.ContainingType.SpecialType != SpecialType.System_Enum)
        {
            return;
        }

        ITypeSymbol? enumType = null;

        // Handle two basic patterns:
        // 1. Enum.GetNames(typeof(TEnum)) - has 1 parameter
        // 2. Enum.GetNames<TEnum>() - has 0 parameters, is generic
        if (methodSymbol is { IsGenericMethod: true, TypeArguments.Length: 1 })
        {
            // Pattern: Enum.GetNames<TEnum>()
            if (invocation.ArgumentList.Arguments.Count is not 0)
            {
                return;
            }

            enumType = methodSymbol.TypeArguments[0];
        }
        else if (methodSymbol.Parameters.Length is 1
                 && invocation.ArgumentList.Arguments is [{ Expression: TypeOfExpressionSyntax typeOfExpression }])
        {
            // Pattern: Enum.GetNames(typeof(TEnum))
            enumType = context.SemanticModel.GetTypeInfo(typeOfExpression.Type).Type;
        }

        if (enumType is null || enumType.TypeKind != TypeKind.Enum)
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
