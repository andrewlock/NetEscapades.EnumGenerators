using System;
using Xunit;

#if PRIVATEASSETS_INTEGRATION_TESTS
using PackageEnumParseOptions = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.EnumInNamespaceExtensions.EnumParseOptions;
using PackageSerializationOptions = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.EnumInNamespaceExtensions.SerializationOptions;
using PackageSerializationTransform = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.EnumInNamespaceExtensions.SerializationTransform;
#elif NUGET_SYSTEMMEMORY_PRIVATEASSETS_INTEGRATION_TESTS
using PackageEnumParseOptions = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.EnumInNamespaceExtensions.EnumParseOptions;
using PackageSerializationOptions = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.EnumInNamespaceExtensions.SerializationOptions;
using PackageSerializationTransform = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.EnumInNamespaceExtensions.SerializationTransform;
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
    protected override string ToStringFast(EnumInNamespace value, SerializationOptions options) => value.ToStringFast(Map(options));
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
        => EnumInNamespaceExtensions.TryParse(name, out parsed, Map(parseOptions));
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumInNamespace parsed, EnumParseOptions parseOptions)
        => EnumInNamespaceExtensions.TryParse(name, out parsed, Map(parseOptions));
#endif

    protected override EnumInNamespace Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override EnumInNamespace Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override EnumInNamespace Parse(string name, EnumParseOptions parseOptions)
        => EnumInNamespaceExtensions.Parse(name, Map(parseOptions));
#if READONLYSPAN
    protected override EnumInNamespace Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumInNamespaceExtensions.Parse(name, Map(parseOptions));
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