using System;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NetEscapades.EnumGenerators.Tests;

internal class TestHelpers
{
    private static readonly string[] _trackingNames = typeof(TrackingNames)
        .GetFields()
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
        .Select(x => (string?)x.GetRawConstantValue())
        .Where(x => !string.IsNullOrEmpty(x))
        .ToArray()!;

    public static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EnumExtensionsAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location),
            });

        var compilation = CSharpCompilation.Create(
            "generator",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var runResult = RunGeneratorAndAssertOutput<T>(compilation, _trackingNames);
        
        var trees = runResult.GeneratedTrees;
        return (runResult.Diagnostics, trees.IsDefaultOrEmpty ? string.Empty : trees[trees.Length - 1].ToString());
    }

    private static GeneratorDriverRunResult RunGeneratorAndAssertOutput<T>(CSharpCompilation compilation, params string[] trackingNames)
        where T : IIncrementalGenerator, new()
    {
        var generator = new T().AsSourceGenerator();

        var opts = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true);

        var driver = CSharpGeneratorDriver.Create([generator], driverOptions: opts);

        // Run twice, once with a clone
        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var compilationClone = compilation.Clone();
        var runResult2 = driver.RunGenerators(compilationClone).GetRunResult();

        AssertRunsEqual(runResult, runResult2, trackingNames);

        return runResult;
    }

    private static void AssertRunsEqual(GeneratorDriverRunResult runResult1, GeneratorDriverRunResult runResult2, string[] trackingNames)
    {
        var trackedSteps1 = runResult1.Results[0].TrackedSteps;
        var trackedSteps2 = runResult2.Results[0].TrackedSteps;

        // these should be the same!
        trackedSteps1.Should()
            .NotBeEmpty()
            .And.HaveSameCount(trackedSteps2);

        foreach (var trackingName in trackingNames)
        {
            AssertEqual(trackedSteps1, trackedSteps2, trackingName);
        }
    }

    private static void AssertEqual(
        ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps1,
        ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps2,
        string stepName)
    {
        ImmutableArray<IncrementalGeneratorRunStep> runSteps1 = trackedSteps1.Should().ContainKey(stepName).WhoseValue;
        ImmutableArray<IncrementalGeneratorRunStep> runSteps2 = trackedSteps2.Should().ContainKey(stepName).WhoseValue;

        for (var i = 0; i < runSteps1.Length; i++)
        {
            ImmutableArray<(object Value, IncrementalStepRunReason Reason)> outputs1 = runSteps1[i].Outputs;
            ImmutableArray<(object Value, IncrementalStepRunReason Reason)> outputs2 = runSteps2[i].Outputs;

            outputs1.Should().Equal(outputs2, $"because {stepName} should produce cacheable outputs");
        }
    }
}