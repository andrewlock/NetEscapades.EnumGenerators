using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
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

public class FlagsEnumExtensionsTests : ExtensionTests<FlagsEnum, int, FlagsEnumExtensionsTests>, ITestData<FlagsEnum>
{
    public TheoryData<FlagsEnum> ValidEnumValues() => new()
    {
        FlagsEnum.First,
        FlagsEnum.Second,
        FlagsEnum.ThirdAndFourth,
        (FlagsEnum)3,
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

    protected override string[] GetNames() => FlagsEnumExtensions.GetNames();
    protected override FlagsEnum[] GetValues() => FlagsEnumExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => FlagsEnumExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(FlagsEnum value) => value.AsUnderlyingType();

    protected override string ToStringFast(FlagsEnum value) => value.ToStringFast();
    protected override string ToStringFast(FlagsEnum value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override bool IsDefined(FlagsEnum value) => FlagsEnumExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => FlagsEnumExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => FlagsEnumExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out FlagsEnum parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.TryParse(name, out parsed, ignoreCase);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out FlagsEnum parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.TryParse(name, out parsed, ignoreCase);
#endif
    protected override FlagsEnum Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override FlagsEnum Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.Parse(name, ignoreCase);
#endif

    public static IEnumerable<object[]> AllFlags()
    {
        var values = new[]
        {
            FlagsEnum.First,
            FlagsEnum.Second,
            FlagsEnum.Third,
            FlagsEnum.ThirdAndFourth,
            FlagsEnum.First | FlagsEnum.Second,
            (FlagsEnum)65,
            (FlagsEnum)0,
        };

        return from v1 in values
            from v2 in values
            select new object[] { v1, v2 };
    }
    
    [Theory]
    [MemberData(nameof(AllFlags))]
    public void HasFlags(FlagsEnum value, FlagsEnum flag)
    {
        var isDefined = value.HasFlagFast(flag);

        isDefined.Should().Be(value.HasFlag(flag));
    }
}