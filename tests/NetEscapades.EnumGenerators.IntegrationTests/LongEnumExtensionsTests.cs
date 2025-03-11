using System;
using Xunit;

#if INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.IntegrationTests;
#elif NETSTANDARD_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NUGET_ATTRS_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Attributes.IntegrationTests;
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#else
#error Unknown integration tests
#endif

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

    protected override string ToStringFast(LongEnum value) => value.ToStringFast();
    protected override string ToStringFast(LongEnum value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override bool IsDefined(LongEnum value) => LongEnumExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => LongEnumExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => LongEnumExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out LongEnum parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => LongEnumExtensions.TryParse(name, out parsed, ignoreCase);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out LongEnum parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => LongEnumExtensions.TryParse(name, out parsed, ignoreCase);
#endif
    protected override LongEnum Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => LongEnumExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override LongEnum Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => LongEnumExtensions.Parse(name, ignoreCase);
#endif

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(LongEnum value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFastWithMetadata(LongEnum value) => GeneratesToStringFastWithMetadataTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(LongEnum value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name, ignoreCase:false, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsAspan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesAsUnderlyingType(LongEnum value) => GeneratesAsUnderlyingTypeTest(value, value.AsUnderlyingType());

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(LongEnumExtensions.GetValues());

    [Fact]
    public void GeneratesGetValuesAsUnderlyingType() => GeneratesGetValuesAsUnderlyingTypeTest(LongEnumExtensions.GetValuesAsUnderlyingType());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(LongEnumExtensions.GetNames());
}