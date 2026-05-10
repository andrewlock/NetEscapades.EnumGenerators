using FluentAssertions;
using System;
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

public class BitFlagsEnumExtensionsTests
{
    [Fact]
    public void DistinctFlagCountExcludesNone()
    {
        BitFlagsEnumExtensions.DistinctFlagCount.Should().Be(4);
    }

#if READONLYSPAN
    public static TheoryData<BitFlagsEnum, int, BitFlagsEnum[]> TryGetFlagsData => new()
    {
        // Single flag → itself
        { BitFlagsEnum.First, 1, new[] { BitFlagsEnum.First } },
        // Multiple flags — bit-value order
        { BitFlagsEnum.First | BitFlagsEnum.Third, 2, new[] { BitFlagsEnum.First, BitFlagsEnum.Third } },
        // None → empty output
        { BitFlagsEnum.None, 0, Array.Empty<BitFlagsEnum>() },
        // Undefined bits silently dropped (16 is not defined)
        { (BitFlagsEnum)17, 1, new[] { BitFlagsEnum.First } },
        // Only undefined bits → empty output
        { (BitFlagsEnum)16, 0, Array.Empty<BitFlagsEnum>() },
        // All defined flags set → 4 single-bit members
        { BitFlagsEnumExtensions.All, 4,
            new[] { BitFlagsEnum.First, BitFlagsEnum.Second, BitFlagsEnum.Third, BitFlagsEnum.Fourth } },
        // All bits set across underlying type → still just the 4 defined flags
        { (BitFlagsEnum)0xFF, 4,
            new[] { BitFlagsEnum.First, BitFlagsEnum.Second, BitFlagsEnum.Third, BitFlagsEnum.Fourth } },
    };

    [Theory]
    [MemberData(nameof(TryGetFlagsData))]
    public void TryGetFlags(BitFlagsEnum value, int expectedCount, BitFlagsEnum[] expectedFlags)
    {
        Span<BitFlagsEnum> buffer = stackalloc BitFlagsEnum[BitFlagsEnumExtensions.DistinctFlagCount];
        var result = value.TryGetFlags(buffer, out var count);
        result.Should().BeTrue();
        count.Should().Be(expectedCount);
        buffer.Slice(0, count).ToArray().Should().Equal(expectedFlags);
    }

    [Fact]
    public void TryGetFlags_BufferSmallerThanDistinctFlagCount_ReturnsFalse()
    {
        Span<BitFlagsEnum> buffer = stackalloc BitFlagsEnum[BitFlagsEnumExtensions.DistinctFlagCount - 1];
        var result = BitFlagsEnum.First.TryGetFlags(buffer, out var count);
        result.Should().BeFalse();
        count.Should().Be(0);
    }
#endif
}
