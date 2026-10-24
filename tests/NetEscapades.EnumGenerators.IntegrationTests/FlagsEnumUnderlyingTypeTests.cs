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

public class SignedBitFlagsEnumExtensionsTests
{
    [Fact]
    public void AllIncludesSignBit()
    {
        ((sbyte)SignedBitFlagsEnumExtensions.All).Should().Be(unchecked((sbyte)0b1000_0011));
    }

    [Fact]
    public void DistinctFlagCountIncludesSignBit()
    {
        SignedBitFlagsEnumExtensions.DistinctFlagCount.Should().Be(3);
    }

    [Theory]
    [InlineData(SignedBitFlagsEnum.SignBit, true)]
    [InlineData(SignedBitFlagsEnum.First | SignedBitFlagsEnum.SignBit, true)]
    [InlineData((SignedBitFlagsEnum)4, false)]
    [InlineData((SignedBitFlagsEnum)(-1), false)]
    public void AreFlagsDefined(SignedBitFlagsEnum value, bool expected)
    {
        SignedBitFlagsEnumExtensions.AreFlagsDefined(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(SignedBitFlagsEnum.First | SignedBitFlagsEnum.Second, false)]
    [InlineData(SignedBitFlagsEnum.First | SignedBitFlagsEnum.Second | SignedBitFlagsEnum.SignBit, true)]
    [InlineData((SignedBitFlagsEnum)(-1), true)]
    public void HasAllFlags(SignedBitFlagsEnum value, bool expected)
    {
        value.HasAllFlags().Should().Be(expected);
    }

#if READONLYSPAN
    [Fact]
    public void TryGetFlagsReturnsSignBitLast()
    {
        Span<SignedBitFlagsEnum> buffer = stackalloc SignedBitFlagsEnum[SignedBitFlagsEnumExtensions.DistinctFlagCount];
        var result = ((SignedBitFlagsEnum)(-1)).TryGetFlags(buffer, out var count);
        result.Should().BeTrue();
        buffer.Slice(0, count).ToArray().Should().Equal(
            SignedBitFlagsEnum.First, SignedBitFlagsEnum.Second, SignedBitFlagsEnum.SignBit);
    }
#endif
}

public class UnsignedLongBitFlagsEnumExtensionsTests
{
    [Fact]
    public void AllIncludesHighBit()
    {
        ((ulong)UnsignedLongBitFlagsEnumExtensions.All).Should().Be((1UL << 63) | 1UL);
    }

    [Fact]
    public void DistinctFlagCountExcludesCompositeMember()
    {
        UnsignedLongBitFlagsEnumExtensions.DistinctFlagCount.Should().Be(2);
    }

    [Theory]
    [InlineData(UnsignedLongBitFlagsEnum.HighBit, true)]
    [InlineData(UnsignedLongBitFlagsEnum.FirstAndHighBit, true)]
    [InlineData((UnsignedLongBitFlagsEnum)2, false)]
    [InlineData((UnsignedLongBitFlagsEnum)ulong.MaxValue, false)]
    public void AreFlagsDefined(UnsignedLongBitFlagsEnum value, bool expected)
    {
        UnsignedLongBitFlagsEnumExtensions.AreFlagsDefined(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(UnsignedLongBitFlagsEnum.None, false)]
    [InlineData(UnsignedLongBitFlagsEnum.HighBit, true)]
    public void HasAnyFlagsWithoutArgument(UnsignedLongBitFlagsEnum value, bool expected)
    {
        value.HasAnyFlags().Should().Be(expected);
    }

    [Theory]
    [InlineData(UnsignedLongBitFlagsEnum.HighBit, false)]
    [InlineData(UnsignedLongBitFlagsEnum.FirstAndHighBit, true)]
    public void HasAllFlags(UnsignedLongBitFlagsEnum value, bool expected)
    {
        value.HasAllFlags().Should().Be(expected);
    }

#if READONLYSPAN
    [Fact]
    public void TryGetFlagsDecomposesHighBitComposite()
    {
        Span<UnsignedLongBitFlagsEnum> buffer = stackalloc UnsignedLongBitFlagsEnum[UnsignedLongBitFlagsEnumExtensions.DistinctFlagCount];
        var result = UnsignedLongBitFlagsEnum.FirstAndHighBit.TryGetFlags(buffer, out var count);
        result.Should().BeTrue();
        buffer.Slice(0, count).ToArray().Should().Equal(
            UnsignedLongBitFlagsEnum.First, UnsignedLongBitFlagsEnum.HighBit);
    }
#endif
}
