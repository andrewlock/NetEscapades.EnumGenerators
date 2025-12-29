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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TryParseCodeFixProvider)), Shared]
public class TryParseCodeFixProvider : CodeFixProviderBase
{
    private const string Title = "Replace with generated TryParse()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(TryParseAnalyzer.DiagnosticId);

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!context.Diagnostics.IsDefaultOrEmpty)
        {
            // Register a code action for TryParse() replacement
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
        ArgumentSyntax? outArgument = null;
        ArgumentSyntax? ignoreCaseArgument = null;

        // Determine which arguments to use
        if (methodSymbol is { IsGenericMethod: true, TypeArguments.Length: 1 })
        {
            // Pattern: Enum.TryParse<TEnum>(value, out result) or Enum.TryParse<TEnum>(value, ignoreCase, out result)
            if (invocation.ArgumentList.Arguments.Count >= 2)
            {
                valueArgument = invocation.ArgumentList.Arguments[0];
                
                if (invocation.ArgumentList.Arguments.Count == 2)
                {
                    // TryParse<TEnum>(value, out result)
                    outArgument = invocation.ArgumentList.Arguments[1];
                }
                else if (invocation.ArgumentList.Arguments.Count == 3)
                {
                    // TryParse<TEnum>(value, ignoreCase, out result)
                    ignoreCaseArgument = invocation.ArgumentList.Arguments[1];
                    outArgument = invocation.ArgumentList.Arguments[2];
                }
            }
        }
        else if (methodSymbol.Parameters.Length is 3 or 4
                 && invocation.ArgumentList.Arguments.Count >= 3)
        {
            // Pattern: Enum.TryParse(typeof(TEnum), value, out result) or Enum.TryParse(typeof(TEnum), value, ignoreCase, out result)
            valueArgument = invocation.ArgumentList.Arguments[1];
            
            if (invocation.ArgumentList.Arguments.Count == 3)
            {
                // TryParse(typeof(TEnum), value, out result)
                outArgument = invocation.ArgumentList.Arguments[2];
            }
            else if (invocation.ArgumentList.Arguments.Count == 4)
            {
                // TryParse(typeof(TEnum), value, ignoreCase, out result)
                ignoreCaseArgument = invocation.ArgumentList.Arguments[2];
                outArgument = invocation.ArgumentList.Arguments[3];
            }
        }

        if (valueArgument is null || outArgument is null)
        {
            return Task.CompletedTask;
        }

        // Create new invocation: ExtensionsClass.TryParse(value, out result) or ExtensionsClass.TryParse(value, out result, ignoreCase)
        var generator = editor.Generator;
        
        editor.ReplaceNode(invocation, (node, _) =>
        {
            var inv = (InvocationExpressionSyntax)node;
            SyntaxNode newInvocation;
            if (ignoreCaseArgument is not null)
            {
                // Call with ignoreCase parameter
                newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "TryParse"),
                        valueArgument,
                        outArgument,
                        ignoreCaseArgument)
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);
            }
            else
            {
                // Call without ignoreCase parameter
                newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(generator.TypeExpression(extensionTypeSymbol), "TryParse"),
                        valueArgument,
                        outArgument)
                    .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.Annotation);
            }
            return newInvocation.WithTriviaFrom(inv);
        });
        
        return Task.CompletedTask;
    }
}
