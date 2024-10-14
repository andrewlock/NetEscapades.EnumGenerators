using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

#if INTERCEPTORS && NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#elif INTERCEPTORS && INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests.Roslyn4_4;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests.Roslyn4_4;
#else
#error Unknown project combination
#endif

[EnumExtensions]
public enum EnumWithDisplayNameInNamespace
{
    First = 0,

    [Display(Name = "2nd")]
    Second = 1,

    Third = 2,
}

[EnumExtensions]
[Flags]
public enum FlagsEnum
{
    None = 0,
    First = 1,
    Second = 2,
}

public class InterceptorTests
{
#if INTERCEPTORS
    [Fact]
#else
    [Fact(Skip = "Interceptors are supported in this SDK")]
#endif
    public void CallingToStringIsIntercepted()
    {
        var result1 = EnumWithDisplayNameInNamespace.Second.ToString();
        var result2 = EnumWithDisplayNameInNamespace.Second.ToStringFast();
        Assert.Equal(result1, result2);

        AssertValue(EnumWithDisplayNameInNamespace.First);
        AssertValue(EnumWithDisplayNameInNamespace.Second);
        AssertValue(EnumWithDisplayNameInNamespace.Third);

        void AssertValue(EnumWithDisplayNameInNamespace value)
        {
            // These return different values when interception is not enabled
            var toString = value.ToString();
            var fast = value.ToStringFast();
            Assert.Equal(fast, toString);
        }
    }

    [Fact]
    public void CallingHasFlagIsIntercepted()
    {
        // This doesn't _actually_ test interception, because can't
        // differentiate with built-in version, it's only really verifying the generated code compiles
        var value1 = FlagsEnum.First;
        var result2 = FlagsEnum.Second.HasFlag(value1);
        Assert.False(result2);
        Assert.True(value1.HasFlag(FlagsEnum.None));

        var combined = FlagsEnum.First | FlagsEnum.Second;
        Assert.True(combined.HasFlag(FlagsEnum.First));
        Assert.False(FlagsEnum.First.HasFlag(combined));
    }

#if INTERCEPTORS
    [Fact(Skip = "Interceptors are supported in this SDK")]
#else
    [Fact]
#endif
    public void CallingToStringIsNotIntercepted()
    {
        var result1 = EnumWithDisplayNameInNamespace.Second.ToString();
        var result2 = EnumWithDisplayNameInNamespace.Second.ToStringFast();
        Assert.NotEqual(result1, result2);
    }
}