using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParseCodeFixProvider)), Shared]
public class ParseCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with generated Parse()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(ParseAnalyzer.DiagnosticId);

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for Parse() replacement
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

        ArgumentSyntax? valueArgument = null;
        ArgumentSyntax? ignoreCaseArgument = null;

        // Determine which arguments to use
        if (methodSymbol is { IsGenericMethod: true, TypeArguments.Length: 1 })
        {
            // Pattern: Enum.Parse<TEnum>(value) or Enum.Parse<TEnum>(value, ignoreCase)
            if (invocation.ArgumentList.Arguments.Count >= 1)
            {
                valueArgument = invocation.ArgumentList.Arguments[0];
            }

            if (invocation.ArgumentList.Arguments.Count >= 2)
            {
                ignoreCaseArgument = invocation.ArgumentList.Arguments[1];
            }
        }
        else if (methodSymbol.Parameters.Length is 2 or 3
                 && invocation.ArgumentList.Arguments.Count >= 2)
        {
            // Pattern: Enum.Parse(typeof(TEnum), value) or Enum.Parse(typeof(TEnum), value, ignoreCase)
            valueArgument = invocation.ArgumentList.Arguments[1];
            if (invocation.ArgumentList.Arguments.Count >= 3)
            {
                ignoreCaseArgument = invocation.ArgumentList.Arguments[2];
            }
        }

        if (valueArgument is null)
        {
            return Task.CompletedTask;
        }

        // Create new invocation: ExtensionsClass.Parse(value) or ExtensionsClass.Parse(value, ignoreCase)
        var generator = editor.Generator;
        SyntaxNode newInvocation;
        if (ignoreCaseArgument is not null)
        {
            // Call with ignoreCase parameter
            newInvocation = generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "Parse"),
                    valueArgument,
                    ignoreCaseArgument)
                .WithTriviaFrom(invocation)
                .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);
        }
        else
        {
            // Call without ignoreCase parameter
            newInvocation = generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "Parse"),
                    valueArgument)
                .WithTriviaFrom(invocation)
                .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);
        }

        editor.ReplaceNode(invocation, newInvocation);
        return Task.CompletedTask;
    }
}