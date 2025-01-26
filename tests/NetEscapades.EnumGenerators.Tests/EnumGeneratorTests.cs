using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

public class EnumGeneratorTests : EnumGeneratorTestsBase
{
    protected override IIncrementalGenerator[] Generators() => [new EnumGenerator()];
}

public class EnumGeneratorInterceptorTests : EnumGeneratorTestsBase
{
    protected override IIncrementalGenerator[] Generators()
        => [new EnumGenerator(), new Interceptors.EnumGenerator()];
}

[UsesVerify]
public abstract class EnumGeneratorTestsBase
{
    protected abstract IIncrementalGenerator[] Generators();
    
    private VerifySettings Settings()
    {
        var settings = new VerifySettings();
        settings.ScrubExpectedChanges();
        settings.UseDirectory("Snapshots");
        settings.UseTypeName(nameof(EnumGeneratorTests));
        settings.DisableRequireUniquePrefix();

        return settings;
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseMethodName("CanGenerateEnumExtensionsWithCustomNames");
        return Verifier.Verify(output, settings);
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
        }
        """;

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseMethodName("CanGenerateEnumExtensionsWithCustomNames");
        return Verifier.Verify(output, settings);
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
        }
        """;

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseMethodName("CanGenerateEnumExtensionsWithCustomNames");
        return Verifier.Verify(output, settings);
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
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
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

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseTextForParameters("Params");
        return Verifier.Verify(output, settings);
    }

    [Fact]
    public Task CanHandleNamespaceAndClassNameAreTheSame()
    {
        const string input = @"using NetEscapades.EnumGenerators;

namespace Foo
{
    public class Foo {}
  
    [EnumExtensions]
    public enum TestEnum
    {
        Value1
    }
}";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task HandlesStringsWithQuotesAndSlashesInDescription()
    {
        const string input =
            """"
            using NetEscapades.EnumGenerators;
            using System.ComponentModel;

            namespace Test;

            [EnumExtensions]
            public enum StringTesting
            {
               [Description("Quotes \"")]   Quotes,
               [Description(@"Literal Quotes """)]   LiteralQuotes,
               [Description("Backslash \\")]   Backslash,
               [Description(@"LiteralBackslash \")]   BackslashLiteral,
               [Description("New\nLine")]   NewLine,
            }
            """";

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateForExternalEnum()
    {
        const string input = """
                             using System;
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<StringComparison>()]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateForExternalFlagsEnum()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.IO.FileShare>()]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateForMultipleExternalEnums()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.ConsoleColor>()]
                             [assembly:EnumExtensions<System.DateTimeKind>()]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedTrees<TrackingNames>(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output.WhereNotGeneratedAttribute(), Settings());
    }

    [Fact]
    public Task CanGenerateExternalEnumExtensionsWithCustomName()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.DateTimeKind>(ExtensionClassName = "A")]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateExternalEnumExtensionsWithCustomNamespace()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.DateTimeKind>(ExtensionClassNamespace = "A.B")]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateExternalEnumExtensionsWithCustomNamespaceAndName()
    {
        const string input = """
                             using NetEscapades.EnumGenerators;

                             [assembly:EnumExtensions<System.DateTimeKind>(ExtensionClassNamespace = "A.B", ExtensionClassName = "C")]
                             """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task DoesNotGenerateWarningsForObsoleteMembers_CS0612_Issue97()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum MyEnum
            {
                First,
                [Obsolete]
                Second,
            }

            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task DoesNotGenerateWarningsForObsoleteMembers_CS0618_Issue97()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum MyEnum
            {
                First,
                [Obsolete("This is obsolete")]
                Second,
            }

            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task DoesNotGenerateWarningsForObsoleteEnums_CS0612_Issue97()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;

            [Obsolete]
            [EnumExtensions]
            public enum MyEnum
            {
                First,
                Second,
            }

            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task DoesNotGenerateWarningsForObsoleteEnums_CS0618_Issue97()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;

            [Obsolete("This is obsolete", false)]
            [EnumExtensions]
            public enum MyEnum
            {
                First,
                Second,
            }

            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }
}