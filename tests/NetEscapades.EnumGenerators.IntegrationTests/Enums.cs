using System;

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
        First = 1,
        Second = 1<<2,
        Third = 1 << 3,
        Fourth = 1 << 4,
    }
}