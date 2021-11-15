using FluentAssertions;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class ToStringFastTests
{
    [Theory]
    [InlineData(EnumInNamespace.First, nameof(EnumInNamespace.First))]
    [InlineData(EnumInNamespace.Second, nameof(EnumInNamespace.Second))]
    [InlineData((EnumInNamespace)3, "3")]
    public void GeneratesToStringFast(EnumInNamespace value, string name)
    {
        var serialized = value.ToStringFast();

        serialized.Should().Be(name);
    }

    [Theory]
    [InlineData(EnumInNamespace.First, true)]
    [InlineData(EnumInNamespace.Second, true)]
    [InlineData((EnumInNamespace)3, false)]
    public void GeneratesIsDefined(EnumInNamespace value, bool defined)
    {
        var isDefined = value.IsDefined();

        isDefined.Should().Be(defined);
    }
}