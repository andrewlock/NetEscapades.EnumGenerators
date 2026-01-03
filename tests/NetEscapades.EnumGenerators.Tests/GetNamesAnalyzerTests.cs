using Microsoft.CodeAnalysis.CSharp.Testing;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetNamesAnalyzer, 
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetNamesCodeFixProvider, 
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetNamesAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetNamesCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class GetNamesAnalyzerTests

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
                    var value = Enum.GetNames(typeof(TestEnumWithoutAttribute));
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task GetNamesOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG008:Enum.GetNames(typeof(MyEnum))|};
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
                    var value = MyEnumExtensions.GetNames();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetNamesGenericOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG008:Enum.GetNames<MyEnum>()|};
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
                    var value = MyEnumExtensions.GetNames();
                }
            }
            """);

        // force using .NET 6+ runtime so that can test with generic
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task MultipleGetNamesCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = {|NEEG008:System.Enum.GetNames(typeof(MyEnum))|};
                    var value2 = {|NEEG008:System.Enum.GetNames(typeof(MyEnum))|};
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
                    var value1 = MyEnumExtensions.GetNames();
                    var value2 = MyEnumExtensions.GetNames();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetNamesInVariableDeclarationShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG008:System.Enum.GetNames(typeof(MyEnum))|};
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
                    var value = MyEnumExtensions.GetNames();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetNamesOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = {|NEEG008:System.Enum.GetNames(typeof(MyEnum))|};
                    var value2 = System.Enum.GetNames(typeof(TestEnumWithoutAttribute)); // Should not flag
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
                    var value1 = MyEnumExtensions.GetNames();
                    var value2 = System.Enum.GetNames(typeof(TestEnumWithoutAttribute)); // Should not flag
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetNamesInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public string[] TestMethod()
                {
                    return {|NEEG008:System.Enum.GetNames(typeof(MyEnum))|};
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public string[] TestMethod()
                {
                    return MyEnumExtensions.GetNames();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetNamesOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG008:System.Enum.GetNames(typeof(System.IO.FileShare))|};
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
                    var value = FileShareExtensions.GetNames();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetNamesGenericOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG008:System.Enum.GetNames<System.IO.FileShare>()|};
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
                    var value = FileShareExtensions.GetNames();
                }
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetNamesOnNonExtensionExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.Enum.GetNames(typeof(System.DateTimeKind));
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task GetNamesWithEnumExtensionClassInOtherNamespaceAddsUsing()
    {
        var test = TestCode(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = {|NEEG008:Enum.GetNames(typeof(MyEnum))|};
                    var value2 = {|NEEG008:Enum.GetNames<MyEnum>()|};
                }
            }
            """);

        var fix = TestCode(addUsing: true,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = MyTestExtensions.GetNames();
                    var value2 = MyTestExtensions.GetNames();
                }
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
                      public static string[] GetNames() => new[] { "First", "Second", "Third" };
                  }
              }

              {{TestHelpers.LoadEmbeddedAttribute()}}
              {{TestHelpers.LoadEmbeddedMetadataSource()}}
              """;
    }

    [Fact]
    public async Task GetNamesInForeachLoopShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    foreach (var name in {|NEEG008:Enum.GetNames(typeof(MyEnum))|})
                    {
                        System.Console.WriteLine(name);
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
                    foreach (var name in MyEnumExtensions.GetNames())
                    {
                        System.Console.WriteLine(name);
                    }
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
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

    private static string GetTestCodeWithExternalEnum(string testCode) =>
        $$"""
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
                  public static string[] GetNames() => new[] { "Read", "Write" };
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
                public static string[] GetNames() => new[] { "First", "Second", "Third" };
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;


    [Fact]
    public async Task WhenUsageAnalyzersNotEnabled_GetNamesShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var names = Enum.GetNames(typeof(MyEnum));
                }
            }
            """);
        // Don't set the config option - analyzer should not run
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task WhenUsageAnalyzersDisabled_GetNamesShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var names = Enum.GetNames(typeof(MyEnum));
                }
            }
            """);
        
        var analyzerTest = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.GetNamesAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.GetNamesAnalyzer, DefaultVerifier>
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
        var test = new CSharpCodeFixTest<Diagnostics.UsageAnalyzers.GetNamesAnalyzer, Diagnostics.UsageAnalyzers.GetNamesCodeFixProvider, DefaultVerifier>
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