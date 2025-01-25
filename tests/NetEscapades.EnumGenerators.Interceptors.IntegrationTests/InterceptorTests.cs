using Foo;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using NetEscapades.EnumGenerators;
using Xunit;

#if NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NETSTANDARD_INTERCEPTOR_TESTS
using NetEscapades.EnumGenerators.NetStandard.IntegrationTests;

namespace NetEscapades.EnumGenerators.NetStandard.Interceptors.IntegrationTests;
#elif NUGET_NETSTANDARD_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.NetStandard.Interceptors.IntegrationTests;
#else
#error Unknown project combination
#endif

public class InterceptorTests
{
    [Fact]
    public void CallingToStringIsIntercepted()
    {
        AssertValue(EnumWithDisplayNameInNamespace.First);
        AssertValue(EnumWithDisplayNameInNamespace.Second);
        AssertValue(EnumWithDisplayNameInNamespace.Third);

        void AssertValue(EnumWithDisplayNameInNamespace value)
        {
            // This doesn't _actually_ test interception, because can't
            // differentiate with built-in version, it's only really verifying the generated code compiles 
            var toString = value.ToString();
            var fast = value.ToStringFast();
            Assert.Equal(fast, toString);
        }
    }

    [Fact]
    public void CallingToStringIsIntercepted_StringTesting()
    {
#pragma warning disable CS0612
        var result1 = StringTesting.Backslash.ToString();
        var result2 = StringTesting.Backslash.ToStringFast();
#pragma warning restore CS0612
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void CallingToStringIsIntercepted_EnumInFoo()
    {
        // This doesn't _actually_ test interception, because can't
        // differentiate with built-in version, it's only really verifying the generated code compiles 
        var result1 = EnumInFoo.Second.ToString();
        var result2 = EnumInFoo.Second.ToStringFast();
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void CallingToStringIsIntercepted_EnumWithExtensionInOtherNamespace()
    {
        // This doesn't _actually_ test interception, because can't
        // differentiate with built-in version, it's only really verifying the generated code compiles 
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

    [Fact(Skip = "Can't actually verify it's not interceptable")]
    public void CallingNonInterceptableEnumIsNotIntercepted()
    {
        var result1 = NonInterceptableEnum.Second.ToString();
        var result2 = NonInterceptableEnum.Second.ToStringFast();
        Assert.NotEqual(result1, result2);
    }
}