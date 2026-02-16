using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

public class IsDefinedAnalyzerTests : AnalyzerTestsBase<IsDefinedAnalyzer, IsDefinedCodeFixProvider>
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
        await VerifyAnalyzerAsync(test, EnableState.Missing);
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

        await VerifyAnalyzerAsync(test, EnableState.Disabled);
    }

    [Fact]
    public async Task IsDefinedOnEnumFromReferencedAssemblyWithAssemblyLevelEnumExtensionsShouldHaveDiagnostic()
    {
        var mainCode = """
            using System;
            using ExternalNamespace;

            namespace ConsoleApplication1
            {
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var value = ExternalEnum.Value1;
                        var isDefined = {|NEEG006:Enum.IsDefined(typeof(ExternalEnum), value)|};
                    }
                }
            }

            namespace ExternalNamespace
            {
                // This code would be generated, just hacked in here for simplicity
                public static class ExternalEnumExtensions
                {
                    public static bool IsDefined(ExternalEnum value) => true;
                }
            }
            """;

        var externalCode = $$"""
            [assembly: NetEscapades.EnumGenerators.EnumExtensions<ExternalNamespace.ExternalEnum>()]

            namespace ExternalNamespace
            {
                public enum ExternalEnum
                {
                    Value1 = 1,
                    Value2 = 2,
                }
            }

            {{TestHelpers.LoadEmbeddedAttribute()}}
            {{TestHelpers.LoadEmbeddedMetadataSource()}}
            """;

        var test = new CSharpAnalyzerTest<IsDefinedAnalyzer, DefaultVerifier>
        {
            TestCode = mainCode,
        };

        test.TestState.AdditionalProjects.Add("ExternalProject", new ProjectState("ExternalProject", LanguageNames.CSharp, "external_", ".cs"));
        test.TestState.AdditionalProjects["ExternalProject"].Sources.Add(("external_Code.cs", externalCode));
        test.TestState.AdditionalProjectReferences.Add("ExternalProject");

        test.TestState.AnalyzerConfigFiles.Add(
            ("/.editorconfig",
                $"""
                 is_global = true
                 {UsageAnalyzerConfig.EnableKey} = true
                 """));

        await test.RunAsync();
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
}