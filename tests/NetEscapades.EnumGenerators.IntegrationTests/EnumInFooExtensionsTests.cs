using System;
using Foo;
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

public class EnumInFooExtensionsTests : ExtensionTests<EnumInFoo, int, EnumInFooExtensionsTests>, ITestData<EnumInFoo>
{
    public TheoryData<EnumInFoo> ValidEnumValues() => new()
    {
        EnumInFoo.First,
        EnumInFoo.Second,
        (EnumInFoo)3,
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

    protected override string[] GetNames() => EnumInFooExtensions.GetNames();
    protected override EnumInFoo[] GetValues() => EnumInFooExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => EnumInFooExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(EnumInFoo value) => value.AsUnderlyingType();

    protected override string ToStringFast(EnumInFoo value) => value.ToStringFast();
    protected override string ToStringFast(EnumInFoo value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override string ToStringFast(EnumInFoo value, SerializationOptions options) => value.ToStringFast(options);
    protected override bool IsDefined(EnumInFoo value) => EnumInFooExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumInFooExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => EnumInFooExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumInFoo parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumInFoo parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumInFoo parsed, EnumParseOptions parseOptions)
        => EnumInFooExtensions.TryParse(name, out parsed, parseOptions);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumInFoo parsed, EnumParseOptions parseOptions)
        => EnumInFooExtensions.TryParse(name, out parsed, parseOptions);
#endif

    protected override EnumInFoo Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override EnumInFoo Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override EnumInFoo Parse(string name, EnumParseOptions parseOptions)
        => EnumInFooExtensions.Parse(name, parseOptions);
#if READONLYSPAN
    protected override EnumInFoo Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => EnumInFooExtensions.Parse(name, parseOptions);
#endif
}