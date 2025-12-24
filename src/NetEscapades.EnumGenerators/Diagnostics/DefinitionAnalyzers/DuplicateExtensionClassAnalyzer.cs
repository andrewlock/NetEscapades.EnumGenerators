using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.DefinitionAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DuplicateExtensionClassAnalyzer: DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG001";
    private static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Duplicate generated extension class",
        messageFormat:
        "The generated extension class '{1}.{2}' for enum '{0}' clashes with other generated extension classes. Use ExtensionClassNamespace or ExtensionClassName to specify a unique combination.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: "CompilationEnd");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze symbols instead of syntax
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

#pragma warning disable RS1012 // 'startContext' does not register any analyzer actions - false positive
        context.RegisterCompilationStartAction(startContext =>
#pragma warning restore RS1012 // 'startContext' does not register any analyzer actions - false positive
        {
            var enumMap = new ConcurrentDictionary<Tuple<string, string>, List<Tuple<Location, string>>>();

            startContext.RegisterSymbolAction(symbolContext =>
            {
                var ct = symbolContext.CancellationToken;
                var enumSymbol = (INamedTypeSymbol)symbolContext.Symbol;
                if (enumSymbol.TypeKind != TypeKind.Enum)
                {
                    return;
                }

                Location? location = null;
                string? ns = null;
                string? name = null;
                MetadataSource? source = null;
                foreach (var attributeData in enumSymbol.GetAttributes())
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    if (EnumGenerator.TryGetExtensionAttributeDetails(attributeData, ref ns, ref name, ref source))
                    {
                        location = attributeData.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation()
                                   ?? enumSymbol.Locations[0];
                        break;
                    }
                }

                if (location is null || ct.IsCancellationRequested)
                {
                    return;
                }

                // we have the attribute, get the calculated names
                ns ??= EnumGenerator.GetEnumExtensionNamespace(enumSymbol); 
                name ??= EnumGenerator.GetEnumExtensionName(enumSymbol);

                enumMap.AddOrUpdate(new(ns, name),
                    _ => [new(location, enumSymbol.Name)],
                    (_, list) =>
                    {
                        list.Add(new(location, enumSymbol.Name));
                        return list;
                    });
            }, SymbolKind.NamedType);

            startContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (var kvp in enumMap)
                {
                    var duplicates = kvp.Value;
                    if (duplicates.Count > 1)
                    {
                        foreach (var symbol in duplicates)
                        {
                            var ns = kvp.Key.Item1;
                            var name = kvp.Key.Item2;
                            var diag = Diagnostic.Create(Rule, symbol.Item1,
                                symbol.Item2, ns, name);
                            endContext.ReportDiagnostic(diag);
                        }
                    }
                }
            });
        });
    }
}