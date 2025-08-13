using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

#if INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.IntegrationTests;
#elif NETSTANDARD_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#else
#error Unknown integration tests
#endif

#nullable enable
public abstract class ExtensionTests<T, TUnderlying, TITestData>
    where T : struct
    where TUnderlying : struct
    where TITestData : ITestData<T>, new()
{
    public static TheoryData<T> GetValidEnumValues() => new TITestData().ValidEnumValues();
    public static TheoryData<string> GetValuesToParse() => new TITestData().ValuesToParse();

    protected abstract string[] GetNames();
    protected abstract T[] GetValues();
    protected abstract TUnderlying[] GetValuesAsUnderlyingType();
    protected abstract string ToStringFast(T value);
    protected abstract string ToStringFast(T value, bool withMetadata);
    protected abstract TUnderlying AsUnderlyingValue(T value);
    protected abstract bool IsDefined(T value);
    protected abstract bool IsDefined(string name, bool allowMatchingMetadataAttribute = false);
#if READONLYSPAN
    protected abstract bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute);
#endif
    protected abstract bool TryParse(string name, out T parsed, bool ignoreCase, bool allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected abstract bool TryParse(in ReadOnlySpan<char> name, out T parsed, bool ignoreCase, bool allowMatchingMetadataAttribute);
#endif

    protected abstract T Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected abstract T Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute);
#endif

    [Theory]
    [MemberData(nameof(GetValidEnumValues))]
    public void GeneratesToStringFast(T value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(GetValidEnumValues))]
    public void GeneratesToStringFastWithMetadata(T value) => GeneratesToStringFastWithMetadataTest(value);

    [Theory]
    [MemberData(nameof(GetValidEnumValues))]
    public void GeneratesIsDefined(T value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseUsingSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsAspan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

    [Theory]
    [MemberData(nameof(GetValidEnumValues))]
    public void GeneratesAsUnderlyingType(T value) => GeneratesAsUnderlyingTypeTest(value, AsUnderlyingValue(value));

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(GetValues());

    [Fact]
    public void GeneratesGetValuesAsUnderlyingType() => GeneratesGetValuesAsUnderlyingTypeTest(GetValuesAsUnderlyingType());

    [Fact]
    public void GeneratesGetNames() => GeneratesGetNamesTest(GetNames());


    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttribute(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: true);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttributeAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: true);
#endif

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: true);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: true);
#endif

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: true);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(GetValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: true);
#endif

    private void GeneratesToStringFastTest(T value)
    {
        var serialized = ToStringFast(value);
        var expectedValue = value.ToString();

        serialized.Should().Be(expectedValue);

        var serializedAltPath = ToStringFast(value, withMetadata: false);
        serializedAltPath.Should().Be(expectedValue);
    }

    private void GeneratesToStringFastWithMetadataTest(T value)
    {
        var serialized = ToStringFast(value, withMetadata: true);
        var valueAsString = value.ToString();

        TryGetDisplayNameOrDescription(valueAsString, out var displayName);
        var expectedValue = displayName is null ? valueAsString : displayName;
        
        serialized.Should().Be(expectedValue);
    }

    private void GeneratesIsDefinedTest(T value)
    {
        var isDefined = IsDefined(value);

        isDefined.Should().Be(Enum.IsDefined(typeof(T), value));
    }

    private void GeneratesIsDefinedTest(string name, bool allowMatchingMetadataAttribute)
    {
        var isDefined = IsDefined(name, allowMatchingMetadataAttribute);
        var expectedResult = ValidateIsDefined(name, allowMatchingMetadataAttribute);
        isDefined.Should().Be(expectedResult);
    }

#if READONLYSPAN
    private void GeneratesIsDefinedTest(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute)
    {
        var isDefined = IsDefined(name, allowMatchingMetadataAttribute);
        var expectedResult = ValidateIsDefined(name.ToString(), allowMatchingMetadataAttribute);
        isDefined.Should().Be(expectedResult);
    }
#endif

    private bool ValidateIsDefined(string name, bool allowMatchingMetadataAttribute)
    {
        bool expectedResult;
        if (allowMatchingMetadataAttribute)
        {
            expectedResult = TryGetEnumByDisplayNameOrDescription(name, ignoreCase: false, out _);
            if (!expectedResult)
            {
                expectedResult = Enum.IsDefined(typeof(T), name);
            }
        }
        else
        {
            expectedResult = Enum.IsDefined(typeof(T), name);
        }

        return expectedResult;
    }

    private void GeneratesTryParseTest(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var isValid = TryParse(name, out var result, ignoreCase, allowMatchingMetadataAttribute);
        ValidateTryParse(name, ignoreCase, allowMatchingMetadataAttribute, out bool expectedValidity, out T expectedResult);

        _ = new AssertionScope();
        isValid.Should().Be(expectedValidity);
        result.Should().Be(expectedResult);
    }

