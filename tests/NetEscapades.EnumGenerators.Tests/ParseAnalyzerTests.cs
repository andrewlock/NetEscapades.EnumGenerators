using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    NetEscapades.EnumGenerators.Diagnostics.ParseAnalyzer, 
    NetEscapades.EnumGenerators.Diagnostics.ParseCodeFixProvider, 
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    NetEscapades.EnumGenerators.Diagnostics.ParseAnalyzer,
    NetEscapades.EnumGenerators.Diagnostics.ParseCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class ParseAnalyzerTests
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
                    var value = Enum.Parse(typeof(TestEnumWithoutAttribute), "First");
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ParseOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:Enum.Parse(typeof(MyEnum), "First")|};
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
                    var value = MyEnumExtensions.Parse("First");
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseWithIgnoreCaseOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:Enum.Parse(typeof(MyEnum), "First", true)|};
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
                    var value = MyEnumExtensions.Parse("First", true);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseWithIgnoreCaseFalseOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:Enum.Parse(typeof(MyEnum), "First", false)|};
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
                    var value = MyEnumExtensions.Parse("First", false);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseGenericOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            using System;

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:Enum.Parse<MyEnum>("First")|};
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
                    var value = MyEnumExtensions.Parse("First");
                }
            }
            """);

        // force using .NET 6+ runtime so that can test with generic
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task ParseGenericWithIgnoreCaseOnEnumWithExtensionsShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            using System;

            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:Enum.Parse<MyEnum>("First", true)|};
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
                    var value = MyEnumExtensions.Parse("First", true);
                }
            }
            """);

        // force using .NET 6+ runtime so that can test with generic
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task MultipleParseCallsShouldHaveMultipleDiagnostics()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = {|NEEG007:System.Enum.Parse(typeof(MyEnum), "First")|};
                    var value2 = {|NEEG007:System.Enum.Parse(typeof(MyEnum), "Second", true)|};
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
                    var value1 = MyEnumExtensions.Parse("First");
                    var value2 = MyEnumExtensions.Parse("Second", true);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseInVariableDeclarationShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:System.Enum.Parse(typeof(MyEnum), "First")|};
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
                    var value = MyEnumExtensions.Parse("First");
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseOnDifferentEnumsShouldOnlyFlagThoseWithAttribute()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = {|NEEG007:System.Enum.Parse(typeof(MyEnum), "First")|};
                    var value2 = System.Enum.Parse(typeof(TestEnumWithoutAttribute), "First"); // Should not flag
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
                    var value1 = MyEnumExtensions.Parse("First");
                    var value2 = System.Enum.Parse(typeof(TestEnumWithoutAttribute), "First"); // Should not flag
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseInReturnStatementShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public MyEnum TestMethod()
                {
                    return (MyEnum){|NEEG007:System.Enum.Parse(typeof(MyEnum), "First")|};
                }
            }
            """);
        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public MyEnum TestMethod()
                {
                    return (MyEnum)MyEnumExtensions.Parse("First");
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:System.Enum.Parse(typeof(System.IO.FileShare), "Read")|};
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
                    var value = FileShareExtensions.Parse("Read");
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseGenericOnExternalEnumWithEnumExtensionsAttributeShouldHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:System.Enum.Parse<System.IO.FileShare>("Read")|};
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
                    var value = FileShareExtensions.Parse("Read");
                }
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task ParseOnNonExtensionExternalEnumShouldNotHaveDiagnostic()
    {
        var test = GetTestCodeWithExternalEnum(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = System.Enum.Parse(typeof(System.DateTimeKind), "Local");
                }
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ParseWithVariableNameShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    string name = "First";
                    var value = {|NEEG007:System.Enum.Parse(typeof(MyEnum), name)|};
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
                    var value = MyEnumExtensions.Parse(name);
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseWithMethodCallShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = {|NEEG007:Enum.Parse<MyEnum>(GetName())|};
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
                    var value = MyEnumExtensions.Parse(GetName());
                }
                
                private string GetName() => "First";
            }
            """);
        await VerifyCodeFixWithNet6AssembliesAsync(test, fix);
    }

    [Fact]
    public async Task ParseWithEnumExtensionClassInOtherNamespaceAddsUsing()
    {
        var test = TestCode(addUsing: false,
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value1 = {|NEEG007:Enum.Parse(typeof(MyEnum), GetName())|};
                    var value2 = {|NEEG007:Enum.Parse<MyEnum>("Second")|};
                    var value3 = {|NEEG007:Enum.Parse(typeof(MyEnum), "Third", true)|};
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
                    var value1 = MyTestExtensions.Parse(GetName());
                    var value2 = MyTestExtensions.Parse("Second");
                    var value3 = MyTestExtensions.Parse("Third", true);
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
                      public static Some.Namespace.MyEnum Parse(string name)
                      {
                          return name switch
                          {
                              "First" => Some.Namespace.MyEnum.First,
                              "Second" => Some.Namespace.MyEnum.Second,
                              "Third" => Some.Namespace.MyEnum.Third,
                              _ => throw new System.ArgumentException("Invalid value", nameof(name)),
                          };
                      }

                      public static Some.Namespace.MyEnum Parse(string name, bool ignoreCase)
                      {
                          return name switch
                          {
                              "First" => Some.Namespace.MyEnum.First,
                              "Second" => Some.Namespace.MyEnum.Second,
                              "Third" => Some.Namespace.MyEnum.Third,
                              _ => throw new System.ArgumentException("Invalid value", nameof(name)),
                          };
                      }
                  }
              }

              {{TestHelpers.LoadEmbeddedAttribute()}}
              {{TestHelpers.LoadEmbeddedMetadataSource()}}
              """;
    }

    [Fact]
    public async Task ParseWithCastShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public void TestMethod()
                {
                    var value = (MyEnum){|NEEG007:System.Enum.Parse(typeof(MyEnum), "First")|};
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
                    var value = (MyEnum)MyEnumExtensions.Parse("First");
                }
            }
            """);
        await Verifier.VerifyCodeFixAsync(test, fix);
    }

    [Fact]
    public async Task ParseInSwitchExpressionShouldHaveDiagnostic()
    {
        var test = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public string TestMethod(string name)
                {
                    return {|NEEG007:Enum.Parse<MyEnum>(name)|} switch
                    {
                        MyEnum.First => "1",
                        MyEnum.Second => "2",
                        MyEnum.Third => "3",
                        _ => "unknown"
                    };
                }
            }
            """);

        var fix = GetTestCode(
            /* lang=c# */
            """
            public class TestClass
            {
                public string TestMethod(string name)
                {
                    return MyEnumExtensions.Parse(name) switch
                    {
                        MyEnum.First => "1",
                        MyEnum.Second => "2",
                        MyEnum.Third => "3",
                        _ => "unknown"
                    };
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
                public static System.IO.FileShare Parse(string name)
                {
                    return name switch
                    {
                        "Read" => System.IO.FileShare.Read,
                        "Write" => System.IO.FileShare.Write,
                        _ => throw new System.ArgumentException("Invalid value", nameof(name)),
                    };
                }

                public static System.IO.FileShare Parse(string name, bool ignoreCase)
                {
                    return name switch
                    {
                        "Read" => System.IO.FileShare.Read,
                        "Write" => System.IO.FileShare.Write,
                        _ => throw new System.ArgumentException("Invalid value", nameof(name)),
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
                public static MyEnum Parse(string name)
                {
                    return name switch
                    {
                        "First" => MyEnum.First,
                        "Second" => MyEnum.Second,
                        "Third" => MyEnum.Third,
                        _ => throw new System.ArgumentException("Invalid value", nameof(name)),
                    };
                }

                public static MyEnum Parse(string name, bool ignoreCase)
                {
                    return name switch
                    {
                        "First" => MyEnum.First,
                        "Second" => MyEnum.Second,
                        "Third" => MyEnum.Third,
                        _ => throw new System.ArgumentException("Invalid value", nameof(name)),
                    };
                }
            }
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        {{TestHelpers.LoadEmbeddedMetadataSource()}}
        """;
}
