using System;
using NetEscapades.EnumGenerators.IntegrationTests;
using Xunit;

namespace NetEscapades.EnumGenerators.Benchmarks;

public class AnalyzerTests
{
    // These method calls should all be flagged by the analyzer
    [Fact]
    public void Neeg004Testing()
    {
#pragma warning disable NEEG004
        var test = EnumInSystem.First;
        _ = test.ToString();
        _ = EnumInSystem.First.ToString();
        _ = EnumInSystem.First.ToString("G");
        _ = EnumInSystem.First.ToString("x"); // no error
        _ = EnumInSystem.First.ToString(format: "g");
        _ = EnumInSystem.First.ToString(format: null); // no error
        _ = DateTimeKind.Local.ToString();
        _ = $"Some value: {test} <-";
        _ = $"Some value: {test:G} <-";
        _ = $"Some value: {EnumInSystem.First} <-";
        _ = $"Some value: {EnumInSystem.First:G} <-";
#pragma warning restore NEEG004
    }

    [Fact]
    public void Neeg005Testing()
    {
#pragma warning disable NEEG005
        var test = FlagsEnum.First;
        _ = test.HasFlag(FlagsEnum.Second);
        _ = $"Some value: {test.HasFlag(FlagsEnum.Second)} <-";
#pragma warning restore NEEG005
    }
}