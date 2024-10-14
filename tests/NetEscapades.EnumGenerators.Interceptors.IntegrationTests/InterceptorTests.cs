using Foo;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using NetEscapades.EnumGenerators;
using Xunit;

[assembly:NetEscapades.EnumGenerators.EnumExtensions<DateTimeKind>()]
[assembly:NetEscapades.EnumGenerators.EnumExtensions<System.IO.FileShare>()]

namespace Foo
{
    // causes Error CS0426 : The type name 'TestEnum' does not exist in the type 'Foo'.
    // workaround is to use global prefix 
    public class Foo { }

    [EnumExtensions]
    public enum EnumInFoo
    {
        First = 0,
        [Display(Name = "2nd")]
        Second = 1,
        Third = 2,
    }
}

#if INTERCEPTORS && NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests
#elif INTERCEPTORS && INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests.Roslyn4_4
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests.Roslyn4_4
#else
#error Unknown project combination
#endif
{
    [EnumExtensions]
    public enum EnumWithDisplayNameInNamespace
    {
        First = 0,

        [Display(Name = "2nd")] Second = 1,

        Third = 2,
    }

    [EnumExtensions(ExtensionClassName="SomeExtension", ExtensionClassNamespace = "SomethingElse")]
    public enum EnumWithExtensionInOtherNamespace
    {
        First = 0,

        [Display(Name = "2nd")] Second = 1,

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

    [EnumExtensions]
    public enum StringTesting
    {
        [System.ComponentModel.Description("Quotes \"")]
        Quotes,

        [System.ComponentModel.Description(@"Literal Quotes """)]
        LiteralQuotes,

        [System.ComponentModel.Description("Backslash \\")]
        Backslash,

        [System.ComponentModel.Description(@"LiteralBackslash \")]
        BackslashLiteral,
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
        [Fact]
#else
        [Fact(Skip = "Interceptors are supported in this SDK")]
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
        [Fact(Skip = "Interceptors are supported in this SDK")]
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
    [Fact(Skip = "Interceptors are supported in this SDK")]
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
}