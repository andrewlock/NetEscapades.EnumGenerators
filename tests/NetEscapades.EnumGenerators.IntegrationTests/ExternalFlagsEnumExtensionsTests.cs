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
#elif NUGET_INTEGRATION_TESTS
namespace NetEscapades.EnumGenerators.Nuget.IntegrationTests;
#elif NUGET_INTERCEPTOR_TESTS
namespace NetEscapades.EnumGenerators.Nuget.Interceptors.IntegrationTests;
#else
#error Unknown integration tests
#endif

public class ExternalFileShareExtensionsTests : ExtensionTests<FileShare, int, ExternalFileShareExtensionsTests>, ITestData<FileShare>
{
    public TheoryData<FileShare> ValidEnumValues() => new()
    {
        FileShare.Read,
        FileShare.Write,
        FileShare.Delete | FileShare.Read,
        (FileShare)3,
    };

    public TheoryData<string> ValuesToParse() => new()
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

    protected override string[] GetNames() => FileShareExtensions.GetNames();
    protected override FileShare[] GetValues() => FileShareExtensions.GetValues();
    protected override int[] GetValuesAsUnderlyingType() => FileShareExtensions.GetValuesAsUnderlyingType();
    protected override int AsUnderlyingValue(FileShare value) => value.AsUnderlyingType();

    protected override string ToStringFast(FileShare value) => value.ToStringFast();
    protected override string ToStringFast(FileShare value, bool withMetadata) => value.ToStringFast(withMetadata);
    protected override string ToStringFast(FileShare value, SerializationOptions options) => value.ToStringFast(options);
    protected override bool IsDefined(FileShare value) => FileShareExtensions.IsDefined(value);
    protected override bool IsDefined(string name, bool allowMatchingMetadataAttribute) => FileShareExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#if READONLYSPAN
    protected override bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute) => FileShareExtensions.IsDefined(name, allowMatchingMetadataAttribute: false);
#endif
    protected override bool TryParse(string name, out FileShare parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out FileShare parsed, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.TryParse(name, out parsed, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override bool TryParse(string name, out FileShare parsed, EnumParseOptions parseOptions)
        => FileShareExtensions.TryParse(name, out parsed, parseOptions);
#if READONLYSPAN
    protected override bool TryParse(in ReadOnlySpan<char> name, out FileShare parsed, EnumParseOptions parseOptions)
        => FileShareExtensions.TryParse(name, out parsed, parseOptions);
#endif

    protected override FileShare Parse(string name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#if READONLYSPAN
    protected override FileShare Parse(in ReadOnlySpan<char> name, bool ignoreCase, bool allowMatchingMetadataAttribute)
        => FileShareExtensions.Parse(name, ignoreCase, allowMatchingMetadataAttribute);
#endif
    protected override FileShare Parse(string name, EnumParseOptions parseOptions)
        => FileShareExtensions.Parse(name, parseOptions);
#if READONLYSPAN
    protected override FileShare Parse(in ReadOnlySpan<char> name, EnumParseOptions parseOptions)
        => FileShareExtensions.Parse(name, parseOptions);
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
}