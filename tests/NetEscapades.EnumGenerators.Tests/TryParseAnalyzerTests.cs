using Microsoft.CodeAnalysis.CSharp.Testing;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.TryParseAnalyzer, 
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.TryParseCodeFixProvider, 
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.TryParseAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.TryParseCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class TryParseAnalyzerTests

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
                    Enum.TryParse<TestEnumWithoutAttribute>("First", out var result);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("\"First\"")]
    [InlineData("\"First\".AsSpan()")]
    public async Task TryParseOnEnumWithExtensionsShouldHaveDiagnostic(string parseValue)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var success = {|NEEG011:Enum.TryParse(typeof(MyEnum), {{parseValue}}, out var result)|};
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
                    var success = MyEnumExtensions.TryParse({{parseValue}}, out var result);
                }
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("ignoreCase: true")]
    [InlineData("ignoreCase: false")]
    public async Task TryParseWithIgnoreCaseOnEnumWithExtensionsShouldHaveDiagnostic(string ignoreCaseParam)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var success = {|NEEG011:Enum.TryParse(typeof(MyEnum), "First", {{ignoreCaseParam}}, out var result)|};
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
                    var success = MyEnumExtensions.TryParse("First", out var result, {{ignoreCaseParam}});
                }
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Theory]
    [InlineData("\"First\"")]
    [InlineData("\"First\".AsSpan()")]
    public async Task TryParseGenericOnEnumWithExtensionsShouldHaveDiagnostic(string parseValue)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            public class TestClass
            {
                public void TestMethod()
                {
                    var success = {|NEEG011:Enum.TryParse<MyEnum>({{parseValue}}, out var result)|};
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
                    var success = MyEnumExtensions.TryParse({{parseValue}}, out var result);
                }
            }
            """);

        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("ignoreCase: true")]
    [InlineData("ignoreCase: false")]
    public async Task TryParseGenericWithIgnoreCaseOnEnumWithExtensionsShouldHaveDiagnostic(string ignoreCaseParam)
    {
        var test = GetTestCode(
            /* lang=c# */
            $$"""
            using System;

            public class TestClass
            {
                public void TestMethod()
                {
                    var r = {|NEEG011:Enum.TryParse<MyEnum>("First", {{ignoreCaseParam}}, out var result)|};
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            $$"""
            using System;

            public class TestClass
            {
                public void TestMethod()
                {
                    var r = MyEnumExtensions.TryParse("First", out var result, {{ignoreCaseParam}});
                }
            }
            """);

        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task MultipleTryParseCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var r1 = {|NEEG011:System.Enum.TryParse<MyEnum>("First", out var result1)|};
                    var r2 = {|NEEG011:System.Enum.TryParse<MyEnum>("Second", true, out var result2)|};
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
                    var r1 = MyEnumExtensions.TryParse("First", out var result1);
                    var r2 = MyEnumExtensions.TryParse("Second", out var result2, true);
                }
            }
            """);
        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task TryParseInVariableDeclarationShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var success = {|NEEG011:System.Enum.TryParse("First", out MyEnum result)|};
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
                    var success = MyEnumExtensions.TryParse("First", out var result);
                }
            }
            """);
        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task TryParseOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var r = {|NEEG011:System.Enum.TryParse<MyEnum>("First", out var result1)|};
                    var r2 = System.Enum.TryParse<TestEnumWithoutAttribute>("First", out var result2); // Should not flag
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
                    var r = MyEnumExtensions.TryParse("First", out var result1);
                    var r2 = System.Enum.TryParse<TestEnumWithoutAttribute>("First", out var result2); // Should not flag
                }
            }
            """);
        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task TryParseInIfStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    if ({|NEEG011:System.Enum.TryParse("First", out MyEnum result)|})
                    {
                        // Use result
                    }
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
                    if (MyEnumExtensions.TryParse("First", out var result))
                    {
                        // Use result
                    }
                }
            }
            """);
        await VerifyCodeFix(
            test, fix);
    }

    [Fact]
    public async Task TryParseOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var r = {|NEEG011:System.Enum.TryParse<System.IO.FileShare>("Read", out var result)|};
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
                    var r = FileShareExtensions.TryParse("Read", out var result);
                }
            }
            """);
        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task TryParseGenericOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var r = {|NEEG011:System.Enum.TryParse<System.IO.FileShare>("Read", out var result)|};
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
                    var r = FileShareExtensions.TryParse("Read", out var result);
                }
            }
            """);
        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task TryParseOnNonExtensionExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    System.Enum.TryParse<System.DateTimeKind>("Local", out var result);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TryParseWithVariableNameShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    string name = "First";
                    var success = {|NEEG011:System.Enum.TryParse<MyEnum>(name, out var result)|};
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
                    string name = "First";
                    var success = MyEnumExtensions.TryParse(name, out var result);
                }
            }
            """);
        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task TryParseWithMethodCallShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var success = {|NEEG011:Enum.TryParse<MyEnum>(GetName(), out var result)|};
                }
                
                private string GetName() => "First";
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var success = MyEnumExtensions.TryParse(GetName(), out var result);
                }
                
                private string GetName() => "First";
            }
            """);
        await VerifyCodeFix(test, fix);
    }

    [Fact]
    public async Task TryParseWithEnumExtensionClassInOtherNamespaceAddsUsing()
    {
        var test = TestCode(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    ReadOnlySpan<char> value = "Second";
                    {
                        _ = {|NEEG011:Enum.TryParse<MyEnum>(GetName(), out var result1)|};
                        _ = {|NEEG011:Enum.TryParse<MyEnum>("Second", out var result2)|};
                        _ = {|NEEG011:Enum.TryParse<MyEnum>("Third", true, out var result3)|};
                        _ = {|NEEG011:Enum.TryParse<MyEnum>(GetName().AsSpan(), out var result4)|};
                        _ = {|NEEG011:Enum.TryParse<MyEnum>(value, out var result5)|};
                        _ = {|NEEG011:Enum.TryParse<MyEnum>("Third".AsSpan(), true, out var result6)|};
                    }
                    {
                        _ = {|NEEG011:Enum.TryParse(typeof(MyEnum), GetName(), out var result1)|};
                        _ = {|NEEG011:Enum.TryParse(typeof(MyEnum), "Second", out var result2)|};
                        _ = {|NEEG011:Enum.TryParse(typeof(MyEnum), "Third", true, out var result3)|};
                        _ = {|NEEG011:Enum.TryParse(typeof(MyEnum), GetName().AsSpan(), out var result4)|};
                        _ = {|NEEG011:Enum.TryParse(typeof(MyEnum), value, out var result5)|};
                        _ = {|NEEG011:Enum.TryParse(typeof(MyEnum), "Third".AsSpan(), true, out var result6)|};
                    }
                }

                public string GetName() => "First";
            }
            """);

        var fix = TestCode(addUsing: true,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    ReadOnlySpan<char> value = "Second";
                    {
                        _ = MyTestExtensions.TryParse(GetName(), out var result1);
                        _ = MyTestExtensions.TryParse("Second", out var result2);
                        _ = MyTestExtensions.TryParse("Third", out var result3, true);
                        _ = MyTestExtensions.TryParse(GetName().AsSpan(), out var result4);
                        _ = MyTestExtensions.TryParse(value, out var result5);
                        _ = MyTestExtensions.TryParse("Third".AsSpan(), out var result6, true);
                    }
                    {
                        _ = MyTestExtensions.TryParse(GetName(), out var result1);
                        _ = MyTestExtensions.TryParse("Second", out var result2);
                        _ = MyTestExtensions.TryParse("Third", out var result3, true);
                        _ = MyTestExtensions.TryParse(GetName().AsSpan(), out var result4);
                        _ = MyTestExtensions.TryParse(value, out var result5);
                        _ = MyTestExtensions.TryParse("Third".AsSpan(), out var result6, true);
                    }
                }

                public string GetName() => "First";
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);


        static string TestCode(bool addUsing, string testCode) =>
            $$"""
              using System;
              using System.Collections.Generic;
              using System.Linq;
              using System.Text;
              using System.Threading;
              using System.Threading.Tasks;
              using System.Diagnostics;
              using Some.Namespace;
              using NetEscapades.EnumGenerators;{{(addUsing ? Environment.NewLine + "using Some.Other.Namespace;" : "")}}

              namespace ConsoleApplication1
              {
                  {{testCode}}
              }

              namespace Some.Namespace
              {
                  [EnumExtensions(ExtensionClassNamespace = "Some.Other.Namespace", ExtensionClassName = "MyTestExtensions")]
                  public enum MyEnum
                  {
                      First = 1,
                      Second = 2,
                      Third = 3,
                  }
              }

              namespace Some.Other.Namespace
              {
                  // This code would be generated, just hacked in here for simplicity
                  public static class MyTestExtensions
                  {

                      public static bool TryParse(string name, out MyEnum value)
                      {
                          value = MyEnum.First;
                          return true;
                      }

                      public static bool TryParse(string name, out MyEnum value, bool ignoreCase)
                      {
                          value = MyEnum.First;
                          return true;
                      }

                      public static bool TryParse(ReadOnlySpan<char> name, out MyEnum value)
                      {
                          value = MyEnum.First;
                          return true;
                      }

                      public static bool TryParse(ReadOnlySpan<char> name, out MyEnum value, bool ignoreCase)
                      {
                          value = MyEnum.First;
                          return true;
                      }
                  }
              }

              {{TestHelpers.LoadEmbeddedAttribute()}}
              {{TestHelpers.LoadEmbeddedMetadataSource()}}
              """;
    }

    [Fact]
    public async Task TryParseInWhileLoopShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    while ({|NEEG011:System.Enum.TryParse("First", out MyEnum result)|})
                    {
                        // Use result
                    }
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
                    while (MyEnumExtensions.TryParse("First", out var result))
                    {
                        // Use result
                    }
                }
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    private static Task VerifyCodeFixWithNet6AssembliesAsync(string source, string fixedSource)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
        };

        return test.RunAsync(CancellationToken.None);
    }


    [Fact]
    public async Task WhenUsageAnalyzersNotEnabled_TryParseShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    Enum.TryParse("First", out MyEnum result);
                }
            }
            """);
        // Don't set the config option - analyzer should not run
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task WhenUsageAnalyzersDisabled_TryParseShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    Enum.TryParse("First", out MyEnum result);
                }
            }
            """);
        
        var analyzerTest = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.TryParseAnalyzer, DefaultVerifier>
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
        var test = new Test
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Default
#if NETFRAMEWORK || NETCOREAPP2_1
                .WithPackages([new PackageIdentity("System.Memory", "4.6.3")]),
#endif
        };

        return test.RunAsync(CancellationToken.None);
    }

    private static Task VerifyCodeFix(string source, string fixedSource)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Default
#if NETFRAMEWORK || NETCOREAPP2_1
                .WithPackages([new PackageIdentity("System.Memory", "4.6.3")]),
#endif
        };

        return test.RunAsync(CancellationToken.None);
    }

    private static string GetTestCodeWithExternalEnum(string testCode) => $$"""
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Diagnostics;
        using NetEscapades.EnumGenerators;

        [assembly: EnumExtensions<System.IO.FileShare>()]

        namespace ConsoleApplication1
        {
            {{testCode}}

        }
        
        namespace System.IO
        {
            // This code would be generated, just hacked in here for simplicity
            public static class FileShareExtensions
            {
                public static bool TryParse(string name, out System.IO.FileShare value)
                {
                    value = System.IO.FileShare.Read;
                    return true;
                }

                public static bool TryParse(string name, out System.IO.FileShare value, bool ignoreCase)
                {
                    value = System.IO.FileShare.Read;
                    return true;
                }

                public static bool TryParse(ReadOnlySpan<char> name, out System.IO.FileShare value)
                {
                    value = System.IO.FileShare.Read;
                    return true;
                }

                public static bool TryParse(ReadOnlySpan<char> name, out System.IO.FileShare value, bool ignoreCase)
                {
                    value = System.IO.FileShare.Read;
                    return true;
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
            public enum MyEnum
            {
                First = 1,
                Second = 2,
                Third = 3,
            }

            public enum TestEnumWithoutAttribute
            {
                First = 1,
                Second = 2,
            }

            // This code would be generated, just hacked in here for simplicity
            public static class MyEnumExtensions
            {
                public static bool TryParse(string name, out MyEnum value)
                {
                    value = MyEnum.First;
                    return true;
                }

                public static bool TryParse(string name, out MyEnum value, bool ignoreCase)
                {
                    value = MyEnum.First;
                    return true;
                }

                public static bool TryParse(ReadOnlySpan<char> name, out MyEnum value)
                {
                    value = MyEnum.First;
                    return true;
                }

                public static bool TryParse(ReadOnlySpan<char> name, out MyEnum value, bool ignoreCase)
                {
                    value = MyEnum.First;
                    return true;
                }
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;
}
