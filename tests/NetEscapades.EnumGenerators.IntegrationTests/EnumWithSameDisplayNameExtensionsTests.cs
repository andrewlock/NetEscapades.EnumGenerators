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
        "2ND",
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
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumWithSameDisplayNameExtensions.IsDefined(name, allowMatchingMetadataAttribute);
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute = false) => EnumWithSameDisplayNameExtensions.IsDefined(name, allowMatchingMetadataAttribute);
    protected override bool TryParse(string name, out EnumWithSameDisplayName parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithSameDisplayNameExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithSameDisplayName parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithSameDisplayNameExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumWithSameDisplayName value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumWithSameDisplayName value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttribute(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttributeAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: true);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: true);

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumWithSameDisplayNameExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumWithSameDisplayNameExtensions.GetNames());
}