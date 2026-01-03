using Microsoft.CodeAnalysis.CSharp.Testing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.IsDefinedAnalyzer, 
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.IsDefinedCodeFixProvider, 
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.IsDefinedAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.IsDefinedCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class IsDefinedAnalyzerTests
{
    private const string EnableUsageAnalyzers = "netescapades_enumgenerators_usage_analyzers_enable = true";

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
                    var isDefined = Enum.IsDefined(typeof(TestEnumWithoutAttribute), value);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task IsDefinedOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var isDefined = {|NEEG006:Enum.IsDefined(typeof(MyEnum), value)|};
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
                    var value = MyEnum.First;
                    var isDefined = MyEnumExtensions.IsDefined(value);
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedGenericOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            using System;

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var isDefined = {|NEEG006:Enum.IsDefined<MyEnum>(value)|};
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            using System;

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var isDefined = MyEnumExtensions.IsDefined(value);
                }
            }
            """);

        // force using .NET 6+ runtime so that can test with generic
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task MultipleIsDefinedCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = MyEnum.First;
                    var isDefined1 = {|NEEG006:System.Enum.IsDefined(typeof(MyEnum), value1)|};
                    
                    var value2 = MyEnum.Second;
                    var isDefined2 = {|NEEG006:System.Enum.IsDefined(typeof(MyEnum), value2)|};
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
                    var value1 = MyEnum.First;
                    var isDefined1 = MyEnumExtensions.IsDefined(value1);
                    
                    var value2 = MyEnum.Second;
                    var isDefined2 = MyEnumExtensions.IsDefined(value2);
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedInIfStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    if ({|NEEG006:System.Enum.IsDefined(typeof(MyEnum), value)|})
                    {
                        System.Console.WriteLine("Is defined");
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
                    var value = MyEnum.First;
                    if (MyEnumExtensions.IsDefined(value))
                    {
                        System.Console.WriteLine("Is defined");
                    }
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = MyEnum.First;
                    var isDefined1 = {|NEEG006:System.Enum.IsDefined(typeof(MyEnum), value1)|};
                    
                    var value2 = TestEnumWithoutAttribute.First;
                    var isDefined2 = System.Enum.IsDefined(typeof(TestEnumWithoutAttribute), value2); // Should not flag
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
                    var value1 = MyEnum.First;
                    var isDefined1 = MyEnumExtensions.IsDefined(value1);
                    
                    var value2 = TestEnumWithoutAttribute.First;
                    var isDefined2 = System.Enum.IsDefined(typeof(TestEnumWithoutAttribute), value2); // Should not flag
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public bool TestMethod()
                {
                    var value = MyEnum.First;
                    return {|NEEG006:System.Enum.IsDefined(typeof(MyEnum), value)|};
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public bool TestMethod()
                {
                    var value = MyEnum.First;
                    return MyEnumExtensions.IsDefined(value);
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.IO.FileShare.Read;
                    var isDefined = {|NEEG006:System.Enum.IsDefined(typeof(System.IO.FileShare), value)|};
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
                    var value = System.IO.FileShare.Read;
                    var isDefined = FileShareExtensions.IsDefined(value);
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedGenericOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.IO.FileShare.Read;
                    var isDefined = {|NEEG006:System.Enum.IsDefined<System.IO.FileShare>(value)|};
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
                    var value = System.IO.FileShare.Read;
                    var isDefined = FileShareExtensions.IsDefined(value);
                }
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedOnNonExtensionExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var isDefined = System.Enum.IsDefined(typeof(System.DateTimeKind), value);
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task IsDefinedWithInlineEnumValuesShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var isDefined = {|NEEG006:System.Enum.IsDefined(typeof(MyEnum), MyEnum.First)|};
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
                    var isDefined = MyEnumExtensions.IsDefined(MyEnum.First);
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedWithCastValueShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var isDefined = {|NEEG006:System.Enum.IsDefined(typeof(MyEnum), (MyEnum)5)|};
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
                    var isDefined = MyEnumExtensions.IsDefined((MyEnum)5);
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedOnFieldShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private MyEnum _value = MyEnum.First;
                
                public void TestMethod()
                {
                    var isDefined = {|NEEG006:System.Enum.IsDefined(typeof(MyEnum), _value)|};
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private MyEnum _value = MyEnum.First;
                
                public void TestMethod()
                {
                    var isDefined = MyEnumExtensions.IsDefined(_value);
                }
            }
            """);
        await VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedOnMethodReturnValueShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var isDefined = {|NEEG006:Enum.IsDefined<MyEnum>(GetValue())|};
                }
                
                private MyEnum GetValue() => MyEnum.First;
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var isDefined = MyEnumExtensions.IsDefined(GetValue());
                }
                
                private MyEnum GetValue() => MyEnum.First;
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task IsDefinedWithEnumExtensionClassInOtherNamespaceAddsUsing()
    {
        var test = TestCode(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public bool TestMethod()
                {
                    var value = MyEnum.First;
                    var isDefined1 = {|NEEG006:Enum.IsDefined(typeof(MyEnum), GetValue())|};
                    var isDefined2 = {|NEEG006:Enum.IsDefined<MyEnum>(value)|};
                    return {|NEEG006:Enum.IsDefined(typeof(MyEnum), value)|};
                }

                public MyEnum GetValue() => MyEnum.First;
            }
            """);

        var fix = TestCode(addUsing: true,
            /* lang=c# */
            """
            public class TestClass
            {
                public bool TestMethod()
                {
                    var value = MyEnum.First;
                    var isDefined1 = MyTestExtensions.IsDefined(GetValue());
                    var isDefined2 = MyTestExtensions.IsDefined(value);
                    return MyTestExtensions.IsDefined(value);
                }

                public MyEnum GetValue() => MyEnum.First;
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
                      public static bool IsDefined(Some.Namespace.MyEnum value)
                      {
                          return value switch
                          {
                              Some.Namespace.MyEnum.First => true,
                              Some.Namespace.MyEnum.Second => true,
                              Some.Namespace.MyEnum.Third => true,
                              _ => false,
                          };
                      }
                  }
              }

              {{TestHelpers.LoadEmbeddedAttribute()}}
              {{TestHelpers.LoadEmbeddedMetadataSource()}}
              """;
    }

    private static Task VerifyCodeFixWithNet6AssembliesAsync(string source, string fixedSource)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
        };
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", $"""
            is_global = true
            {EnableUsageAnalyzers}
            """));

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
                public static bool IsDefined(System.IO.FileShare value)
                {
                    return value switch
                    {
                        System.IO.FileShare.Read => true,
                        System.IO.FileShare.Write => true,
                        _ => false,
                    };
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
                public static bool IsDefined(MyEnum value) => true;
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;


    [Fact]
    public async Task WhenUsageAnalyzersNotEnabled_IsDefinedShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var result = Enum.IsDefined(typeof(MyEnum), value);
                }
            }
            """);
        // Don't set the config option - analyzer should not run
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task WhenUsageAnalyzersDisabled_IsDefinedShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var result = Enum.IsDefined(typeof(MyEnum), value);
                }
            }
            """);
        
        var analyzerTest = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.IsDefinedAnalyzer, DefaultVerifier>
        {
            TestState = { Sources = { test } },
        };
        analyzerTest.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", """
            is_global = true
            netescapades_enumgenerators_usage_analyzers_enable = false
            """));
        await analyzerTest.RunAsync();
    }

    private static Task VerifyAnalyzerAsync(string source)
    {
        var test = new CSharpAnalyzerTest<Diagnostics.UsageAnalyzers.IsDefinedAnalyzer, DefaultVerifier>
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
        var test = new CSharpCodeFixTest<Diagnostics.UsageAnalyzers.IsDefinedAnalyzer, Diagnostics.UsageAnalyzers.IsDefinedCodeFixProvider, DefaultVerifier>
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