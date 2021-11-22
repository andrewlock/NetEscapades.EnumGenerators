using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NetEscapades.EnumGenerators;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args);

[EnumExtensions]
public enum TestEnum
{
    First = 0,
    Second = 1,
    Third = 2,
}

[MemoryDiagnoser]
public class ToStringBenchmark
{
    private static readonly TestEnum _enum = TestEnum.Second;

    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string EnumToString()
    {
        return _enum.ToString();
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string ToStringFast()
    {
        return _enum.ToStringFast();
    }
}

[MemoryDiagnoser]
public class IsDefinedBenchmark
{
    private static readonly TestEnum _enum = TestEnum.Second;

    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool EnumIsDefinedName()
    {
        return Enum.IsDefined(typeof(TestEnum), _enum);
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ExtensionsIsDefinedName()
    {
        return TestEnumExtensions.IsDefined(_enum);
    }
}

[MemoryDiagnoser]
public class IsDefinedNameBenchmark
{
    private static readonly string _enum = nameof(TestEnum.Second);

    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool EnumIsDefined()
    {
        return Enum.IsDefined(typeof(TestEnum), _enum);
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ExtensionsIsDefined()
    {
        return TestEnumExtensions.IsDefined(_enum);
    }
}

[MemoryDiagnoser]
public class GetValuesBenchmark
{
#if NETFRAMEWORK
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum[] EnumGetValues()
    {
        return (TestEnum[])Enum.GetValues(typeof(TestEnum));
    }
#else
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum[] EnumGetValues()
    {
        return Enum.GetValues<TestEnum>();
    }
#endif

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum[] ExtensionsGetValues()
    {
        return TestEnumExtensions.GetValues();
    }
}

[MemoryDiagnoser]
public class GetNamesBenchmark
{
#if NETFRAMEWORK
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string[] EnumGetNames()
    {
        return Enum.GetNames(typeof(TestEnum));
    }
#else
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string[] EnumGetNames()
    {
        return Enum.GetNames<TestEnum>();
    }
#endif

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string[] ExtensionsGetNames()
    {
        return TestEnumExtensions.GetNames();
    }
}

[MemoryDiagnoser]
public class TryParseBenchmark
{
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum EnumTryParse()
    {
        return Enum.TryParse("Second", ignoreCase: false, out TestEnum result)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParse()
    {
        return TestEnumExtensions.TryParse("Second", ignoreCase: false, out TestEnum result)
            ? result
            : default;
    }
}

[MemoryDiagnoser]
public class TryParseIgnoreCaseBenchmark
{
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum EnumTryParseIgnoreCase()
    {
        return Enum.TryParse("second", ignoreCase: true, out TestEnum result)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseIgnoreCase()
    {
        return TestEnumExtensions.TryParse("second", ignoreCase: true, out TestEnum result)
            ? result
            : default;
    }
}