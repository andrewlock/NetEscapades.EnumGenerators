using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class LongEnumExtensionsTests : ExtensionTests<LongEnum>
{
    public static TheoryData<LongEnum> ValidEnumValues() => new()
    {
        LongEnum.First,
        LongEnum.Second,
        (LongEnum)3,
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

    protected override string ToStringFast(LongEnum value) => value.ToStringFast();
    protected override bool IsDefined(LongEnum value) => value.IsDefined();
    protected override bool TryParse(string name,bool ignoreCase, out LongEnum parsed)
        => LongEnumExtensions.TryParse(name, ignoreCase, out parsed);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(LongEnum value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(LongEnum value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseIgnoreCaseTest(name);

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(LongEnumExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(LongEnumExtensions.GetNames());
}