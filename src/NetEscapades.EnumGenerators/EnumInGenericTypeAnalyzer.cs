using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnumInGenericTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(DiagnosticHelper.EnumInGenericType);

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
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is IMethodSymbol method &&
                    method.ContainingType.ToDisplayString() == Attributes.EnumExtensionsAttribute)
                {
                    hasEnumExtensionsAttribute = true;
                    break;
                }
            }
            if (hasEnumExtensionsAttribute) break;
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

        // Check if nested in generic type
        if (IsNestedInGenericType(enumSymbol))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticHelper.EnumInGenericType,
                enumDeclaration.Identifier.GetLocation(),
                enumSymbol.Name);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNestedInGenericType(INamedTypeSymbol enumSymbol)
    {
        var containingType = enumSymbol.ContainingType;
        while (containingType is not null)
        {
            if (containingType.IsGenericType)
            {
                return true;
            }
            containingType = containingType.ContainingType;
        }
        return false;
    }
}