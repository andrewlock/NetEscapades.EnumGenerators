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

public class LongEnumExtensionsTests : ExtensionTests<LongEnum, long, LongEnumExtensionsTests>, ITestData<LongEnum>
{
    public TheoryData<LongEnum> ValidEnumValues() => new()
    {
        LongEnum.First,
        LongEnum.Second,
        (LongEnum)3,
    };

    public TheoryData<string> ValuesToParse() => new()
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

    protected override string[] GetNames() => LongEnumExtensions.GetNames();
    protected override LongEnum[] GetValues() => LongEnumExtensions.GetValues();
    protected override long[] GetValuesAsUnderlyingType() => LongEnumExtensions.GetValuesAsUnderlyingType();
    protected override long AsUnderlyingValue(LongEnum value) => value.AsUnderlyingType();

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
}