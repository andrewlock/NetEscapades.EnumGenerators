using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics;
using NetEscapades.EnumGenerators.Diagnostics.DefinitionAnalyzers;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    NetEscapades.EnumGenerators.Diagnostics.DefinitionAnalyzers.DuplicateExtensionClassAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class DuplicateExtensionClassAnalyzerTests
{
    private const string DiagnosticId = DuplicateExtensionClassAnalyzer.DiagnosticId;

    // No diagnostics expected to show up
    [Fact]
    public async Task EmptySourceShouldNotHaveDiagnostics()
    {
        var test = string.Empty;
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("")]
    [InlineData("""(ExtensionClassName = "SomeExtension")""")]
    [InlineData("""(ExtensionClassNamespace = "SomeNamespace")""")]
    [InlineData("""(ExtensionClassName = "SomeExtension", ExtensionClassNamespace = "SomeNamespace")""")]
    public async Task SingleEnumShouldNotHaveDiagnostics(string args)
    {
        var test = GetTestCode(
            $$"""
              [EnumExtensions{{args}}]
              public enum TestEnum
              {
                  First,
                  Second,
              }
              """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleEnumsInDifferentNamespaceShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [EnumExtensions]
                  public enum TestEnum
                  {
                      First,
                      Second,
                  }
              }
              namespace ConsoleApplication2
              {
                  [EnumExtensions]
                  public enum TestEnum
                  {
                      First,
                      Second,
                  }
              }
              """);

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultipleEnumsWithDifferentNamesShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [EnumExtensions]
                  public enum TestEnum
                  {
                      First,
                      Second,
                  }

                  [EnumExtensions]
                  public enum TestEnum2
                  {
                      First,
                      Second,
                  }
              }

              
              """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("")]
    [InlineData("""(ExtensionClassName = "SomeExtension")""")]
    [InlineData("""(ExtensionClassNamespace = "SomeNamespace")""")]
    [InlineData("""(ExtensionClassName = "SomeExtension", ExtensionClassNamespace = "SomeNamespace")""")]
    public async Task MultipleEnumsWithMultipleValuesShouldNotHaveDiagnostics(string args)
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [{|#0:EnumExtensions|}]
                  public enum TestEnum1
                  {
                      First,
                      Second,
                  }

                  [{|#1:EnumExtensions{{args}}|}]
                  public enum TestEnum
                  {
                      First,
                      Second,
                  }
              }
              """);

        // Don't bother to validate message 
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ShouldFlagEnumsThatGenerateTheSameExtensionMethod()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [{|#0:EnumExtensions|}]
                  public enum TestEnum
                  {
                      First,
                      Second,
                  }

                  [{|#1:EnumExtensions(ExtensionClassName = "TestEnumExtensions")|}]
                  public enum TestEnum2
                  {
                      First,
                      Second,
                  }
              }
              """);

        // Don't bother to validate message 
        var expected1 = Verifier.Diagnostic(DiagnosticId).WithLocation(0).WithMessage(null);
        var expected2 = Verifier.Diagnostic(DiagnosticId).WithLocation(1).WithMessage(null);
        await Verifier.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("""(ExtensionClassName = "SomeExtension")""")]
    [InlineData("""(ExtensionClassNamespace = "SomeNamespace")""")]
    [InlineData("""(ExtensionClassName = "SomeExtension", ExtensionClassNamespace = "SomeNamespace")""")]
    public async Task ShouldFlagEnumsThatGenerateTheSameExtensionMethodDueToNestedClass(string args)
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [{|#0:EnumExtensions{{args}}|}]
                  public enum TestEnum
                  {
                      First,
                      Second,
                  }

                  public class Nested
                  {
                      [{|#1:EnumExtensions{{args}}|}]
                      public enum TestEnum
                      {
                          First,
                          Second,
                      }
                  }
              }
              """);

        // Don't bother to validate message 
        var expected1 = Verifier.Diagnostic(DiagnosticId).WithLocation(0).WithMessage(null);
        var expected2 = Verifier.Diagnostic(DiagnosticId).WithLocation(1).WithMessage(null);
        await Verifier.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task ShouldFlagEnumsThatGenerateTheSameExtensionMethodDueToNamespaceClash()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [{|#0:EnumExtensions|}]
                  public enum TestEnum1
                  {
                      First,
                      Second,
                  }
              }

              namespace SomeNamespace
              {
                  [{|#1:EnumExtensions(ExtensionClassNamespace = "ConsoleApplication1")|}]
                  public enum TestEnum1
                  {
                      First,
                      Second,
                  }
              }
              """);

        // Don't bother to validate message 
        var expected1 = Verifier.Diagnostic(DiagnosticId).WithLocation(0).WithMessage(null);
        var expected2 = Verifier.Diagnostic(DiagnosticId).WithLocation(1).WithMessage(null);
        await Verifier.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task ShouldFlagEnumsThatGenerateTheSameExtensionMethodDueToNameClash()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [{|#0:EnumExtensions|}]
                  public enum TestEnum1
                  {
                      First,
                      Second,
                  }

                  [{|#1:EnumExtensions(ExtensionClassName = "TestEnum1Extensions")|}]
                  public enum TestEnum2
                  {
                      First,
                      Second,
                  }
              }
              """);

        // Don't bother to validate message 
        var expected1 = Verifier.Diagnostic(DiagnosticId).WithLocation(0).WithMessage(null);
        var expected2 = Verifier.Diagnostic(DiagnosticId).WithLocation(1).WithMessage(null);
        await Verifier.VerifyAnalyzerAsync(test, expected1, expected2);
    }
    [Fact]
    public async Task ShouldFlagEnumsThatGenerateTheSameExtensionMethodDueToNameAndNamespaceClash()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  [{|#0:EnumExtensions|}]
                  public enum TestEnum1
                  {
                      First,
                      Second,
                  }
              }

              namespace ConsoleApplication2
              {
                  [{|#1:EnumExtensions(ExtensionClassName = "TestEnum1Extensions", ExtensionClassNamespace = "ConsoleApplication1")|}]
                  public enum TestEnum2
                  {
                      First,
                      Second,
                  }
              }
              """);

        // Don't bother to validate message 
        var expected1 = Verifier.Diagnostic(DiagnosticId).WithLocation(0).WithMessage(null);
        var expected2 = Verifier.Diagnostic(DiagnosticId).WithLocation(1).WithMessage(null);
        await Verifier.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    private static string GetTestCode(string testFragment)
        => $$"""

                 using System;
                 using System.Collections.Generic;
                 using System.Linq;
                 using System.Text;
                 using System.Threading;
                 using System.Threading.Tasks;
                 using System.Diagnostics;
                 using NetEscapades.EnumGenerators;

                 {{testFragment}}

             {{TestHelpers.LoadEmbeddedAttribute()}}
             {{TestHelpers.LoadEmbeddedMetadataSource()}}
             """;
}