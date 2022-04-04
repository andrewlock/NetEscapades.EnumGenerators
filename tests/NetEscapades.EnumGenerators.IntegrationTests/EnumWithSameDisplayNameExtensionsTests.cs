using System;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class EnumWithSameDisplayNameExtensionsTests : ExtensionTests<EnumWithSameDisplayName>
{
    public static TheoryData<EnumWithSameDisplayName> ValidEnumValues() => new()
    {
        EnumWithSameDisplayName.First,
        EnumWithSameDisplayName.Second,
        (EnumWithSameDisplayName)3,
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

    protected override string ToStringFast(EnumWithSameDisplayName value) => value.ToStringFast();
    protected override bool IsDefined(EnumWithSameDisplayName value) => EnumWithSameDisplayNameExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingDisplayAttribute) => EnumWithSameDisplayNameExtensions.IsDefined(name, allowMatchingDisplayAttribute);
    protected override bool IsDefined(ReadOnlySpan<char> name, bool allowMatchingDisplayAttribute = false) => EnumWithSameDisplayNameExtensions.IsDefined(name, allowMatchingDisplayAttribute);
    protected override bool TryParse(string name,bool ignoreCase, out EnumWithSameDisplayName parsed, bool allowMatchingDisplayAttribute)
        => EnumWithSameDisplayNameExtensions.TryParse(name, ignoreCase, out parsed, allowMatchingDisplayAttribute);
    protected override bool TryParse(ReadOnlySpan<char> name, bool ignoreCase, out EnumWithSameDisplayName parsed, bool allowMatchingDisplayAttribute = false)
        => EnumWithSameDisplayNameExtensions.TryParse(name, ignoreCase, out parsed, allowMatchingDisplayAttribute);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumWithSameDisplayName value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumWithSameDisplayName value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan());

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAllowMatchingDisplayAttribute(string name) => GeneratesIsDefinedTest(name, allowMatchingDisplayAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAllowMatchingDisplayAttributeAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingDisplayAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan());

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAllowMatchingDisplayAttribute(string name) => GeneratesTryParseTest(name, allowMatchingDisplayAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAllowMatchingDisplayAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), allowMatchingDisplayAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAllowMatchingDisplayAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingDisplayAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAllowMatchingDisplayAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingDisplayAttribute: true);

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumWithSameDisplayNameExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumWithSameDisplayNameExtensions.GetNames());
}