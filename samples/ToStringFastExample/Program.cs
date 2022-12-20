using System;
using NetEscapades.EnumGenerators;

var value = ExampleEnums.First;
Console.WriteLine(value.ToStringFast());

var flags = FlagsEnum.Flag1 | FlagsEnum.Flag3;

Console.WriteLine(flags.ToStringFast());
Console.WriteLine($"HasFlag(Flag1), {flags.HasFlagFast(FlagsEnum.Flag1)}");
Console.WriteLine($"HasFlag(Flag1), {flags.HasFlagFast(FlagsEnum.Flag2)}");

[EnumExtensions]
internal enum ExampleEnums
{
    First,
    Second,
    Third,
}

[EnumExtensions]
[Flags]
internal enum FlagsEnum
{
    Flag1,
    Flag2,
    Flag3,
}