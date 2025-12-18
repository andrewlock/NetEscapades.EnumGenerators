using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HasFlagAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG005";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Use HasFlagFast() instead of HasFlag()",
        messageFormat: "Use HasFlagFast() instead of HasFlag() for better performance on enum '{0}'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(ctx =>
        {
            var enumExtensionsAttr =
                ctx.Compilation.GetTypeByMetadataName(Attributes.EnumExtensionsAttribute);
            var externalEnumExtensionsAttr =
                ctx.Compilation.GetTypeByMetadataName(Attributes.ExternalEnumExtensionsAttribute);

            if (enumExtensionsAttr is null)
            {
                return;
            }

            // Collect all enum types that have EnumExtensions<T> attributes
            var externalEnumTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            if (externalEnumExtensionsAttr is not null)
            {
                foreach (var attribute in ctx.Compilation.Assembly.GetAttributes())
                {
                    if (attribute.AttributeClass is { IsGenericType: true } attrClass &&
                        SymbolEqualityComparer.Default.Equals(attrClass.ConstructedFrom, externalEnumExtensionsAttr) &&
                        attrClass.TypeArguments is [INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType])
                    {
                        externalEnumTypes.Add(enumType);
                    }
                }
            }

            ctx.RegisterSyntaxNodeAction(
                c => AnalyzeInvocation(c, enumExtensionsAttr, externalEnumTypes),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumExtensionsAttr, HashSet<INamedTypeSymbol> externalEnumTypes)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a member access expression (e.g., value.HasFlag())
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Check if the method name is "HasFlag"
        if (memberAccess.Name.Identifier.Text != "HasFlag")
        {
            return;
        }

        // Check if there is exactly one argument
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        // Get the symbol information for the invocation
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Verify this is the HasFlag() method from System.Enum
        if (methodSymbol.Name != "HasFlag" ||
            methodSymbol.Parameters.Length != 1 ||
            methodSymbol.ContainingType.SpecialType != SpecialType.System_Enum)
        {
            return;
        }

        // Get the type of the receiver (the thing before .HasFlag())
        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (receiverType is null || receiverType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        if (!IsEnumWithExtensions(receiverType, enumExtensionsAttr, externalEnumTypes))
        {
            return;
        }

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Name.GetLocation(),
            receiverType.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsEnumWithExtensions(
        ITypeSymbol receiverType,
        INamedTypeSymbol enumExtensionsAttr,
        HashSet<INamedTypeSymbol> externalEnumTypes)
    {
        // Check if the enum has the [EnumExtensions] attribute or is referenced in EnumExtensions<T>
        // First check if the enum itself has the attribute
        foreach (var attributeData in receiverType.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(
                    attributeData.AttributeClass,
                    enumExtensionsAttr))
            {
                return true;
            }
        }

        // If not, check if it's in the external enum types (EnumExtensions<T>)
        return receiverType is INamedTypeSymbol namedType
               && externalEnumTypes.Contains(namedType);
    }
}
