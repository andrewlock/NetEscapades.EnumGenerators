using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;

namespace NetEscapades.EnumGenerators.IntegrationTests;

#nullable enable
public abstract class ExtensionTests<T> where T : struct
{
    protected abstract string ToStringFast(T value);
    protected abstract bool IsDefined(T value);
    protected abstract bool IsDefined(string name, bool allowMatchingDisplayAttribute = false);
    protected abstract bool TryParse(string name, bool ignoreCase, out T parsed);

    protected void GeneratesToStringFastTest(T value)
    {
        var serialized = ToStringFast(value);
        var valueAsString = value.ToString();

        var displayName = GetDisplayName(valueAsString);
        var expectedValue = displayName is null ? valueAsString : displayName;
        
        serialized.Should().Be(expectedValue);
    }

    private string? GetDisplayName(string? value)
    {
        if (typeof(T).IsEnum)
        {
            if (value is not null)
            {// Prevent: Warning CS8604  Possible null reference argument for parameter 'name' in 'MemberInfo[] Type.GetMember(string name)'
                var memberInfo = typeof(T).GetMember(value);
                if (memberInfo.Length > 0)
                {
                    return memberInfo[0].GetCustomAttribute<DisplayAttribute>()?.GetName();
                }
            }
        }

        return null;
    }

    protected void GeneratesIsDefinedTest(T value)
    {
        var isDefined = IsDefined(value);

        isDefined.Should().Be(Enum.IsDefined(typeof(T), value));
    }

    protected void GeneratesIsDefinedTest(string name, bool allowMatchingDisplayAttribute = false)
    {
        bool isDefined;
        bool expectedResult = false;

        if (allowMatchingDisplayAttribute)
        {
            isDefined = IsDefined(name, allowMatchingDisplayAttribute);

            var values = (T[])Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                var displayName = GetDisplayName(value.ToString());
                if (displayName is not null && displayName.Equals(name, StringComparison.Ordinal))
                {
                    expectedResult = true;
                    break;
                }
            }

            if (!expectedResult)
            {
                expectedResult = Enum.IsDefined(typeof(T), name);
            }
        }
        else
        {
            isDefined = IsDefined(name, allowMatchingDisplayAttribute);
            expectedResult = Enum.IsDefined(typeof(T), name);
        }        

        isDefined.Should().Be(expectedResult);
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