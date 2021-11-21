using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class EnumInNamespaceExtensionsTests : ExtensionTests<EnumInNamespace>
{
    public static TheoryData<EnumInNamespace> ValidEnumValues() => new()
    {
        EnumInNamespace.First,
        EnumInNamespace.Second,
        (EnumInNamespace)3,
    };

    public static TheoryData<string> ValuesToParse() => new()
    {
        "First",
        "Second",
        "first",
        "SECOND",
        "3",
        "267",
        "-267",
        "2147483647",
        "3000000000",
        "Fourth",
    };

    protected override string ToStringFast(EnumInNamespace value) => value.ToStringFast();
    protected override bool IsDefined(EnumInNamespace value) => value.IsDefined();
    protected override bool TryParse(string name,bool ignoreCase, out EnumInNamespace parsed)
        => EnumInNamespaceExtensions.TryParse(name, ignoreCase, out parsed);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumInNamespace value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumInNamespace value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseIgnoreCaseTest(name);

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumInNamespaceExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumInNamespaceExtensions.GetNames());
}