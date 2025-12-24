using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.DefinitionAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnumInGenericTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG002";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Enum in generic type not supported",
        messageFormat: "The enum '{0}' is nested inside a generic type. [EnumExtension] attribute is not supported.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeEnumDeclaration, SyntaxKind.EnumDeclaration);
    }

    private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        
        // Check if enum has [EnumExtensions] attribute and capture its location
        AttributeSyntax? enumExtensionsAttribute = null;
        foreach (var attributeList in enumDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                // Check attribute name syntactically first
                var attributeName = attribute.Name.ToString();
                if (attributeName == "EnumExtensions" || attributeName == "EnumExtensionsAttribute")
                {
                    // Verify with semantic model if needed for precision
                    var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                    if (symbolInfo.Symbol is IMethodSymbol method &&
                        method.ContainingType.ToDisplayString() == Attributes.EnumExtensionsAttribute)
                    {
                        enumExtensionsAttribute = attribute;
                        break;
                    }
                }
            }

            if (enumExtensionsAttribute is not null)
            {
                break;
            }
        }

        if (enumExtensionsAttribute is null)
        {
            return;
        }

        // Get the enum symbol
        var enumSymbol = context.SemanticModel.GetDeclaredSymbol(enumDeclaration);
        if (enumSymbol is null)
        {
            return;
        }

        // Check if nested in generic type
        if (SymbolHelpers.IsNestedInGenericType(enumSymbol))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                enumExtensionsAttribute.GetLocation(),
                enumSymbol.Name);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

}