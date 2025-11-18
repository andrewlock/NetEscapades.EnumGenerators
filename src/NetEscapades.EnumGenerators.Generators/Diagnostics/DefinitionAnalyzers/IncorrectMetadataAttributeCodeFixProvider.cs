using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetEscapades.EnumGenerators.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IncorrectMetadataAttributeCodeFixProvider)), Shared]
public class IncorrectMetadataAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(IncorrectMetadataAttributeAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the attribute that triggered the diagnostic
        var attributeSyntax = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
        if (attributeSyntax is null)
        {
            return;
        }

        // Find the enum declaration
        var enumDeclaration = attributeSyntax.AncestorsAndSelf().OfType<EnumDeclarationSyntax>().FirstOrDefault();
        if (enumDeclaration is null)
        {
            return;
        }

        // Determine what metadata source should be used based on the attributes present
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        var enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration, context.CancellationToken);
        if (enumSymbol is null)
        {
            return;
        }

        // Determine which metadata source to suggest based on the attributes present
        var suggestedSource = DetermineMetadataSource(enumSymbol);
        if (suggestedSource is null)
        {
            return;
        }

        var metadataSourceName = suggestedSource.Value switch
        {
            MetadataSource.DisplayAttribute => "DisplayAttribute",
            MetadataSource.DescriptionAttribute => "DescriptionAttribute",
            MetadataSource.EnumMemberAttribute => "EnumMemberAttribute",
            _ => null
        };

        if (metadataSourceName is null)
        {
            return;
        }

        // Register a code action that will update the EnumExtensions attribute
        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Set MetadataSource to {metadataSourceName}",
                createChangedDocument: c => UpdateEnumExtensionsAttribute(context.Document, enumDeclaration, suggestedSource.Value, c),
                equivalenceKey: nameof(IncorrectMetadataAttributeCodeFixProvider)),
            diagnostic);
    }

    private static MetadataSource? DetermineMetadataSource(INamedTypeSymbol enumSymbol)
    {
        bool hasDisplay = false;
        bool hasDescription = false;
        bool hasEnumMember = false;

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
                    hasDisplay = true;
                }
                else if (attributeType == Attributes.DescriptionAttribute)
                {
                    hasDescription = true;
                }
                else if (attributeType == Attributes.EnumMemberAttribute)
                {
                    hasEnumMember = true;
                }
            }
        }

        // Prioritize based on what's most commonly used
        if (hasDisplay)
        {
            return MetadataSource.DisplayAttribute;
        }
        if (hasDescription)
        {
            return MetadataSource.DescriptionAttribute;
        }
        if (hasEnumMember)
        {
            return MetadataSource.EnumMemberAttribute;
        }

        return null;
    }

    private static async Task<Document> UpdateEnumExtensionsAttribute(
        Document document,
        EnumDeclarationSyntax enumDeclaration,
        MetadataSource metadataSource,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Find the EnumExtensions attribute
        AttributeSyntax? enumExtensionsAttribute = null;
        foreach (var attributeList in enumDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName == "EnumExtensions" || attributeName == "EnumExtensionsAttribute")
                {
                    enumExtensionsAttribute = attribute;
                    break;
                }
            }

            if (enumExtensionsAttribute is not null)
            {
                break;
            }
        }

        if (enumExtensionsAttribute is null)
        {
            return document;
        }

        // Create the metadata source expression
        var metadataSourceExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("MetadataSource"),
            SyntaxFactory.IdentifierName(metadataSource.ToString()));

        // Create the argument for MetadataSource
        var metadataSourceArgument = SyntaxFactory.AttributeArgument(
            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("MetadataSource")),
            null,
            metadataSourceExpression);

        // Check if the attribute already has an argument list
        AttributeSyntax newAttribute;
        if (enumExtensionsAttribute.ArgumentList is not null)
        {
            // Check if MetadataSource is already specified
            var existingMetadataSourceArg = enumExtensionsAttribute.ArgumentList.Arguments
                .FirstOrDefault(arg => arg.NameEquals?.Name.Identifier.Text == "MetadataSource");

            if (existingMetadataSourceArg is not null)
            {
                // Replace the existing argument
                var newArguments = enumExtensionsAttribute.ArgumentList.Arguments
                    .Replace(existingMetadataSourceArg, metadataSourceArgument);
                newAttribute = enumExtensionsAttribute.WithArgumentList(
                    enumExtensionsAttribute.ArgumentList.WithArguments(newArguments));
            }
            else
            {
                // Add the new argument
                var newArguments = enumExtensionsAttribute.ArgumentList.Arguments.Add(metadataSourceArgument);
                newAttribute = enumExtensionsAttribute.WithArgumentList(
                    enumExtensionsAttribute.ArgumentList.WithArguments(newArguments));
            }
        }
        else
        {
            // Create a new argument list
            newAttribute = enumExtensionsAttribute.WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(metadataSourceArgument)));
        }

        // Replace the old attribute with the new one
        var newRoot = root.ReplaceNode(enumExtensionsAttribute, newAttribute);
        return document.WithSyntaxRoot(newRoot);
    }
}
