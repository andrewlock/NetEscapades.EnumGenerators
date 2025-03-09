using System;
using Foo;
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

public class EnumInFooExtensionsTests : ExtensionTests<EnumInFoo>
{
    public static TheoryData<EnumInFoo> ValidEnumValues() => new()
    {
        EnumInFoo.First,
        EnumInFoo.Second,
        (EnumInFoo)3,
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

    protected override string ToStringFast(EnumInFoo value) => value.ToStringFast();
    protected override string ToStringFast(EnumInFoo value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override bool IsDefined(EnumInFoo value) => EnumInFooExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumInFooExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => EnumInFooExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out EnumInFoo parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.TryParse(name, out parsed, ignoreCase);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumInFoo parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.TryParse(name, out parsed, ignoreCase);
#endif

    protected override EnumInFoo Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override EnumInFoo Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInFooExtensions.Parse(name, ignoreCase);
#endif

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumInFoo value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFastWithMetadata(EnumInFoo value) => GeneratesToStringFastWithMetadataTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumInFoo value) => GeneratesIsDefinedTest(value);

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
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseUsingSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesAsUnderlyingType(EnumInFoo value) => GeneratesAsUnderlyingTypeTest(value, value.AsUnderlyingType());

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumInFooExtensions.GetValues());

    [Fact]
    public void GeneratesGetValuesAsUnderlyingType() => GeneratesGetValuesAsUnderlyingTypeTest(EnumInFooExtensions.GetValuesAsUnderlyingType());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumInFooExtensions.GetNames());
}