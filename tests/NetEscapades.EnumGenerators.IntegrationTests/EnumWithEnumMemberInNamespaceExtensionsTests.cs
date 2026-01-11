using System;
using Xunit;

#if PRIVATEASSETS_INTEGRATION_TESTS
using PackageEnumParseOptions = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.EnumWithEnumMemberInNamespaceExtensions.EnumParseOptions;
using PackageSerializationOptions = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.EnumWithEnumMemberInNamespaceExtensions.SerializationOptions;
using PackageSerializationTransform = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.EnumWithEnumMemberInNamespaceExtensions.SerializationTransform;
#elif NUGET_SYSTEMMEMORY_PRIVATEASSETS_INTEGRATION_TESTS
using PackageEnumParseOptions = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.EnumWithEnumMemberInNamespaceExtensions.EnumParseOptions;
using PackageSerializationOptions = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.EnumWithEnumMemberInNamespaceExtensions.SerializationOptions;
using PackageSerializationTransform = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.EnumWithEnumMemberInNamespaceExtensions.SerializationTransform;
#else
using PackageEnumParseOptions = NetEscapades.EnumGenerators.EnumParseOptions;
using PackageSerializationOptions = NetEscapades.EnumGenerators.SerializationOptions;
using PackageSerializationTransform = NetEscapades.EnumGenerators.SerializationTransform;
#endif


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

public class EnumWithEnumMemberInNamespaceExtensionsTests : ExtensionTests<EnumWithEnumMemberInNamespace, int, EnumWithEnumMemberInNamespaceExtensionsTests>, ITestData<EnumWithEnumMemberInNamespace>
{
    public TheoryData<EnumWithEnumMemberInNamespace> ValidEnumValues() => new()
    {
        EnumWithEnumMemberInNamespace.First,
        EnumWithEnumMemberInNamespace.Second,
        (EnumWithEnumMemberInNamespace)3,
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

    protected override string[] GetNames() => EnumWithEnumMemberInNamespaceExtensions.GetNames();
    protected override EnumWithEnumMemberInNamespace[] GetValues() => EnumWithEnumMemberInNamespaceExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => EnumWithEnumMemberInNamespaceExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(EnumWithEnumMemberInNamespace value) => value.AsUnderlyingType();

    protected override string ToStringFast(EnumWithEnumMemberInNamespace value) => value.ToStringFast();
    protected override string ToStringFast(EnumWithEnumMemberInNamespace value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override string ToStringFast(EnumWithEnumMemberInNamespace value, SerializationOptions options) => value.ToStringFast(Map(options));
    protected override bool IsDefined(EnumWithEnumMemberInNamespace value) => EnumWithEnumMemberInNamespaceExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumWithEnumMemberInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute = false) => EnumWithEnumMemberInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithEnumMemberInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithEnumMemberInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithEnumMemberInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithEnumMemberInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithEnumMemberInNamespace parsed, EnumParseOptions parseOptions)
        => EnumWithEnumMemberInNamespaceExtensions.TryParse(name, out parsed, Map(parseOptions));
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithEnumMemberInNamespace parsed, EnumParseOptions parseOptions)
        => EnumWithEnumMemberInNamespaceExtensions.TryParse(name, out parsed, Map(parseOptions));
#endif

    protected override EnumWithEnumMemberInNamespace Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithEnumMemberInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override EnumWithEnumMemberInNamespace Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithEnumMemberInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override EnumWithEnumMemberInNamespace Parse(string name, EnumParseOptions parseOptions)
        => EnumWithEnumMemberInNamespaceExtensions.Parse(name, Map(parseOptions));
#if READONLYSPAN
    protected override EnumWithEnumMemberInNamespace Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumWithEnumMemberInNamespaceExtensions.Parse(name, Map(parseOptions));
#endif
    
    private PackageEnumParseOptions Map(EnumParseOptions options)
        => new(comparisonType: options.ComparisonType,
            allowMatchingMetadataAttribute: options.AllowMatchingMetadataAttribute,
            enableNumberParsing: options.EnableNumberParsing);

    private PackageSerializationOptions Map(SerializationOptions options)
        => new(useMetadataAttributes: options.UseMetadataAttributes,
            transform: Map(options.Transform));

    private PackageSerializationTransform Map(SerializationTransform options)
        => options switch
        {
            SerializationTransform.LowerInvariant => PackageSerializationTransform.LowerInvariant,
            SerializationTransform.UpperInvariant => PackageSerializationTransform.UpperInvariant,
            SerializationTransform.None => PackageSerializationTransform.None,
            _ => throw new InvalidOperationException("Unknown options type " + options),
        };
}