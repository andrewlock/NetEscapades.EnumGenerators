using System;
using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

public class StringBuilderAppendAnalyzerTests : AnalyzerTestsBase<StringBuilderAppendAnalyzer, StringBuilderAppendCodeFixProvider>
{
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
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnumWithoutAttribute.First;
                    sb.Append(value);
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
                    var sb = new System.Text.StringBuilder();
                    var value = 42;
                    sb.Append(value);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task StringAppendShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = "test";
                    sb.Append(value);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppendWithMultipleParametersShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append("test", 0, 2);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppendEnumWithToStringFastShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.Append(value.ToStringFast());
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppendEnumWithAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.Append({|NEEG012:value|});
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
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.Append(value.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task MultipleAppendCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.Append({|NEEG012:value1|});
                    sb.Append({|NEEG012:value2|});
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
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.Append(value1.ToStringFast());
                    sb.Append(value2.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendInExpressionShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    var result = sb.Append("Value: ").Append({|NEEG012:value|}).ToString();
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
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    var result = sb.Append("Value: ").Append(value.ToStringFast()).ToString();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnumWithoutAttribute.First;
                    sb.Append({|NEEG012:value1|});
                    sb.Append(value2); // Should not flag
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
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnumWithoutAttribute.First;
                    sb.Append(value1.ToStringFast());
                    sb.Append(value2); // Should not flag
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendDirectEnumValueShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append({|NEEG012:TestEnum.First|});
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
                    var sb = new System.Text.StringBuilder();
                    sb.Append(TestEnum.First.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = System.DateTimeKind.Local;
                    sb.Append({|NEEG012:value|});
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
                    var sb = new System.Text.StringBuilder();
                    var value = System.DateTimeKind.Local;
                    sb.Append(value.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendOnExternalEnumNotReferencedShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = System.IO.FileShare.Read;
                    sb.Append(value);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppendWithMethodCallReturningEnumShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append({|NEEG012:GetValue()|});
                }

                private TestEnum GetValue() => TestEnum.First;
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(GetValue().ToStringFast());
                }

                private TestEnum GetValue() => TestEnum.First;
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendWithPropertyReturningEnumShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private TestEnum MyProperty { get; set; } = TestEnum.First;

                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append({|NEEG012:MyProperty|});
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private TestEnum MyProperty { get; set; } = TestEnum.First;

                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(MyProperty.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task EnumInDifferentNamespaceAddsTheRequiredUsingDirectives()
    {
        var test = GetTestCodeWithDifferentNamespace(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.Append({|NEEG012:value1|});
                    sb.Append({|NEEG012:value2|});
                }
            }
            """);

        var fix = GetTestCodeWithDifferentNamespace(addUsing: true,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.Append(value1.ToStringFast());
                    sb.Append(value2.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task WhenUsageAnalyzersNotEnabled_AppendShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.Append(value);
                }
            }
            """);
        // Don't set the config option - analyzer should not run
        await VerifyAnalyzerAsync(test, EnableState.Missing);
    }

    [Fact]
    public async Task WhenUsageAnalyzersDisabled_AppendShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.Append(value);
                }
            }
            """);
        
        await VerifyAnalyzerAsync(test, EnableState.Disabled);
    }

    // AppendFormat tests
    [Fact]
    public async Task AppendFormatWithNonEnumShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendFormat("Value: {0}", 42);
                    sb.AppendFormat("Value: {0}", "test");
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppendFormatWithEnumWithoutAttributeShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnumWithoutAttribute.First;
                    sb.AppendFormat("Value: {0}", value);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AppendFormatWithSingleEnumShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.AppendFormat("Value: {0}", {|NEEG012:value|});
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
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.AppendFormat("Value: {0}", value.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendFormatWithMultipleEnumsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.AppendFormat("Value: {0} and {1}", {|NEEG012:value1|}, {|NEEG012:value2|});
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
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.AppendFormat("Value: {0} and {1}", value1.ToStringFast(), value2.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendFormatWithMixedArgumentsShouldOnlyFlagEnums()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.AppendFormat("Value: {0}, Number: {1}, Text: {2}", {|NEEG012:value|}, 42, "test");
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
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.AppendFormat("Value: {0}, Number: {1}, Text: {2}", value.ToStringFast(), 42, "test");
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendFormatWithProviderShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.AppendFormat(null, "Value: {0}", {|NEEG012:value|});
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
                    var sb = new System.Text.StringBuilder();
                    var value = TestEnum.First;
                    sb.AppendFormat(null, "Value: {0}", value.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendFormatWithProviderAndMultipleEnumsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0} - {1}", {|NEEG012:value1|}, {|NEEG012:value2|});
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
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0} - {1}", value1.ToStringFast(), value2.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendFormatWithDirectEnumValueShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendFormat("Value: {0}", {|NEEG012:TestEnum.First|});
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
                    var sb = new System.Text.StringBuilder();
                    sb.AppendFormat("Value: {0}", TestEnum.First.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendFormatWithExternalEnumShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value = System.DateTimeKind.Local;
                    sb.AppendFormat("Kind: {0}", {|NEEG012:value|});
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
                    var sb = new System.Text.StringBuilder();
                    var value = System.DateTimeKind.Local;
                    sb.AppendFormat("Kind: {0}", value.ToStringFast());
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task AppendFormatWithThreeArgumentsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.AppendFormat("{0}, {1}, {2}", {|NEEG012:value1|}, 42, {|NEEG012:value2|});
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
                    var sb = new System.Text.StringBuilder();
                    var value1 = TestEnum.First;
                    var value2 = TestEnum.Second;
                    sb.AppendFormat("{0}, {1}, {2}", value1.ToStringFast(), 42, value2.ToStringFast());
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
}
