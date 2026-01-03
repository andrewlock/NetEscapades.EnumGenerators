using System;
using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

public class GetValuesAsUnderlyingTypeAnalyzerTests : AnalyzerTestsBase<GetValuesAsUnderlyingTypeAnalyzer, GetValuesAsUnderlyingTypeCodeFixProvider>
{
    [Fact]
    public async Task EmptySourceShouldNotHaveDiagnostics()
    {
        var test = string.Empty;
        await VerifyAnalyzerWithNet7AssembliesAsync(test);
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
                    var values = Enum.GetValuesAsUnderlyingType(typeof(TestEnumWithoutAttribute));
                }
            }
            """);
        await VerifyAnalyzerWithNet7AssembliesAsync(test);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG010:Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
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
                    var values = MyEnumExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeGenericOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG010:Enum.GetValuesAsUnderlyingType<MyEnum>()|};
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
                    var values = MyEnumExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);

        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task MultipleGetValuesAsUnderlyingTypeCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values1 = {|NEEG010:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
                    var values2 = {|NEEG010:System.Enum.GetValuesAsUnderlyingType<MyEnum>()|};
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
                    var values1 = MyEnumExtensions.GetValuesAsUnderlyingType();
                    var values2 = MyEnumExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeInVariableDeclarationShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG010:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
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
                    var values = MyEnumExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values1 = {|NEEG010:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
                    var values2 = System.Enum.GetValuesAsUnderlyingType(typeof(TestEnumWithoutAttribute)); // Should not flag
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
                    var values1 = MyEnumExtensions.GetValuesAsUnderlyingType();
                    var values2 = System.Enum.GetValuesAsUnderlyingType(typeof(TestEnumWithoutAttribute)); // Should not flag
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public System.Array TestMethod()
                {
                    return {|NEEG010:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public System.Array TestMethod()
                {
                    return MyEnumExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG010:System.Enum.GetValuesAsUnderlyingType(typeof(System.IO.FileShare))|};
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
                    var values = FileShareExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeGenericOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = {|NEEG010:System.Enum.GetValuesAsUnderlyingType<System.IO.FileShare>()|};
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
                    var values = FileShareExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeOnNonExtensionExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values = System.Enum.GetValuesAsUnderlyingType(typeof(System.DateTimeKind));
                }
            }
            """);
        await VerifyAnalyzerWithNet7AssembliesAsync(test);
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeWithEnumExtensionClassInOtherNamespaceAddsUsing()
    {
        var test = TestCode(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var values1 = {|NEEG010:Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
                    var values2 = {|NEEG010:Enum.GetValuesAsUnderlyingType<MyEnum>()|};
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
                    var values1 = MyTestExtensions.GetValuesAsUnderlyingType();
                    var values2 = MyTestExtensions.GetValuesAsUnderlyingType();
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);


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
                      public static int[] GetValuesAsUnderlyingType() => new[] { 1, 2, 3 };
                  }
              }

              {{TestHelpers.LoadEmbeddedAttribute()}}
              {{TestHelpers.LoadEmbeddedMetadataSource()}}
              """;
    }

    [Fact]
    public async Task GetValuesAsUnderlyingTypeInFieldInitializerShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private static readonly System.Array _values = {|NEEG010:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private static readonly System.Array _values = MyEnumExtensions.GetValuesAsUnderlyingType();
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }


    [Fact]
    public async Task WhenUsageAnalyzersNotEnabled_GetValuesAsUnderlyingTypeShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var values = Enum.GetValuesAsUnderlyingType(typeof(MyEnum));
                }
            }
            """);
        // Don't set the config option - analyzer should not run
        await VerifyAnalyzerWithNet7AssembliesAsync(test, EnableState.Missing);
    }

    [Fact]
    public async Task WhenUsageAnalyzersDisabled_GetValuesAsUnderlyingTypeShouldNotHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = MyEnum.First;
                    var values = Enum.GetValuesAsUnderlyingType(typeof(MyEnum));
                }
            }
            """);

        await VerifyAnalyzerWithNet7AssembliesAsync(test, EnableState.Disabled);
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
                public static int[] GetValuesAsUnderlyingType() => new[] { 0, 1, 2, 3, 4, 7, 16 };
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
                public static int[] GetValuesAsUnderlyingType() => new[] { 1, 2, 3 };
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;
}
