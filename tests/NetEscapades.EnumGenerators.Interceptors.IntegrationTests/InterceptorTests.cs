using Foo;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using NetEscapades.EnumGenerators;
using Xunit;

#if INTERCEPTORS && NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#elif INTERCEPTORS && INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif INTERCEPTORS && NETSTANDARD_INTERCEPTOR_TESTS
using NetEscapades.EnumGenerators.NetStandard.IntegrationTests;

namespace NetEscapades.EnumGenerators.NetStandard.Interceptors.IntegrationTests;
#elif INTERCEPTORS && NUGET_NETSTANDARD_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.NetStandard.Interceptors.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests.Roslyn4_4;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests.Roslyn4_4;
#elif NETSTANDARD_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.Interceptors.IntegrationTests.Roslyn4_4;
#elif NUGET_NETSTANDARD_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.NetStandard.Interceptors.IntegrationTests.Roslyn4_4;
#else
#error Unknown project combination
#endif

public class InterceptorTests
{
#if INTERCEPTORS
    [Fact]
#else
    [Fact(Skip = "Interceptors are not supported in this SDK")]
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
    [Fact]
#else
    [Fact(Skip = "Interceptors are not supported in this SDK")]
#endif
    public void CallingToStringIsIntercepted_StringTesting()
    {
        var result1 = StringTesting.Backslash.ToString();
        var result2 = StringTesting.Backslash.ToStringFast();
        Assert.Equal(result1, result2);
    }

#if INTERCEPTORS
    [Fact]
#else
    [Fact(Skip = "Interceptors are not supported in this SDK")]
#endif
    public void CallingToStringIsIntercepted_EnumInFoo()
    {
        var result1 = EnumInFoo.Second.ToString();
        var result2 = EnumInFoo.Second.ToStringFast();
        Assert.Equal(result1, result2);
    }

#if INTERCEPTORS
    [Fact]
#else
    [Fact(Skip = "Interceptors are not supported in this SDK")]
#endif
    public void CallingToStringIsIntercepted_EnumWithExtensionInOtherNamespace()
    {
        var result1 = EnumWithExtensionInOtherNamespace.Second.ToString();
        var result2 = SomethingElse.SomeExtension.ToStringFast(EnumWithExtensionInOtherNamespace.Second);
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void CallingToStringIsIntercepted_ExternalEnum()
    {
        // This doesn't _actually_ test interception, because can't
        // differentiate with built-in version, it's only really verifying the generated code compiles 
        var result1 = DateTimeKind.Local.ToString();
        var result2 = DateTimeKind.Local.ToStringFast();
        Assert.Equal(result1, result2);
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

    [Fact]
    public void CallingHasFlagIsIntercepted_ExternalEnumFlags()
    {
        // This doesn't _actually_ test interception, because can't
        // differentiate with built-in version, it's only really verifying the generated code compiles 
        var value1 = FileShare.Read;
        var result2 = FileShare.Write.HasFlag(value1);
        Assert.False(result2);
        Assert.True(value1.HasFlag(FileShare.None));

        var combined = FileShare.Read | FileShare.Write;
        Assert.True(combined.HasFlag(FileShare.Read));
        Assert.False(FileShare.Read.HasFlag(combined));
    }

    [Fact]
    public void CallingNonInterceptableEnumIsNotIntercepted()
    {
        var result1 = NonInterceptableEnum.Second.ToString();
        var result2 = NonInterceptableEnum.Second.ToStringFast();
        Assert.NotEqual(result1, result2);
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