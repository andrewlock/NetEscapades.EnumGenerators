using System;
using System.ComponentModel.DataAnnotations;

namespace NetEscapades.EnumGenerators.IntegrationTests
{
    [EnumExtensions]
    public enum EnumInNamespace
    {
        First = 0,
        Second = 1,
        Third = 2,
    }
    
    [EnumExtensions]
    public enum EnumWithDisplayNameInNamespace
    {
        First = 0,

        [Display(Name = "2nd")]
        Second = 1,

        Third = 2,
    }

    [EnumExtensions]
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
        First = 0,
        Second = 1,
        Third = 2,
    }

    [EnumExtensions]
    [Flags]
    public enum FlagsEnum
    {
        First = 1 << 0,
        Second = 1 << 1,
        Third = 1 << 2,
        Fourth = 1 << 3,
        ThirdAndFourth = Third | Fourth,
    }
}