using System;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class ExtensionTests
{
    [Theory]
    [InlineData(EnumInNamespace.First)]
    [InlineData(EnumInNamespace.Second)]
    [InlineData((EnumInNamespace)3)]
    public void GeneratesToStringFast(EnumInNamespace value)
    {
        var serialized = value.ToStringFast();

        serialized.Should().Be(value.ToString());
    }

    [Theory]
    [InlineData(EnumInNamespace.First)]
    [InlineData(EnumInNamespace.Second)]
    [InlineData((EnumInNamespace)3)]
    public void GeneratesIsDefined(EnumInNamespace value)
    {
        var isDefined = value.IsDefined();

        isDefined.Should().Be(Enum.IsDefined(typeof(EnumInNamespace), value));
    }

    [Theory]
    [InlineData("First" )]
    [InlineData("Second" )]
    [InlineData("first" )]
    [InlineData("SECOND" )]
    [InlineData("3")]
    [InlineData("267")]
    [InlineData("-267")]
    [InlineData("2147483647")]
    [InlineData("3000000000")]
    public void GeneratesTryParse(string name)
    {
        var isValid = Enum.TryParse(name, out EnumInNamespace expected);
        var result = EnumInNamespaceExtensions.TryParse(name, out var parsed);
        using var _ = new AssertionScope();
        result.Should().Be(isValid);
        parsed.Should().Be(expected);
    }

    [Theory]
    [InlineData("First" )]
    [InlineData("Second" )]
    [InlineData("first" )]
    [InlineData("SECOND" )]
    [InlineData("3")]
    [InlineData("267")]
    [InlineData("-267")]
    [InlineData("2147483647")]
    [InlineData("3000000000")]
    [InlineData("Fourth")]
    public void GeneratesTryParseIgnoreCase(string name)
    {
        var isValid = Enum.TryParse(name, ignoreCase: true, out EnumInNamespace expected);
        var result = EnumInNamespaceExtensions.TryParse(name, ignoreCase: true, out var parsed);
        using var _ = new AssertionScope();
        result.Should().Be(isValid);
        parsed.Should().Be(expected);
    }

    [Theory]
    [InlineData("First" )]
    [InlineData("Second" )]
    [InlineData("first" )]
    [InlineData("SECOND" )]
    [InlineData("3")]
    [InlineData("267")]
    [InlineData("-267")]
    [InlineData("2147483647")]
    [InlineData("3000000000")]
    public void GeneratesLongTryParse(string name)
    {
        var isValid = Enum.TryParse(name, out LongEnum expected);
        var result = LongEnumExtensions.TryParse(name, out var parsed);
        using var _ = new AssertionScope();
        result.Should().Be(isValid);
        parsed.Should().Be(expected);
    }

    [Fact]
    public void GeneratesGetValues()
    {
        var expected = (EnumInNamespace[])Enum.GetValues(typeof(EnumInNamespace));
        EnumInNamespaceExtensions.GetValues().Should().Equal(expected);
    }

    [Fact]
    public void GeneratesGetNames()
    {
        var expected = Enum.GetNames(typeof(EnumInNamespace));
        EnumInNamespaceExtensions.GetNames().Should().Equal(expected);
    }
}