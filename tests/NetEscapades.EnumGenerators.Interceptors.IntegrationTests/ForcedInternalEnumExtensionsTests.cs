using Xunit;

#if NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#else
#error Unknown project combination
#endif

/// <summary>
/// Simple tests for ForcedInternalEnum to verify that internal extensions are generated and accessible.
/// These are basic smoke tests - full test coverage is provided by the snapshot tests.
/// </summary>
public class ForcedInternalEnumExtensionsTests
{
    [Fact]
    public void GetNames_ReturnsExpectedNames()
    {
        var names = ForcedInternalEnumExtensions.GetNames();
        Assert.Equal(3, names.Length);
        Assert.Contains("First", names);
        Assert.Contains("Second", names);
        Assert.Contains("Third", names);
    }

    [Fact]
    public void GetValues_ReturnsExpectedValues()
    {
        var values = ForcedInternalEnumExtensions.GetValues();
        Assert.Equal(3, values.Length);
        Assert.Contains(ForcedInternalEnum.First, values);
        Assert.Contains(ForcedInternalEnum.Second, values);
        Assert.Contains(ForcedInternalEnum.Third, values);
    }

    [Fact]
    public void ToStringFast_ReturnsEnumName()
    {
        Assert.Equal("First", ForcedInternalEnum.First.ToStringFast());
        Assert.Equal("Second", ForcedInternalEnum.Second.ToStringFast());
        Assert.Equal("Third", ForcedInternalEnum.Third.ToStringFast());
    }

    [Fact]
    public void IsDefined_ReturnsTrueForDefinedValues()
    {
        Assert.True(ForcedInternalEnumExtensions.IsDefined(ForcedInternalEnum.First));
        Assert.True(ForcedInternalEnumExtensions.IsDefined(ForcedInternalEnum.Second));
        Assert.True(ForcedInternalEnumExtensions.IsDefined(ForcedInternalEnum.Third));
    }

    [Fact]
    public void IsDefined_ReturnsFalseForUndefinedValues()
    {
        Assert.False(ForcedInternalEnumExtensions.IsDefined((ForcedInternalEnum)999));
    }

    [Fact]
    public void TryParse_ParsesValidNames()
    {
        Assert.True(ForcedInternalEnumExtensions.TryParse("First", out var result));
        Assert.Equal(ForcedInternalEnum.First, result);

        Assert.True(ForcedInternalEnumExtensions.TryParse("Second", out result));
        Assert.Equal(ForcedInternalEnum.Second, result);
    }

    [Fact]
    public void TryParse_ReturnsFalseForInvalidNames()
    {
        Assert.False(ForcedInternalEnumExtensions.TryParse("Invalid", out _));
    }
}
