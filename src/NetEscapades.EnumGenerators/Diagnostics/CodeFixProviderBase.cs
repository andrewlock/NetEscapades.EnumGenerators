using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace NetEscapades.EnumGenerators.Diagnostics;

public abstract class CodeFixProviderBase : CodeFixProvider
{
    private static readonly ImmutableArray<FixAllScope> DefaultSupportedFixAllScopes =
        ImmutableArray.CreateRange(
        [
            FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution, FixAllScope.ContainingMember,
            FixAllScope.ContainingType
        ]);


    public sealed override FixAllProvider GetFixAllProvider()
    {
        return FixAllProvider.Create(
            async (fixAllContext, document, diagnostics) =>
            {
                if (diagnostics.Length == 0)
                {
                    return document;
                }

                // Ensure that diagnostics for this document are always in document location order.  This provides a
                // consistent and deterministic order for fixers that want to update a document.
                //
                // Also ensure that we do not pass in duplicates by invoking Distinct.  See
                // https://github.com/dotnet/roslyn/issues/31381, that seems to be causing duplicate diagnostics.
                var filteredDiagnostics = diagnostics
                    .Distinct()
                    .ToImmutableArray()
                    .Sort((d1, d2) => d1.Location.SourceSpan.Start - d2.Location.SourceSpan.Start);

                return await FixAllAsync(document, filteredDiagnostics, fixAllContext.CancellationToken)
                    .ConfigureAwait(false);
            },

            DefaultSupportedFixAllScopes
        );
    }

    protected Task<Document> FixAllAsync(
        Document document,
        ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
        => FixAllAsync(document, diagnostics, FixWithEditor, cancellationToken);

    private static async Task<Document> FixAllAsync(
            Document document,
            ImmutableArray<Diagnostic> diagnostics,
            Func<DocumentEditor, Diagnostic, INamedTypeSymbol, CancellationToken, Task> fixFunc,  
            CancellationToken cancellationToken)
    {
        // Create a document editor used to apply fixes for all diagnostics
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        if (editor is null)
        {
            return document;
        }

        foreach (var diagnostic in diagnostics)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!diagnostic.Properties.TryGetValue(AnalyzerHelpers.ExtensionTypeNameProperty, out var extensionTypeName)
                || extensionTypeName is null)
            {
                continue;
            }

            var type = editor.SemanticModel.Compilation.GetBestTypeByMetadataName(extensionTypeName);
            if (type is null)
            {
                continue;
            }

            await fixFunc(editor, diagnostic, type, cancellationToken);
        }

        return editor.GetChangedDocument();
    }

    protected abstract Task FixWithEditor(
        DocumentEditor editor, Diagnostic diagnostic, INamedTypeSymbol extensionTypeSymbol, CancellationToken cancellationToken);

}