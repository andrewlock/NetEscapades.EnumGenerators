using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.ToStringAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.ToStringCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class ToStringAnalyzerTests
{
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
        await Verifier.VerifyAnalyzerAsync(test);
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
        await Verifier.VerifyAnalyzerAsync(test);
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
        await Verifier.VerifyAnalyzerAsync(test);
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
        await Verifier.VerifyAnalyzerAsync(test);
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
        await Verifier.VerifyAnalyzerAsync(test);
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
        await Verifier.VerifyCodeFixAsync(test, fix);
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
        await Verifier.VerifyCodeFixAsync(test, fix);
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
        await Verifier.VerifyCodeFixAsync(test, fix);
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
        await Verifier.VerifyCodeFixAsync(test, fix);
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
        await Verifier.VerifyCodeFixAsync(test, fix);
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
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ToStringWithDifferentExtensionAttributeArguments()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions(ExtensionClassName = "MyExtensions")]
            public enum TestEnum1
            {
                First,
                Second,
            }

            [EnumExtensions(ExtensionClassNamespace = "MyNamespace")]
            public enum TestEnum3
            {
                First,
                Second,
            }

            // these are generated but it's fine
            public static class extensions
            {
                public static string ToStringFast(this TestEnum1 val) => "Test";
                public static string ToStringFast(this TestEnum3 val) => "Test";
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum1.First;
                    var str1 = value1.{|NEEG004:ToString|}();
                    
                    var value2 = TestEnum3.First;
                    var str2 = value2.{|NEEG004:ToString|}();
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions(ExtensionClassName = "MyExtensions")]
            public enum TestEnum1
            {
                First,
                Second,
            }

            [EnumExtensions(ExtensionClassNamespace = "MyNamespace")]
            public enum TestEnum3
            {
                First,
                Second,
            }
            
            // these are generated but it's fine
            public static class extensions
            {
                public static string ToStringFast(this TestEnum1 val) => "Test";
                public static string ToStringFast(this TestEnum3 val) => "Test";
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum1.First;
                    var str1 = value1.ToStringFast();
                    
                    var value2 = TestEnum3.First;
                    var str2 = value2.ToStringFast();
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

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

            [EnumExtensions]
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
            public static class TestExtensions
            {
                public static string ToStringFast(this TestEnum val)
                {
                    return "Test";
                }

                public static string ToStringFast(this TestEnum2 val)
                {
                    return "Test";
                }
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;
}
