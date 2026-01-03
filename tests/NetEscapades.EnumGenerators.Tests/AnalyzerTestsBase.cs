using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using NetEscapades.EnumGenerators.Diagnostics.UsageAnalyzers;

namespace NetEscapades.EnumGenerators.Tests;

public abstract class AnalyzerTestsBase<TAnalyzer, TCodeFixer>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFixer : CodeFixProvider, new()
{
    protected static Task VerifyAnalyzerAsync(string source, EnableState usageAnalyzers = EnableState.Enabled)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
        };

        AddEditorConfig(test.TestState, usageAnalyzers);
        return test.RunAsync();
    }

    protected static Task VerifyAnalyzerWithNet7AssembliesAsync(string source, EnableState usageAnalyzers = EnableState.Enabled)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
        };

        AddEditorConfig(test.TestState, usageAnalyzers);
        return test.RunAsync();
    }

    protected static Task VerifyAnalyzerWithSystemMemoryAsync(string source, EnableState usageAnalyzers = EnableState.Enabled)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Default
#if NETFRAMEWORK || NETCOREAPP2_1
                .WithPackages([new PackageIdentity("System.Memory", "4.6.3")]),
#endif
        };

        AddEditorConfig(test.TestState, usageAnalyzers);
        return test.RunAsync();
    }

    protected static Task VerifyCodeFixAsync(string source, string fixedSource, EnableState usageAnalyzers = EnableState.Enabled)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixer, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
        };

        AddEditorConfig(test.TestState, usageAnalyzers);

        return test.RunAsync();
    }
    
    protected static Task VerifyCodeFixWithNet6AssembliesAsync(string source, string fixedSource, EnableState usageAnalyzers = EnableState.Enabled)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixer, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
        };

        AddEditorConfig(test.TestState, usageAnalyzers);

        return test.RunAsync(CancellationToken.None);
    }

    protected static Task VerifyCodeFixWithNet7AssembliesAsync(string source, string fixedSource, EnableState usageAnalyzers = EnableState.Enabled)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixer, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
        };

        AddEditorConfig(test.TestState, usageAnalyzers);

        return test.RunAsync(CancellationToken.None);
    }

    protected static Task VerifyCodeFixWithSystemMemoryAsync(string source, string fixedSource, EnableState usageAnalyzers = EnableState.Enabled)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixer, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Default
#if NETFRAMEWORK || NETCOREAPP2_1
                .WithPackages([new PackageIdentity("System.Memory", "4.6.3")]),
#endif
        };

        AddEditorConfig(test.TestState, usageAnalyzers);

        return test.RunAsync(CancellationToken.None);
    }

    private static void AddEditorConfig(SolutionState testState, EnableState usageAnalyzers)
    {
        var config = usageAnalyzers switch
        {
            EnableState.Enabled => $"{UsageAnalyzerConfig.EnableKey}=true",
            EnableState.Disabled => $"{UsageAnalyzerConfig.EnableKey}=false",
            _ => string.Empty,
        };

        testState.AnalyzerConfigFiles.Add(
            ("/.editorconfig",
                $"""
                 is_global = true
                 {config}
                 """));
    }

    public enum EnableState
    {
        Missing,
        Enabled,
        Disabled
    }
}