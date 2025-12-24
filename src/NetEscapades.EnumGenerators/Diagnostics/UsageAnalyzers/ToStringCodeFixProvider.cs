using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToStringCodeFixProvider)), Shared]
public class ToStringCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with ToStringFast()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(ToStringAnalyzer.DiagnosticId);

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for ToString() replacement
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
        // Find the node at the diagnostic location
        var node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan);

        // Check if this is an interpolation case
        var generator = editor.Generator;
        if (node.Parent is InterpolationSyntax interpolation)
        {
            var newInvocation = generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "ToStringFast"),
                    interpolation.Expression) // this parameter
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
                    generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "ToStringFast"),
                    memberAccess.Expression) // this parameter
                .WithTriviaFrom(invocation)
                .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

            editor.ReplaceNode(invocation, newInvocation);
        }

        return Task.CompletedTask;
    }
}