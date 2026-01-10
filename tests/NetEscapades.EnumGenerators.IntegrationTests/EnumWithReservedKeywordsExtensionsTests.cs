using System;
using Xunit;

#if INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.IntegrationTests;
#elif NETSTANDARD_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.IntegrationTests;
#elif NETSTANDARD_SYSTEMMEMORY_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.SystemMemory.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#elif NUGET_SYSTEMMEMORY_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.SystemMemory.IntegrationTests;
#elif NUGET_SYSTEMMEMORY_PRIVATEASSETS_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests;
#else
#error Unknown integration tests
#endif

public class EnumWithReservedKeywordsExtensionsTests : ExtensionTests<EnumWithReservedKeywords, int, EnumWithReservedKeywordsExtensionsTests>, ITestData<EnumWithReservedKeywords>
{
    public TheoryData<EnumWithReservedKeywords> ValidEnumValues() => new()
    {
        EnumWithReservedKeywords.number,
        EnumWithReservedKeywords.@string,
        EnumWithReservedKeywords.date,
        EnumWithReservedKeywords.@class,
    };

    public TheoryData<string> ValuesToParse() => new()
    {
        "number",
        "string",
        "date",
        "class",
        "NUMBER",
        "STRING",
        "1",
        "2",
        "-1",
    };

    protected override string[] GetNames() => EnumWithReservedKeywordsExtensions.GetNames();
    protected override EnumWithReservedKeywords[] GetValues() => EnumWithReservedKeywordsExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => EnumWithReservedKeywordsExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(EnumWithReservedKeywords value) => value.AsUnderlyingType();

    protected override string ToStringFast(EnumWithReservedKeywords value) => value.ToStringFast();
    protected override string ToStringFast(EnumWithReservedKeywords value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override string ToStringFast(EnumWithReservedKeywords value, SerializationOptions options) => value.ToStringFast(options);
    protected override bool IsDefined(EnumWithReservedKeywords value) => EnumWithReservedKeywordsExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumWithReservedKeywordsExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute = false) => EnumWithReservedKeywordsExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithReservedKeywords parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithReservedKeywordsExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithReservedKeywords parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithReservedKeywordsExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithReservedKeywords parsed, EnumParseOptions parseOptions)
        => EnumWithReservedKeywordsExtensions.TryParse(name, out parsed, parseOptions);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithReservedKeywords parsed, EnumParseOptions parseOptions)
        => EnumWithReservedKeywordsExtensions.TryParse(name, out parsed, parseOptions);
#endif

    protected override EnumWithReservedKeywords Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithReservedKeywordsExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override EnumWithReservedKeywords Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithReservedKeywordsExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override EnumWithReservedKeywords Parse(string name, EnumParseOptions parseOptions)
        => EnumWithReservedKeywordsExtensions.Parse(name, parseOptions);
#if READONLYSPAN
    protected override EnumWithReservedKeywords Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumWithReservedKeywordsExtensions.Parse(name, parseOptions);
#endif
}