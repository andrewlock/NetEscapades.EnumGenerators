using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetEscapades.EnumGenerators.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToStringCodeFixProvider)), Shared]
public class ToStringCodeFixProvider : CodeFixProvider
{
    private const string Title = "Replace with ToStringFast()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(ToStringAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the ToString identifier
        var identifierNode = root.FindToken(diagnosticSpan.Start).Parent;
        if (identifierNode is not IdentifierNameSyntax identifierName)
        {
            return;
        }

        // Register a code action that will invoke the fix
        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => ReplaceToStringWithToStringFast(context.Document, identifierName, c),
                equivalenceKey: Title),
            context.Diagnostics);
    }

    private static async Task<Document> ReplaceToStringWithToStringFast(
        Document document,
        IdentifierNameSyntax identifierName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Find the invocation expression to replace arguments as well
        var invocationExpression = identifierName.Parent?.Parent as InvocationExpressionSyntax;
        if (invocationExpression is null)
        {
            return document;
        }

        // Create the new identifier with "ToStringFast"
        var newIdentifier = SyntaxFactory.IdentifierName("ToStringFast")
            .WithTriviaFrom(identifierName);

        // Create new member access with the new identifier
        var memberAccess = identifierName.Parent as MemberAccessExpressionSyntax;
        if (memberAccess is null)
        {
            return document;
        }

        var newMemberAccess = memberAccess.WithName(newIdentifier);

        // Create new invocation with empty argument list
        var newArgumentList = SyntaxFactory.ArgumentList()
            .WithTrailingTrivia(invocationExpression.ArgumentList.GetTrailingTrivia());

        var newInvocation = invocationExpression
            .WithExpression(newMemberAccess)
            .WithArgumentList(newArgumentList);

        // Replace the old invocation with the new one
        var newRoot = root.ReplaceNode(invocationExpression, newInvocation);

        return document.WithSyntaxRoot(newRoot);
    }
}
