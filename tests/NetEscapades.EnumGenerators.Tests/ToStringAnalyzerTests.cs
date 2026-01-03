using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NetEscapades.EnumGenerators.Diagnostics;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.ToStringAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.ToStringCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class ToStringAnalyzerTests
{
    private const string EnableUsageAnalyzers = "netescapades_enumgenerators.usage_analyzers.enable = true";

    [Fact]
    public async Task EmptySourceShouldNotHaveDiagnostics()
    {
        var test = string.Empty;
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithoutAttributeShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnumWithoutAttribute.First;
                    var str = value.ToString();
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NonEnumTypeShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = 42;
                    var str = value.ToString();
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task OtherMethodsShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.GetHashCode().ToString();
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("\"val\"")]
    [InlineData("\"x\"")]
    [InlineData("\"X\"")]
    [InlineData("\"d\"")]
    [InlineData("\"D\"")]
    [InlineData("format: \"x\"")]
    [InlineData("format: \"X\"")]
    [InlineData("format: \"d\"")]
    [InlineData("format: \"D\"")]
    public async Task ToStringWithIncompatibleParametersShouldNotHaveDiagnostics(string param)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.ToString({{param}});
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("format")]
    [InlineData("format: format")]
    [InlineData("format: null")]
    public async Task ToStringWithDynamicParametersOrNullShouldNotHaveDiagnostics(string value)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var format = "g";
                    var str = value.ToString({{value}});
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"G\"")]
    [InlineData("\"g\"")]
    [InlineData("format: \"\"")]
    [InlineData("format: \"g\"")]
    [InlineData("format: \"G\"")]
    public async Task ToStringWithParametersShouldHaveDiagnostics(string param)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.{|NEEG004:ToString|}({{param}});
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringOnEnumWithAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.{|NEEG004:ToString|}();
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task MultipleToStringCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum.First;
                    var str1 = value1.{|NEEG004:ToString|}();
                    
                    var value2 = TestEnum.Second;
                    var str2 = value2.{|NEEG004:ToString|}();
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum.First;
                    var str1 = value1.ToStringFast();
                    
                    var value2 = TestEnum.Second;
                    var str2 = value2.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringInExpressionShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = "Value: " + value.{|NEEG004:ToString|}();
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = "Value: " + value.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum.First;
                    var str1 = value1.{|NEEG004:ToString|}();
                    
                    var value2 = TestEnumWithoutAttribute.First;
                    var str2 = value2.ToString(); // Should not flag
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum.First;
                    var str1 = value1.ToStringFast();
                    
                    var value2 = TestEnumWithoutAttribute.First;
                    var str2 = value2.ToString(); // Should not flag
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringOnEnumInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public string TestMethod()
                {
                    var value = TestEnum.First;
                    return value.{|NEEG004:ToString|}();
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public string TestMethod()
                {
                    var value = TestEnum.First;
                    return value.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = value.{|NEEG004:ToString|}();
                }
            }
            """);
        var fix = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = value.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringOnExternalEnumWithFormatShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = value.{|NEEG004:ToString|}("G");
                }
            }
            """);
        var fix = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = value.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringOnExternalEnumWithIncompatibleFormatShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = value.ToString("D");
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ToStringOnFileShareExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.IO.FileShare.Read;
                    var str = value.ToString();
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumInStringInterpolationShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """

                        public class TestClass
                        {
                            public void TestMethod()
                            {
                                var value = TestEnum.First;
                                var str = $"{{|NEEG004:value|}}";
                            }
                        }
                        
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """

                        public class TestClass
                        {
                            public void TestMethod()
                            {
                                var value = TestEnum.First;
                                var str = $"{value.ToStringFast()}";
                            }
                        }
                        
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task EnumInStringInterpolationWithMultipleExpressionsShouldHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    var str = $"Value1: {{|NEEG004:value1|}}, Value2: {{|NEEG004:value2|}}";
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    var str = $"Value1: {value1.ToStringFast()}, Value2: {value2.ToStringFast()}";
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Theory]
    [InlineData("g")]
    [InlineData("G")]
    public async Task EnumInStringInterpolationWithCompatibleFormatShouldHaveDiagnostic(string format)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = $"{{|NEEG004:value|}:{{{format}}}}";
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = $"{value.ToStringFast()}";
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Theory]
    [InlineData(",10", "")]
    [InlineData(",10", ":g")]
    [InlineData(",-10", ":G")]
    public async Task EnumInStringInterpolationWithAlignmentShouldPreserveAlignment(string alignment, string format)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = $"{{|NEEG004:value|}{{{alignment}}}{{{format}}}}";
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = $"{value.ToStringFast(){{alignment}}}";
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("X")]
    [InlineData("d")]
    [InlineData("D")]
    public async Task EnumInStringInterpolationWithIncompatibleFormatShouldNotHaveDiagnostic(string format)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = $"{value:{{format}}}";
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumDirectAccessInStringInterpolationShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var str = $"SomeValue: {{|NEEG004:TestEnum.First|}}";
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var str = $"SomeValue: {TestEnum.First.ToStringFast()}";
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task NonEnumInStringInterpolationShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = 42;
                    var str = $"{value}";
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithoutAttributeInStringInterpolationShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnumWithoutAttribute.First;
                    var str = $"{value}";
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ExternalEnumInStringInterpolationShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = $"{{|NEEG004:value|}}";
                }
            }
            """);

        var fix = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = $"{value.ToStringFast()}";
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ExternalEnumInStringInterpolationWithFormatShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = $"{{|NEEG004:value|}:g}";
                }
            }
            """);

        var fix = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var str = $"{value.ToStringFast()}";
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ExternalEnumInStringInterpolationWithMethodCall()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var str = $"{{|NEEG004:GetValue()|}:g}";
                }

                private System.DateTimeKind GetValue() => System.DateTimeKind.Local;
            }
            """);

        var fix = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var str = $"{GetValue().ToStringFast()}";
                }
            
                private System.DateTimeKind GetValue() => System.DateTimeKind.Local;
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task EnumInDifferentNamespaceAddsTheRequiredUsingDirectives()
    {
        var test = GetTestCodeWithDifferentNamespace(addUsing: false,
            /* lang=c# */
            $$$"""
               public class TestClass
               {
                   public string TestMethod()
                   {
                       var value1 = TestEnum.First;
                       var value2 = TestEnum.Second;
                       var str1 = $"Value1: {{|NEEG004:value1|}}, Value2: {{|NEEG004:value2|}}";
                       var str2 = $"SomeValue: {{|NEEG004:TestEnum.First|}}";
                       var str3 = value2.{|NEEG004:ToString|}("g");
                       return value1.{|NEEG004:ToString|}();
                   }
               }
               """);

        var fix = GetTestCodeWithDifferentNamespace(addUsing: true,
            /* lang=c# */
            """
            public class TestClass
            {
                public string TestMethod()
                {
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    var str1 = $"Value1: {value1.ToStringFast()}, Value2: {value2.ToStringFast()}";
                    var str2 = $"SomeValue: {TestEnum.First.ToStringFast()}";
                    var str3 = value2.ToStringFast();
                    return value1.ToStringFast();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }
    
    private static string GetTestCodeWithExternalEnum(string testCode) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Diagnostics;
        using NetEscapades.EnumGenerators;

        [assembly: EnumExtensions<System.DateTimeKind>()]

        namespace ConsoleApplication1
        {
            {{testCode}}
        }

        namespace System
        {
            // This code would be generated, just hacked in here for simplicity
            public static class DateTimeKindExtensions
            {
                public static string ToStringFast(this System.DateTimeKind val)
                {
                    return "Test";
                }
            }
        }
        
        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;

    private static string GetTestCode(string testCode) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Diagnostics;
        using NetEscapades.EnumGenerators;

        namespace ConsoleApplication1
        {
            {{testCode}}

            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }

            [EnumExtensions(ExtensionClassNamespace = "Some.Other.Namespace", ExtensionClassName = "MyTestExtensions")]
            public enum TestEnum2
            {
                First,
                Second,
            }

            public enum TestEnumWithoutAttribute
            {
                First,
                Second,
            }

            // This code would be generated, just hacked in here for simplicity
            public static class TestEnumExtensions
            {
                public static string ToStringFast(this TestEnum val)
                {
                    return "Test";
                }
            }
        }

        namespace Some.Other.Namespace
        {
            // This code would be generated, just hacked in here for simplicity
            public static class MyTestExtensions
            {
                public static string ToStringFast(this ConsoleApplication1.TestEnum2 val)
                {
                    return "Test";
                }
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;

    private static string GetTestCodeWithDifferentNamespace(bool addUsing, string testCode) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Diagnostics;
        using NetEscapades.EnumGenerators;{{(addUsing ? Environment.NewLine + "using Some.Other.Namespace;" : "")}}

        namespace ConsoleApplication1
        {
            {{testCode}}

            [EnumExtensions(ExtensionClassNamespace = "Some.Other.Namespace", ExtensionClassName = "MyTestExtensions")]
            public enum TestEnum
            {
                First,
                Second,
            }
        }

        namespace Some.Other.Namespace
        {
            // This code would be generated, just hacked in here for simplicity
            public static class MyTestExtensions
            {
                public static string ToStringFast(this ConsoleApplication1.TestEnum val)
                {
                    return "Test";
                }
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;

    [Fact]
    public async Task WhenUsageAnalyzersNotEnabled_ToStringShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.ToString();
                }
            }
            """);
        // Don't set the config option - analyzer should not run
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task WhenUsageAnalyzersDisabled_ToStringShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.ToString();
                }
            }
            """);
        
        var analyzerTest = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.ToStringAnalyzer, DefaultVerifier>
        {
            TestState = { Sources = { test } },
        };
        analyzerTest.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", """
            is_global = true
            netescapades_enumgenerators.usage_analyzers.enable = false
            """));
        await analyzerTest.RunAsync();
    }

    private static Task VerifyAnalyzerAsync(string source)
    {
        var test = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.ToStringAnalyzer, DefaultVerifier>
        {
            TestState = { Sources = { source } },
        };
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", $"""
            is_global = true
            {EnableUsageAnalyzers}
            """));
        return test.RunAsync();
    }

    private static Task VerifyCodeFixAsync(string source, string fixedSource)
    {
        var test = new CSharpCodeFixTest<Diagnostics.UsageAnalyzers.ToStringAnalyzer, Diagnostics.UsageAnalyzers.ToStringCodeFixProvider, DefaultVerifier>
        {
            TestState = { Sources = { source } },
            FixedState = { Sources = { fixedSource } },
        };
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", $"""
            is_global = true
            {EnableUsageAnalyzers}
            """));
        return test.RunAsync();
    }
}
