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

public class EnumInNamespaceExtensionsTests : ExtensionTests<EnumInNamespace, int, EnumInNamespaceExtensionsTests>, ITestData<EnumInNamespace>
{
    public TheoryData<EnumInNamespace> ValidEnumValues() => new()
    {
        EnumInNamespace.First,
        EnumInNamespace.Second,
        (EnumInNamespace)3,
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

    protected override string[] GetNames() => EnumInNamespaceExtensions.GetNames();
    protected override EnumInNamespace[] GetValues() => EnumInNamespaceExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => EnumInNamespaceExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(EnumInNamespace value) => value.AsUnderlyingType();

    protected override string ToStringFast(EnumInNamespace value) => value.ToStringFast();
    protected override string ToStringFast(EnumInNamespace value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override bool IsDefined(EnumInNamespace value) => EnumInNamespaceExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => EnumInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumInNamespace parsed, EnumParseOptions parseOptions)
        => EnumInNamespaceExtensions.TryParse(name, out parsed, parseOptions);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumInNamespace parsed, EnumParseOptions parseOptions)
        => EnumInNamespaceExtensions.TryParse(name, out parsed, parseOptions);
#endif

    protected override EnumInNamespace Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override EnumInNamespace Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override EnumInNamespace Parse(string name, EnumParseOptions parseOptions)
        => EnumInNamespaceExtensions.Parse(name, parseOptions);
#if READONLYSPAN
    protected override EnumInNamespace Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumInNamespaceExtensions.Parse(name, parseOptions);
#endif
}