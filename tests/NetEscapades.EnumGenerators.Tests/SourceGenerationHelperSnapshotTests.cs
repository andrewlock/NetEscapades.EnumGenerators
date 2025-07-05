using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

[UsesVerify]
public class SourceGenerationHelperSnapshotTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public Task GeneratesEnumCorrectly(bool csharp14IsSupported, bool readonlySpan)
    {
        var value = new EnumToGenerate(
            "ShortName",
            "Something.Blah",
            "Something.Blah.ShortName",
            "int",
            isPublic: true,
            new List<(string Key, EnumValueOption Value)>
            {
                ("First", new EnumValueOption(null, false, 0)),
                ("Second", new EnumValueOption(null, false, 1)),
            },
            hasFlags: false,
            isDisplayAttributeUsed: false);

        var result = SourceGenerationHelper.GenerateExtensionClass(value, new (csharp14IsSupported, readonlySpan)).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported, readonlySpan);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public Task GeneratesEnumWithRepeatedValuesCorrectly(bool csharp14IsSupported, bool readonlySpan)
    {
        var value = new EnumToGenerate(
            "ShortName",
            "Something.Blah",
            "Something.Blah.ShortName",
            "int",
            isPublic: true,
            new List<(string Key, EnumValueOption Value)>
            {
                ("First", new EnumValueOption(null, false, 0)),
                ("Second", new EnumValueOption(null, false, 1)),
                ("Third", new EnumValueOption(null, false, 0)),
            },
            hasFlags: false,
            isDisplayAttributeUsed: false);

        var result = SourceGenerationHelper.GenerateExtensionClass(value, new (csharp14IsSupported, readonlySpan)).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported, readonlySpan);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public Task GeneratesFlagsEnumCorrectly(bool csharp14IsSupported, bool readonlySpan)
    {
        var value = new EnumToGenerate(
            "ShortName",
            "Something.Blah",
            "Something.Blah.ShortName",
            "int",
            isPublic: true,
            new List<(string, EnumValueOption)>
            {
                ("First", new EnumValueOption(null, false, 0)),
                ("Second", new EnumValueOption(null, false, 1)),
            },
            hasFlags: true,
            isDisplayAttributeUsed: false);

        var result = SourceGenerationHelper.GenerateExtensionClass(value, new (csharp14IsSupported, readonlySpan)).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported, readonlySpan);
    }
}