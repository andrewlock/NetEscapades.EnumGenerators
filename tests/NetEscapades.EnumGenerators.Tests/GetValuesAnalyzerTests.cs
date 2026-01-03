using Microsoft.CodeAnalysis.CSharp.Testing;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesAnalyzer, 
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesCodeFixProvider, 
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class GetValuesAnalyzerTests

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
                    var values = Enum.GetValues(typeof(TestEnumWithoutAttribute));
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task GetValuesOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG009:Enum.GetValues(typeof(MyEnum))|};
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
                    var values = MyEnumExtensions.GetValues();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesGenericOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG009:Enum.GetValues<MyEnum>()|};
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
                    var values = MyEnumExtensions.GetValues();
                }
            }
            """);

        // force using .NET 5+ runtime so that can test with generic
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task MultipleGetValuesCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values1 = {|NEEG009:System.Enum.GetValues(typeof(MyEnum))|};
                    var values2 = {|NEEG009:System.Enum.GetValues(typeof(MyEnum))|};
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
                    var values1 = MyEnumExtensions.GetValues();
                    var values2 = MyEnumExtensions.GetValues();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesInVariableDeclarationShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG009:System.Enum.GetValues(typeof(MyEnum))|};
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
                    var values = MyEnumExtensions.GetValues();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values1 = {|NEEG009:System.Enum.GetValues(typeof(MyEnum))|};
                    var values2 = System.Enum.GetValues(typeof(TestEnumWithoutAttribute)); // Should not flag
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
                    var values1 = MyEnumExtensions.GetValues();
                    var values2 = System.Enum.GetValues(typeof(TestEnumWithoutAttribute)); // Should not flag
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public MyEnum[] TestMethod()
                {
                    return (MyEnum[]){|NEEG009:System.Enum.GetValues(typeof(MyEnum))|};
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public MyEnum[] TestMethod()
                {
                    return (MyEnum[])MyEnumExtensions.GetValues();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG009:System.Enum.GetValues(typeof(System.IO.FileShare))|};
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
                    var values = FileShareExtensions.GetValues();
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesGenericOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG009:System.Enum.GetValues<System.IO.FileShare>()|};
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
                    var values = FileShareExtensions.GetValues();
                }
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesOnNonExtensionExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = System.Enum.GetValues(typeof(System.DateTimeKind));
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task GetValuesWithEnumExtensionClassInOtherNamespaceAddsUsing()
    {
        var test = TestCode(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values1 = {|NEEG009:Enum.GetValues(typeof(MyEnum))|};
                    var values2 = {|NEEG009:Enum.GetValues<MyEnum>()|};
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
                    var values1 = MyTestExtensions.GetValues();
                    var values2 = MyTestExtensions.GetValues();
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
                      public static MyEnum[] GetValues() => new[] { MyEnum.First, MyEnum.Second, MyEnum.Third };
                  }
              }

              {{TestHelpers.LoadEmbeddedAttribute()}}
              {{TestHelpers.LoadEmbeddedMetadataSource()}}
              """;
    }

    [Fact]
    public async Task GetValuesInForeachLoopShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    foreach (var value in {|NEEG009:Enum.GetValues(typeof(MyEnum))|})
                    {
                        System.Console.WriteLine(value);
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
                    foreach (var value in MyEnumExtensions.GetValues())
                    {
                        System.Console.WriteLine(value);
                    }
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesGenericInForeachLoopShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    foreach (var value in {|NEEG009:Enum.GetValues<MyEnum>()|})
                    {
                        System.Console.WriteLine(value);
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
                    foreach (var value in MyEnumExtensions.GetValues())
                    {
                        System.Console.WriteLine(value);
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

    private static string GetTestCodeWithExternalEnum(string testCode)
        => $$"""
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
                     public static System.IO.FileShare[] GetValues() => new[] { System.IO.FileShare.Read };
                 }
             }

             {{TestHelpers.LoadEmbeddedAttribute()}}
             {{TestHelpers.LoadEmbeddedMetadataSource()}}
             """;

    private static string GetTestCode(string testCode)
        => $$"""
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
                     public static MyEnum[] GetValues() => new[] { MyEnum.First, MyEnum.Second, MyEnum.Third };
                 }
             }

             {{TestHelpers.LoadEmbeddedAttribute()}}
             {{TestHelpers.LoadEmbeddedMetadataSource()}}
             """;


    [Fact]
    public async Task WhenUsageAnalyzersNotEnabled_GetValuesShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var values = Enum.GetValues(typeof(MyEnum));
                }
            }
            """);
        // Don't set the config option - analyzer should not run
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task WhenUsageAnalyzersDisabled_GetValuesShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var values = Enum.GetValues(typeof(MyEnum));
                }
            }
            """);
        
        var analyzerTest = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.GetValuesAnalyzer, DefaultVerifier>
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
        var test = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.GetValuesAnalyzer, DefaultVerifier>
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
        var test = new CSharpCodeFixTest<Diagnostics.UsageAnalyzers.GetValuesAnalyzer, Diagnostics.UsageAnalyzers.GetValuesCodeFixProvider, DefaultVerifier>
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