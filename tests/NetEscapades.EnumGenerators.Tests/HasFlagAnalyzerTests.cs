using System;
using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.HasFlagAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.HasFlagCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class HasFlagAnalyzerTests
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
                    var flag = TestEnumWithoutAttribute.Second;
                    var hasFlag = value.HasFlag(flag);
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task HasFlagOnEnumWithAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    var hasFlag = value.{|NEEG005:HasFlag|}(flag);
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
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    var hasFlag = value.HasFlagFast(flag);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact(Skip = "Doesn't yet pass")]
    public async Task HasFlagOnEnumWithExtensionClassInOtherNamespaceAddsUsing()
    {
        var test = TestCode(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    var hasFlag = value.{|NEEG005:HasFlag|}(flag);
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
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    var hasFlag = value.HasFlagFast(flag);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);


        static string TestCode(bool addUsing, string testCode) =>
            $$"""
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
                  using Some.Namespace;

                  {{testCode}}
              }

              namespace Some.Namespace
              {
                  [EnumExtensions(ExtensionClassNamespace = "Some.Other.Namespace")]
                  [System.Flags]
                  public enum FlagsEnum
                  {
                      First = 1,
                      Second = 2,
                      Third = 4,
                  }
              }

              namespace Some.Other.Namespace
              {
                  // This code would be generated, just hacked in here for simplicity
                  public static class FlagsEnumExtensions
                  {
                      public static bool HasFlagFast(this Some.Namespace.FlagsEnum val, Some.Namespace.FlagsEnum flag)
                      {
                          return (val & flag) == flag;
                      }
                  }
              }

              {{TestHelpers.LoadEmbeddedAttribute()}}
              {{TestHelpers.LoadEmbeddedMetadataSource()}}
              """;
    }

    [Fact]
    public async Task MultipleHasFlagCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = FlagsEnum.First;
                    var flag1 = FlagsEnum.Second;
                    var hasFlag1 = value1.{|NEEG005:HasFlag|}(flag1);
                    
                    var value2 = FlagsEnum.Third;
                    var flag2 = FlagsEnum.Second;
                    var hasFlag2 = value2.{|NEEG005:HasFlag|}(flag2);
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
                    var value1 = FlagsEnum.First;
                    var flag1 = FlagsEnum.Second;
                    var hasFlag1 = value1.HasFlagFast(flag1);
                    
                    var value2 = FlagsEnum.Third;
                    var flag2 = FlagsEnum.Second;
                    var hasFlag2 = value2.HasFlagFast(flag2);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagInExpressionShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    if (value.{|NEEG005:HasFlag|}(flag))
                    {
                        System.Console.WriteLine("Has flag");
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
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    if (value.HasFlagFast(flag))
                    {
                        System.Console.WriteLine("Has flag");
                    }
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = FlagsEnum.First;
                    var flag1 = FlagsEnum.Second;
                    var hasFlag1 = value1.{|NEEG005:HasFlag|}(flag1);
                    
                    var value2 = TestEnumWithoutAttribute.First;
                    var flag2 = TestEnumWithoutAttribute.Second;
                    var hasFlag2 = value2.HasFlag(flag2); // Should not flag
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
                    var value1 = FlagsEnum.First;
                    var flag1 = FlagsEnum.Second;
                    var hasFlag1 = value1.HasFlagFast(flag1);
                    
                    var value2 = TestEnumWithoutAttribute.First;
                    var flag2 = TestEnumWithoutAttribute.Second;
                    var hasFlag2 = value2.HasFlag(flag2); // Should not flag
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagOnEnumInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public bool TestMethod()
                {
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    return value.{|NEEG005:HasFlag|}(flag);
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
                    var value = FlagsEnum.First;
                    var flag = FlagsEnum.Second;
                    return value.HasFlagFast(flag);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagWithDifferentExtensionAttributeArguments()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions(ExtensionClassName = "MyExtensions")]
            public enum FlagsEnum1
            {
                First,
                Second,
            }

            [EnumExtensions(ExtensionClassNamespace = "MyNamespace")]
            public enum FlagsEnum2
            {
                First,
                Second,
            }

            // these are generated but it's fine
            public static class extensions
            {
                public static bool HasFlagFast(this FlagsEnum1 val, FlagsEnum1 flag) => true;
                public static bool HasFlagFast(this FlagsEnum2 val, FlagsEnum2 flag) => true;
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = FlagsEnum1.First;
                    var flag1 = FlagsEnum1.Second;
                    var hasFlag1 = value1.{|NEEG005:HasFlag|}(flag1);
                    
                    var value2 = FlagsEnum2.First;
                    var flag2 = FlagsEnum2.Second;
                    var hasFlag2 = value2.{|NEEG005:HasFlag|}(flag2);
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            [EnumExtensions(ExtensionClassName = "MyExtensions")]
            public enum FlagsEnum1
            {
                First,
                Second,
            }

            [EnumExtensions(ExtensionClassNamespace = "MyNamespace")]
            public enum FlagsEnum2
            {
                First,
                Second,
            }

            // these are generated but it's fine
            public static class extensions
            {
                public static bool HasFlagFast(this FlagsEnum1 val, FlagsEnum1 flag) => true;
                public static bool HasFlagFast(this FlagsEnum2 val, FlagsEnum2 flag) => true;
            }

            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = FlagsEnum1.First;
                    var flag1 = FlagsEnum1.Second;
                    var hasFlag1 = value1.HasFlagFast(flag1);
                    
                    var value2 = FlagsEnum2.First;
                    var flag2 = FlagsEnum2.Second;
                    var hasFlag2 = value2.HasFlagFast(flag2);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.IO.FileShare.Read;
                    var flag = System.IO.FileShare.Write;
                    var hasFlag = value.{|NEEG005:HasFlag|}(flag);
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
                    var flag = System.IO.FileShare.Write;
                    var hasFlag = value.HasFlagFast(flag);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagOnNonExtensionExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.DateTimeKind.Local;
                    var flag = System.DateTimeKind.Utc;
                    var hasFlag = value.HasFlag(flag);
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task HasFlagWithInlineEnumValuesShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var hasFlag = FlagsEnum.First.{|NEEG005:HasFlag|}(FlagsEnum.Second);
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
                    var hasFlag = FlagsEnum.First.HasFlagFast(FlagsEnum.Second);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagWithCombinedFlagsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = FlagsEnum.First | FlagsEnum.Second;
                    var flag = FlagsEnum.Second;
                    var hasFlag = value.{|NEEG005:HasFlag|}(flag);
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
                    var value = FlagsEnum.First | FlagsEnum.Second;
                    var flag = FlagsEnum.Second;
                    var hasFlag = value.HasFlagFast(flag);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagInComplexBooleanExpression()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = FlagsEnum.First;
                    var flag1 = FlagsEnum.Second;
                    var flag2 = FlagsEnum.Third;
                    var result = value.{|NEEG005:HasFlag|}(flag1) && value.{|NEEG005:HasFlag|}(flag2);
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
                    var value = FlagsEnum.First;
                    var flag1 = FlagsEnum.Second;
                    var flag2 = FlagsEnum.Third;
                    var result = value.HasFlagFast(flag1) && value.HasFlagFast(flag2);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagOnFieldShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private FlagsEnum _value = FlagsEnum.First;
                
                public void TestMethod()
                {
                    var flag = FlagsEnum.Second;
                    var hasFlag = _value.{|NEEG005:HasFlag|}(flag);
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                private FlagsEnum _value = FlagsEnum.First;
                
                public void TestMethod()
                {
                    var flag = FlagsEnum.Second;
                    var hasFlag = _value.HasFlagFast(flag);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagOnPropertyShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public FlagsEnum Value { get; set; } = FlagsEnum.First;
                
                public void TestMethod()
                {
                    var flag = FlagsEnum.Second;
                    var hasFlag = Value.{|NEEG005:HasFlag|}(flag);
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public FlagsEnum Value { get; set; } = FlagsEnum.First;
                
                public void TestMethod()
                {
                    var flag = FlagsEnum.Second;
                    var hasFlag = Value.HasFlagFast(flag);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task HasFlagOnMethodReturnValueShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var flag = FlagsEnum.Second;
                    var hasFlag = GetValue().{|NEEG005:HasFlag|}(flag);
                }
                
                private FlagsEnum GetValue() => FlagsEnum.First;
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var flag = FlagsEnum.Second;
                    var hasFlag = GetValue().HasFlagFast(flag);
                }
                
                private FlagsEnum GetValue() => FlagsEnum.First;
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
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

        [assembly: EnumExtensions<System.IO.FileShare>()]

        namespace ConsoleApplication1
        {
            {{testCode}}

            // This code would be generated, just hacked in here for simplicity
            public static class ExternalEnumExtensions
            {
                public static bool HasFlagFast(this System.IO.FileShare val, System.IO.FileShare flag)
                {
                    return (val & flag) == flag;
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
            [System.Flags]
            public enum FlagsEnum
            {
                First = 1,
                Second = 2,
                Third = 4,
            }

            public enum TestEnumWithoutAttribute
            {
                First = 1,
                Second = 2,
            }

            // This code would be generated, just hacked in here for simplicity
            public static class FlagsExtensions
            {
                public static bool HasFlagFast(this FlagsEnum val, FlagsEnum flag)
                {
                    return (val & flag) == flag;
                }
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;
}
