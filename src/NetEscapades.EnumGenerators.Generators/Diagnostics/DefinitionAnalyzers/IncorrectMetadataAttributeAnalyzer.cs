using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetEscapades.EnumGenerators.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class IncorrectMetadataAttributeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NEEG004";
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable Analyzer Release Tracking
        id: DiagnosticId,
#pragma warning restore RS2008
        title: "Metadata attribute will be ignored",
        messageFormat: "The '{0}' attribute on enum member '{1}' will be ignored because the enum is configured to use '{2}'",
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

    private static MetadataSource GetDefaultMetadataSource(AnalyzerOptions options)
    {
        const MetadataSource defaultValue = MetadataSource.EnumMemberAttribute;
        
        if (options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(
                $"build_property.{Constants.MetadataSourcePropertyName}",
                out var source))
        {
            return source switch
            {
                nameof(MetadataSource.None) => MetadataSource.None,
                nameof(MetadataSource.DisplayAttribute) => MetadataSource.DisplayAttribute,
                nameof(MetadataSource.DescriptionAttribute) => MetadataSource.DescriptionAttribute,
                nameof(MetadataSource.EnumMemberAttribute) => MetadataSource.EnumMemberAttribute,
                _ => defaultValue,
            };
        }
        
        return defaultValue;
    }

    private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        // Get the default metadata source from MSBuild properties
        var defaultMetadataSource = GetDefaultMetadataSource(context.Options);
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        
        // Check if enum has [EnumExtensions] attribute
        AttributeSyntax? enumExtensionsAttribute = null;
        MetadataSource? explicitMetadataSource = null;
        
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
                        enumExtensionsAttribute = attribute;
                        
                        // Check if MetadataSource is explicitly set
                        if (attribute.ArgumentList is not null)
                        {
                            foreach (var arg in attribute.ArgumentList.Arguments)
                            {
                                if (arg.NameEquals?.Name.Identifier.Text == "MetadataSource")
                                {
                                    // Try to get the metadata source value
                                    var attrData = context.SemanticModel.GetSymbolInfo(attribute).Symbol?.ContainingType;
                                    foreach (var attrDataItem in context.SemanticModel.GetDeclaredSymbol(enumDeclaration)?.GetAttributes() ?? Enumerable.Empty<AttributeData>())
                                    {
                                        if (attrDataItem.AttributeClass?.ToDisplayString() == Attributes.EnumExtensionsAttribute)
                                        {
                                            foreach (var namedArg in attrDataItem.NamedArguments)
                                            {
                                                if (namedArg.Key == "MetadataSource" && namedArg.Value.Value is int metadataSourceValue)
                                                {
                                                    explicitMetadataSource = (MetadataSource)metadataSourceValue;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
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

        // Determine the effective metadata source
        var effectiveMetadataSource = explicitMetadataSource ?? defaultMetadataSource;
        
        // If MetadataSource is None, no attributes will be used, so no need to warn
        if (effectiveMetadataSource == MetadataSource.None)
        {
            return;
        }

        // Get the enum symbol
        var enumSymbol = context.SemanticModel.GetDeclaredSymbol(enumDeclaration);
        if (enumSymbol is null)
        {
            return;
        }

        // Track which metadata attributes are found
        bool hasCorrectAttribute = false;
        var incorrectAttributes = new System.Collections.Generic.List<(Location Location, string AttributeName, string MemberName)>();

        // Analyze each enum member
        foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.IsConst)
            {
                continue;
            }

            foreach (var attribute in member.GetAttributes())
            {
                var attributeType = attribute.AttributeClass?.ToDisplayString();
                
                if (attributeType == Attributes.DisplayAttribute)
                {
                    if (effectiveMetadataSource == MetadataSource.DisplayAttribute)
                    {
                        hasCorrectAttribute = true;
                    }
                    else
                    {
                        var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation();
                        if (location is not null)
                        {
                            incorrectAttributes.Add((location, "Display", member.Name));
                        }
                    }
                }
                else if (attributeType == Attributes.DescriptionAttribute)
                {
                    if (effectiveMetadataSource == MetadataSource.DescriptionAttribute)
                    {
                        hasCorrectAttribute = true;
                    }
                    else
                    {
                        var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation();
                        if (location is not null)
                        {
                            incorrectAttributes.Add((location, "Description", member.Name));
                        }
                    }
                }
                else if (attributeType == Attributes.EnumMemberAttribute)
                {
                    if (effectiveMetadataSource == MetadataSource.EnumMemberAttribute)
                    {
                        hasCorrectAttribute = true;
                    }
                    else
                    {
                        var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation();
                        if (location is not null)
                        {
                            incorrectAttributes.Add((location, "EnumMember", member.Name));
                        }
                    }
                }
            }
        }

        // Only report diagnostics if we found incorrect attributes and no correct attributes
        if (incorrectAttributes.Count > 0 && !hasCorrectAttribute)
        {
            var effectiveSourceName = effectiveMetadataSource switch
            {
                MetadataSource.DisplayAttribute => "DisplayAttribute",
                MetadataSource.DescriptionAttribute => "DescriptionAttribute",
                MetadataSource.EnumMemberAttribute => "EnumMemberAttribute",
                _ => "None"
            };

            foreach (var (location, attributeName, memberName) in incorrectAttributes)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    attributeName,
                    memberName,
                    effectiveSourceName);
                
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
