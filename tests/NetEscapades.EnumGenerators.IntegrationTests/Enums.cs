using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace System
{
    using NetEscapades.EnumGenerators;

    [EnumExtensions]
    [EnumJsonConverter(typeof(EnumInSystemConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(EnumInSystem))]
    [JsonConverter(typeof(EnumInSystemConverter))]
    public enum EnumInSystem
    {
        First = 0,
        Second = 1,
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
    [EnumJsonConverter(typeof(EnumInFooConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(EnumInFoo))]
    [JsonConverter(typeof(EnumInFooConverter))]
    public enum EnumInFoo
    {
        First = 0,
        Second = 1,
        Third = 2,
    }
}

namespace NetEscapades.EnumGenerators.IntegrationTests
{
    [EnumExtensions]
    [EnumJsonConverter(typeof(EnumInNamespaceConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(EnumInNamespace))]
    [JsonConverter(typeof(EnumInNamespaceConverter))]
    public enum EnumInNamespace
    {
        First = 0,
        Second = 1,
        Third = 2,
    }
    
    [EnumExtensions]
    [EnumJsonConverter(typeof(EnumWithDisplayNameInNamespaceConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(EnumWithDisplayNameInNamespace))]
    [JsonConverter(typeof(EnumWithDisplayNameInNamespaceConverter))]
    public enum EnumWithDisplayNameInNamespace
    {
        First = 0,

        [Display(Name = "2nd")]
        Second = 1,

        Third = 2,
    }

    [EnumExtensions]
    [EnumJsonConverter(typeof(EnumWithDescriptionInNamespaceConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(EnumWithDescriptionInNamespace))]
    [JsonConverter(typeof(EnumWithDescriptionInNamespaceConverter))]
    public enum EnumWithDescriptionInNamespace
    {
        First = 0,

        [Description("2nd")]
        Second = 1,

        Third = 2,
    }

    [EnumExtensions]
    [EnumJsonConverter(typeof(EnumWithSameDisplayNameConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(EnumWithSameDisplayName))]
    [JsonConverter(typeof(EnumWithSameDisplayNameConverter))]
    public enum EnumWithSameDisplayName
    {
        First = 0,

        [Display(Name = "2nd")]
        Second = 1,

        [Display(Name = "2nd")]
        Third = 2,
    }

    [EnumExtensions]
    [EnumJsonConverter(typeof(LongEnumConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(LongEnum))]
    [JsonConverter(typeof(LongEnumConverter))]
    public enum LongEnum: long
    {
        First = 0,
        Second = 1,
        Third = 2,
    }

    [EnumExtensions]
    [EnumJsonConverter(typeof(FlagEnumsConverter), CamelCase = true, CaseSensitive = false, AllowMatchingMetadataAttribute = true, PropertyName = nameof(FlagsEnum))]
    [JsonConverter(typeof(FlagEnumsConverter))]
    [Flags]
    public enum FlagsEnum
    {
        First = 1 << 0,
        Second = 1 << 1,
        Third = 1 << 2,
        Fourth = 1 << 3,
        ThirdAndFourth = Third | Fourth,
    }

    public enum StringTesting
    {
        [System.ComponentModel.Description("Quotes \"")]   Quotes,
        [System.ComponentModel.Description(@"Literal Quotes """)]   LiteralQuotes,
        [System.ComponentModel.Description("Backslash \\")]   Backslash,
        [System.ComponentModel.Description(@"LiteralBackslash \")]   BackslashLiteral,
    }
}