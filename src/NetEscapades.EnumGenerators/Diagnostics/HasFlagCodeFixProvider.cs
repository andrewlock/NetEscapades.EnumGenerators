using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HasFlagCodeFixProvider)), Shared]
public class HasFlagCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with HasFlagFast()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(HasFlagAnalyzer.DiagnosticId);

    // We can't use the batch fixer because it causes multiple iterations

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for HasFlag() replacement
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ReplaceHasFlagWithHasFlagFast(context.Document, context.Diagnostics, c),
                    equivalenceKey: Title),
                context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    protected sealed override Task<Document> FixAllAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
        => ReplaceHasFlagWithHasFlagFast(document, diagnostics, cancellationToken);

    private static async Task<Document> ReplaceHasFlagWithHasFlagFast(
        Document document,
        ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        // Create the new identifier with "HasFlagFast"
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        if (editor is null)
        {
            return document;
        }

        foreach (var diagnostic in diagnostics)
        {
            if (!diagnostic.Properties.TryGetValue(AnalyzerHelpers.ExtensionTypeNameProperty, out var extensionTypeName)
                || extensionTypeName is null)
            {
                continue;
            }

            // Find the node at the diagnostic location
            var node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan);

            if (node is not IdentifierNameSyntax identifierName
                || identifierName.Parent is not MemberAccessExpressionSyntax memberAccess
                || memberAccess.Parent is not InvocationExpressionSyntax invocation)
            {
                continue;
            }

            var generator = editor.Generator;
            var semanticModel = editor.SemanticModel;

            var type = semanticModel.Compilation.GetTypeByMetadataName(extensionTypeName);
            if (type is null)
            {
                continue;
            }

            var newInvocation = generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpression(type), "HasFlagFast"),
                    [
                        memberAccess.Expression, // this parameter 
                        ..invocation.ArgumentList.Arguments,
                    ])
                .WithTriviaFrom(invocation)
                .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

            // Create new member access with the new identifier
            editor.ReplaceNode(invocation, newInvocation);
        }

        return editor.GetChangedDocument();
    }
}
