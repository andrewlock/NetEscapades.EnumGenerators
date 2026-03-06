By default, adding [NetEscapades.EnumGenerators.Interceptors](https://www.nuget.org/packages/NetEscapades.EnumGenerators.Interceptors) to a project enables interception for all enums _defined in that project_ that use the `[EnumExtensions]` or `[EnumExtensions<T>]` attributes. If you wish to intercept calls made to enums with extensions defined in _other_ projects, you must add the `[Interceptable<T>]` attribute in the project where you want the interception to happen, e.g.

```csharp
[assembly:Interceptable<DateTimeKind>]
[assembly:Interceptable<Color>]
```

If you don't want a specific enum to be intercepted, you can set the `IsInterceptable` property to `false`, e.g.

```csharp
[EnumExtensions(IsInterceptable = false)]
public enum Colour
{
    Red = 0,
    Blue = 1,
}
```

Interception only works when the target type is unambiguously an interceptable enum, so it won't work

- When `ToString()` is called in other source generated code.
- When `ToString()` is called in already-compiled code.
- If the `ToString()` call is _implicit_ (for example in `string` interpolation)
- If the `ToString()` call is made on a base type, such as `System.Enum` or `object`
- If the `ToString()` call is made on a generic type

For example:

```csharp
// All the examples in this method CAN be intercepted
public void CanIntercept()
{
    var ok1 = Color.Red.ToString(); // ✅
    var red = Color.Red;
    var ok2 = red.ToString(); // ✅
    var ok3 = "The colour is " + red.ToString(); // ✅
    var ok4 = $"The colour is {red.ToString()}"; // ✅
}

// The examples in this method can NOT be intercepted
public void CantIntercept()
{
    var bad1 = ((System.Enum)Color.Red).ToString(); // ❌ Base type
    var bad2 = ((object)Color.Red).ToString(); // ❌ Base type

    var bad3 = "The colour is " + red; // ❌ implicit
    var bad4 = $"The colour is {red}"; // ❌ implicit

    string Write<T>(T val)
        where T : Enum
    {
        return val.ToString(); // ❌ generic
    }
}
```
