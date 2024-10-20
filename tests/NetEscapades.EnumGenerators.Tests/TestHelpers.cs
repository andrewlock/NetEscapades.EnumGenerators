using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using VerifyTests;

namespace NetEscapades.EnumGenerators.Tests;

internal static class TestHelpers
{
    private static readonly Regex InterceptorAttributeRegex =
        new(@"InterceptsLocation\(\d+, \"".+\""\)\]");

    public static SettingsTask ScrubExpectedChanges(this SettingsTask settings)
        => settings.ScrubLinesWithReplace(
            line => line.Replace($"""GeneratedCodeAttribute("NetEscapades.EnumGenerators", "{Constants.Version}")""",
                """GeneratedCodeAttribute("NetEscapades.EnumGenerators", "FIXED_VERSION")"""))
            .AddScrubber(builder =>
            {
                var value = builder.ToString();
                var result = InterceptorAttributeRegex.Replace(value, "InterceptsLocation(123, \"REDACTED\")]");

                if (value.Equals(result, StringComparison.Ordinal))
                {
                    return;
                }

                builder.Clear();
                builder.Append(result);
            });
    
    public static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(Options opts)
        where T : IIncrementalGenerator, new()
    {
        var (diagnostics, trees) = GetGeneratedTrees<T, TrackingNames>(opts);
        return (diagnostics, trees.LastOrDefault() ?? string.Empty);
    }

    public static (ImmutableArray<Diagnostic> Diagnostics, string[] Output) GetGeneratedTrees<TGenerator, TTrackingNames>(Options options)
        where TGenerator : IIncrementalGenerator, new()
    {
        // get all the const string fields
        return GetGeneratedTrees<TGenerator>(options, options.Stages ?? GetTrackingNames<TTrackingNames>());
    }

    public static (ImmutableArray<Diagnostic> Diagnostics, string[] Output) GetGeneratedTrees<T>(Options opts, params string[] stages)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTrees = opts.Sources
            .Select(x =>
            {
                var tree = CSharpSyntaxTree.ParseText(x, path: "Program.cs");
                var options = new CSharpParseOptions(opts.LanguageVersion)
                    .WithFeatures(opts.Features);
                return tree.WithRootAndOptions(tree.GetRoot(), options);
            });
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EnumExtensionsAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).Assembly.Location),
            });

        var compilation = CSharpCompilation.Create(
            "generator",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var (runResult, diagnostics)  = RunGeneratorAndAssertOutput<T>(opts, compilation, stages);

        var combinedDiagnostics = runResult.Diagnostics.AddRange(diagnostics);

        return (combinedDiagnostics, runResult.GeneratedTrees.Select(x => x.ToString()).ToArray());
    }

    private static (GeneratorDriverRunResult runResult, ImmutableArray<Diagnostic> diagnostics) RunGeneratorAndAssertOutput<T>(Options options, CSharpCompilation compilation, string[] stages, bool assertOutput = true)
        where T : IIncrementalGenerator, new()
    {
        ISourceGenerator generator = new T().AsSourceGenerator();

        var opts = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true);

        GeneratorDriver driver =
            CSharpGeneratorDriver.Create(
                [generator],
                driverOptions: opts,
                optionsProvider: options.OptionsProvider,
                parseOptions: new CSharpParseOptions(options.LanguageVersion).WithFeatures(options.Features));

        var clone = compilation.Clone();
        // Run twice, once with a clone of the compilation
        // Note that we store the returned drive value, as it contains cached previous outputs
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        GeneratorDriverRunResult runResult = driver.GetRunResult();

        if (assertOutput)
        {
            // Run with a clone of the compilation
            GeneratorDriverRunResult runResult2 = driver
                                                 .RunGenerators(clone)
                                                 .GetRunResult();

            AssertRunsEqual(runResult, runResult2, stages);
            
            // verify the second run only generated cached source outputs
            runResult2.Results[0]
                .TrackedOutputSteps
                .SelectMany(x => x.Value) // step executions
                .SelectMany(x => x.Outputs) // execution results
                .Should()
                .OnlyContain(x => x.Reason == IncrementalStepRunReason.Cached);
        }

        return (runResult, outputCompilation.GetDiagnostics());
    }

    private static void AssertRunsEqual(GeneratorDriverRunResult runResult1, GeneratorDriverRunResult runResult2, string[] trackingNames)
    {
        // We're given all the tracking names, but not all the stages have necessarily executed so filter
        Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        // These should be the same
        trackedSteps1.Should()
                     .NotBeEmpty()
                     .And.HaveSameCount(trackedSteps2)
                     .And.ContainKeys(trackedSteps2.Keys);

        foreach (var trackedStep in trackedSteps1)
        {
            var trackingName = trackedStep.Key;
            var runSteps1 = trackedStep.Value;
            var runSteps2 = trackedSteps2[trackingName];
            AssertEqual(runSteps1, runSteps2, trackingName);
        }

        return;

        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(
            GeneratorDriverRunResult runResult, string[] trackingNames) =>
            runResult.Results[0]
                     .TrackedSteps
                     .Where(step => trackingNames.Contains(step.Key))
                     .ToDictionary(x => x.Key, x => x.Value);
    }

    private static void AssertEqual(
        ImmutableArray<IncrementalGeneratorRunStep> runSteps1,
        ImmutableArray<IncrementalGeneratorRunStep> runSteps2,
        string stepName)
    {
        runSteps1.Should().HaveSameCount(runSteps2);

        for (var i = 0; i < runSteps1.Length; i++)
        {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            // The outputs should be equal between different runs
            IEnumerable<object> outputs1 = runStep1.Outputs.Select(x => x.Value);
            IEnumerable<object> outputs2 = runStep2.Outputs.Select(x => x.Value);

            outputs1.Should()
                    .Equal(outputs2, $"because {stepName} should produce cacheable outputs");

            // Therefore, on the second run the results should always be cached or unchanged!
            // - Unchanged is when the _input_ has changed, but the output hasn't
            // - Cached is when the the input has not changed, so the cached output is used 
            runStep2.Outputs.Should()
                .OnlyContain(
                    x => x.Reason == IncrementalStepRunReason.Cached || x.Reason == IncrementalStepRunReason.Unchanged,
                    $"{stepName} expected to have reason {IncrementalStepRunReason.Cached} or {IncrementalStepRunReason.Unchanged}");

            // Make sure we're not using anything we shouldn't
            AssertObjectGraph(runStep1, stepName);
            AssertObjectGraph(runStep2, stepName);
        }

        static void AssertObjectGraph(IncrementalGeneratorRunStep runStep, string stepName)
        {
            var because = $"{stepName} shouldn't contain banned symbols";
            var visited = new HashSet<object>();

            foreach (var (obj, _) in runStep.Outputs)
            {
                Visit(obj);
            }

            void Visit(object? node)
            {
                if (node is null || !visited.Add(node))
                {
                    return;
                }

                node.Should()
                    .NotBeOfType<Compilation>(because)
                    .And.NotBeOfType<ISymbol>(because)
                    .And.NotBeOfType<SyntaxNode>(because);

                Type type = node.GetType();
                if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                {
                    return;
                }

                if (node is IEnumerable collection and not string)
                {
                    foreach (var element in collection)
                    {
                        Visit(element);
                    }

                    return;
                }

                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object? fieldValue = field.GetValue(node);
                    Visit(fieldValue);
                }
            }
        }
    }

    private static string[] GetTrackingNames<TTrackingNames>()
        => typeof(TTrackingNames)
            .GetFields()
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string?)x.GetRawConstantValue()!)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

    private class OptionsProvider(AnalyzerConfigOptions options) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => options;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => options;
        public override AnalyzerConfigOptions GlobalOptions => options;
    }

    internal sealed class DictionaryAnalyzerOptions(Dictionary<string, string> properties) : AnalyzerConfigOptions
    {
        public static DictionaryAnalyzerOptions Empty { get; } = new(new());

        public override bool TryGetValue(string key, out string value)
            => properties.TryGetValue(key, out value!);
    }

    public record Options
    {
        public Options(params string[] sources)
            : this(LanguageVersion.Default, null, null, sources, null)
        {
        }

        public Options(Dictionary<string, string> options, params string[] sources)
            : this(LanguageVersion.Default, options, null, sources, null)
        {
        }

        public Options(LanguageVersion languageVersion, Dictionary<string, string> options, params string[] sources)
            : this(languageVersion, options, null, sources, null)
        {
        }

        public Options(Dictionary<string, string> options, Dictionary<string, string> features, params string[] sources)
            : this(LanguageVersion.Default, options, features, sources, null)
        {
        }
 
        public Options(LanguageVersion languageVersion, Dictionary<string, string> options, Dictionary<string, string> features, params string[] sources)
            : this(languageVersion, options, features, sources, null)
        {
        }

        public Options(LanguageVersion LanguageVersion,
            Dictionary<string, string>? AnalyzerOptions,
            Dictionary<string, string>? Features,
            string[] Sources,
            string[]? Stages)
        {
            this.LanguageVersion = LanguageVersion;
            this.AnalyzerOptions = AnalyzerOptions;
            this.Features = Features;
            this.Sources = Sources;
            this.Stages = Stages;
        }

        public AnalyzerConfigOptionsProvider? OptionsProvider =>
            AnalyzerOptions is not null ? new OptionsProvider(new DictionaryAnalyzerOptions(AnalyzerOptions)) : null;

        public LanguageVersion LanguageVersion { get;  }
        public Dictionary<string, string>? AnalyzerOptions { get; }
        public Dictionary<string, string>? Features { get; }
        public string[] Sources { get; }
        public string[]? Stages { get; }

    }
}
