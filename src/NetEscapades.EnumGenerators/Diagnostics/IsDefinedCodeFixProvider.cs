using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics;

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
                    createChangedDocument: c => ReplaceIsDefinedWithGenerated(context.Document, context.Diagnostics, c),
                    equivalenceKey: Title),
                context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    protected sealed override Task<Document> FixAllAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
        => ReplaceIsDefinedWithGenerated(document, diagnostics, cancellationToken);

    private static async Task<Document> ReplaceIsDefinedWithGenerated(
        Document document,
        ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        if (editor is null)
        {
            return document;
        }

        var generator = editor.Generator;
        var semanticModel = editor.SemanticModel;

        foreach (var diagnostic in diagnostics)
        {
            if (!diagnostic.Properties.TryGetValue(AnalyzerHelpers.ExtensionTypeNameProperty, out var extensionTypeName)
                || extensionTypeName is null)
            {
                continue;
            }

            // Find the invocation node at the diagnostic location
            var node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan);

            if (node is not InvocationExpressionSyntax invocation)
            {
                continue;
            }

            var type = semanticModel.Compilation.GetTypeByMetadataName(extensionTypeName);
            if (type is null)
            {
                continue;
            }

            // Get the symbol to determine which pattern we're dealing with
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            ArgumentSyntax? valueArgument = null;

            // Determine which argument is the value to check
            if (methodSymbol.IsGenericMethod && methodSymbol.TypeArguments.Length == 1)
            {
                // Pattern: Enum.IsDefined<TEnum>(value)
                if (invocation.ArgumentList.Arguments.Count >= 1)
                {
                    valueArgument = invocation.ArgumentList.Arguments[0];
                }
            }
            else if (methodSymbol.Parameters.Length == 2)
            {
                // Pattern: Enum.IsDefined(typeof(TEnum), value)
                if (invocation.ArgumentList.Arguments.Count == 2)
                {
                    valueArgument = invocation.ArgumentList.Arguments[1];
                }
            }

            if (valueArgument is null)
            {
                continue;
            }

            // Create new invocation: ExtensionsClass.IsDefined(value)
            var newInvocation = generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpression(type), "IsDefined"),
                    [valueArgument.Expression])
                .WithTriviaFrom(invocation)
                .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

            editor.ReplaceNode(invocation, newInvocation);
        }

        return editor.GetChangedDocument();
    }
}
