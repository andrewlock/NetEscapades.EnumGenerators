using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IsDefinedCodeFixProvider)), Shared]
public class IsDefinedCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with generated IsDefined()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(IsDefinedAnalyzer.DiagnosticId);

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for IsDefined() replacement
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => FixAllAsync(context.Document, context.Diagnostics, c),
                    equivalenceKey: Title),
                context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    protected override Task FixWithEditor(DocumentEditor editor, Diagnostic diagnostic,
        INamedTypeSymbol extensionTypeSymbol,
        CancellationToken cancellationToken)
    {
        // Find the invocation node at the diagnostic location
        var node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan);
        if (node is not InvocationExpressionSyntax invocation)
        {
            return Task.CompletedTask;
        }

        // Get the symbol to determine which pattern we're dealing with
        var symbolInfo = editor.SemanticModel.GetSymbolInfo(invocation, cancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return Task.CompletedTask;
        }

        ArgumentSyntax? valueArgument = null;

        // Determine which argument is the value to check
        if (methodSymbol is { IsGenericMethod: true, TypeArguments.Length: 1 })
        {
            // Pattern: Enum.IsDefined<TEnum>(value)
            if (invocation.ArgumentList.Arguments.Count >= 1)
            {
                valueArgument = invocation.ArgumentList.Arguments[0];
            }
        }
        else if (methodSymbol.Parameters.Length == 2
                 && invocation.ArgumentList.Arguments.Count == 2)
        {
            // Pattern: Enum.IsDefined(typeof(TEnum), value)
            valueArgument = invocation.ArgumentList.Arguments[1];
        }

        if (valueArgument is null)
        {
            return Task.CompletedTask;
        }

        // Create new invocation: ExtensionsClass.IsDefined(value)
        var generator = editor.Generator;
        var newInvocation = generator.InvocationExpression(
                generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "IsDefined"),
                valueArgument.Expression)
            .WithTriviaFrom(invocation)
            .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

        editor.ReplaceNode(invocation, newInvocation);
        return Task.CompletedTask;
    }
}