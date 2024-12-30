using System;
using FluentAssertions;
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

public class ExternalEnumExtensionsTests : ExtensionTests<DateTimeKind>
{
    public static TheoryData<DateTimeKind> ValidEnumValues() => new()
    {
        DateTimeKind.Unspecified,
        DateTimeKind.Utc,
        (DateTimeKind)3, // not actually valid
    };

    public static TheoryData<string> ValuesToParse() => new()
    {
        "Unspecified",
        "Utc",
        "Local",
        "NotLocal",
        "SomethingElse",
        "utc",
        "UTC",
        "3",
        "267",
        "-267",
        "2147483647",
        "3000000000",
        "Fourth",
        "Fifth",
    };

    protected override string ToStringFast(DateTimeKind value) => value.ToStringFast();
    protected override string ToStringFast(DateTimeKind value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override bool IsDefined(DateTimeKind value) => DateTimeKindExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => DateTimeKindExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => DateTimeKindExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out DateTimeKind parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => DateTimeKindExtensions.TryParse(name, out parsed, ignoreCase);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out DateTimeKind parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => DateTimeKindExtensions.TryParse(name, out parsed, ignoreCase);
#endif
    protected override DateTimeKind Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => DateTimeKindExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override DateTimeKind Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => DateTimeKindExtensions.Parse(name, ignoreCase);
#endif
    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(DateTimeKind value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFastWithMetadata(DateTimeKind value) => GeneratesToStringFastWithMetadataTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(DateTimeKind value) => GeneratesIsDefinedTest(value);

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

    // Ignoring order in these
    [Fact]
    public void GeneratesGetValues()
        => DateTimeKindExtensions.GetValues()
            .Should()
            .BeEquivalentTo((DateTimeKind[])Enum.GetValues(typeof(DateTimeKind)));

    [Fact]
    public void GeneratesGetNames()
        => DateTimeKindExtensions.GetNames()
            .Should()
            .BeEquivalentTo(Enum.GetNames(typeof(DateTimeKind)));
}