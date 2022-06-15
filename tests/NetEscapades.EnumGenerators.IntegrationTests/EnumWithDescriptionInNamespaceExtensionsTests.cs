using FluentAssertions;
using System;
using System.ComponentModel;
using System.Reflection;
using Xunit;

namespace NetEscapades.EnumGenerators.IntegrationTests;

public class EnumWithDescriptionInNamespaceExtensionsTests : ExtensionTests<EnumWithDescriptionInNamespace>
{
    public static TheoryData<EnumWithDescriptionInNamespace> ValidEnumValues() => new()
    {
        EnumWithDescriptionInNamespace.First,
        EnumWithDescriptionInNamespace.Second,
        EnumWithDescriptionInNamespace.Third,
        (EnumWithDescriptionInNamespace)3,
    };

    public static TheoryData<string> ValuesToParse() => new()
    {
        "First",
        "Second",
        "2nd",
        "2ND",
        "first",
        "SECOND",
        "3",
        "3rd",
        "3RD",
        "267",
        "-267",
        "2147483647",
        "3000000000",
        "Fourth",
        "Fifth",
    };

    protected override string ToStringFast(EnumWithDescriptionInNamespace value) => value.ToStringFast();
    protected override bool IsDefined(EnumWithDescriptionInNamespace value) => EnumWithDescriptionInNamespaceExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => EnumWithDescriptionInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute = false) => EnumWithDescriptionInNamespaceExtensions.IsDefined(name, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out EnumWithDescriptionInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDescriptionInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
    protected override bool TryParse(in ReadOnlySpan<char> name, out EnumWithDescriptionInNamespace parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => EnumWithDescriptionInNamespaceExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(EnumWithDescriptionInNamespace value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(EnumWithDescriptionInNamespace value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesGetDescription(EnumWithDescriptionInNamespace value)
    {
        var serialized = value.GetDescription();
        var valueAsString = value.ToString();

        TryGetDescription<EnumWithDescriptionInNamespace>(valueAsString, out var description);
        var expectedValue = description is null ? valueAsString : description;

        serialized.Should().Be(expectedValue);
    }

    private bool TryGetDescription<T>(
        string? value,
#if NETCOREAPP3_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? description)
#else
        out string? description) where T : struct
#endif
    {
        description = default;

        if (typeof(T).IsEnum)
        {
            // Prevent: Warning CS8604  Possible null reference argument for parameter 'name' in 'MemberInfo[] Type.GetMember(string name)'
            if (value is not null)
            {
                var memberInfo = typeof(T).GetMember(value);
                if (memberInfo.Length > 0)
                {
                    description = memberInfo[0].GetCustomAttribute<DescriptionAttribute>()?.Description;
                    if (description is null)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: false);

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttribute(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: true);

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameallowMatchingMetadataAttributeAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: true);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: false);

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: true);

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: true);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

#if NETCOREAPP
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttribute(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: true);

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseallowMatchingMetadataAttributeAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: true);
#endif

    [Fact]
    public void GeneratesGetValues() => GeneratesGetValuesTest(EnumWithDescriptionInNamespaceExtensions.GetValues());

    [Fact]
    public void GeneratesGetNames() => base.GeneratesGetNamesTest(EnumWithDescriptionInNamespaceExtensions.GetNames());
}