using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

#if PRIVATEASSETS_INTEGRATION_TESTS
using PackageEnumParseOptions = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.FlagsEnumExtensions.EnumParseOptions;
using PackageSerializationOptions = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.FlagsEnumExtensions.SerializationOptions;
using PackageSerializationTransform = NetEscapades.EnumGenerators.PrivateAssets.IntegrationTests.FlagsEnumExtensions.SerializationTransform;
#elif NUGET_SYSTEMMEMORY_PRIVATEASSETS_INTEGRATION_TESTS
using PackageEnumParseOptions = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.FlagsEnumExtensions.EnumParseOptions;
using PackageSerializationOptions = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.FlagsEnumExtensions.SerializationOptions;
using PackageSerializationTransform = NetEscapades.EnumGenerators.Nuget.SystemMemory.PrivateAssets.IntegrationTests.FlagsEnumExtensions.SerializationTransform;
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


#pragma warning disable NEEG001
#pragma warning disable NEEG002
#pragma warning disable NEEG003
#pragma warning disable NEEG004
#pragma warning disable NEEG005

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
    protected override string ToStringFast(FlagsEnum value, SerializationOptions options) => value.ToStringFast(Map(options));
    protected override bool IsDefined(FlagsEnum value) => FlagsEnumExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => FlagsEnumExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => FlagsEnumExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out FlagsEnum parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out FlagsEnum parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out FlagsEnum parsed, EnumParseOptions parseOptions)
        => FlagsEnumExtensions.TryParse(name, out parsed, Map(parseOptions));
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out FlagsEnum parsed, EnumParseOptions parseOptions)
        => FlagsEnumExtensions.TryParse(name, out parsed, Map(parseOptions));
#endif

    protected override FlagsEnum Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override FlagsEnum Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FlagsEnumExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override FlagsEnum Parse(string name, EnumParseOptions parseOptions)
        => FlagsEnumExtensions.Parse(name, Map(parseOptions));
#if READONLYSPAN
    protected override FlagsEnum Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => FlagsEnumExtensions.Parse(name, Map(parseOptions));
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
    [InlineData(FlagsEnum.First)]
    [InlineData(FlagsEnum.Second)]
    [InlineData(FlagsEnum.Third)]
    [InlineData(FlagsEnum.Fourth)]
    [InlineData(FlagsEnum.ThirdAndFourth)]
    public void AllContainsFlag(FlagsEnum flag)
    {
        FlagsEnumExtensions.All.HasFlag(flag).Should().BeTrue();
    }

    [Fact]
    public void AllHasExpectedValue()
    {
        ((int)FlagsEnumExtensions.All).Should().Be(0b1111); // 15
    }

    [Fact]
    public void AllIsConst()
    {
        const FlagsEnum all = FlagsEnumExtensions.All;
        all.Should().Be(FlagsEnumExtensions.All);
    }

    public static TheoryData<FlagsEnum, FlagsEnum, bool> HasAnyFlagsData => new()
    {
        // Single flag present
        { FlagsEnum.First | FlagsEnum.Second, FlagsEnum.First, true },
        // No queried flags present
        { FlagsEnum.First | FlagsEnum.Second, FlagsEnum.Third, false },
        // Partial overlap — only some flags match
        { FlagsEnum.First, FlagsEnum.First | FlagsEnum.Third, true },
        // None has no flags
        { FlagsEnum.None, FlagsEnum.First, false },
        // Querying with zero (None) — consistent with HasFlag(0) behavior
        { FlagsEnum.First, FlagsEnum.None, true },
        // Both zero
        { FlagsEnum.None, FlagsEnum.None, true },
        // Composite member overlaps
        { FlagsEnum.Third, FlagsEnum.ThirdAndFourth, true },
        // Composite member no overlap
        { FlagsEnum.First, FlagsEnum.ThirdAndFourth, false },
        // Undefined value with overlap (65 = 64|1, First = 1)
        { (FlagsEnum)65, FlagsEnum.First, true },
        // Undefined value no overlap
        { (FlagsEnum)64, FlagsEnum.First, false },
    };

    [Theory]
    [MemberData(nameof(HasAnyFlagsData))]
    public void HasAnyFlags(FlagsEnum value, FlagsEnum otherFlags, bool expected)
    {
        value.HasAnyFlags(otherFlags).Should().Be(expected);
    }

#if READONLYSPAN
    public static TheoryData<FlagsEnum, int, FlagsEnum[]> GetFlagsData => new()
    {
        // Single flag decomposes to itself
        { FlagsEnum.First, 1, new[] { FlagsEnum.First } },
        // Multiple flags decompose into individual bits, lowest first
        { FlagsEnum.First | FlagsEnum.Third, 2, new[] { FlagsEnum.First, FlagsEnum.Third } },
        // Composite member decomposes to individual bits
        { FlagsEnum.ThirdAndFourth, 2, new[] { FlagsEnum.Third, FlagsEnum.Fourth } },
        // None returns empty
        { FlagsEnum.None, 0, Array.Empty<FlagsEnum>() },
        // Undefined bits still returned
        { (FlagsEnum)65, 2, new[] { (FlagsEnum)1, (FlagsEnum)64 } },
        // All defined flags decomposes to 4 individual bits
        { FlagsEnumExtensions.All, 4, new[] { FlagsEnum.First, FlagsEnum.Second, FlagsEnum.Third, FlagsEnum.Fourth } },
        // Multiple bits set in underlying int
        { (FlagsEnum)0xFF, 8, new[] { (FlagsEnum)1, (FlagsEnum)2, (FlagsEnum)4, (FlagsEnum)8,
            (FlagsEnum)16, (FlagsEnum)32, (FlagsEnum)64, (FlagsEnum)128 } },
    };

    [Theory]
    [MemberData(nameof(GetFlagsData))]
    public void GetFlags(FlagsEnum value, int expectedCount, FlagsEnum[] expectedFlags)
    {
        Span<FlagsEnum> buffer = stackalloc FlagsEnum[32];
        var count = value.GetFlags(buffer);
        count.Should().Be(expectedCount);
        buffer.Slice(0, count).ToArray().Should().Equal(expectedFlags);
    }
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