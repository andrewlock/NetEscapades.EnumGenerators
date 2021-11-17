using FluentAssertions;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class ExtensionTests
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

    [Theory]
    [InlineData(EnumInNamespace.First, true, "First" )]
    [InlineData(EnumInNamespace.Second, true, "Second" )]
    [InlineData(EnumInNamespace.First, false, "first" )]
    [InlineData(EnumInNamespace.Second, false, "SECOND" )]
    [InlineData((EnumInNamespace)3, false, "3" )]
    public void GeneratesTryParse(EnumInNamespace value, bool isValid, string name)
    {
        EnumInNamespaceExtensions.TryParse(name, out var parsed).Should().Be(isValid);
        if (isValid)
        {
            parsed.Should().Be(value);
        }
    }

    [Theory]
    [InlineData(EnumInNamespace.First, true, "First" )]
    [InlineData(EnumInNamespace.Second, true, "Second" )]
    [InlineData(EnumInNamespace.First, true, "first" )]
    [InlineData(EnumInNamespace.Second, true, "SECOND" )]
    [InlineData((EnumInNamespace)3, false, "3" )]
    [InlineData((EnumInNamespace)0, false, "Fourth" )]
    public void GeneratesTryParseIgnoreCase(EnumInNamespace value, bool isValid, string name)
    {
        EnumInNamespaceExtensions.TryParse(name, ignoreCase: true, out var parsed).Should().Be(isValid);
        if (isValid)
        {
            parsed.Should().Be(value);
        }
    }

    [Fact]
    public void GeneratesGetValues()
    {
        EnumInNamespaceExtensions.GetValues().Should().Equal(new[]
        {
            EnumInNamespace.First,
            EnumInNamespace.Second,
            EnumInNamespace.Third,
        });
    }

    [Fact]
    public void GeneratesGetNames()
    {
        EnumInNamespaceExtensions.GetNames().Should().Equal(new[]
        {
            "First",
            "Second",
            "Third",
        });
    }
}