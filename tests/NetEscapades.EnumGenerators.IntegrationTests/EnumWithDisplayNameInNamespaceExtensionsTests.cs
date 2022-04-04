using System;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class EnumWithDisplayNameInNamespaceExtensionsTests : ExtensionTests<EnumWithDisplayNameInNamespace>
{
    public static TheoryData<EnumWithDisplayNameInNamespace> ValidEnumValues() => new()
    {
        EnumWithDisplayNameInNamespace.First,
        EnumWithDisplayNameInNamespace.Second,
        (EnumWithDisplayNameInNamespace)3,
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

    protected override string ToStringFast(EnumWithDisplayNameInNamespace value) => value.ToStringFast();
    protected override bool IsDefined(EnumWithDisplayNameInNamespace value) => EnumWithDisplayNameInNamespaceExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingDisplayAttribute) => EnumWithDisplayNameInNamespaceExtensions.IsDefined(name, allowMatchingDisplayAttribute);
    protected override bool IsDefined(ReadOnlySpan<char> name, bool allowMatchingDisplayAttribute = false) => EnumWithDisplayNameInNamespaceExtensions.IsDefined(name, allowMatchingDisplayAttribute);
    protected override bool TryParse(string name,bool ignoreCase, out EnumWithDisplayNameInNamespace parsed, bool allowMatchingDisplayAttribute)
        => EnumWithDisplayNameInNamespaceExtensions.TryParse(name, ignoreCase, out parsed, allowMatchingDisplayAttribute);
    protected override bool TryParse(ReadOnlySpan<char> name, bool ignoreCase, out EnumWithDisplayNameInNamespace parsed, bool allowMatchingDisplayAttribute = false)
        => EnumWithDisplayNameInNamespaceExtensions.TryParse(name, ignoreCase, out parsed, allowMatchingDisplayAttribute);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumWithDisplayNameInNamespace value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumWithDisplayNameInNamespace value) => GeneratesIsDefinedTest(value);

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
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumWithDisplayNameInNamespaceExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumWithDisplayNameInNamespaceExtensions.GetNames());
}