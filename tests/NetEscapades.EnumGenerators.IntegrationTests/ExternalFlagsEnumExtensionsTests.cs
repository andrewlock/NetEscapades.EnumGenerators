using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

#if INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.IntegrationTests;
#elif NETSTANDARD_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.NetStandard.IntegrationTests;
#elif INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Interceptors.IntegrationTests;
#elif NUGET_ATTRS_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Attributes.IntegrationTests;
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#else
#error Unknown integration tests
#endif

public class ExternalFileShareExtensionsTests : ExtensionTests<FileShare>
{
    public static TheoryData<FileShare> ValidEnumValues() => new()
    {
        FileShare.Read,
        FileShare.Write,
        FileShare.Delete | FileShare.Read,
        (FileShare)3,
    };

    public static TheoryData<string> ValuesToParse() => new()
    {
        "Read",
        "Write",
        "BEEP",
        "Boop",
        "read",
        "WRITE",
        "3",
        "267",
        "-267",
        "2147483647",
        "3000000000",
        "Fourth",
        "Fifth",
    };

    protected override string ToStringFast(FileShare value) => value.ToStringFast();
    protected override bool IsDefined(FileShare value) => FileShareExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => FileShareExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => FileShareExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out FileShare parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.TryParse(name, out parsed, ignoreCase);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out FileShare parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.TryParse(name, out parsed, ignoreCase);
#endif
    protected override FileShare Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.Parse(name, ignoreCase);
#if READONLYSPAN
    protected override FileShare Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.Parse(name, ignoreCase);
#endif
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <remarks>If the underlying value of <paramref name="flag"/> is zero, the method returns true.
    /// This is consistent with the behaviour of <see cref=""Enum.HasFlag(Enum)""></remarks>
    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesToStringFast(FileShare value) => GeneratesToStringFastTest(value);

    [Theory]
    [MemberData(nameof(ValidEnumValues))]
    public void GeneratesIsDefined(FileShare value) => GeneratesIsDefinedTest(value);

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingName(string name) => GeneratesIsDefinedTest(name, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesIsDefinedUsingNameAsSpan(string name) => GeneratesIsDefinedTest(name.AsSpan(), allowMatchingMetadataAttribute: false);
#endif

    public static IEnumerable<object[]> AllFlags()
    {
        var values = new[]
        {
            FileShare.Read,
            FileShare.Write,
            FileShare.ReadWrite,
            FileShare.Inheritable,
            FileShare.Delete | FileShare.Read,
            (FileShare)65,
            (FileShare)0,
        };

        return from v1 in values
            from v2 in values
            select new object[] { v1, v2 };
    }
    
    [Theory]
    [MemberData(nameof(AllFlags))]
    public void HasFlags(FileShare value, FileShare flag)
    {
        var isDefined = value.HasFlagFast(flag);

        isDefined.Should().Be(value.HasFlag(flag));
    }

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParse(string name) => GeneratesTryParseTest(name, ignoreCase: false, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: false, allowMatchingMetadataAttribute: false);
#endif

    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCase(string name) => GeneratesTryParseTest(name, ignoreCase: true, allowMatchingMetadataAttribute: false);

#if READONLYSPAN
    [Theory]
    [MemberData(nameof(ValuesToParse))]
    public void GeneratesTryParseIgnoreCaseAsSpan(string name) => GeneratesTryParseTest(name.AsSpan(), ignoreCase: true, allowMatchingMetadataAttribute: false);
#endif

    // Ignoring order in these
    [Fact]
    public void GeneratesGetValues()
        => FileShareExtensions.GetValues()
            .Should()
            .BeEquivalentTo((FileShare[])Enum.GetValues(typeof(FileShare)));

    [Fact]
    public void GeneratesGetNames()
        => FileShareExtensions.GetNames()
            .Should()
            .BeEquivalentTo(Enum.GetNames(typeof(FileShare)));
}