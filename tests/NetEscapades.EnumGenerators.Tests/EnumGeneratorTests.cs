using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NetEscapades.EnumGenerators.Interceptors;
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
        => [new EnumGenerator(), new InterceptorGenerator()];
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
        const string input =
            """
            using NetEscapades.EnumGenerators;

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

    [Fact(Skip = "CSharp14 is not available as an enum yet and throws at runtime if you cast to it")]
    public Task CanGenerateEnumExtensionsInGlobalNamespace_CSharp14()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum MyEnum
            {
                First,
                Second,
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(LanguageVersion.CSharp13, options: null!, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateEnumExtensionsInGlobalNamespace_ForceExtensions()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum MyEnum
            {
                First,
                Second,
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(
            Generators(), new(LanguageVersion.Preview, options: new() { { "build_property.EnumGenerator_ForceExtensionMembers", "true" } }, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task PreviewLangVersionDoesntGenerateExtensionMembers()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum MyEnum
            {
                First,
                Second,
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(LanguageVersion.Preview, options: null!, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateEnumExtensionsInChildNamespace()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                public enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateEnumExtensionsInNestedClass()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

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
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithCustomName()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(ExtensionClassName = "A")]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithCustomNamespace()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(ExtensionClassNamespace = "A.B")]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Fact]
    public Task CanGenerateEnumExtensionsWithCustomNamespaceAndName()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(ExtensionClassNamespace = "A.B", ExtensionClassName = "C")]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Theory]
    [InlineData(MetadataSource.DisplayAttribute)]
    [InlineData(MetadataSource.DescriptionAttribute)]
    [InlineData(MetadataSource.EnumMemberAttribute)]
    public Task CanGenerateEnumExtensionsWithMetadataName(MetadataSource source)
    {
        var attr = GetAttribute(source);

        var input =
            $$"""
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(MetadataSource = MetadataSource.{{source}})]
                public enum MyEnum
                {
                    First = 0,

                    [{{attr}}"2nd")]
                    Second = 1,
                    Third = 2,

                    [{{attr}}"4th")]
                    Fourth = 3
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseMethodName("CanGenerateEnumExtensionsWithCustomNames");
        settings.UseTextForParameters("_");
        ScrubAttribute(settings, source);
        return Verifier.Verify(output, settings);
    }

    [Theory]
    [InlineData(MetadataSource.DisplayAttribute)]
    [InlineData(MetadataSource.DescriptionAttribute)]
    [InlineData(MetadataSource.EnumMemberAttribute)]
    public Task CanGenerateEnumExtensionsWithNoneMetadataName(MetadataSource source)
    {
        var attr = GetAttribute(source);

        var input =
            $$"""
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(MetadataSource = MetadataSource.None)]
                public enum MyEnum
                {
                    First = 0,

                    [{{attr}}"2nd")]
                    Second = 1,
                    Third = 2,

                    [{{attr}}"4th")]
                    Fourth = 3
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseTextForParameters("_");
        ScrubAttribute(settings, source);
        return Verifier.Verify(output, settings);
    }

    [Theory]
    [InlineData(MetadataSource.DisplayAttribute)]
    [InlineData(MetadataSource.DescriptionAttribute)]
    [InlineData(MetadataSource.EnumMemberAttribute)]
    public Task CanGenerateEnumExtensionsWithWrongMetadataName(MetadataSource source)
    {
        var wrongSource = GetWrongSource(source);
        var attr = GetAttribute(source);

        var input =
            $$"""
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(MetadataSource = MetadataSource.{{wrongSource}})]
                public enum MyEnum
                {
                    First = 0,

                    [{{attr}}"2nd")]
                    Second = 1,
                    Third = 2,

                    [{{attr}}"4th")]
                    Fourth = 3
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseTextForParameters("_");
        ScrubAttribute(settings, wrongSource);
        return Verifier.Verify(output, settings);
    }

    [Theory]
    [InlineData(MetadataSource.DisplayAttribute)]
    [InlineData(MetadataSource.DescriptionAttribute)]
    [InlineData(MetadataSource.EnumMemberAttribute)]
    public Task CanGenerateEnumExtensionsWithMetadataName_IgnoringOthers(MetadataSource source)
    {
        var attr = GetAttribute(source);
        var other = GetWrongAttribute(source);

        var input =
            $$"""
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(MetadataSource = MetadataSource.{{source}})]
                public enum MyEnum
                {
                    First = 0,

                    [{{attr}}"2nd")]
                    [{{other}}"Secundo")]
                    Second = 1,
                    Third = 2,

                    [{{attr}}"4th")]
                    [{{other}}"Number 4")]
                    Fourth = 3
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseTextForParameters("_");
        ScrubAttribute(settings, source);
        return Verifier.Verify(output, settings);
    }

    [Theory]
    [InlineData(MetadataSource.DisplayAttribute)]
    [InlineData(MetadataSource.DescriptionAttribute)]
    [InlineData(MetadataSource.EnumMemberAttribute)]
    public Task CanGenerateEnumExtensionsWithSameDisplayName(MetadataSource source)
    {
        var attr = GetAttribute(source);

        var input =
            $$"""
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions(MetadataSource = MetadataSource.{{source}})]
                public enum MyEnum
                {
                    First = 0,

                    [{{attr}}"2nd")]
                    Second = 1,
                    Third = 2,

                    [{{attr}}"2nd")]
                    Fourth = 3
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseTextForParameters("_");
        ScrubAttribute(settings, source);
        return Verifier.Verify(output, settings);
    }

    [Theory]
    [InlineData("", "System.Flags")]
    [InlineData("", "System.FlagsAttribute")]
    [InlineData("using System;", "FlagsAttribute")]
    [InlineData("using System;", "Flags")]
    public Task CanGenerateEnumExtensionsForFlagsEnum(string usings, string attribute)
    {
        string input =
            $$"""
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
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace Foo
            {
                public class Foo {}

                [EnumExtensions]
                public enum TestEnum
                {
                    Value1
                }
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output, Settings());
    }

    [Theory]
    [InlineData(MetadataSource.DisplayAttribute)]
    [InlineData(MetadataSource.DescriptionAttribute)]
    [InlineData(MetadataSource.EnumMemberAttribute)]
    public Task HandlesStringsWithQuotesAndSlashesInDescription(MetadataSource source)
    {
        var attr = GetAttribute(source);
        var input =
            $$""""
            using NetEscapades.EnumGenerators;

            namespace Test;

            [EnumExtensions(MetadataSource = MetadataSource.{{source}})]
            public enum StringTesting
            {
               [{{attr}}"Quotes \"")]   Quotes,
               [{{attr}}@"Literal Quotes """)]   LiteralQuotes,
               [{{attr}}"Backslash \\")]   Backslash,
               [{{attr}}@"LiteralBackslash \")]   BackslashLiteral,
               [{{attr}}"New\nLine")]   NewLine,
            }
            """";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(input));

        Assert.Empty(diagnostics);
        var settings = Settings();
        settings.UseTextForParameters("_");
        ScrubAttribute(settings, source);
        return Verifier.Verify(output, settings);
    }

    [Fact]
    public Task CanGenerateForExternalEnum()
    {
        const string input =
            """
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
        const string input =
            """
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
        const string input =
            """
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
        const string input =
            """
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
        const string input =
            """
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
        const string input =
            """
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

    [Fact]
    public void DoesNotGenerateWithoutAttribute()
    {
        const string input =
            """
            public enum MyEnum
            {
                First,
                Second,
            }
            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(stages: [], input));

        diagnostics.Should().BeEmpty();
        output.Should().BeEmpty();
    }

    [Fact]
    public void DoesNotGenerateInNestedGenericClass()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            public class Nested<T>
            {
                [EnumExtensions]
                public enum MyEnum
                {
                    First,
                    Second,
                }
            }
            """;

        var (diagnostics, output) = TestHelpers.GetGeneratedOutput(Generators(), new(stages: [], input));

        diagnostics.Should().BeEmpty();
        output.Should().BeEmpty();
    }

    private static void ScrubAttribute(VerifySettings settings, MetadataSource source)
    {
        var toScrub = source switch
        {
            MetadataSource.DisplayAttribute => "System.ComponentModel.DataAnnotations.Display",
            MetadataSource.DescriptionAttribute => "System.ComponentModel.Description",
            MetadataSource.EnumMemberAttribute => "System.Runtime.Serialization.EnumMember",
            _ => null,
        };

        if (toScrub is null)
        {
            return;
        }

        settings.ScrubLinesWithReplace(original => original?.Replace(toScrub, "Metadata"));
    }

    private static string GetAttribute(MetadataSource source) =>
        source switch
        {
            MetadataSource.DisplayAttribute => "System.ComponentModel.DataAnnotations.Display(Name = ",
            MetadataSource.DescriptionAttribute => "System.ComponentModel.Description(",
            MetadataSource.EnumMemberAttribute => "System.Runtime.Serialization.EnumMember(Value = ",
            _ => throw new InvalidOperationException("Unknown source type " + source),
        };

    private static string GetWrongAttribute(MetadataSource source)
        => GetAttribute(GetWrongSource(source));

    private static MetadataSource GetWrongSource(MetadataSource source)
    {
        return source switch
        {
            MetadataSource.DisplayAttribute => MetadataSource.EnumMemberAttribute,
            MetadataSource.DescriptionAttribute => MetadataSource.DisplayAttribute,
            MetadataSource.EnumMemberAttribute => MetadataSource.DescriptionAttribute,
            _ => throw new InvalidOperationException("Unknown source type " + source),
        };
    }
}