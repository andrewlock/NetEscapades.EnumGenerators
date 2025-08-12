using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

[UsesVerify]
public class SourceGenerationHelperSnapshotTests
{
    private const MetadataSource DefaultMetadataSource = MetadataSource.EnumMemberAttribute;

    [Theory]
    [InlineData(true, MetadataSource.EnumMemberAttribute)]
    [InlineData(false, MetadataSource.EnumMemberAttribute)]
    [InlineData(true, MetadataSource.None)]
    [InlineData(false, MetadataSource.None)]
    public Task GeneratesEnumCorrectly(bool csharp14IsSupported, MetadataSource defaultSource)
    {
        var value = new EnumToGenerate(
            "ShortName",
            "Something.Blah",
            "Something.Blah.ShortName",
            "int",
            isPublic: true,
            new List<(string Key, EnumValueOption Value)>
            {
                ("First", EnumValueOption.CreateWithoutAttributes(0)),
                ("Second", EnumValueOption.CreateWithoutAttributes(1)),
            },
            hasFlags: false,
            metadataSource: null);

        var result = SourceGenerationHelper.GenerateExtensionClass(value, csharp14IsSupported, defaultSource).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported, defaultSource);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task GeneratesEnumWithRepeatedValuesCorrectly(bool csharp14IsSupported)
    {
        var value = new EnumToGenerate(
            "ShortName",
            "Something.Blah",
            "Something.Blah.ShortName",
            "int",
            isPublic: true,
            new List<(string Key, EnumValueOption Value)>
            {
                ("First", EnumValueOption.CreateWithoutAttributes(0)),
                ("Second", EnumValueOption.CreateWithoutAttributes(1)),
                ("Third", EnumValueOption.CreateWithoutAttributes(0)),
            },
            hasFlags: false,
            null);

        var result = SourceGenerationHelper.GenerateExtensionClass(value, csharp14IsSupported, DefaultMetadataSource).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task GeneratesFlagsEnumCorrectly(bool csharp14IsSupported)
    {
        var value = new EnumToGenerate(
            "ShortName",
            "Something.Blah",
            "Something.Blah.ShortName",
            "int",
            isPublic: true,
            new List<(string, EnumValueOption)>
            {
                ("First", EnumValueOption.CreateWithoutAttributes(0)),
                ("Second", EnumValueOption.CreateWithoutAttributes(1)),
            },
            hasFlags: true,
            metadataSource: null);

        var result = SourceGenerationHelper.GenerateExtensionClass(value, csharp14IsSupported, DefaultMetadataSource).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported);
    }
}