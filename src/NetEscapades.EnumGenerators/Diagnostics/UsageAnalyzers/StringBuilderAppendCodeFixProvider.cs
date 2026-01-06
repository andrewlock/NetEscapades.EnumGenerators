using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringBuilderAppendCodeFixProvider)), Shared]
public class StringBuilderAppendCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with ToStringFast()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(StringBuilderAppendAnalyzer.DiagnosticId);

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for StringBuilder.Append() replacement
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
        // Find the node at the diagnostic location (this is the argument)
        var node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan);

        // The diagnostic is on the argument, we need to find the ArgumentSyntax
        ArgumentSyntax? argument;
        if (node is not ArgumentSyntax argumentNode)
        {
            // Try to find the argument containing this node
            argument = node.FirstAncestorOrSelf<ArgumentSyntax>();
        }
        else
        {
            argument = argumentNode;
        }

        if (argument is null)
        {
            return Task.CompletedTask;
        }

        var generator = editor.Generator;

        // Create the new expression: enumValue.ToStringFast()
        var newInvocation = generator.InvocationExpression(
                generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "ToStringFast"),
                argument.Expression) // this parameter
            .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);

        // Create a new argument with the invocation
        var newArgument = argument.WithExpression((ExpressionSyntax)newInvocation);

        editor.ReplaceNode(argument, newArgument);

        return Task.CompletedTask;
    }
}
