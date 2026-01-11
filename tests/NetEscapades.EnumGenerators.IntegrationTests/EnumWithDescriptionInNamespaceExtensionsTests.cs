using System;
using Xunit;

#if INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.IntegrationTests;
#elif PRIVATEASSETS_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests;
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

public class EnumWithDescriptionInNamespaceExtensionsTests : ExtensionTests<EnumWithDescriptionInNamespace, int, EnumWithDescriptionInNamespaceExtensionsTests>, ITestData<EnumWithDescriptionInNamespace>
{
    public TheoryData<EnumWithDescriptionInNamespace> ValidEnumValues() => new()
    {
        EnumWithDescriptionInNamespace.First,
        EnumWithDescriptionInNamespace.Second,
        (EnumWithDescriptionInNamespace)3,
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

    protected override string[] GetNames() => EnumWithDescriptionInNamespaceExtensions.GetNames();
    protected override EnumWithDescriptionInNamespace[] GetValues() => EnumWithDescriptionInNamespaceExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => EnumWithDescriptionInNamespaceExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(EnumWithDescriptionInNamespace value) => value.AsUnderlyingType();

    protected override string ToStringFast(EnumWithDescriptionInNamespace value) => value.ToStringFast();
    protected override string ToStringFast(EnumWithDescriptionInNamespace value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override string ToStringFast(EnumWithDescriptionInNamespace value, SerializationOptions options) => value.ToStringFast(options);
    protected override bool IsDefined(EnumWithDescriptionInNamespace value) => EnumWithDescriptionInNamespaceExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumWithDescriptionInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute = false) => EnumWithDescriptionInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithDescriptionInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDescriptionInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithDescriptionInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDescriptionInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithDescriptionInNamespace parsed, EnumParseOptions parseOptions)
        => EnumWithDescriptionInNamespaceExtensions.TryParse(name, out parsed, parseOptions);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithDescriptionInNamespace parsed, EnumParseOptions parseOptions)
        => EnumWithDescriptionInNamespaceExtensions.TryParse(name, out parsed, parseOptions);
#endif

    protected override EnumWithDescriptionInNamespace Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDescriptionInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override EnumWithDescriptionInNamespace Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDescriptionInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override EnumWithDescriptionInNamespace Parse(string name, EnumParseOptions parseOptions)
        => EnumWithDescriptionInNamespaceExtensions.Parse(name, parseOptions);
#if READONLYSPAN
    protected override EnumWithDescriptionInNamespace Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumWithDescriptionInNamespaceExtensions.Parse(name, parseOptions);
#endif
}