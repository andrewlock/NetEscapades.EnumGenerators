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

#if NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests
#else
#error Unknown project combination
#endif
{
    [EnumExtensions]
    public enum EnumWithDisplayNameInNamespace
    {
        First = 0,

        [System.ComponentModel.Description("2nd")] Second = 1,

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
        First = 1 << 0,
        Second = 1 << 1,
        Third = 1 << 2,
        Fourth = 1 << 3,
        ThirdAndFourth = Third | Fourth,
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

        [System.ComponentModel.Description("Line\nBreak")]
        LineBreak,
    }

    [EnumExtensions(IsInterceptable = false)]
    public enum NonInterceptableEnum
    {
        First = 0,
        [Display(Name = "2nd")] Second = 1,
        Third = 2,
    }
}