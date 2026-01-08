using System;
using System.Text;

namespace NetEscapades.EnumGenerators.Nuget.AnalyzerTests;

public class AnalyzerTests
{
    // These method calls should all be flagged by the analyzer and should cause failures to build
    public void Neeg004Testing()
    {
        var test = EnumInSystem.First;
        _ = test.ToString();
        _ = EnumInSystem.First.ToString();
        _ = EnumInSystem.First.ToString("G");
        _ = EnumInSystem.First.ToString("x"); // no error
        _ = EnumInSystem.First.ToString(format: "g");
        _ = EnumInSystem.First.ToString(format: null); // no error
        _ = DateTimeKind.Local.ToString();
        _ = $"Some value: {test} <-";
        _ = $"Some value: {test:G} <-";
        _ = $"Some value: {EnumInSystem.First} <-";
        _ = $"Some value: {EnumInSystem.First:G} <-";
    }

    public void Neeg005Testing()
    {
        var test = FlagsEnum.First;
        _ = test.HasFlag(FlagsEnum.Second);
        _ = $"Some value: {test.HasFlag(FlagsEnum.Second)} <-";
    }

    public void Neeg006Testing()
    {
        var test = FlagsEnum.First;
        _ = Enum.IsDefined(typeof(FlagsEnum), FlagsEnum.Second);
        _ = Enum.IsDefined(typeof(FlagsEnum), test);
        _ = FlagsEnum.IsDefined(typeof(FlagsEnum), (FlagsEnum)23);
        _ = $"Some value: {Enum.IsDefined(typeof(FlagsEnum), test)} <-";

#if NET5_0_OR_GREATER
        _ = Enum.IsDefined<FlagsEnum>((FlagsEnum)23);
        _ = Enum.IsDefined((FlagsEnum)23);
        _ = Enum.IsDefined(test);
        _ = FlagsEnum.IsDefined((FlagsEnum)23);
        _ = FlagsEnum.IsDefined(test);
        _ = $"Some value: {Enum.IsDefined((FlagsEnum)23)} <-";
#endif
    }

    public void Neeg007Testing()
    {
        _ = Enum.Parse(typeof(FlagsEnum), "Second");
        _ = Enum.Parse(typeof(FlagsEnum), "Second", true);
        _ = Enum.Parse(typeof(FlagsEnum), "Second", ignoreCase: false);
#if NETCOREAPP        
        _ = Enum.Parse<FlagsEnum>("Second");
        _ = Enum.Parse<FlagsEnum>("Second", true);
        _ = $"Some value: {Enum.Parse(typeof(FlagsEnum), "First")} <-";

#if NET5_0_OR_GREATER
        var toParse = "Second".AsSpan();
        _ = Enum.Parse(typeof(FlagsEnum), toParse);
        _ = Enum.Parse(typeof(FlagsEnum), toParse, ignoreCase: true);
        _ = Enum.Parse<FlagsEnum>(toParse);
        _ = Enum.Parse<FlagsEnum>(toParse, true);
        _ = $"Some value: {Enum.Parse(typeof(FlagsEnum), "First".AsSpan())} <-";
#endif
#endif
    }

    public void Neeg008Testing()
    {
        _ = Enum.GetNames(typeof(FlagsEnum));
#if NET5_0_OR_GREATER
        _ = Enum.GetNames<FlagsEnum>().Length;
#endif
    }

    public void Neeg009Testing()
    {
        _ = Enum.GetValues(typeof(FlagsEnum));
#if NET5_0_OR_GREATER
        _ = Enum.GetValues<FlagsEnum>().Length;
#endif
    }

#if NET7_0_OR_GREATER
    public void Neeg010Testing()
    {
        _ = Enum.GetValuesAsUnderlyingType(typeof(FlagsEnum));
        _ = Enum.GetValuesAsUnderlyingType<FlagsEnum>().Length;
    }
#endif

    public void Neeg011Testing()
    {
        _ = Enum.TryParse<FlagsEnum>("Second", out _);
        Enum.TryParse<FlagsEnum>("Second", true, out var e);
        _ = $"Some value: {Enum.TryParse<FlagsEnum>("First", out _)} <-";
#if NETCOREAPP
        _ = Enum.TryParse(typeof(FlagsEnum), "Second", out var t);
        _ = Enum.TryParse(typeof(FlagsEnum), "Second", true, out object? t2);
        _ = Enum.TryParse(typeof(FlagsEnum), "Second", ignoreCase: false, out _);
#if NET5_0_OR_GREATER
        var toParse = "Second".AsSpan();
        _ = Enum.TryParse(typeof(FlagsEnum), toParse, out _);
        _ = Enum.TryParse(typeof(FlagsEnum), toParse, ignoreCase: true, out _);
        _ = Enum.TryParse<FlagsEnum>(toParse, out _);
        _ = Enum.TryParse<FlagsEnum>(toParse, true, out _);
        _ = $"Some value: {Enum.TryParse(typeof(FlagsEnum), "First".AsSpan(), out _)} <-";
#endif
#endif
    }

    public void Neeg012Testing()
    {
        var flagsEnum = FlagsEnum.First;
        var sb = new StringBuilder();
        sb.Append(flagsEnum);
    }
}