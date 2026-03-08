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
        var generator = editor.Generator;
        var isNullable = diagnostic.Properties.ContainsKey(AnalyzerHelpers.IsNullableProperty);

        // Check if this is an interpolation case
        if (node.Parent is InterpolationSyntax interpolation)
        {
            ExpressionSyntax newExpression;
            if (isNullable)
            {
                // $"{nullableValue}" → $"{nullableValue?.ToStringFast()}"
                newExpression = SyntaxFactory.ConditionalAccessExpression(
                        interpolation.Expression,
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberBindingExpression(
                                SyntaxFactory.IdentifierName("ToStringFast"))))
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);
            }
            else
            {
                // $"{value}" → $"{ExtensionType.ToStringFast(value)}"
                newExpression = (ExpressionSyntax)generator.InvocationExpression(
                        generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "ToStringFast"),
                        interpolation.Expression) // this parameter
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);
            }

            // Create a new interpolation with the invocation and no format clause
            var newInterpolation = SyntaxFactory.Interpolation(newExpression)
                .WithLeadingTrivia(interpolation.GetLeadingTrivia())
                .WithTrailingTrivia(interpolation.GetTrailingTrivia());

            if (interpolation.AlignmentClause is { } alignment)
            {
                newInterpolation = newInterpolation.WithAlignmentClause(alignment);
            }

            editor.ReplaceNode(interpolation, newInterpolation);
        }
        // Check if this is a conditional access case (value?.ToString())
        else if (node is IdentifierNameSyntax
                 && node.Parent is MemberBindingExpressionSyntax
                 && node.Parent.Parent is InvocationExpressionSyntax bindingInvocation)
        {
            // value?.ToString() → value?.ToStringFast() (just rename and strip args)
            var newInvocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberBindingExpression(
                        SyntaxFactory.IdentifierName("ToStringFast")))
                .WithTriviaFrom(bindingInvocation);

            editor.ReplaceNode(bindingInvocation, newInvocation);
        }
        // Check if this is a ToString invocation (original case)
        else if (node is IdentifierNameSyntax
                 && node.Parent is MemberAccessExpressionSyntax memberAccess
                 && memberAccess.Parent is InvocationExpressionSyntax invocation)
        {
            if (isNullable)
            {
                // value.ToString() on MyEnum? → value?.ToStringFast() ?? ""
                var conditionalAccess = SyntaxFactory.ConditionalAccessExpression(
                    memberAccess.Expression,
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberBindingExpression(
                            SyntaxFactory.IdentifierName("ToStringFast"))));

                ExpressionSyntax newExpression = SyntaxFactory.BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        conditionalAccess,
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("")))
                    .WithTriviaFrom(invocation);

                editor.ReplaceNode(invocation, newExpression);
            }
            else
            {
                // value.ToString() → ExtensionType.ToStringFast(value)
                var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "ToStringFast"),
                        memberAccess.Expression) // this parameter
                    .WithTriviaFrom(invocation)
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

                editor.ReplaceNode(invocation, newInvocation);
            }
        }

        return Task.CompletedTask;
    }
}