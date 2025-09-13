using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators;

[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var defaultMetadataSource = context.AnalyzerConfigOptionsProvider
            .Select(GetDefaultMetadataSource);

        var csharp14IsSupported = context.CompilationProvider
            .Select((x,_) => x is CSharpCompilation
            {
                LanguageVersion: not LanguageVersion.Preview and >= (LanguageVersion)1400 // C#14
            });

        var defaults = csharp14IsSupported.Combine(defaultMetadataSource);
        
        IncrementalValuesProvider<EnumToGenerate> enumsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(Attributes.EnumExtensionsAttribute,
                predicate: static (node, _) => node is EnumDeclarationSyntax,
                transform: GetTypeToGenerate)
            .WithTrackingName(TrackingNames.InitialExtraction)
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value)
            .WithTrackingName(TrackingNames.RemovingNulls);

        IncrementalValuesProvider<EnumToGenerate> externalEnums = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(Attributes.ExternalEnumExtensionsAttribute,
                predicate: static (node, _) => node is CompilationUnitSyntax,
                transform: static (context1, ct) => GetEnumToGenerateFromGenericAssemblyAttribute(context1, ct, "EnumExtensionsAttribute", "EnumExtensions"))
            .Where(static m => m is not null)
            .SelectMany(static (m, _) => m!.Value)
            .WithTrackingName(TrackingNames.InitialExternalExtraction);

        context.RegisterSourceOutput(enumsToGenerate.Combine(defaults),
            static (spc, enumToGenerate) => Execute(in enumToGenerate.Left,
                enumToGenerate.Right.Left || enumToGenerate.Right.Right.Item2, enumToGenerate.Right.Right.Item1, spc));

        context.RegisterSourceOutput(externalEnums.Combine(defaults),
            static (spc, enumToGenerate) => Execute(in enumToGenerate.Left,
                enumToGenerate.Right.Left || enumToGenerate.Right.Right.Item2, enumToGenerate.Right.Right.Item1, spc));
    }

    private static Tuple<MetadataSource, bool> GetDefaultMetadataSource(AnalyzerConfigOptionsProvider configOptions, CancellationToken ct)
    {
        const MetadataSource defaultValue = MetadataSource.EnumMemberAttribute;
        MetadataSource selectedSource;
        if (configOptions.GlobalOptions.TryGetValue($"build_property.{Constants.MetadataSourcePropertyName}",
                out var source))
        {
            selectedSource = source switch
            {
                nameof(MetadataSource.None) => MetadataSource.None,
                nameof(MetadataSource.DisplayAttribute) => MetadataSource.DisplayAttribute,
                nameof(MetadataSource.DescriptionAttribute) => MetadataSource.DescriptionAttribute,
                nameof(MetadataSource.EnumMemberAttribute) => MetadataSource.EnumMemberAttribute,
                _ => defaultValue,
            };
        }
        else
        {
            selectedSource = defaultValue;
        }

        var forceExtensionMembers =
            configOptions.GlobalOptions.TryGetValue($"build_property.{Constants.ForceExtensionMembers}", out var force)
            && string.Equals(force, "true", StringComparison.OrdinalIgnoreCase);

        return new(selectedSource, forceExtensionMembers);
    }

    static void Execute(in EnumToGenerate enumToGenerate, bool useExtensionMembers, MetadataSource source, SourceProductionContext context)
    {
        var (result, filename) = SourceGenerationHelper.GenerateExtensionClass(in enumToGenerate, useExtensionMembers, source);
        context.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }

    static EquatableArray<EnumToGenerate>? GetEnumToGenerateFromGenericAssemblyAttribute(
        GeneratorAttributeSyntaxContext context, CancellationToken ct, string fullAttributeName, string shortAttributeName)
    {
        List<EnumToGenerate>? enums = null;
        foreach (AttributeData attribute in context.Attributes)
        {
            if (!((attribute.AttributeClass?.Name == fullAttributeName ||
                   attribute.AttributeClass?.Name == shortAttributeName) &&
                  attribute.AttributeClass.IsGenericType &&
                  attribute.AttributeClass.TypeArguments.Length == 1))
            {
                // wrong attribute
                continue;
            }

            var enumSymbol = attribute.AttributeClass.TypeArguments[0] as INamedTypeSymbol;
            if (enumSymbol is null)
            {
                continue;
            }

            bool hasFlags = false;
            string? name = null;
            string? nameSpace = null;
            MetadataSource? source = null;

            foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key == "ExtensionClassNamespace"
                    && namedArgument.Value.Value?.ToString() is { } ns)
                {
                    nameSpace = ns;
                    continue;
                }

                if (namedArgument.Key == "ExtensionClassName"
                    && namedArgument.Value.Value?.ToString() is { } n)
                {
                    name = n;
                }

                if (namedArgument.Key == "MetadataSource"
                    && namedArgument.Value is { Kind: TypedConstantKind.Enum, Value: { } ms })
                {
                    source = (MetadataSource)(int)ms;
                }
            }

            foreach (var attrData in enumSymbol.GetAttributes())
            {
                if ((attrData.AttributeClass?.Name == "FlagsAttribute" ||
                     attrData.AttributeClass?.Name == "Flags") &&
                    attrData.AttributeClass.ToDisplayString() == Attributes.FlagsAttribute)
                {
                    hasFlags = true;
                    break;
                }
            }

            var enumToGenerate = TryExtractEnumSymbol(enumSymbol, name, nameSpace, source, hasFlags);
            if (enumToGenerate is not null)
            {
                enums ??= new();
                enums.Add(enumToGenerate.Value);
            }
        }

        return enums is not null
            ? new EquatableArray<EnumToGenerate>(enums.ToArray())
            : null;
    }

    static EnumToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        INamedTypeSymbol? enumSymbol = context.TargetSymbol as INamedTypeSymbol;
        if (enumSymbol is null)
        {
            // nothing to do if this type isn't available
            return null;
        }

        ct.ThrowIfCancellationRequested();

        // Skip enums in generic types as they won't be valid - error will be raised from analyzer
        if (SymbolHelpers.IsNestedInGenericType(enumSymbol))
        {
            return null;
        }

        var hasFlags = false;
        string? nameSpace = null;
        string? name = null;
        MetadataSource? metadataSource = null;

        foreach (AttributeData attributeData in enumSymbol.GetAttributes())
        {
            if ((attributeData.AttributeClass?.Name == "FlagsAttribute" ||
                 attributeData.AttributeClass?.Name == "Flags") &&
                attributeData.AttributeClass.ToDisplayString() == Attributes.FlagsAttribute)
            {
                hasFlags = true;
                continue;
            }

            TryGetExtensionAttributeDetails(attributeData, ref nameSpace, ref name, ref metadataSource);
        }

        return TryExtractEnumSymbol(enumSymbol, name, nameSpace, metadataSource, hasFlags);
    }

    internal static bool TryGetExtensionAttributeDetails(
        AttributeData attributeData,
        ref string? nameSpace,
        ref string? name,
        ref MetadataSource? source)
    {
        if (attributeData.AttributeClass?.Name != "EnumExtensionsAttribute" ||
            attributeData.AttributeClass.ToDisplayString() != Attributes.EnumExtensionsAttribute)
        {
            return false;
        }

        foreach (KeyValuePair<string, TypedConstant> namedArgument in attributeData.NamedArguments)
        {
            if (namedArgument.Key == "ExtensionClassNamespace"
                && namedArgument.Value.Value?.ToString() is { } ns)
            {
                nameSpace = ns;
                continue;
            }

            if (namedArgument.Key == "ExtensionClassName"
                && namedArgument.Value.Value?.ToString() is { } n)
            {
                name = n;
            }

            if (namedArgument.Key == "MetadataSource"
                && namedArgument.Value is { Kind: TypedConstantKind.Enum, Value: { } ms })
            {
                source = (MetadataSource)(int)ms;
            }
        }

        return true;
    }

    internal static string GetEnumExtensionNamespace(INamedTypeSymbol enumSymbol)
        => enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();

    internal static string GetEnumExtensionName(INamedTypeSymbol enumSymbol)
        => enumSymbol.Name + "Extensions";

    static EnumToGenerate? TryExtractEnumSymbol(
        INamedTypeSymbol enumSymbol,
        string? name,
        string? nameSpace,
        MetadataSource? metadataSource,
        bool hasFlags)
    {
        name ??= GetEnumExtensionName(enumSymbol);
        nameSpace ??= GetEnumExtensionNamespace(enumSymbol);

        string fullyQualifiedName = enumSymbol.ToString();
        string underlyingType = enumSymbol.EnumUnderlyingType?.ToString() ?? "int";

        var enumMembers = enumSymbol.GetMembers();
        var members = new List<(string, EnumValueOption)>(enumMembers.Length);

        foreach (var member in enumMembers)
        {
            if (member is not IFieldSymbol {ConstantValue: { } constantValue})
            {
                continue;
            }

            string? displayName = null;
            string? description = null;
            string? enumMemberValue = null;
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == "DisplayAttribute" &&
                    attribute.AttributeClass.ToDisplayString() == Attributes.DisplayAttribute)
                {
                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == "Name" && namedArgument.Value.Value?.ToString() is { } dn)
                        {
                            displayName = dn;
                        }
                    }
                }
                
                if (attribute.AttributeClass?.Name == "DescriptionAttribute" 
                    && attribute.AttributeClass.ToDisplayString() == Attributes.DescriptionAttribute
                    && attribute.ConstructorArguments.Length == 1)
                {
                    if (attribute.ConstructorArguments[0].Value?.ToString() is { } dn)
                    {
                        description = dn;
                    }
                }

                if (attribute.AttributeClass?.Name == "EnumMemberAttribute" &&
                    attribute.AttributeClass.ToDisplayString() == Attributes.EnumMemberAttribute)
                {
                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == "Value" && namedArgument.Value.Value?.ToString() is { } dn)
                        {
                            enumMemberValue = dn;
                        }
                    }
                }
            }

            members.Add((member.Name, new EnumValueOption(displayName, description, enumMemberValue, constantValue)));
        }

        return new EnumToGenerate(
            name: name,
            fullyQualifiedName: fullyQualifiedName,
            ns: nameSpace,
            underlyingType: underlyingType,
            isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public,
            hasFlags: hasFlags,
            names: members,
            metadataSource: metadataSource);
    }

}
