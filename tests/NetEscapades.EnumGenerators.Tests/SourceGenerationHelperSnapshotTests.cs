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
    [CombinatorialData]
    public Task GeneratesEnumCorrectly(
        bool csharp14IsSupported,
        [CombinatorialValues(MetadataSource.None, MetadataSource.EnumMemberAttribute)]
        MetadataSource defaultSource,
        bool useCollectionExpressions,
        bool hasRuntimeDeps)
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
            defaultSource,
            hasRuntimeDeps).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseTextForParameters($"{csharp14IsSupported}_{defaultSource}_{useCollectionExpressions}_{(hasRuntimeDeps ? "deps" : "nodeps")}");
    }

    [Theory]
    [CombinatorialData]
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
            useCollectionExpressions: false, DefaultMetadataSource, hasRuntimeDependencies: true).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported);
    }

    [Theory]
    [CombinatorialData]
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
            useCollectionExpressions: false, DefaultMetadataSource, hasRuntimeDependencies: true).Content;

        return Verifier.Verify(result)
            .ScrubExpectedChanges()
            .UseDirectory("Snapshots")
            .UseParameters(csharp14IsSupported);
    }
}