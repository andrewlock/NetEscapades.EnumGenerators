using System;
using Xunit;

#if INTEGRATION_TESTS
using NetEscapades.EnumGenerators.IntegrationTests;
#elif NETSTANDARD_INTEGRATION_TESTS
using NetEscapades.EnumGenerators.NetStandard.IntegrationTests;
#elif NETSTANDARD_SYSTEMMEMORY_INTEGRATION_TESTS
using NetEscapades.EnumGenerators.NetStandard.SystemMemory.IntegrationTests;
#elif INTERCEPTOR_TESTS
using NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NUGET_INTEGRATION_TESTS
using NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
using NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#elif NUGET_NETSTANDARD_INTERCEPTOR_TESTS
using NetEscapades.EnumGenerators.Nuget.NetStandard.Interceptors.IntegrationTests;
#elif NUGET_SYSTEMMEMORY_INTEGRATION_TESTS
using NetEscapades.EnumGenerators.Nuget.SystemMemory.IntegrationTests;
#else
#error Unknown integration tests
#endif

namespace NetEscapades.EnumGenerators.Benchmarks;

public class AnalyzerTests
{
    // These method calls should all be flagged by the analyzer
    [Fact]
    public void Neeg004Testing()
    {
#pragma warning disable NEEG004
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
#pragma warning restore NEEG004
    }

    [Fact]
    public void Neeg005Testing()
    {
#pragma warning disable NEEG005
        var test = FlagsEnum.First;
        _ = test.HasFlag(FlagsEnum.Second);
        _ = $"Some value: {test.HasFlag(FlagsEnum.Second)} <-";
#pragma warning restore NEEG005
    }

    [Fact]
    public void Neeg006Testing()
    {
#pragma warning disable NEEG006
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
#pragma warning restore NEEG006
    }

    [Fact]
    public void Neeg007Testing()
    {
#pragma warning disable NEEG007
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
#pragma warning restore NEEG007
    }

    [Fact]
    public void Neeg008Testing()
    {
#pragma warning disable NEEG008
        _ = Enum.GetNames(typeof(FlagsEnum));
#if NET5_0_OR_GREATER
        _ = Enum.GetNames<FlagsEnum>().Length;
#endif
#pragma warning restore NEEG008
    }

    [Fact]
    public void Neeg009Testing()
    {
#pragma warning disable NEEG009
        _ = Enum.GetValues(typeof(FlagsEnum));
#if NET5_0_OR_GREATER
        _ = Enum.GetValues<FlagsEnum>().Length;
#endif
#pragma warning restore NEEG009
    }

#if NET7_0_OR_GREATER
    [Fact]
    public void Neeg010Testing()
    {
#pragma warning disable NEEG010
        _ = Enum.GetValuesAsUnderlyingType(typeof(FlagsEnum));
        _ = Enum.GetValuesAsUnderlyingType<FlagsEnum>().Length;
#pragma warning restore NEEG010
    }
#endif

    [Fact]
    public void Neeg011Testing()
    {
#pragma warning disable NEEG011
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
#pragma warning restore NEEG011
    }
}