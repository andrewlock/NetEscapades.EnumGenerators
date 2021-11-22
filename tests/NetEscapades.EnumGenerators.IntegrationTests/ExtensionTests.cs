using System;
using FluentAssertions;
using FluentAssertions.Execution;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public abstract class ExtensionTests<T> where T : struct
{
    protected abstract string ToStringFast(T value);
    protected abstract bool IsDefined(T value);
    protected abstract bool IsDefined(string name);
    protected abstract bool TryParse(string name, bool ignoreCase, out T parsed);

    protected void GeneratesToStringFastTest(T value)
    {
        var serialized = ToStringFast(value);

        serialized.Should().Be(value.ToString());
    }

    protected void GeneratesIsDefinedTest(T value)
    {
        var isDefined = IsDefined(value);

        isDefined.Should().Be(Enum.IsDefined(typeof(T), value));
    }

    protected void GeneratesIsDefinedTest(string name)
    {
        var isDefined = IsDefined(name);

        isDefined.Should().Be(Enum.IsDefined(typeof(T), name));
    }

    protected void GeneratesTryParseTest(string name)
    {
        var isValid = Enum.TryParse(name, out T expected);
        var result = TryParse(name, ignoreCase: false, out var parsed);
        using var _ = new AssertionScope();
        result.Should().Be(isValid);
        parsed.Should().Be(expected);
    }

    protected void GeneratesTryParseIgnoreCaseTest(string name)
    {
        var isValid = Enum.TryParse(name, ignoreCase: true, out T expected);
        var result = TryParse(name, ignoreCase: true, out var parsed);
        using var _ = new AssertionScope();
        result.Should().Be(isValid);
        parsed.Should().Be(expected);
    }

    protected void GeneratesGetValuesTest(T[] values)
    {
        var expected = (T[])Enum.GetValues(typeof(T));
        values.Should().Equal(expected);
    }

    protected void GeneratesGetNamesTest(string[] names)
    {
        var expected = Enum.GetNames(typeof(T));
        names.Should().Equal(expected);
    }
}