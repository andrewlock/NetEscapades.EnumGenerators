using System.Collections.Generic;
using System.Linq;
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
            new Dictionary<string, EnumValueOption>
            {
                { "First", new EnumValueOption(null, false) }, 
                { "Second", new EnumValueOption(null, false) }
            }.ToList(),
            hasFlags: false,
            isDisplaAttributeUsed: false);

        var sb = new StringBuilder();
        var result = SourceGenerationHelper.GenerateExtensionClass(sb, value);

        return Verifier.Verify(result)
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
            new Dictionary<string, EnumValueOption>
            {
                { "First", new EnumValueOption(null, false) },
                { "Second", new EnumValueOption(null, false) }
            }.ToList(),
            hasFlags: true,
            isDisplaAttributeUsed: false);

        var sb = new StringBuilder();
        var result = SourceGenerationHelper.GenerateExtensionClass(sb, value);

        return Verifier.Verify(result)
            .UseDirectory("Snapshots");
    }
}