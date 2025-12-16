using System;
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
#pragma warning restore NEEG004
    }
}