using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TryParseCodeFixProvider)), Shared]
public class TryParseCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with generated TryParse()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(TryParseAnalyzer.DiagnosticId);

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for TryParse() replacement
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

        // assume that we've already met all the requirements, so just make the fix
        var skipFirstArg = methodSymbol is not { IsGenericMethod: true, TypeArguments.Length: 1 };

        // Create new invocation: ExtensionsClass.TryParse(value, out result) or ExtensionsClass.TryParse(value, out result, ignoreCase)
        var generator = editor.Generator;
        var newInvocation = generator.InvocationExpression(
                generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "TryParse"),
                skipFirstArg ? invocation.ArgumentList.Arguments.Skip(1) : invocation.ArgumentList.Arguments)
            .WithTriviaFrom(invocation)
            .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

        editor.ReplaceNode(invocation, newInvocation);
        return Task.CompletedTask;
    }
}
