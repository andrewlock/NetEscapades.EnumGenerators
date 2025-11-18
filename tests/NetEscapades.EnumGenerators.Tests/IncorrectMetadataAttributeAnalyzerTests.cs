using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    NetEscapades.EnumGenerators.Diagnostics.IncorrectMetadataAttributeAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class IncorrectMetadataAttributeAnalyzerTests
{
    private const string DiagnosticId = IncorrectMetadataAttributeAnalyzer.DiagnosticId;

    [Fact]
    public async Task EmptySourceShouldNotHaveDiagnostics()
    {
        var test = string.Empty;
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithoutAttributeShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            """
            public enum TestEnum
            {
                [Display(Name = "First")]
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithCorrectDefaultAttributeShouldNotHaveDiagnostics()
    {
        // Default is EnumMemberAttribute, so using EnumMember should not trigger diagnostic
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                [EnumMember(Value = "first")]
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithExplicitMetadataSourceMatchingAttributesShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            """
            [EnumExtensions(MetadataSource = MetadataSource.DisplayAttribute)]
            public enum TestEnum
            {
                [Display(Name = "First")]
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithNoMetadataAttributesShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithMetadataSourceNoneShouldNotHaveDiagnostics()
    {
        // When MetadataSource is None, attributes are ignored so no warning needed
        var test = GetTestCode(
            """
            [EnumExtensions(MetadataSource = MetadataSource.None)]
            public enum TestEnum
            {
                [Display(Name = "First")]
                First,
                [Description("Second")]
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithWrongMetadataAttributeShouldHaveDiagnostic()
    {
        // Default is EnumMemberAttribute, but we're using Display
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                [{|NEEG004:Display(Name = "First")|}]
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithMultipleWrongMetadataAttributesShouldHaveDiagnostics()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                [{|NEEG004:Display(Name = "First")|}]
                First,
                [{|NEEG004:Display(Name = "Second")|}]
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithMixedAttributesShouldNotHaveDiagnostic()
    {
        // Has both correct (EnumMember) and incorrect (Display) attributes
        // Should not report diagnostic because at least one correct attribute exists
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                [EnumMember(Value = "first")]
                First,
                [Display(Name = "Second")]
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithExplicitMetadataSourceAndWrongAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            """
            [EnumExtensions(MetadataSource = MetadataSource.DisplayAttribute)]
            public enum TestEnum
            {
                [{|NEEG004:EnumMember(Value = "first")|}]
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithDescriptionAttributeWhenExpectingDisplayShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            """
            [EnumExtensions(MetadataSource = MetadataSource.DisplayAttribute)]
            public enum TestEnum
            {
                [{|NEEG004:Description("First description")|}]
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithDisplayAttributeWhenExpectingDescriptionShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            """
            [EnumExtensions(MetadataSource = MetadataSource.DescriptionAttribute)]
            public enum TestEnum
            {
                [{|NEEG004:Display(Name = "First")|}]
                First,
                Second,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact(Skip = "Global metadata source tests need special test infrastructure")]
    public async Task EnumWithGlobalMetadataSourceShouldRespectIt()
    {
        var options = new System.Collections.Generic.Dictionary<string, string>
        {
            [$"build_property.{Constants.MetadataSourcePropertyName}"] = "DisplayAttribute"
        };

        var test = GetTestCodeWithOptions(
            options,
            """
            [EnumExtensions]
            public enum TestEnum
            {
                [{|NEEG004:EnumMember(Value = "first")|}]
                First,
                Second,
            }
            """);
        
        await test.RunAsync();
    }

    [Fact(Skip = "Global metadata source tests need special test infrastructure")]
    public async Task EnumWithGlobalMetadataSourceAndCorrectAttributeShouldNotHaveDiagnostic()
    {
        var options = new System.Collections.Generic.Dictionary<string, string>
        {
            [$"build_property.{Constants.MetadataSourcePropertyName}"] = "DisplayAttribute"
        };

        var test = GetTestCodeWithOptions(
            options,
            """
            [EnumExtensions]
            public enum TestEnum
            {
                [Display(Name = "First")]
                First,
                Second,
            }
            """);
        
        await test.RunAsync();
    }

    [Fact(Skip = "Global metadata source tests need special test infrastructure")]
    public async Task EnumWithExplicitMetadataSourceOverridesGlobal()
    {
        // Global is DisplayAttribute, but explicit is EnumMemberAttribute
        var options = new System.Collections.Generic.Dictionary<string, string>
        {
            [$"build_property.{Constants.MetadataSourcePropertyName}"] = "DisplayAttribute"
        };

        var test = GetTestCodeWithOptions(
            options,
            """
            [EnumExtensions(MetadataSource = MetadataSource.EnumMemberAttribute)]
            public enum TestEnum
            {
                [EnumMember(Value = "first")]
                First,
                Second,
            }
            """);
        
        await test.RunAsync();
    }

    private static string GetTestCode(string testFragment)
        => $$"""
             using System;
             using System.ComponentModel;
             using System.ComponentModel.DataAnnotations;
             using System.Runtime.Serialization;
             using NetEscapades.EnumGenerators;

             {{testFragment}}

             {{TestHelpers.LoadEmbeddedAttribute()}}
             {{TestHelpers.LoadEmbeddedMetadataSource()}}
             """;

    private static Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<IncorrectMetadataAttributeAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier> GetTestCodeWithOptions(
        System.Collections.Generic.Dictionary<string, string> options,
        string testFragment)
    {
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<IncorrectMetadataAttributeAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>
        {
            TestCode = GetTestCode(testFragment),
            SolutionTransforms =
            {
                (solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId)!.CompilationOptions;
                    compilationOptions = compilationOptions!.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(
                            System.Collections.Immutable.ImmutableDictionary<string, Microsoft.CodeAnalysis.ReportDiagnostic>.Empty));
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    var analyzerConfigDocumentId = Microsoft.CodeAnalysis.DocumentId.CreateNewId(projectId);
                    var text = "[*]\r\n";
                    foreach (var kvp in options)
                    {
                        text += $"{kvp.Key} = {kvp.Value}\r\n";
                    }
                    solution = solution.AddAnalyzerConfigDocument(
                        analyzerConfigDocumentId,
                        ".editorconfig",
                        Microsoft.CodeAnalysis.Text.SourceText.From(text),
                        filePath: "/.editorconfig");

                    return solution;
                }
            }
        };

        return test;
    }
}
