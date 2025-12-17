using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

#pragma warning disable NEEG003

[assembly:NetEscapades.EnumGenerators.EnumExtensions<DateTimeKind>()]
[assembly:NetEscapades.EnumGenerators.EnumExtensions<System.IO.FileShare>()]

namespace System
{
    using NetEscapades.EnumGenerators;

    [EnumExtensions]
    public enum EnumInSystem
    {
        First = 0,
        Second = 1,
        Third = 2,
    }

    [EnumExtensions(IsInterceptable = false)]
    public enum NonInterceptableEnum
    {
        First = 0,
        [EnumMember(Value = "2nd")] Second = 1,
        Third = 2,
    }
}

namespace Foo
{
    using NetEscapades.EnumGenerators;

    // causes Error CS0426 : The type name 'TestEnum' does not exist in the type 'Foo'.
    // workaround is to use global prefix 

    public class Foo
    {
    }

    [EnumExtensions]
    public enum EnumInFoo
    {
        First = 0,
        [EnumMember(Value = "2nd")]
        Second = 1,
        Third = 2,
    }
}


#if INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.IntegrationTests
#elif NETSTANDARD_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.IntegrationTests
#elif NETSTANDARD_SYSTEMMEMORY_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.SystemMemory.IntegrationTests
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests
#elif NUGET_NETSTANDARD_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.NetStandard.Interceptors.IntegrationTests
#elif NUGET_SYSTEMMEMORY_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.SystemMemory.IntegrationTests
#else
#error Unknown integration tests
#endif
{
    [EnumExtensions]
    public enum EnumInNamespace
    {
        First = 0,
        Second = 1,
        Third = 2,
    }
    
    [EnumExtensions(MetadataSource = MetadataSource.DisplayAttribute)]
    public enum EnumWithDisplayNameInNamespace
    {
        First = 0,

        [Display(Name = "2nd")]
        Second = 1,

        Third = 2,
    }

    [EnumExtensions(MetadataSource = MetadataSource.DescriptionAttribute)]
    public enum EnumWithDescriptionInNamespace
    {
        First = 0,

        [Description("2nd")]
        Second = 1,

        Third = 2,
    }

    [EnumExtensions(MetadataSource = MetadataSource.EnumMemberAttribute)]
    public enum EnumWithEnumMemberInNamespace
    {
        First = 0,

        [EnumMember(Value = "2nd")]
        Second = 1,

        Third = 2,
    }

    [EnumExtensions(MetadataSource = MetadataSource.None)]
    public enum EnumWithNoMetadataSources
    {
        First = 0,

        [EnumMember(Value = "2nd")]
        Second = 1,

        Third = 2,
    }

    [EnumExtensions(MetadataSource = MetadataSource.DisplayAttribute)]
    public enum EnumWithSameDisplayName
    {
        First = 0,

        [Display(Name = "2nd")]
        Second = 1,

        [Display(Name = "2nd")]
        Third = 2,
    }

    [EnumExtensions]
    public enum LongEnum: long
    {
        Second = 1,
        First = 0,
        Third = 2,
    }

    [EnumExtensions]
    [Flags]
    public enum FlagsEnum
    {
        None = 0,
        Second = 1 << 1,
        First = 1 << 0,
        Third = 1 << 2,
        Fourth = 1 << 3,
        ThirdAndFourth = Third | Fourth,
    }

    [EnumExtensions(MetadataSource = MetadataSource.DescriptionAttribute)]
    public enum StringTesting
    {
        [System.ComponentModel.Description("Quotes \"")]   Quotes,
        [System.ComponentModel.Description(@"Literal Quotes """)]   LiteralQuotes,
        [Obsolete]
        [System.ComponentModel.Description("Backslash \\")]   Backslash,
        [System.ComponentModel.Description(@"LiteralBackslash \")]   BackslashLiteral,
        [System.ComponentModel.Description("Line\nBreak")]   LineBreak,
    }

    [EnumExtensions(ExtensionClassName="SomeExtension", ExtensionClassNamespace = "SomethingElse")]
    public enum EnumWithExtensionInOtherNamespace
    {
        First = 0,

        [EnumMember(Value = "2nd")] Second = 1,

        Third = 2,
    }

    [EnumExtensions]
    public enum EnumWithRepeatedValues
    {
        First = 0,
        Second = 1,
        Third = 0, // Repeated value
        Fourth = Second, // Repeated value
        Fifth = Second + Third, // Repeated value
    }

    [EnumExtensions]
    public enum EnumWithRepeatedValuesWithDisplayNames
    {
        [EnumMember(Value = "Main")] First = 0,
        Second = 1,
        [EnumMember(Value = "Repeated")] Third = 0, // Repeated value with display name
    }

    [EnumExtensions(MetadataSource = MetadataSource.DescriptionAttribute)]
    public enum EnumWithReservedKeywords
    {
        [Description("number")]
        number,
        [Description("string")]
        @string,
        [Description("date")]
        date,
        [Description("class")]
        @class,
    }
}