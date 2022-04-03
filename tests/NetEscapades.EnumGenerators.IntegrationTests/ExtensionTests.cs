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
    protected abstract bool IsDefined(ReadOnlySpan<char> name, bool allowMatchingDisplayAttribute = false);
    protected abstract bool TryParse(string name, bool ignoreCase, out T parsed, bool allowMatchingDisplayAttribute = false);
    protected abstract bool TryParse(ReadOnlySpan<char> name, bool ignoreCase, out T parsed, bool allowMatchingDisplayAttribute = false);

    protected void GeneratesToStringFastTest(T value)
    {
        var serialized = ToStringFast(value);
        var valueAsString = value.ToString();

        TryGetDisplayName(valueAsString, out var displayName);
        var expectedValue = displayName is null ? valueAsString : displayName;
        
        serialized.Should().Be(expectedValue);
    }

    protected void GeneratesIsDefinedTest(T value)
    {
        var isDefined = IsDefined(value);

        isDefined.Should().Be(Enum.IsDefined(typeof(T), value));
    }

    protected void GeneratesIsDefinedTest(string name, bool allowMatchingDisplayAttribute = false)
    {
        bool expectedResult;
        var isDefined = IsDefined(name, allowMatchingDisplayAttribute);

        if (allowMatchingDisplayAttribute)
        {
            expectedResult = TryGetEnumByDisplayName(name, out _);
            if (!expectedResult)
            {
                expectedResult = Enum.IsDefined(typeof(T), name);
            }
        }
        else
        {
            expectedResult = Enum.IsDefined(typeof(T), name);
        }        

        isDefined.Should().Be(expectedResult);
    }

    protected void GeneratesIsDefinedTest(in ReadOnlySpan<char> name)
    {
        var isDefined = IsDefined(name);

        isDefined.Should().Be(Enum.IsDefined(typeof(T), name.ToString()));
    }

    protected void GeneratesTryParseTest(string name, bool ignoreCase = false, bool allowMatchingDisplayAttribute = false)
    {
        bool expectedValidity;
        T expectedResult;
        var isValid = TryParse(name, ignoreCase, out var result, allowMatchingDisplayAttribute);

        if (allowMatchingDisplayAttribute)
        {
            expectedValidity = TryGetEnumByDisplayName(name, out expectedResult);
            if (!expectedValidity)
            {
                expectedValidity = Enum.TryParse(name, out expectedResult);
            }
        }
        else
        {
            expectedValidity = Enum.TryParse(name, out expectedResult);
        }
        _ = new AssertionScope();
        isValid.Should().Be(expectedValidity);
        result.Should().Be(expectedResult);
    }

    protected void GeneratesTryParseTest(in ReadOnlySpan<char> name)
    {
        var isValid = Enum.TryParse(name.ToString(), out T expected);
        var result = TryParse(name, ignoreCase: false, out var parsed);
        using var _ = new AssertionScope();
        result.Should().Be(isValid);
        parsed.Should().Be(expected);
    }

    protected void GeneratesTryParseIgnoreCaseTest(in ReadOnlySpan<char> name)
    {
        var isValid = Enum.TryParse(name.ToString(), ignoreCase: true, out T expected);
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

    private bool TryGetEnumByDisplayName(string name, out T enumValue)
    {
        enumValue = default;

        var enumValues = (T[])Enum.GetValues(typeof(T));
        foreach (var value in enumValues)
        {
            if (TryGetDisplayName(value.ToString(), out var displayName) && displayName.Equals(name, StringComparison.Ordinal))
            {
                enumValue = value;
                return true;
            }
        }

        return false;
    }

    private bool TryGetDisplayName(
        string? value,
#if NETCOREAPP3_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? displayName)
#else
        out string? displayName)
#endif
    {
        displayName = default;

        if (typeof(T).IsEnum)
        {
            if (value is not null)
            {// Prevent: Warning CS8604  Possible null reference argument for parameter 'name' in 'MemberInfo[] Type.GetMember(string name)'
                var memberInfo = typeof(T).GetMember(value);
                if (memberInfo.Length > 0)
                {
                    displayName = memberInfo[0].GetCustomAttribute<DisplayAttribute>()?.GetName();
                    if (displayName is null)
                    {
                        return false;
                    }
                    
                    return true;
                }
            }
        }

        return false;
    }
}