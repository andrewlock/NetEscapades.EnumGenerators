using System.ComponentModel.DataAnnotations;
using Xunit;

#if NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
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