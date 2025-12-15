using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    NetEscapades.EnumGenerators.Diagnostics.ToStringAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class ToStringAnalyzerTests
{
    private const string DiagnosticId = ToStringAnalyzer.DiagnosticId;

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
            public enum TestEnum
            {
                First,
                Second,
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
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
            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }

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

    [Fact]
    public async Task ToStringWithParametersShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.ToString("G");
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ToStringOnEnumWithAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = value.{|NEEG004:ToString|}();
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleToStringCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }

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
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ToStringInExpressionShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = TestEnum.First;
                    var str = "Value: " + value.{|NEEG004:ToString|}();
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ToStringOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions]
            public enum TestEnumWithAttribute
            {
                First,
                Second,
            }

            public enum TestEnumWithoutAttribute
            {
                First,
                Second,
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnumWithAttribute.First;
                    var str1 = value1.{|NEEG004:ToString|}();
                    
                    var value2 = TestEnumWithoutAttribute.First;
                    var str2 = value2.ToString(); // Should not flag
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ToStringOnEnumInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First,
                Second,
            }

            public class TestClass
            {
                public string TestMethod()
                {
                    var value = TestEnum.First;
                    return value.{|NEEG004:ToString|}();
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
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
            public enum TestEnum2
            {
                First,
                Second,
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = TestEnum1.First;
                    var str1 = value1.{|NEEG004:ToString|}();
                    
                    var value2 = TestEnum2.First;
                    var str2 = value2.{|NEEG004:ToString|}();
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
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
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;
}
