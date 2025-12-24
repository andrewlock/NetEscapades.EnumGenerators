using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

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

    protected abstract Task<Document> FixAllAsync(
        Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken);
}