using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToStringCodeFixProvider)), Shared]
public class ToStringCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with ToStringFast()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(ToStringAnalyzer.DiagnosticId);

    // We can't use the batch fixer because it causes multiple iterations

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for ToString() replacement
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ReplaceToStringWithToStringFast(context.Document, context.Diagnostics, c),
                    equivalenceKey: Title),
                context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    protected sealed override Task<Document> FixAllAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
        => ReplaceToStringWithToStringFast(document, diagnostics, cancellationToken);

    private static async Task<Document> ReplaceToStringWithToStringFast(
        Document document,
        ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        // Create the document editor
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

            var generator = editor.Generator;
            var semanticModel = editor.SemanticModel;

            var type = semanticModel.Compilation.GetTypeByMetadataName(extensionTypeName);
            if (type is null)
            {
                continue;
            }

            // Check if this is an interpolation case
            if (node.Parent is InterpolationSyntax interpolation)
            {
                var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(generator.TypeExpression(type), "ToStringFast"),
                        [interpolation.Expression]) // this parameter
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

                // Create a new interpolation with the invocation and no format clause
                var newInterpolation = SyntaxFactory.Interpolation((ExpressionSyntax)newInvocation)
                    .WithLeadingTrivia(interpolation.GetLeadingTrivia())
                    .WithTrailingTrivia(interpolation.GetTrailingTrivia());

                if (interpolation.AlignmentClause is { } alignment)
                {
                    newInterpolation = newInterpolation.WithAlignmentClause(alignment);
                }

                editor.ReplaceNode(interpolation, newInterpolation);
            }
            // Check if this is a ToString invocation (original case)
            else if (node is IdentifierNameSyntax identifierName
                && identifierName.Parent is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Parent is InvocationExpressionSyntax invocation)
            {
                var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(generator.TypeExpression(type), "ToStringFast"),
                        [memberAccess.Expression]) // this parameter
                    .WithTriviaFrom(invocation)
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

                editor.ReplaceNode(invocation, newInvocation);
            }
        }

        return editor.GetChangedDocument();
    }
}
