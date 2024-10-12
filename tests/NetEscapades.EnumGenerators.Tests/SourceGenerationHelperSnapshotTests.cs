using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

#if INTERCEPTORS
namespace NetEscapades.EnumGenerators.Tests;
#else
namespace NetEscapades.EnumGenerators.Tests.Roslyn4_04;
#endif

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
                ("First", new EnumValueOption(null, false)),
                ("Second", new EnumValueOption(null, false)),
            },
            hasFlags: false,
            isDisplayAttributeUsed: false);

        var sb = new StringBuilder();
        var result = SourceGenerationHelper.GenerateExtensionClass(sb, value);

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
                ("First", new EnumValueOption(null, false)),
                ("Second", new EnumValueOption(null, false)),
            },
            hasFlags: true,
            isDisplayAttributeUsed: false);

        var sb = new StringBuilder();
        var result = SourceGenerationHelper.GenerateExtensionClass(sb, value);

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots");
    }
}