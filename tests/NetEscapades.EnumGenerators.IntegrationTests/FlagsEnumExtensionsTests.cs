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

public class FlagsEnumExtensionsTests : ExtensionTests<FlagsEnum>
{
    public static TheoryData<FlagsEnum> ValidEnumValues() => new()
    {
        FlagsEnum.First,
        FlagsEnum.Second,
        FlagsEnum.ThirdAndFourth,
        (FlagsEnum)3,
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <remarks>If the underlying value of <paramref name="flag"/> is zero, the method returns true.
    /// This is consistent with the behaviour of <see cref=""Enum.HasFlag(Enum)""></remarks>
    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(FlagsEnum value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFastWithMetadata(FlagsEnum value) => GeneratesToStringFastWithMetadataTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(FlagsEnum value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: false);
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
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesAsUnderlyingType(FlagsEnum value) => GeneratesAsUnderlyingTypeTest(value, value.AsUnderlyingType());

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(FlagsEnumExtensions.GetValues());

    [Fact]
    public void GeneratesGetValuesAsUnderlyingType() => GeneratesGetValuesAsUnderlyingTypeTest(FlagsEnumExtensions.GetValuesAsUnderlyingType());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(FlagsEnumExtensions.GetNames());
}