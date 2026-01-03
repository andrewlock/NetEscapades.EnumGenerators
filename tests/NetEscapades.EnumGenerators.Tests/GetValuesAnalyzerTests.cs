using System;
using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

public class GetValuesAnalyzerTests : AnalyzerTestsBase<GetValuesAnalyzer, GetValuesCodeFixProvider>
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
        await VerifyAnalyzerAsync(test, EnableState.Missing);
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
        await VerifyAnalyzerAsync(test, EnableState.Disabled);
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
}
