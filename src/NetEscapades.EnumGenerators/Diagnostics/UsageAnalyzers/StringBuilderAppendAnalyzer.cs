using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StringBuilderAppendAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG012";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Call ToStringFast() on enum in StringBuilder.Append() for better performance",
        messageFormat: "Use ToStringFast() instead of passing enum '{0}' directly to StringBuilder.Append() for better performance",
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

            var stringBuilder = ctx.Compilation.GetBestTypeByMetadataName("System.Text.StringBuilder");
            if (stringBuilder is null)
            {
                return;
            }
            

            ctx.RegisterSyntaxNodeAction(
                c => AnalyzeInvocation(c, enumExtensionsAttr, externalEnumTypes, stringBuilder),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumExtensionsAttr,
        ExternalEnumDictionary externalEnumTypes, INamedTypeSymbol stringBuilder)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name.Identifier.Text != "Append"
            || invocation.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        // Get the symbol information for the invocation
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Verify this is the StringBuilder.Append(object?) method
        // We're looking for the overload that takes a single object parameter
        if (methodSymbol.Name != "Append" ||
            methodSymbol.Parameters.Length != 1 ||
            methodSymbol.Parameters[0].Type.SpecialType != SpecialType.System_Object ||
            !SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, stringBuilder))
        {
            return;
        }

        var argument = invocation.ArgumentList.Arguments[0];
        
        // Get the type of the argument
        var argumentType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
        if (argumentType is null || argumentType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        if (!AnalyzerHelpers.IsEnumWithExtensions(argumentType, enumExtensionsAttr, externalEnumTypes, out var extensionType))
        {
            return;
        }

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(
            descriptor: Rule,
            location: argument.GetLocation(),
            messageArgs: argumentType.Name,
            properties: ImmutableDictionary.CreateRange<string, string?>([
                new(AnalyzerHelpers.ExtensionTypeNameProperty, extensionType),
            ]));

        context.ReportDiagnostic(diagnostic);
    }
}
