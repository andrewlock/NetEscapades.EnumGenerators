using FluentAssertions;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class FlagsEnumExtensionsTests : ExtensionTests<FlagsEnum>
{
    public static TheoryData<FlagsEnum> ValidEnumValues() => new()
    {
        FlagsEnum.First,
        FlagsEnum.Second,
        FlagsEnum.ThirdAndFourth,
        (FlagsEnum)3,
    };

    public static TheoryData<string> ValuesToParse() => new()
    {
        "First",
        "Second",
        "2nd",
        "first",
        "SECOND",
        "3",
        "267",
        "-267",
        "2147483647",
        "3000000000",
        "Fourth",
        "Fifth",
    };

    protected override string ToStringFast(FlagsEnum value) => value.ToStringFast();
    protected override bool IsDefined(FlagsEnum value) => FlagsEnumExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingDisplayAttribute) => FlagsEnumExtensions.IsDefined(name);
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingDisplayAttribute) => FlagsEnumExtensions.IsDefined(name);
    protected override bool TryParse(string name,bool ignoreCase, out FlagsEnum parsed, bool allowMatchingDisplayAttribute)
        => FlagsEnumExtensions.TryParse(name, ignoreCase, out parsed);
    protected override bool TryParse(in ReadOnlySpan<char> name, bool ignoreCase, out FlagsEnum parsed, bool allowMatchingDisplayAttribute)
        => FlagsEnumExtensions.TryParse(name, ignoreCase, out parsed);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(FlagsEnum value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(FlagsEnum value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name);

    [Theory]
    [InlineData(FlagsEnum.First)]
    [InlineData(FlagsEnum.Second)]
    [InlineData(FlagsEnum.First | FlagsEnum.Second)]
    [InlineData(FlagsEnum.Third)]
    [InlineData((FlagsEnum)65)]
    public void HasFlags(FlagsEnum value)
    {
        var flag = FlagsEnum.Second;
        var isDefined = value.HasFlag(flag);

        isDefined.Should().Be(value.HasFlag(flag));
    }

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true);

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(FlagsEnumExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(FlagsEnumExtensions.GetNames());
}