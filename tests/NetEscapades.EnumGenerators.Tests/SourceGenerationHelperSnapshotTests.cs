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
    [InlineData(true, MetadataSource.EnumMemberAttribute, false)]
    [InlineData(false, MetadataSource.EnumMemberAttribute, false)]
    [InlineData(true, MetadataSource.None, false)]
    [InlineData(false, MetadataSource.None, false)]
    [InlineData(true, MetadataSource.EnumMemberAttribute, true)]
    [InlineData(false, MetadataSource.EnumMemberAttribute, true)]
    [InlineData(true, MetadataSource.None, true)]
    [InlineData(false, MetadataSource.None, true)]
    public Task GeneratesEnumCorrectly(bool csharp14IsSupported, MetadataSource defaultSource, bool useCollectionExpressions)
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

        var result = SourceGenerationHelper.GenerateExtensionClass(
            value, 
            csharp14IsSupported,
            useCollectionExpressions: useCollectionExpressions,
            defaultSource).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseTextForParameters($"{csharp14IsSupported}_{defaultSource}_{useCollectionExpressions}");
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

        var result = SourceGenerationHelper.GenerateExtensionClass(value, csharp14IsSupported,
            useCollectionExpressions: false, DefaultMetadataSource).Content;

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

        var result = SourceGenerationHelper.GenerateExtensionClass(value, csharp14IsSupported,
            useCollectionExpressions: false, DefaultMetadataSource).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported);
    }
}