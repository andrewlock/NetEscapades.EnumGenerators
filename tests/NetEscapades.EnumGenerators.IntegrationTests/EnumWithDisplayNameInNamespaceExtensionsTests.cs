using System;
using Xunit;

#if INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.IntegrationTests;
#elif NETSTANDARD_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#else
#error Unknown integration tests
#endif

public class EnumWithDisplayNameInNamespaceExtensionsTests : ExtensionTests<EnumWithDisplayNameInNamespace, int, EnumWithDisplayNameInNamespaceExtensionsTests>, ITestData<EnumWithDisplayNameInNamespace>
{
    public TheoryData<EnumWithDisplayNameInNamespace> ValidEnumValues() => new()
    {
        EnumWithDisplayNameInNamespace.First,
        EnumWithDisplayNameInNamespace.Second,
        (EnumWithDisplayNameInNamespace)3,
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

    protected override string[] GetNames() => EnumWithDisplayNameInNamespaceExtensions.GetNames();
    protected override EnumWithDisplayNameInNamespace[] GetValues() => EnumWithDisplayNameInNamespaceExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => EnumWithDisplayNameInNamespaceExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(EnumWithDisplayNameInNamespace value) => value.AsUnderlyingType();

    protected override string ToStringFast(EnumWithDisplayNameInNamespace value) => value.ToStringFast();
    protected override string ToStringFast(EnumWithDisplayNameInNamespace value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override bool IsDefined(EnumWithDisplayNameInNamespace value) => EnumWithDisplayNameInNamespaceExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumWithDisplayNameInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute = false) => EnumWithDisplayNameInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithDisplayNameInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDisplayNameInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithDisplayNameInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDisplayNameInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithDisplayNameInNamespace parsed, EnumParseOptions parseOptions)
        => EnumWithDisplayNameInNamespaceExtensions.TryParse(name, out parsed, parseOptions);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithDisplayNameInNamespace parsed, EnumParseOptions parseOptions)
        => EnumWithDisplayNameInNamespaceExtensions.TryParse(name, out parsed, parseOptions);
#endif

    protected override EnumWithDisplayNameInNamespace Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDisplayNameInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override EnumWithDisplayNameInNamespace Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDisplayNameInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override EnumWithDisplayNameInNamespace Parse(string name, EnumParseOptions parseOptions)
        => EnumWithDisplayNameInNamespaceExtensions.Parse(name, parseOptions);
#if READONLYSPAN
    protected override EnumWithDisplayNameInNamespace Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumWithDisplayNameInNamespaceExtensions.Parse(name, parseOptions);
#endif
}