using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

[UsesVerify]
public class SourceGenerationHelperSnapshotTests
{
    [Fact]
    public Task GeneratesEnumCorrectly()
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

        var result = SourceGenerationHelper.GenerateExtensionClass(value).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots");
    }

    [Fact]
    public Task GeneratesFlagsEnumCorrectly()
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

        var result = SourceGenerationHelper.GenerateExtensionClass(value).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots");
    }
}