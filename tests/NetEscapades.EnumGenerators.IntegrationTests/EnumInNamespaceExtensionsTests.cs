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

public class EnumInNamespaceExtensionsTests : ExtensionTests<EnumInNamespace>
{
    public static TheoryData<EnumInNamespace> ValidEnumValues() => new()
    {
        EnumInNamespace.First,
        EnumInNamespace.Second,
        (EnumInNamespace)3,
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

    protected override string ToStringFast(EnumInNamespace value) => value.ToStringFast();
    protected override string ToStringFast(EnumInNamespace value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override bool IsDefined(EnumInNamespace value) => EnumInNamespaceExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => EnumInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out EnumInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.TryParse(name, out parsed, ignoreCase);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.TryParse(name, out parsed, ignoreCase);
#endif

    protected override EnumInNamespace Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override EnumInNamespace Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumInNamespaceExtensions.Parse(name, ignoreCase);
#endif

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumInNamespace value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFastWithMetadata(EnumInNamespace value) => GeneratesToStringFastWithMetadataTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumInNamespace value) => GeneratesIsDefinedTest(value);

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
    public void GeneratesAsUnderlyingType(EnumInNamespace value) => GeneratesAsUnderlyingTypeTest(value, value.AsUnderlyingType());

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumInNamespaceExtensions.GetValues());

    [Fact]
    public void GeneratesGetValuesAsUnderlyingType() => GeneratesGetValuesAsUnderlyingTypeTest(EnumInNamespaceExtensions.GetValuesAsUnderlyingType());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumInNamespaceExtensions.GetNames());
}