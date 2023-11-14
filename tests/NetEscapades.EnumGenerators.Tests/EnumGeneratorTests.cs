using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

namespace NetEscapades.EnumGenerators.Tests;

[UsesVerify]
public class EnumGeneratorTests
{
    readonly ITestOutputHelper _output;

    public EnumGeneratorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public Task CanGenerateEnumExtensionsInGlobalNamespace()
    {
        const string input = @"using NetEscapades.EnumGenerators;

[EnumExtensions]
public enum MyEnum
{
    First,
    Second,
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsInChildNamespace()
    {
        const string input = @"using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions]
    public enum MyEnum
    {
        First = 0,
        Second = 1,
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsInNestedClass()
    {
        const string input = @"using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    public class InnerClass
    {
        [EnumExtensions]
        internal enum MyEnum
        {
            First = 0,
            Second = 1,
        }
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithCustomName()
    {
        const string input = @"using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions(ExtensionClassName = ""A"")]
    internal enum MyEnum
    {
        First = 0,
        Second = 1,
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithCustomNamespace()
    {
        const string input = @"using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions(ExtensionClassNamespace = ""A.B"")]
    internal enum MyEnum
    {
        First = 0,
        Second = 1,
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithCustomNamespaceAndName()
    {
        const string input = @"using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions(ExtensionClassNamespace = ""A.B"", ExtensionClassName = ""C"")]
    internal enum MyEnum
    {
        First = 0,
        Second = 1,
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithDisplayName()
    {
        const string input = @"using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;

namespace MyTestNameSpace
{
    [EnumExtensions]
    public enum MyEnum
    {
        First = 0,

        [Display(Name = ""2nd"")]
        Second = 1,
        Third = 2,

        [Display(Name = ""4th"")]
        Fourth = 3
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output)
            .UseMethodName("CanGenerateEnumExtensionsWithCustomNames")
            .DisableRequireUniquePrefix()
            .ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithDescription()
    {
        var input = """
        using NetEscapades.EnumGenerators;
        using System.ComponentModel;

        namespace MyTestNameSpace
        {
            [EnumExtensions]
            public enum MyEnum
            {
                First = 0,

                [Description("2nd")]
                Second = 1,
                Third = 2,

                [Description("4th")]
                Fourth = 3
            }
        }"
        """;

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output)
            .UseMethodName("CanGenerateEnumExtensionsWithCustomNames")
            .DisableRequireUniquePrefix()
            .ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithDescriptionAndDisplayName()
    {
        var input = """
        using NetEscapades.EnumGenerators;
        using System.ComponentModel;
        using System.ComponentModel.DataAnnotations;

        namespace MyTestNameSpace
        {
            [EnumExtensions]
            public enum MyEnum
            {
                First = 0,

                [Description("2nd")] // takes precedence
                [Display(Name = "Secundo")] 
                Second = 1,
                Third = 2,

                [Display(Name = "4th")] // takes precedence 
                [Description("Number 4")]
                Fourth = 3
            }
        }"
        """;

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output)
            .UseMethodName("CanGenerateEnumExtensionsWithCustomNames")
            .DisableRequireUniquePrefix()
            .ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithSameDisplayName()
    {
        const string input = @"using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;

namespace MyTestNameSpace
{
    [EnumExtensions]
    public enum MyEnum
    {
        First = 0,

        [Display(Name = ""2nd"")]
        Second = 1,
        Third = 2,

        [Display(Name = ""2nd"")]
        Fourth = 3
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData("", "System.Flags")]
    [InlineData("", "System.FlagsAttribute")]
    [InlineData("using System;", "FlagsAttribute")]
    [InlineData("using System;", "Flags")]
    public Task CanGenerateEnumExtensionsForFlagsEnum(string usings, string attribute)
    {
        string input = $$"""
        using NetEscapades.EnumGenerators;
        {{usings}}

        namespace MyTestNameSpace
        {
            [EnumExtensions, {{attribute}}]
            public enum MyEnum
            {
                First = 1,
                Second = 2,
                Third = 4,
            }
        }
        """;

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output)
            .UseTextForParameters("Params")
            .DisableRequireUniquePrefix()
            .ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanHandleNamespaceAndClassNameAreTheSame()
    {
        const string input = @"using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;

namespace Foo
{
    public class Foo {}
  
    [EnumExtensions]
    public enum TestEnum
    {
        Value1
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task HandlesStringsWithQuotesAndSlashesInDescription()
    {
        const string input =
            """"
            using NetEscapades.EnumGenerators;
            using System.ComponentModel.DataAnnotations;

            namespace Test;

            [EnumExtensions]
            public enum StringTesting
            {
               [System.ComponentModel.Description("Quotes \"")]   Quotes,
               [System.ComponentModel.Description(@"Literal Quotes """)]   LiteralQuotes,
               [System.ComponentModel.Description("Backslash \\")]   Backslash,
               [System.ComponentModel.Description(@"LiteralBackslash \")]   BackslashLiteral,
            }
            """";

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptEnum()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptEnum2()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                [EnumExtensions]
                internal enum AnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }
                
                public class InnerClass
                {
                    public void MyTest()
                    {
                        var result = AnotherEnum.First.ToString();
                        AssertValue(MyEnum.First);
                        AssertValue(AnotherEnum.Second);
                        AssertValue(MyEnum.Third);
                        
                        void AssertValue(MyEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue(AnotherEnum value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    
    [Fact]
    public Task CanInterceptExternalEnum()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;
            
            [assembly:EnumExtensions<StringComparison>()]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);

                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateForExternalEnum()
    {
        const string input = """
                             using System;
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<StringComparison>()]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateForExternalFlagsEnum()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.IO.FileShare>()]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateForMultipleExternalEnums()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.ConsoleColor>()]
                             [assembly:EnumExtensions<System.DateTimeKind>()]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output.Skip(1)).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateExternalEnumExtensionsWithCustomName()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.DateTimeKind>(ExtensionClassName = "A")]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateExternalEnumExtensionsWithCustomNamespace()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.DateTimeKind>(ExtensionClassNamespace = "A.B")]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanGenerateExternalEnumExtensionsWithCustomNamespaceAndName()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.DateTimeKind>(ExtensionClassNamespace = "A.B", ExtensionClassName = "C")]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubGeneratedCodeAttribute().UseDirectory("Snapshots");
    }

}