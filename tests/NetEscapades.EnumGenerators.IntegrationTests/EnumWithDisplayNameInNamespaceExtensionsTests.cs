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

public class EnumWithDisplayNameInNamespaceExtensionsTests : ExtensionTests<EnumWithDisplayNameInNamespace>
{
    public static TheoryData<EnumWithDisplayNameInNamespace> ValidEnumValues() => new()
    {
        EnumWithDisplayNameInNamespace.First,
        EnumWithDisplayNameInNamespace.Second,
        (EnumWithDisplayNameInNamespace)3,
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
    protected override EnumWithDisplayNameInNamespace Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDisplayNameInNamespaceExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override EnumWithDisplayNameInNamespace Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDisplayNameInNamespaceExtensions.Parse(name, ignoreCase);
#endif
    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumWithDisplayNameInNamespace value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFastWithMetadata(EnumWithDisplayNameInNamespace value) => GeneratesToStringFastWithMetadataTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumWithDisplayNameInNamespace value) => GeneratesIsDefinedTest(value);

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
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttribute(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: true);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttributeAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: true);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: true);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: true);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: true);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: true);
#endif

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesAsUnderlyingType(EnumWithDisplayNameInNamespace value) => GeneratesAsUnderlyingTypeTest(value, value.AsUnderlyingType());

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumWithDisplayNameInNamespaceExtensions.GetValues());

    [Fact]
    public void GeneratesGetValuesAsUnderlyingType() => GeneratesGetValuesAsUnderlyingTypeTest(EnumWithDisplayNameInNamespaceExtensions.GetValuesAsUnderlyingType());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumWithDisplayNameInNamespaceExtensions.GetNames());
}