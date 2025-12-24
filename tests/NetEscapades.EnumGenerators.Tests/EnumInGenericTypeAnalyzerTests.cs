using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics;
using NetEscapades.EnumGenerators.Diagnostics.DefinitionAnalyzers;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    NetEscapades.EnumGenerators.Diagnostics.DefinitionAnalyzers.EnumInGenericTypeAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class EnumInGenericTypeAnalyzerTests
{
    private const string DiagnosticId = EnumInGenericTypeAnalyzer.DiagnosticId;

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
    public async Task EnumInNestedClassShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  public class Nested
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
              }
              """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumInNestedClassThatDerivesFromConcreteGenericShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  public class Nested : List<string>
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
              }
              """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumInDoubleNestedClassShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            $$"""
              public class Parent
              {
                  public class Nested
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
              }


              """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ShouldFlagEnumInNestedGenericCode()
    {
        
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  public class Nested<T>
                  {
                      [{|#0:EnumExtensions|}]
                      public enum TestEnum
                      {
                          First,
                          Second,
                      }

                      [{|#1:EnumExtensions|}]
                      public enum TestEnum2
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
    public async Task ShouldFlagEnumInNestedGenericThatDerivesFromGenericCode()
    {
        
        var test = GetTestCode(
            $$"""
              namespace ConsoleApplication1
              {
                  public class Nested<T> : List<T>
                  {
                      [{|#0:EnumExtensions|}]
                      public enum TestEnum
                      {
                          First,
                          Second,
                      }

                      [{|#1:EnumExtensions|}]
                      public enum TestEnum2
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
    public async Task ShouldFlagEnumInDoubleNestedGenericCode()
    {
        
        var test = GetTestCode(
            $$"""
              public class Parent<TArg>
              {
                  public class Nested
                  {
                      [{|#0:EnumExtensions|}]
                      public enum TestEnum
                      {
                          First,
                          Second,
                      }

                      [{|#1:EnumExtensions|}]
                      public enum TestEnum2
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
    public async Task ShouldFlagEnumInDeeplyNestedGenericCode()
    {
        
        var test = GetTestCode(
            $$"""
              public class Parent
              {
                  public class Nested<T>
                  {
                      public class NonGeneric
                      {
                          [{|#0:EnumExtensions|}]
                          public enum TestEnum
                          {
                              First,
                              Second,
                          }
    
                          [{|#1:EnumExtensions|}]
                          public enum TestEnum2
                          {
                              First,
                              Second,
                          }
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
    public async Task ShouldFlagEnumInBothNestedGenericCode()
    {
        
        var test = GetTestCode(
            $$"""
              public class Parent<TArg>
              {
                  public class Nested<T>
                  {
                      [{|#0:EnumExtensions|}]
                      public enum TestEnum
                      {
                          First,
                          Second,
                      }

                      [{|#1:EnumExtensions|}]
                      public enum TestEnum2
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