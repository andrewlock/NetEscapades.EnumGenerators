using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.Emit;
using NetEscapades.EnumGenerators;
using NetEscapades.EnumGenerators.Benchmarks;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args);

[EnumExtensions]
public enum TestEnum
{
    First = 0,

    [Display(Name = "2nd")]
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
    public string EnumToStringDisplayNameWithReflection()
    {
        return EnumHelper<TestEnum>.GetDisplayName(_enum);
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
    private static readonly string _enumDisplaName = "2nd";

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
        return TestEnumExtensions.IsDefined(_enum, allowMatchingMetadataAttribute: false);
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool EnumIsDefinedNameDisplayNameWithReflection()
    {
        return EnumHelper<TestEnum>.TryParseByDisplayName(_enumDisplaName, ignoreCase: false, out _);
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ExtensionsIsDefinedNameDisplayName()
    {
        return TestEnumExtensions.IsDefined(_enumDisplaName, allowMatchingMetadataAttribute: true);
    }
}

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
[MemoryDiagnoser]
public class IsDefinedNameFromSpanBenchmark
{
    private static readonly char[] _enum = new char[] { 'S', 'e', 'c', 'o', 'n', 'd' };
    private static readonly char[] _enumDisplayName = new char[] { '2', 'n', 'd' };

    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool EnumIsDefined()
    {
        ReadOnlySpan<char> _enumAsSpan = _enum;
        return Enum.IsDefined(typeof(TestEnum), _enumAsSpan.ToString());
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool EnumIsDefinedNameDisplayNameWithReflection()
    {
        ReadOnlySpan<char> _enumAsSpan = _enumDisplayName;
        return EnumHelper<TestEnum>.TryParseByDisplayName(_enumAsSpan.ToString(), ignoreCase:false, out _);
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ExtensionsIsDefined()
    {
        ReadOnlySpan<char> _enumAsSpan = _enum;
        return TestEnumExtensions.IsDefined(_enumAsSpan.ToString(), allowMatchingMetadataAttribute: false);
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ExtensionsIsDefinedSpan()
    {
        return TestEnumExtensions.IsDefined(_enum.AsSpan(), allowMatchingMetadataAttribute: false);
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool ExtensionsIsDefinedDisplayNameSpan()
    {
        return TestEnumExtensions.IsDefined(_enumDisplayName.AsSpan(), allowMatchingMetadataAttribute: true);
    }
}
#endif

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
public class GetValuesAsUnderlyingTypeBenchmark
{
#if NETFRAMEWORK
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int[] EnumGetValuesAsUnderlyingType()
    {
        return Enum.GetValues(typeof(TestEnum)).Cast<int>().ToArray();
    }
#elif NET7_OR_GREATER
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int[] EnumGetValuesAsUnderlyingType()
    {
        return Enum.GetValuesAsUnderlyingType<TestEnum>();
    }
#else
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int[] EnumGetValuesAsUnderlyingType()
    {
        return Enum.GetValues<TestEnum>().Cast<int>().ToArray();
    }
#endif

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int[] ExtensionsGetValuesAsUnderlyingType()
    {
        return TestEnumExtensions.GetValuesAsUnderlyingType();
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
        return TestEnumExtensions.TryParse("Second", out TestEnum result, ignoreCase: false)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum EnumTryParseDisplayNameWithReflection()
    {
        return EnumHelper<TestEnum>.TryParseByDisplayName("2nd", ignoreCase: false, out TestEnum result) ? result : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseDisplayName()
    {
        return TestEnumExtensions.TryParse("2nd", out TestEnum result, ignoreCase: false, allowMatchingMetadataAttribute: true)
            ? result
            : default;
    }
}

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
[MemoryDiagnoser]
public class TryParseFromSpanBenchmark
{
    private static readonly char[] _enum = new char[] { 'S', 'e', 'c', 'o', 'n', 'd' };
    private static readonly char[] _enumDisplayName = new char[] { '2', 'n', 'd' };

    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum EnumTryParse()
    {
        ReadOnlySpan<char> _enumAsSpan = _enum;
        return Enum.TryParse(_enumAsSpan.ToString(), ignoreCase: false, out TestEnum result)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParse()
    {
        ReadOnlySpan<char> _enumAsSpan = _enum;
        return TestEnumExtensions.TryParse(_enumAsSpan.ToString(), out TestEnum result, ignoreCase: false)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseSpan()
    {
        return TestEnumExtensions.TryParse(_enum.AsSpan(), out TestEnum result, ignoreCase: false)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum EnumTryParseDisplayNameWithReflection()
    {
        ReadOnlySpan<char> _enumAsSpan = _enumDisplayName;
        return EnumHelper<TestEnum>.TryParseByDisplayName(_enumAsSpan.ToString(), ignoreCase: false, out TestEnum result) ? result : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseDisplayName()
    {
        ReadOnlySpan<char> _enumAsSpan = _enumDisplayName;
        return TestEnumExtensions.TryParse(_enumAsSpan.ToString(), out TestEnum result, ignoreCase: false, allowMatchingMetadataAttribute: true)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseDisplayNameSpan()
    {
        return TestEnumExtensions.TryParse(_enumDisplayName.AsSpan(), out TestEnum result, ignoreCase: false, allowMatchingMetadataAttribute: true)
            ? result
            : default;
    }
}
#endif

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
        return TestEnumExtensions.TryParse("second", out TestEnum result, ignoreCase: true)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum EnumTryParseIgnoreCaseDisplayNameWithReflection()
    {
        return EnumHelper<TestEnum>.TryParseByDisplayName("2ND", ignoreCase: true, out TestEnum result) ? result : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseIgnoreCaseDisplayName()
    {
        return TestEnumExtensions.TryParse("2ND", out TestEnum result, ignoreCase: true, allowMatchingMetadataAttribute: true)
            ? result
            : default;
    }
}

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
[MemoryDiagnoser]
public class TryParseIgnoreCaseFromSpanBenchmark
{
    private static readonly char[] _enum = new char[] { 's', 'e', 'c', 'o', 'n', 'd' };

    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum EnumTryParseIgnoreCase()
    {
        ReadOnlySpan<char> _enumAsSpan = _enum;
        return Enum.TryParse(_enumAsSpan.ToString(), ignoreCase: true, out TestEnum result)
            ? result
            : default;
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseIgnoreCase()
    {
        ReadOnlySpan<char> _enumAsSpan = _enum;
        return TestEnumExtensions.TryParse(_enumAsSpan.ToString(), out TestEnum result, ignoreCase: true)
            ? result
            : default;
    }


    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public TestEnum ExtensionsTryParseIgnoreCaseSpan()
    {
        return TestEnumExtensions.TryParse(_enum.AsSpan(), out TestEnum result, ignoreCase: true)
            ? result
            : default;
    }
}
#endif

[MemoryDiagnoser]
public class EnumLengthBenchmark
{
    [Benchmark(Baseline = true)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int EnumLength() => Enum.GetNames(typeof(TestEnum)).Length;

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int EnumLengthExtensions() => TestEnumExtensions.GetNames().Length;

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int EnumLengthProperty() => TestEnumExtensions.Length;
}