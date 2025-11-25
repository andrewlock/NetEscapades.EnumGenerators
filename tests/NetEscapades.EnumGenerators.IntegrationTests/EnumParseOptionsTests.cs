using System;
using FluentAssertions;
using Xunit;

namespace NetEscapades.EnumGenerators.Benchmarks;

public class EnumParseOptionsTests
{
    [Fact]
    public void Default()
    {
        EnumParseOptions options = default;
        options.AllowMatchingMetadataAttribute.Should().BeFalse();
        options.EnableNumberParsing.Should().BeTrue();
        options.ComparisonType.Should().Be(StringComparison.Ordinal);
    }

    [Fact]
    public void DefaultConstructor()
    {
        var options = new EnumParseOptions();
        options.AllowMatchingMetadataAttribute.Should().BeFalse();
        options.EnableNumberParsing.Should().BeTrue();
        options.ComparisonType.Should().Be(StringComparison.Ordinal);
    }

    [Fact]
    public void SingleValue()
    {
        var options = new EnumParseOptions(comparisonType: StringComparison.OrdinalIgnoreCase);
        options.AllowMatchingMetadataAttribute.Should().BeFalse();
        options.EnableNumberParsing.Should().BeTrue();
        options.ComparisonType.Should().Be(StringComparison.OrdinalIgnoreCase);
    }
}