using System;
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

    protected override string ToStringFast(LongEnum value) => value.ToStringFast();
    protected override bool IsDefined(LongEnum value) => LongEnumExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingDisplayAttribute) => LongEnumExtensions.IsDefined(name);
    protected override bool IsDefined(ReadOnlySpan<char> name, bool allowMatchingDisplayAttribute) => LongEnumExtensions.IsDefined(name);
    protected override bool TryParse(string name, bool ignoreCase, out LongEnum parsed, bool allowMatchingDisplayAttribute)
        => LongEnumExtensions.TryParse(name, ignoreCase, out parsed);
    protected override bool TryParse(ReadOnlySpan<char> name, bool ignoreCase, out LongEnum parsed, bool allowMatchingDisplayAttribute)
        => LongEnumExtensions.TryParse(name, ignoreCase, out parsed);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(LongEnum value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(LongEnum value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan());

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan());

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsAspan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true);

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(LongEnumExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(LongEnumExtensions.GetNames());
}