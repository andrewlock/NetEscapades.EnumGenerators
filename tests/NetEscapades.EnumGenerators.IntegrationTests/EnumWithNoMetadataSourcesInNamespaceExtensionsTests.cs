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

#nullable enable
public class EnumWithNoMetadataSourcesInNamespaceExtensionsTests : ExtensionTests<EnumWithNoMetadataSources, int, EnumWithNoMetadataSourcesInNamespaceExtensionsTests>, ITestData<EnumWithNoMetadataSources>
{
    public TheoryData<EnumWithNoMetadataSources> ValidEnumValues() => new()
    {
        EnumWithNoMetadataSources.First,
        EnumWithNoMetadataSources.Second,
        (EnumWithNoMetadataSources)3,
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

    protected override string[] GetNames() => EnumWithNoMetadataSourcesExtensions.GetNames();
    protected override EnumWithNoMetadataSources[] GetValues() => EnumWithNoMetadataSourcesExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => EnumWithNoMetadataSourcesExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(EnumWithNoMetadataSources value) => value.AsUnderlyingType();

    // Can't call the "withMetadata" versions of all these
    protected override string ToStringFast(EnumWithNoMetadataSources value) => value.ToStringFast();
    protected override string ToStringFast(EnumWithNoMetadataSources value, bool withMetadata) => value.ToStringFast();
    protected override bool IsDefined(EnumWithNoMetadataSources value) => EnumWithNoMetadataSourcesExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumWithNoMetadataSourcesExtensions.IsDefined(name);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute = false) => EnumWithNoMetadataSourcesExtensions.IsDefined(name);
#endif
    protected override bool TryParse(string name, out EnumWithNoMetadataSources parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithNoMetadataSourcesExtensions.TryParse(name, out parsed, ignoreCase);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithNoMetadataSources parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithNoMetadataSourcesExtensions.TryParse(name, out parsed, ignoreCase);
#endif
    protected override bool TryParse(string name, out EnumWithNoMetadataSources parsed, EnumParseOptions parseOptions)
        => EnumWithNoMetadataSourcesExtensions.TryParse(name, out parsed, parseOptions);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithNoMetadataSources parsed, EnumParseOptions parseOptions)
        => EnumWithNoMetadataSourcesExtensions.TryParse(name, out parsed, parseOptions);
#endif

    protected override EnumWithNoMetadataSources Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithNoMetadataSourcesExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override EnumWithNoMetadataSources Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithNoMetadataSourcesExtensions.Parse(name, ignoreCase);
#endif
    protected override EnumWithNoMetadataSources Parse(string name, EnumParseOptions parseOptions)
        => EnumWithNoMetadataSourcesExtensions.Parse(name, parseOptions);
#if READONLYSPAN
    protected override EnumWithNoMetadataSources Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumWithNoMetadataSourcesExtensions.Parse(name, parseOptions);
#endif

    protected override bool TryGetDisplayNameOrDescription(
        string? value,
#if NETCOREAPP3_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? displayName)
#else
        out string? displayName)
#endif
    {
        // no metadata
        displayName = null;
        return false;
    }
}