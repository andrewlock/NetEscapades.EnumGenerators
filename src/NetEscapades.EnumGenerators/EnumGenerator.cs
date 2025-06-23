using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace NetEscapades.EnumGenerators;

[Generator]
public class EnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        var csharp14IsSupported = context.CompilationProvider
            .Select((x,_) => x is CSharpCompilation
            {
                LanguageVersion: LanguageVersion.Preview or >= (LanguageVersion)1400 // C#14
            });
        
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

        context.RegisterSourceOutput(enumsToGenerate.Combine(csharp14IsSupported),
            static (spc, enumToGenerate) => Execute(in enumToGenerate.Left, enumToGenerate.Right, spc));

        context.RegisterSourceOutput(externalEnums.Combine(csharp14IsSupported),
            static (spc, enumToGenerate) => Execute(in enumToGenerate.Left, enumToGenerate.Right, spc));
    }

    static void Execute(in EnumToGenerate enumToGenerate, bool csharp14IsSupported, SourceProductionContext context)
    {
        var (result, filename) = SourceGenerationHelper.GenerateExtensionClass(in enumToGenerate, csharp14IsSupported);
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

            var enumToGenerate = TryExtractEnumSymbol(enumSymbol, name, nameSpace, hasFlags);
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

        // Skip enums in generic types - these will be handled by diagnostics provider
        if (IsNestedInGenericType(enumSymbol))
        {
            return null;
        }

        var hasFlags = false;
        string? nameSpace = null;
        string? name = null;

        foreach (AttributeData attributeData in enumSymbol.GetAttributes())
        {
            if ((attributeData.AttributeClass?.Name == "FlagsAttribute" ||
                 attributeData.AttributeClass?.Name == "Flags") &&
                attributeData.AttributeClass.ToDisplayString() == Attributes.FlagsAttribute)
            {
                hasFlags = true;
                continue;
            }

            if (attributeData.AttributeClass?.Name != "EnumExtensionsAttribute" ||
                attributeData.AttributeClass.ToDisplayString() != Attributes.EnumExtensionsAttribute)
            {
                continue;
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
            }
        }

        return TryExtractEnumSymbol(enumSymbol, name, nameSpace, hasFlags);
    }

    static EnumToGenerate? TryExtractEnumSymbol(INamedTypeSymbol enumSymbol, string? name, string? nameSpace, bool hasFlags)
    {
        name ??= enumSymbol.Name + "Extensions";
        nameSpace ??= enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();

        string fullyQualifiedName = enumSymbol.ToString();
        string underlyingType = enumSymbol.EnumUnderlyingType?.ToString() ?? "int";

        var enumMembers = enumSymbol.GetMembers();
        var members = new List<(string, EnumValueOption)>(enumMembers.Length);
        HashSet<string>? displayNames = null;
        var isDisplayNameTheFirstPresence = false;

        foreach (var member in enumMembers)
        {
            if (member is not IFieldSymbol {ConstantValue: { } constantValue})
            {
                continue;
            }

            string? displayName = null;
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == "DisplayAttribute" &&
                    attribute.AttributeClass.ToDisplayString() == Attributes.DisplayAttribute)
                {
                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == "Name" && namedArgument.Value.Value?.ToString() is { } dn)
                        {
                            // found display attribute, all done
                            displayName = dn;
                            goto addDisplayName;
                        }
                    }
                }
                
                if (attribute.AttributeClass?.Name == "DescriptionAttribute" 
                    && attribute.AttributeClass.ToDisplayString() == Attributes.DescriptionAttribute
                    && attribute.ConstructorArguments.Length == 1)
                {
                    if (attribute.ConstructorArguments[0].Value?.ToString() is { } dn)
                    {
                        // found display attribute, all done
                        // Handle cases where contains a quote or a backslash
                        displayName = dn;
                        goto addDisplayName;
                    }
                }
            }

            addDisplayName:
            if (displayName is not null)
            {
                displayNames ??= new();
                isDisplayNameTheFirstPresence = displayNames.Add(displayName);    
            }
            
            members.Add((member.Name, new EnumValueOption(displayName, isDisplayNameTheFirstPresence, constantValue)));
        }

        return new EnumToGenerate(
            name: name,
            fullyQualifiedName: fullyQualifiedName,
            ns: nameSpace,
            underlyingType: underlyingType,
            isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public,
            hasFlags: hasFlags,
            names: members,
            isDisplayAttributeUsed: displayNames?.Count > 0);
    }

    static bool IsNestedInGenericType(INamedTypeSymbol enumSymbol)
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
