using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ToStringAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG004";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Use ToStringFast() instead of ToString()",
        messageFormat: "Use ToStringFast() instead of ToString() for better performance on enum '{0}'",
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
            
            ctx.RegisterSyntaxNodeAction(
                c => AnalyzeInterpolation(c, enumExtensionsAttr, externalEnumTypes),
                SyntaxKind.Interpolation);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumExtensionsAttr, HashSet<INamedTypeSymbol> externalEnumTypes)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a member access expression (e.g., value.ToString())
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        // Check if there are too many arguments)
        if (invocation.ArgumentList.Arguments.Count > 1)
        {
            return;
        }

        // Check if the method name is "ToString"
        if (memberAccess.Name.Identifier.Text != "ToString")
        {
            return;
        }

        // Get the symbol information for the invocation
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        // Verify this is the ToString() method from System.Object or System.Enum
        // We handle format specifiers, so accept ToString() with 0 or 1 parameters
        if (methodSymbol.Name != "ToString" ||
            methodSymbol.Parameters.Length > 1 ||
            (methodSymbol.ContainingType.SpecialType != SpecialType.System_Object &&
             methodSymbol.ContainingType.SpecialType != SpecialType.System_Enum))
        {
            return;
        }

        // If there's a format parameter, check if it's compatible with ToStringFast()
        // ToStringFast() is equivalent to ToString() with no args or ToString("G")/ToString("g")/ToString("")
        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            var argument = invocation.ArgumentList.Arguments[0];
            var constantValue = context.SemanticModel.GetConstantValue(argument.Expression);
            
            // If we can't determine the value at compile time, don't suggest replacement
            // If it's not a string (e.g., it's an IFormatProvider), don't suggest replacement
            if (!constantValue.HasValue || constantValue.Value is not string formatString)
            {
                return;
            }

            // Check if the format string is compatible with ToStringFast()
            // Only "", "G", and "g" are compatible
            if (!string.IsNullOrEmpty(formatString) && 
                !string.Equals(formatString, "G", StringComparison.Ordinal) &&
                !string.Equals(formatString, "g", StringComparison.Ordinal))
            {
                return;
            }
        }

        // Get the type of the receiver (the thing before .ToString())
        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (receiverType is null || receiverType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        // Check if the enum has the [EnumExtensions] attribute or is referenced in EnumExtensions<T>
        bool hasEnumExtensionsAttribute = false;
        
        // First check if the enum itself has the attribute
        foreach (var attributeData in receiverType.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(
                    attributeData.AttributeClass,
                    enumExtensionsAttr))
            {
                hasEnumExtensionsAttribute = true;
                break;
            }
        }

        // If not, check if it's in the external enum types (EnumExtensions<T>)
        if (!hasEnumExtensionsAttribute && receiverType is INamedTypeSymbol namedType)
        {
            hasEnumExtensionsAttribute = externalEnumTypes.Contains(namedType);
        }

        if (!hasEnumExtensionsAttribute)
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

    private static void AnalyzeInterpolation(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumExtensionsAttr, HashSet<INamedTypeSymbol> externalEnumTypes)
    {
        var interpolation = (InterpolationSyntax)context.Node;

        // Get the expression inside the interpolation
        var expression = interpolation.Expression;
        if (expression is null)
        {
            return;
        }

        // Get the type of the expression using GetSymbolInfo first, then fall back to GetTypeInfo
        var symbolInfo = context.SemanticModel.GetSymbolInfo(expression);
        ITypeSymbol? expressionType = null;
        
        if (symbolInfo.Symbol is ILocalSymbol localSymbol)
        {
            expressionType = localSymbol.Type;
        }
        else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
        {
            expressionType = fieldSymbol.Type;
        }
        else if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
        {
            expressionType = propertySymbol.Type;
        }
        else
        {
            // Fall back to GetTypeInfo
            expressionType = context.SemanticModel.GetTypeInfo(expression).Type;
        }
        
        if (expressionType is null || expressionType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        // Check if there's a format clause (e.g., :g, :G, :x)
        if (interpolation.FormatClause is not null)
        {
            var formatString = interpolation.FormatClause.FormatStringToken.Text;
            
            // Check if the format string is compatible with ToStringFast()
            // Only "", "g", and "G" are compatible (empty is when there's no format clause)
            if (!string.IsNullOrEmpty(formatString) && 
                !string.Equals(formatString, "G", StringComparison.Ordinal) &&
                !string.Equals(formatString, "g", StringComparison.Ordinal))
            {
                return;
            }
        }

        // Check if the enum has the [EnumExtensions] attribute or is referenced in EnumExtensions<T>
        bool hasEnumExtensionsAttribute = false;
        
        // First check if the enum itself has the attribute
        foreach (var attributeData in expressionType.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(
                    attributeData.AttributeClass,
                    enumExtensionsAttr))
            {
                hasEnumExtensionsAttribute = true;
                break;
            }
        }

        // If not, check if it's in the external enum types (EnumExtensions<T>)
        if (!hasEnumExtensionsAttribute && expressionType is INamedTypeSymbol namedType)
        {
            hasEnumExtensionsAttribute = externalEnumTypes.Contains(namedType);
        }

        if (!hasEnumExtensionsAttribute)
        {
            return;
        }

        // Report the diagnostic on the expression itself
        var diagnostic = Diagnostic.Create(
            Rule,
            expression.GetLocation(),
            expressionType.Name);

        context.ReportDiagnostic(diagnostic);
    }
}