#if READONLYSPAN
    private void GeneratesTryParseTest(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var isValid = TryParse(name, out var result, ignoreCase, allowMatchingMetadataAttribute);
        ValidateTryParse(name.ToString(), ignoreCase, allowMatchingMetadataAttribute, out bool expectedValidity, out T expectedResult);

        _ = new AssertionScope();
        isValid.Should().Be(expectedValidity);
        result.Should().Be(expectedResult);
    }
#endif

    private void ValidateTryParse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute, out bool expectedValidity, out T expectedResult)
    {
        if (allowMatchingMetadataAttribute)
        {
            expectedValidity = TryGetEnumByDisplayNameOrDescription(name, ignoreCase, out expectedResult);
            if (!expectedValidity)
            {
                expectedValidity = Enum.TryParse(name, ignoreCase, out expectedResult);
            }
        }
        else
        {
            expectedValidity = Enum.TryParse(name, ignoreCase, out expectedResult);
        }
    }

    private void GeneratesParseTest(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        Exception? ex = null;
        T? result = null;
        try
        {
            result = Parse(name, ignoreCase, allowMatchingMetadataAttribute);
        }
        catch (Exception e)
        {
            ex = e;
        }
        
        ValidateParse(name, ignoreCase, allowMatchingMetadataAttribute, out Exception? expectedException, out T expectedResult);

        _ = new AssertionScope();
        if (expectedException is null)
        {
            ex.Should().Be(expectedException);
        }
        else
        {
            ex.Should().BeNull();
            result.Should().Be(expectedResult);
        }
    }

#if READONLYSPAN
    private void GeneratesParseTest(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        Exception? ex = null;
        T? result = null;
        try
        {
            result = Parse(name, ignoreCase, allowMatchingMetadataAttribute);
        }
        catch (Exception e)
        {
            ex = e;
        }
        
        ValidateParse(name.ToString(), ignoreCase, allowMatchingMetadataAttribute, out Exception? expectedException, out T expectedResult);

        _ = new AssertionScope();
        if (expectedException is null)
        {
            ex.Should().Be(expectedException);
        }
        else
        {
            ex.Should().BeNull();
            result.Should().Be(expectedResult);
        }
    }
#endif

    private void ValidateParse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute, out Exception? expectedException, out T expectedResult)
    {
        expectedException = null;
        if (allowMatchingMetadataAttribute)
        {
            var expectedValidity = TryGetEnumByDisplayNameOrDescription(name, ignoreCase, out expectedResult);
            if (!expectedValidity)
            {
                try
                {
                    expectedResult = (T)Enum.Parse(typeof(T), name, ignoreCase);
                }
                catch (Exception ex)
                {
                    expectedException = ex;
                }
            }
        }
        else
        {
            expectedResult = (T)Enum.Parse(typeof(T), name, ignoreCase);
        }
    }

    private void GeneratesAsUnderlyingTypeTest(T value, TUnderlying underlyingValue)
    {
        var expected = (TUnderlying) (object) value;
        underlyingValue.Should().Be(expected);
    }

    private void GeneratesGetValuesTest(T[] values)
    {
        var expected = (T[]) Enum.GetValues(typeof(T));
        values.Should().Equal(expected);
    }

    private void GeneratesGetValuesAsUnderlyingTypeTest(TUnderlying[] values)
    {
#if NET7_OR_GREATER
        var expected = (TUnderlying[]) Enum.GetValuesAsUnderlyingType(typeof(T));
#else
        var expected = Enum.GetValues(typeof(T)).Cast<TUnderlying>().ToArray();
#endif
        values.Should().Equal(expected);
    }

    private void GeneratesGetNamesTest(string[] names)
    {
        var expected = Enum.GetNames(typeof(T));
        names.Should().Equal(expected);
    }

    private bool TryGetEnumByDisplayNameOrDescription(string name, bool ignoreCase, out T enumValue)
    {
        enumValue = default;

        var stringComparisonOptions = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var enumValues = (T[])Enum.GetValues(typeof(T));
        foreach (var value in enumValues)
        {
            if (TryGetDisplayNameOrDescription(value.ToString(), out var displayName) && string.Equals(displayName,name, stringComparisonOptions))
            {
                enumValue = value;
                return true;
            }
        }

        return false;
    }

    protected virtual bool TryGetDisplayNameOrDescription(
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
            // Prevent: Warning CS8604  Possible null reference argument for parameter 'name' in 'MemberInfo[] Type.GetMember(string name)'
            if (value is not null)
            {
                var memberInfo = typeof(T).GetMember(value);
                if (memberInfo.Length > 0)
                {
                    // Doesn't take order into account, but we don't test with both currently
                    displayName = memberInfo[0].GetCustomAttribute<DisplayAttribute>()?.GetName()
                        ?? memberInfo[0].GetCustomAttribute<DescriptionAttribute>()?.Description
                        ?? memberInfo[0].GetCustomAttribute<EnumMemberAttribute>()?.Value;
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

public interface ITestData<TEnum>
{
    public TheoryData<TEnum> ValidEnumValues();
    public TheoryData<string> ValuesToParse();
}