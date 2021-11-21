using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class FlagsEnumExtensionsTests : ExtensionTests<FlagsEnum>
{
    public static TheoryData<FlagsEnum> ValidEnumValues() => new()
    {
        FlagsEnum.First,
        FlagsEnum.Second,
        (FlagsEnum)3,
    };

    public static TheoryData<string> ValuesToParse() => new()
    {
        "First",
        "Second",
        "first",
        "SECOND",
        "3",
        "267",
        "-267",
        "2147483647",
        "3000000000",
        "Fourth",
    };

    protected override string ToStringFast(FlagsEnum value) => value.ToStringFast();
    protected override bool IsDefined(FlagsEnum value) => value.IsDefined();
    protected override bool TryParse(string name,bool ignoreCase, out FlagsEnum parsed)
        => FlagsEnumExtensions.TryParse(name, ignoreCase, out parsed);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(FlagsEnum value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(FlagsEnum value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseIgnoreCaseTest(name);

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(FlagsEnumExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(FlagsEnumExtensions.GetNames());
}