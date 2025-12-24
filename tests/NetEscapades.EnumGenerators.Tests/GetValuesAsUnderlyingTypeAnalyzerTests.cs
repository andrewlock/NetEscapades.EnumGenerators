using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesAsUnderlyingTypeAnalyzer, 
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesAsUnderlyingTypeCodeFixProvider, 
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesAsUnderlyingTypeAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers.GetValuesAsUnderlyingTypeCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class GetValuesAsUnderlyingTypeAnalyzerTests
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
                    var values = Enum.GetValuesAsUnderlyingType(typeof(TestEnumWithoutAttribute));
                }
            }
            """);
        await VerifyAnalyzerAsync(test);
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
                    var values = {|NEEG008:Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
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
                    var values = {|NEEG008:Enum.GetValuesAsUnderlyingType<MyEnum>()|};
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

        // force using .NET 7+ runtime so that can test with generic
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
                    var values1 = {|NEEG008:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
                    var values2 = {|NEEG008:System.Enum.GetValuesAsUnderlyingType<MyEnum>()|};
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
                    var values = {|NEEG008:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
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
                    var values1 = {|NEEG008:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
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
                    return {|NEEG008:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
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
                    var values = {|NEEG008:System.Enum.GetValuesAsUnderlyingType(typeof(System.IO.FileShare))|};
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
                    var values = {|NEEG008:System.Enum.GetValuesAsUnderlyingType<System.IO.FileShare>()|};
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
        await VerifyAnalyzerAsync(test);
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
                    var values1 = {|NEEG008:Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
                    var values2 = {|NEEG008:Enum.GetValuesAsUnderlyingType<MyEnum>()|};
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
                private static readonly System.Array _values = {|NEEG008:System.Enum.GetValuesAsUnderlyingType(typeof(MyEnum))|};
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
    public async Task GetValuesAsUnderlyingTypeInLinqQueryShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var count = {|NEEG008:Enum.GetValuesAsUnderlyingType<MyEnum>()|}.Length;
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
                    var count = MyEnumExtensions.GetValuesAsUnderlyingType().Length;
                }
            }
            """);
        await VerifyCodeFixWithNet7AssembliesAsync(test, fix);
    }

    private static Task VerifyCodeFixWithNet7AssembliesAsync(string source, string fixedSource)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
        };

        return test.RunAsync(CancellationToken.None);
    }

    private static Task VerifyAnalyzerAsync(string source)
    {
        var test = new Test
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
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
