using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HasFlagCodeFixProvider)), Shared]
public class HasFlagCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with HasFlagFast()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(HasFlagAnalyzer.DiagnosticId);

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for HasFlag() replacement
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => FixAllAsync(context.Document, context.Diagnostics, c),
                    equivalenceKey: Title),
                context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    protected override Task FixWithEditor(DocumentEditor editor, Diagnostic diagnostic, INamedTypeSymbol extensionTypeSymbol,
        CancellationToken cancellationToken)
    {
        var generator = editor.Generator;

        // Find the node at the diagnostic location
        var node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan);

        if (node is not IdentifierNameSyntax identifierName
            || identifierName.Parent is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Parent is not InvocationExpressionSyntax invocation)
        {
            return Task.CompletedTask;
        }

        var newInvocation = generator.InvocationExpression(
                generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "HasFlagFast"),
                [
                    memberAccess.Expression, // this parameter 
                    ..invocation.ArgumentList.Arguments,
                ])
            .WithTriviaFrom(invocation)
            .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

        // Create new member access with the new identifier
        editor.ReplaceNode(invocation, newInvocation);
        return Task.CompletedTask;
    }
}
