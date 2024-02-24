using System;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace NetEscapades.EnumGenerators.Tests;

[UsesVerify]
public class JsonConverterGeneratorTests
{
    [Flags]
    public enum TestParameters
    {
        None = 0,
        CamelCase = 1 << 0,
        CaseSensitive = 1 << 1,
        AllowMatchingMetadataAttribute = 1 << 2,
        PropertyName = 1 << 3
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsInGlobalNamespace(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

[EnumExtensions]
[EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
[JsonConverter(typeof(MyEnumConverter))]
public enum MyEnum
{
    First,
    Second
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsInChildNamespace(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions]
    [EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
    [JsonConverter(typeof(MyEnumConverter))]
    public enum MyEnum
    {
        First = 0,
        Second = 1
    }
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsInNestedClass(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    public class InnerClass
    {
        [EnumExtensions]
        [EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
        [JsonConverter(typeof(MyEnumConverter))]
        internal enum MyEnum
        {
            First = 0,
            Second = 1
        }
    }
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsWithCustomName(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions(ExtensionClassName = "A")]
    [EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
    [JsonConverter(typeof(MyEnumConverter))]
    internal enum MyEnum
    {
        First = 0,
        Second = 1
    }
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsWithCustomNamespace(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions(ExtensionClassNamespace = "A.B")]
    [EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
    [JsonConverter(typeof(MyEnumConverter))]
    internal enum MyEnum
    {
        First = 0,
        Second = 1
    }
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsWithCustomNamespaceAndName(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;

namespace MyTestNameSpace
{
    [EnumExtensions(ExtensionClassNamespace = "A.B", ExtensionClassName = "C")]
    [EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
    [JsonConverter(typeof(MyEnumConverter))]
    internal enum MyEnum
    {
        First = 0,
        Second = 1
    }
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsWithDisplayName(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;

namespace MyTestNameSpace
{
    [EnumExtensions]
    [EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
    [JsonConverter(typeof(MyEnumConverter))]
    public enum MyEnum
    {
        First = 0,

        [Display(Name = "2nd")]
        Second = 1,
        Third = 2,

        [Display(Name = "4th")]
        Fourth = 3
    }
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    [Theory]
    [InlineData(TestParameters.None)]
    [InlineData(TestParameters.CamelCase)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CamelCase | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.CamelCase | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.PropertyName)]
    [InlineData(TestParameters.CaseSensitive | TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute)]
    [InlineData(TestParameters.AllowMatchingMetadataAttribute | TestParameters.PropertyName)]
    [InlineData(TestParameters.PropertyName)]
    public Task CanGenerateEnumExtensionsWithSameDisplayName(TestParameters testParameters)
    {
        var (camelCase, caseSensitive, allowMatchingMetadataAttribute, propertyName) = GetTestSettings(testParameters);
        var input = $$"""
using System.Text.Json.Serialization;
using NetEscapades.EnumGenerators;
using System.ComponentModel.DataAnnotations;

namespace MyTestNameSpace
{
    [EnumExtensions]
    [EnumJsonConverterAttribute(typeof(MyEnumConverter), CamelCase = {{camelCase}}, CaseSensitive = {{caseSensitive}}, AllowMatchingMetadataAttribute = {{allowMatchingMetadataAttribute}}{{propertyName}}]
    [JsonConverter(typeof(MyEnumConverter))]
    public enum MyEnum
    {
        First = 0,

        [Display(Name = "2nd")]
        Second = 1,
        Third = 2,

        [Display(Name = "2nd")]
        Fourth = 3
    }
}
""";
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<JsonConverterGenerator>(input);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseTextForParameters(GetParametersText(testParameters)).UseDirectory("Snapshots");
    }

    private static string GetParametersText(TestParameters testParameters) =>
        testParameters.ToString().Replace(", ", "_");

    private static (string CamelCase, string CaseSensitive, string AllowMatchingMetadataAttribute, string PropertyName)
        GetTestSettings(TestParameters testParameters) =>
    (
        (testParameters & TestParameters.CamelCase) != 0 ? "true" : "false",
        (testParameters & TestParameters.CaseSensitive) != 0 ? "true" : "false",
        (testParameters & TestParameters.AllowMatchingMetadataAttribute) != 0 ? "true" : "false",
        (testParameters & TestParameters.PropertyName) != 0 ? """, PropertyName = "Bla")""" : ")"
    );
}
