using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetEscapades.EnumGenerators.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HasFlagCodeFixProvider)), Shared]
public class HasFlagCodeFixProvider : CodeFixProvider
{
    private const string Title = "Replace with HasFlagFast()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(HasFlagAnalyzer.DiagnosticId);

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

        // Find the node at the diagnostic location
        var node = root.FindNode(diagnosticSpan);

        // Check if this is a HasFlag invocation
        if (node is IdentifierNameSyntax identifierName)
        {
            // Register a code action for HasFlag() replacement
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ReplaceHasFlagWithHasFlagFast(context.Document, identifierName, c),
                    equivalenceKey: Title),
                context.Diagnostics);
        }
    }

    private static async Task<Document> ReplaceHasFlagWithHasFlagFast(
        Document document,
        IdentifierNameSyntax identifierName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Create the new identifier with "HasFlagFast"
        var newIdentifier = SyntaxFactory.IdentifierName("HasFlagFast")
            .WithTriviaFrom(identifierName);

        // Create new member access with the new identifier
        var memberAccess = identifierName.Parent as MemberAccessExpressionSyntax;
        if (memberAccess is null)
        {
            return document;
        }

        var newMemberAccess = memberAccess.WithName(newIdentifier);

        // Replace the old member access with the new one
        var newRoot = root.ReplaceNode(memberAccess, newMemberAccess);

        return document.WithSyntaxRoot(newRoot);
    }
}
