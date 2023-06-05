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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
            .UseDirectory("Snapshots");
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
            .UseDirectory("Snapshots");
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
            .UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
            .UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
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
        return Verifier.Verify(output).UseDirectory("Snapshots");
    }
}