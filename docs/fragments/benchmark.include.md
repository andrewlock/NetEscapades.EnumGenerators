Many methods that work with enums are surprisingly slow. Calling `ToString()` or `HasFlag()` on an enum seems like it _should_ be fast, but it often isn't. This package provides a set of extension methods, such as `ToStringFast()` or `HasFlagFast()` that are designed to be very fast, with fewer allocations.


For example, the following benchmark shows the advantage of calling `ToStringFast()` over `ToString()`:

``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1348 (20H2/October2020Update)
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
  DefaultJob : .NET Framework 4.8 (4.8.4420.0), X64 RyuJIT
.NET SDK=6.0.100
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
```

|       Method | FX        |       Mean |     Error |      StdDev | Ratio |  Gen 0 | Allocated |
|------------- |-----------|-----------:|----------:|------------:|------:|-------:|----------:|
| ToString | `net48`   | 578.276 ns | 3.3109 ns |   3.0970 ns | 1.000 | 0.0458 |      96 B |
| ToStringFast | `net48`   |   3.091 ns | 0.0567 ns |   0.0443 ns | 0.005 |      - |         - |
| ToString | `net6.0`  | 17.985 ns | 0.1230 ns |   0.1151 ns | 1.000 | 0.0115 |      24 B |
| ToStringFast | `net6.0`  |  0.121 ns | 0.0225 ns |   0.0199 ns | 0.007 |      - |         - |
| ToString | `net10.0` | 6.4389 ns | 0.1038 ns |   0.0971 ns | 0.004 |  1.000 |      24 B |
| ToStringFast | `net10.0` | 0.0050 ns | 0.0202 ns |   0.0189 ns | 0.001 |       - |         - |


Enabling these additional extension methods is as simple as adding an attribute to your enum:

```csharp
[EnumExtensions] // 👈 Add this
public enum Color
{
    Red = 0,
    Blue = 1,
}
```
