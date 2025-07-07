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
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#else
#error Unknown integration tests
#endif

public class ExternalEnumExtensionsTests : ExtensionTests<DateTimeKind, int, ExternalEnumExtensionsTests>, ITestData<DateTimeKind>
{
    public TheoryData<DateTimeKind> ValidEnumValues() => new()
    {
        DateTimeKind.Unspecified,
        DateTimeKind.Utc,
        (DateTimeKind)3, // not actually valid
    };

    public TheoryData<string> ValuesToParse() => new()
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

    protected override string[] GetNames() => DateTimeKindExtensions.GetNames();
    protected override DateTimeKind[] GetValues() => DateTimeKindExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => DateTimeKindExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(DateTimeKind value) => value.AsUnderlyingType();

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
}