using System.ComponentModel.DataAnnotations;
using Xunit;

namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;


[EnumExtensions]
public enum EnumWithDisplayNameInNamespace
{
    First = 0,

    [Display(Name = "2nd")]
    Second = 1,

    Third = 2,
}

public class InterceptorTests
{
    [Fact]
    public void CallingToStringIsIntercepted()
    {
        // var result1 = EnumWithDisplayNameInNamespace.Second.ToString();
        // var result2 = EnumWithDisplayNameInNamespace.Second.ToStringFast();
        // Assert.Equal(result1, result2);


        // AssertValue(EnumWithDisplayNameInNamespace.First);
        // AssertValue(EnumWithDisplayNameInNamespace.Second);
        // AssertValue(EnumWithDisplayNameInNamespace.Third);

        // void AssertValue(EnumWithDisplayNameInNamespace value)
        // {
        //     // These return different values when interception is not enabled
        //     var toString = value.ToString();
        //     var fast = value.ToStringFast();
        //     Assert.Equal(fast, toString);
        // }
    }
    
}