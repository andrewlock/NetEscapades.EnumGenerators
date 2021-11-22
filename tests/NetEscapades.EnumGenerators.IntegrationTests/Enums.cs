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
        First = 1 << 0,
        Second = 1 << 1,
        Third = 1 << 2,
        Fourth = 1 << 3,
    }
}