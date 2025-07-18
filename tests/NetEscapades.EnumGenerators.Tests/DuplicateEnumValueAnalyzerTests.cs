using System.Threading.Tasks;
using NetEscapades.EnumGenerators.Diagnostics;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    NetEscapades.EnumGenerators.Diagnostics.DuplicateEnumValueAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace NetEscapades.EnumGenerators.Tests;

public class DuplicateEnumValueAnalyzerTests
{
    private const string DiagnosticId = DuplicateEnumValueAnalyzer.DiagnosticId;

    // No diagnostics expected to show up
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
            """
            public enum TestEnum
            {
                First = 0,
                Second = 0,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithoutDuplicatesShouldNotHaveDiagnostics()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First = 0,
                Second = 1,
                Third = 2,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithDuplicateValuesShouldHaveDiagnosticsForSecondOccurrence()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First = 0,
                {|NEEG003:Second|} = 0,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithMultipleDuplicateValuesShouldHaveDiagnosticsForAllButFirst()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First = 0,
                {|NEEG003:Second|} = 0,
                {|NEEG003:Third|} = 0,
                Fourth = 1,
                {|NEEG003:Fifth|} = 1,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithImplicitValuesShouldDetectDuplicates()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum
            {
                First,  // 0
                {|NEEG003:Second|} = 0,  // duplicate of First
                {|NEEG003:Third|} = 0,  // duplicate of First and Second
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task EnumWithDifferentTypesButSameValuesShouldDetectDuplicates()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum TestEnum : byte
            {
                First = 0,
                {|NEEG003:Second|} = 0,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact] 
    public async Task MultipleEnumsWithDuplicatesShouldTrackSeparately()
    {
        var test = GetTestCode(
            """
            [EnumExtensions]
            public enum FirstEnum
            {
                A = 0,
                {|NEEG003:B|} = 0,
            }

            [EnumExtensions]
            public enum SecondEnum  
            {
                X = 0,
                {|NEEG003:Y|} = 0,
            }
            """);
        await Verifier.VerifyAnalyzerAsync(test);
    }

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
        }

        {{TestHelpers.LoadEmbeddedAttribute()}}
        """;
}