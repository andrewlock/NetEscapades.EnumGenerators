using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics.DefinitionAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DuplicateEnumValueAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG003";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Enum has duplicate values and will give inconsistent values for ToStringFast()",
        messageFormat: "The enum member '{0}' has the same value as a previous member so will return the '{1}' value for ToStringFast()",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
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
        
        // Check if enum has [EnumExtensions] attribute
        bool hasEnumExtensionsAttribute = false;
        foreach (var attributeList in enumDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                // Check attribute name syntactically first
                var attributeName = attribute.Name.ToString();
                if (attributeName == "EnumExtensions" || attributeName == "EnumExtensionsAttribute")
                {
                    // Verify with semantic model for precision
                    var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                    if (symbolInfo.Symbol is IMethodSymbol method &&
                        method.ContainingType.ToDisplayString() == Attributes.EnumExtensionsAttribute)
                    {
                        hasEnumExtensionsAttribute = true;
                        break;
                    }
                }
            }

            if (hasEnumExtensionsAttribute)
            {
                break;
            }
        }

        if (!hasEnumExtensionsAttribute)
        {
            return;
        }

        // Get the enum symbol
        var enumSymbol = context.SemanticModel.GetDeclaredSymbol(enumDeclaration);
        if (enumSymbol is null)
        {
            return;
        }

        // Track which constant values we've seen and the first name
        var seenValues = new Dictionary<object, string>();
        
        // Analyze each enum member
        foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.IsConst || member.ConstantValue is null)
            {
                continue;
            }

            var constantValue = member.ConstantValue;
            var memberName = member.Name;
            
            // If we've already seen this value, this member will be excluded
            if (seenValues.TryGetValue(constantValue, out var previousName))
            {
                // Find the syntax location for this member
                var memberLocation = member.Locations.FirstOrDefault();
                if (memberLocation is not null)
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        memberLocation,
                        memberName,
                        previousName);
                    
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else
            {
                seenValues[member.ConstantValue] = memberName;
            }
        }
    }
}