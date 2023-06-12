using System.Text;

namespace NetEscapades.EnumGenerators;

public static class SourceGenerationHelper
{
    private const string Header = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the NetEscapades.EnumGenerators source generator
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable";

    public const string Attribute = Header + @"

#if NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES
namespace NetEscapades.EnumGenerators
{
    /// <summary>
    /// Add to enums to indicate that extension methods should be generated for the type
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Enum)]
    [global::System.Diagnostics.Conditional(""NETESCAPADES_ENUMGENERATORS_USAGES"")]
#if NET5_0_OR_GREATER
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = ""Generated by the NetEscapades.EnumGenerators source generator."")]
#else
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
    public class EnumExtensionsAttribute : global::System.Attribute
    {
        /// <summary>
        /// The namespace to generate the extension class.
        /// If not provided the namespace of the enum will be used
        /// </summary>
        public string? ExtensionClassNamespace { get; set; }

        /// <summary>
        /// The name to use for the extension class.
        /// If not provided, the enum name with ""Extensions"" will be used.
        /// For example for an Enum called StatusCodes, the default name
        /// will be StatusCodesExtensions
        /// </summary>
        public string? ExtensionClassName { get; set; }
    }
}
#endif
";

    public static string GenerateExtensionClass(StringBuilder sb, in EnumToGenerate enumToGenerate)
    {
        sb
            .Append(Header)
            .Append(@"
#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
using System;
#endif
");
        if (!string.IsNullOrEmpty(enumToGenerate.Namespace))
        {
            sb.Append(@"
namespace ").Append(enumToGenerate.Namespace).Append(@"
{");
        }

        var fullyQualifiedName = $"global::{enumToGenerate.FullyQualifiedName}";

        sb.Append(@"
    /// <summary>
    /// Extension methods for <see cref=""").Append(fullyQualifiedName).Append(@""" />
    /// </summary>
    ").Append(enumToGenerate.IsPublic ? "public" : "internal").Append(@" static partial class ").Append(enumToGenerate.Name).Append(@"
    {
        /// <summary>
        /// The number of members in the enum.
        /// This is a non-distinct count of defined names.
        /// </summary>
        public const int Length = ").Append(enumToGenerate.Names.Count).Append(";").Append(@"

        /// <summary>
        /// Returns the string representation of the <see cref=""").Append(fullyQualifiedName).Append(@"""/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name=""value""/>.
        /// </summary>
        /// <param name=""value"">The value to retrieve the string value for</param>
        /// <returns>The string representation of the value</returns>
        public static string ToStringFast(this ").Append(fullyQualifiedName).Append(@" value)
            => value switch
            {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                ").Append(fullyQualifiedName).Append('.').Append(member.Key)
                .Append(" => ");

            if (member.Value.DisplayName is null)
            {
                sb.Append("nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key).Append("),");
            }
            else
            {
                sb.Append('"').Append(member.Value.DisplayName).Append(@""",");
            }
        }

        sb.Append(@"
                _ => value.ToString(),
            };");

        if (enumToGenerate.HasFlags)
        {
            sb.Append(@"

        /// <summary>
        /// Determines whether one or more bit fields are set in the current instance.
        /// Equivalent to calling <see cref=""global::System.Enum.HasFlag"" /> on <paramref name=""value""/>.
        /// </summary>
        /// <param name=""value"">The value of the instance to investigate</param>
        /// <param name=""flag"">The flag to check for</param>
        /// <returns><c>true</c> if the fields set in the flag are also set in the current instance; otherwise <c>false</c>.</returns>
        /// <remarks>If the underlying value of <paramref name=""flag""/> is zero, the method returns true.
        /// This is consistent with the behaviour of <see cref=""global::System.Enum.HasFlag"" /></remarks>
        public static bool HasFlagFast(this ").Append(fullyQualifiedName).Append(@" value, ").Append(fullyQualifiedName).Append(@" flag)
            => flag == 0 ? true : (value & flag) == flag;");
        }

        sb.Append(@"

        /// <summary>
        /// Returns a boolean telling whether the given enum value exists in the enumeration.
        /// </summary>
        /// <param name=""value"">The value to check if it's defined</param>
        /// <returns><c>true</c> if the value exists in the enumeration, <c>false</c> otherwise</returns>
       public static bool IsDefined(").Append(fullyQualifiedName).Append(@" value)
            => value switch
            {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                ").Append(fullyQualifiedName).Append('.').Append(member.Key)
                .Append(" => true,");
        }

        sb.Append(@"
                _ => false,
            };");

        sb.Append(@"

        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
        /// </summary>
        /// <param name=""name"">The name to check if it's defined</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
        public static bool IsDefined(string name) => IsDefined(name, allowMatchingMetadataAttribute: false);

        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
        /// or if a member decorated with a <c>[Display]</c> attribute
        /// with the required name exists.
        /// </summary>
        /// <param name=""name"">The name to check if it's defined</param>
        /// <param name=""allowMatchingMetadataAttribute"">If <c>true</c>, considers the value of metadata attributes,otherwise ignores them</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
        /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
        public static bool IsDefined(string name, bool allowMatchingMetadataAttribute)
        {");
        if (enumToGenerate.IsDisplayAttributeUsed)
        {
            sb.Append(@"
            var isDefinedInDisplayAttribute = false;
            if (allowMatchingMetadataAttribute)
            {
                isDefinedInDisplayAttribute = name switch
                {");
            foreach (var member in enumToGenerate.Names)
            {
                if (member.Value.DisplayName is not null && member.Value.IsDisplayNameTheFirstPresence)
                {
                    sb.Append(@"
                    """).Append(member.Value.DisplayName).Append(@""" => true,");
                }
            }

            sb.Append(@"
                    _ => false,
                };
            }

            if (isDefinedInDisplayAttribute)
            {
                return true;
            }

            ");
        }

        sb.Append(@"
            return name switch
            {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@") => true,");
        }

        sb.Append(@"
                _ => false,
            };
        }");

        sb.Append(@"

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration
        /// </summary>
        /// <param name=""name"">The name to check if it's defined</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
        public static bool IsDefined(in ReadOnlySpan<char> name) => IsDefined(name, allowMatchingMetadataAttribute: false);

        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
        /// or optionally if a member decorated with a <c>[Display]</c> attribute
        /// with the required name exists.
        /// Slower then the <see cref=""IsDefined(string, bool)"" /> overload, but doesn't allocate memory./>
        /// </summary>
        /// <param name=""name"">The name to check if it's defined</param>
        /// <param name=""allowMatchingMetadataAttribute"">If <c>true</c>, considers the value of metadata attributes,otherwise ignores them</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
        /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
        public static bool IsDefined(in ReadOnlySpan<char> name, bool allowMatchingMetadataAttribute)
        {");

        if (enumToGenerate.IsDisplayAttributeUsed)
        {
            sb.Append(@"
            var isDefinedInDisplayAttribute = false;
            if (allowMatchingMetadataAttribute)
            {
                isDefinedInDisplayAttribute = name switch
                {");
            foreach (var member in enumToGenerate.Names)
            {
                if (member.Value.DisplayName is not null && member.Value.IsDisplayNameTheFirstPresence)
                {
                    sb.Append(@"
                    ReadOnlySpan<char> current when current.Equals(""").Append(member.Value.DisplayName).Append(@""".AsSpan(), global::System.StringComparison.Ordinal) => true,");
                }
            }

            sb.Append(@"
                    _ => false,
                };
            }

            if (isDefinedInDisplayAttribute)
            {
                return true;
            }
");
        }

        sb.Append(@"
            return name switch
            {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                ReadOnlySpan<char> current when current.Equals(nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key)
                .Append(@").AsSpan(), global::System.StringComparison.Ordinal) => true,");
        }

        sb.Append(@"
                _ => false,
            };
        }
#endif");

        sb.Append(@"

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref=""").Append(fullyQualifiedName).Append(@""" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name=""name"">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name=""value"">When this method returns, contains an object of type 
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" /> whose
        /// value is represented by <paramref name=""value""/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref=""").Append(fullyQualifiedName).Append(@""" />. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            out ").Append(fullyQualifiedName).Append(@" value)
            => TryParse(name, out value, false, false);");
        sb.Append(@"

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref=""").Append(fullyQualifiedName).Append(@""" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name=""name"">The string representation of the enumeration name or underlying value to convert</param>
        /// <param name=""value"">When this method returns, contains an object of type 
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" /> whose
        /// value is represented by <paramref name=""value""/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref=""").Append(fullyQualifiedName).Append(@""" />. This parameter is passed uninitialized.</param>
        /// <param name=""ignoreCase""><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            out ").Append(fullyQualifiedName).Append(@" value,
            bool ignoreCase) 
            => TryParse(name, out value, ignoreCase, false);");
        sb.Append(@"

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref=""").Append(fullyQualifiedName).Append(@""" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name=""name"">The string representation of the enumeration name or underlying value to convert</param>
        /// <param name=""value"">When this method returns, contains an object of type 
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" /> whose
        /// value is represented by <paramref name=""value""/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref=""").Append(fullyQualifiedName).Append(@""" />. This parameter is passed uninitialized.</param>
        /// <param name=""ignoreCase""><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <param name=""allowMatchingMetadataAttribute"">If <c>true</c>, considers the value included in metadata attributes such as
        /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            out ").Append(fullyQualifiedName).Append(@" value, 
            bool ignoreCase, 
            bool allowMatchingMetadataAttribute)
        {");

        if (enumToGenerate.IsDisplayAttributeUsed)
        {
            sb.Append(@"
            if (allowMatchingMetadataAttribute)
            {
                if (ignoreCase)
                {
                    switch (name)
                    {");
            foreach (var member in enumToGenerate.Names)
            {
                if (member.Value.DisplayName is not null && member.Value.IsDisplayNameTheFirstPresence)
                {
                    sb.Append(@"
                        case string s when s.Equals(""").Append(member.Value.DisplayName).Append(@""", global::System.StringComparison.OrdinalIgnoreCase):
                            value = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                            return true;");
                }
            }

            sb.Append(@"
                        default:
                            break;
                    };
                }
                else
                {
                    switch (name)
                    {");
            foreach (var member in enumToGenerate.Names)
            {
                if (member.Value.DisplayName is not null && member.Value.IsDisplayNameTheFirstPresence)
                {
                    sb.Append(@"
                        case """).Append(member.Value.DisplayName).Append(@""":
                            value = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                            return true;");
                }
            }

            sb.Append(@"
                        default:
                            break;
                    };
                }
            }
");
        }

        sb.Append(@"
            if (ignoreCase)
            {
                switch (name)
                {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                    case string s when s.Equals(nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(
                @"), global::System.StringComparison.OrdinalIgnoreCase):
                        value = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                        return true;");
        }

        sb.Append(@"
                    case string s when ").Append(enumToGenerate.UnderlyingType).Append(@".TryParse(name, out var val):
                        value = (").Append(fullyQualifiedName).Append(@")val;
                        return true;
                    default:
                        value = default;
                        return false;
                }
            }
            else
            {
                switch (name)
                {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                    case nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@"):
                        value = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                        return true;");
        }

        sb.Append(@"
                    case string s when ").Append(enumToGenerate.UnderlyingType).Append(@".TryParse(name, out var val):
                        value = (").Append(fullyQualifiedName).Append(@")val;
                        return true;
                    default:
                        value = default;
                        return false;
                }
            }
        }");

        sb.Append(@"

#if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
        /// <summary>
        /// Converts the span representation of the name or numeric value of
        /// an <see cref=""").Append(fullyQualifiedName).Append(@""" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name=""name"">The span representation of the enumeration name or underlying value to convert</param>
        /// <param name=""value"">When this method returns, contains an object of type 
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" /> whose
        /// value is represented by <paramref name=""value""/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref=""").Append(fullyQualifiedName).Append(@""" />. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            in ReadOnlySpan<char> name, 
            out ").Append(fullyQualifiedName).Append(@" value)
            => TryParse(name, out value, false, false);");
        sb.Append(@"

        /// <summary>
        /// Converts the span representation of the name or numeric value of
        /// an <see cref=""").Append(fullyQualifiedName).Append(@""" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name=""name"">The span representation of the enumeration name or underlying value to convert</param>
        /// <param name=""value"">When this method returns, contains an object of type 
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" /> whose
        /// value is represented by <paramref name=""value""/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref=""").Append(fullyQualifiedName).Append(@""" />. This parameter is passed uninitialized.</param>
        /// <param name=""ignoreCase""><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            in ReadOnlySpan<char> name, 
            out ").Append(fullyQualifiedName).Append(@" value,
            bool ignoreCase) 
            => TryParse(name, out value, ignoreCase, false);");

        sb.Append(@"

        /// <summary>
        /// Converts the span representation of the name or numeric value of
        /// an <see cref=""").Append(fullyQualifiedName).Append(@""" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name=""name"">The span representation of the enumeration name or underlying value to convert</param>
        /// <param name=""result"">When this method returns, contains an object of type 
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" /> whose
        /// value is represented by <paramref name=""result""/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref=""").Append(fullyQualifiedName).Append(@""" />. This parameter is passed uninitialized.</param>
        /// <param name=""ignoreCase""><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <param name=""allowMatchingMetadataAttribute"">If <c>true</c>, considers the value included in metadata attributes such as
        /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            in ReadOnlySpan<char> name, 
            out ").Append(fullyQualifiedName).Append(@" result, 
            bool ignoreCase,             
            bool allowMatchingMetadataAttribute)
        {");

        if (enumToGenerate.IsDisplayAttributeUsed)
        {
            sb.Append(@"
            if (allowMatchingMetadataAttribute)
            {
                if (ignoreCase)
                {
                    switch (name)
                    {");
            foreach (var member in enumToGenerate.Names)
            {
                if (member.Value.DisplayName is not null && member.Value.IsDisplayNameTheFirstPresence)
                {
                    sb.Append(@"
                        case ReadOnlySpan<char> current when current.Equals(""").Append(member.Value.DisplayName).Append(
                        @""".AsSpan(), global::System.StringComparison.OrdinalIgnoreCase):
                            result = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                            return true;");
                }
            }

            sb.Append(@"
                        default:
                            break;
                    };
                }
                else
                {
                    switch (name)
                    {");
            foreach (var member in enumToGenerate.Names)
            {
                if (member.Value.DisplayName is not null && member.Value.IsDisplayNameTheFirstPresence)
                {
                    sb.Append(@"
                        case ReadOnlySpan<char> current when current.Equals(""").Append(member.Value.DisplayName).Append(@""".AsSpan(), global::System.StringComparison.Ordinal):
                            result = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                            return true;");
                }
            }

            sb.Append(@"
                        default:
                            break;
                    };
                }
            }
");
        }

        sb.Append(@"
            if (ignoreCase)
            {
                switch (name)
                {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                    case ReadOnlySpan<char> current when current.Equals(nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(
                @").AsSpan(), global::System.StringComparison.OrdinalIgnoreCase):
                        result = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                        return true;");
        }

        sb.Append(@"
                    case ReadOnlySpan<char> current when ").Append(enumToGenerate.UnderlyingType).Append(@".TryParse(name, out var numericResult):
                        result = (").Append(fullyQualifiedName).Append(@")numericResult;
                        return true;
                    default:
                        result = default;
                        return false;
                }
            }
            else
            {
                switch (name)
                {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                    case ReadOnlySpan<char> current when current.Equals(nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(
                @").AsSpan(), global::System.StringComparison.Ordinal):
                        result = ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(@";
                        return true;");
        }

        sb.Append(@"
                    case ReadOnlySpan<char> current when ").Append(enumToGenerate.UnderlyingType).Append(@".TryParse(name, out var numericResult):
                        result = (").Append(fullyQualifiedName).Append(@")numericResult;
                        return true;
                    default:
                        result = default;
                        return false;
                }
            }
        }
#endif");

        sb.Append(@"

        /// <summary>
        /// Retrieves an array of the values of the members defined in
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" />.
        /// Note that this returns a new array with every invocation, so
        /// should be cached if appropriate.
        /// </summary>
        /// <returns>An array of the values defined in <see cref=""").Append(fullyQualifiedName).Append(@""" /></returns>
        public static ").Append(fullyQualifiedName).Append(@"[] GetValues()
        {
            return new[]
            {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                ").Append(fullyQualifiedName).Append('.').Append(member.Key).Append(',');
        }

        sb.Append(@"
            };
        }");

        sb.Append(@"

        /// <summary>
        /// Retrieves an array of the names of the members defined in
        /// <see cref=""").Append(fullyQualifiedName).Append(@""" />.
        /// Note that this returns a new array with every invocation, so
        /// should be cached if appropriate.
        /// </summary>
        /// <returns>An array of the names of the members defined in <see cref=""").Append(fullyQualifiedName).Append(@""" /></returns>
        public static string[] GetNames()
        {
            return new[]
            {");
        foreach (var member in enumToGenerate.Names)
        {
            sb.Append(@"
                nameof(").Append(fullyQualifiedName).Append('.').Append(member.Key).Append("),");
        }

        sb.Append(@"
            };
        }
    }");
        if (!string.IsNullOrEmpty(enumToGenerate.Namespace))
        {
            sb.Append(@"
}");
        }

        return sb.ToString();
    }

    internal const string JsonConverterAttribute = $$"""
{{Header}}

#if NETESCAPADES_ENUMGENERATORS_EMBED_ATTRIBUTES

#if NET5_0_OR_GREATER
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Generated by the NetEscapades.EnumGenerators source generator.")]
#else
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
namespace NetEscapades.EnumGenerators
{
    /// <summary>
    /// Add to enums to indicate that a JsonConverter for the enum should be generated.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Enum)]
    [global::System.Diagnostics.Conditional("NETESCAPADES_ENUMGENERATORS_USAGES")]
    public class EnumJsonConverterAttribute : System.Attribute
    {
        /// <summary>
        /// The converter that should be generated.
        /// </summary>
        public global::System.Type ConverterType { get; }

        /// <summary>
        /// Indicates if the string representation is case sensitive when deserializing it as an enum.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Indicates if the value of <see cref=""PropertyName""/> should be camel cased.
        /// </summary>
        public bool CamelCase { get; set; }

        /// <summary>
        /// If set, this value will be used in messages when there are problems with validation and/or serialization/deserialization occurs.
        /// </summary>
        public string? PropertyName { get; set; }
        
        /// <summary>
        /// Creates an instance of <see cref="EnumJsonConverterAttribute"/>.
        /// </summary>
        /// <param name="converterType">The converter to generate.</param>
        public EnumJsonConverterAttribute(System.Type converterType) => ConverterType = converterType;
    }
}
#endif
""";

    public static string GenerateJsonConverterClass(StringBuilder sb, JsonConverterToGenerate jsonConverterToGenerate)
    {
        sb.Append(Header)
            .AppendLine()
            .AppendLine();

        if (!string.IsNullOrEmpty(jsonConverterToGenerate.ConverterNamespace))
            sb.AppendLine($$"""
        namespace {{jsonConverterToGenerate.ConverterNamespace}}
        {
        """);

        sb.AppendLine($$"""
                /// <summary>
                /// Converts a <see cref="global::{{jsonConverterToGenerate.FullyQualifiedName}}" /> to or from JSON.
                /// </summary>
                {{(jsonConverterToGenerate.IsPublic ? "public" : "internal")}} sealed class {{jsonConverterToGenerate.ConverterType}} : global::System.Text.Json.Serialization.JsonConverter<global::{{jsonConverterToGenerate.FullyQualifiedName}}>
                {
            """);

        var propertyName = jsonConverterToGenerate.PropertyName;
        if (!string.IsNullOrEmpty(propertyName) && jsonConverterToGenerate.CamelCase)
            propertyName = propertyName.ToCamelCase();

        if (!string.IsNullOrEmpty(propertyName))
            sb.AppendLine($"""        private const string PropertyName = "{propertyName}";""")
                .AppendLine();

        var fullyQualifiedExtension = string.IsNullOrEmpty(jsonConverterToGenerate.ExtensionNamespace)
            ? jsonConverterToGenerate.ExtensionName
            : $"{jsonConverterToGenerate.ExtensionNamespace}.{jsonConverterToGenerate.ExtensionName}";

        sb.AppendLine($$"""
                    /// <inheritdoc />
                    /// <summary>
                    /// Read and convert the JSON to <see cref="global::{{jsonConverterToGenerate.FullyQualifiedName}}" />.
                    /// </summary>
                    /// <remarks>
                    /// A converter may throw any Exception, but should throw <see cref="global::System.Text.Json.JsonException" /> when the JSON is invalid.
                    /// </remarks>
                    public override global::{{jsonConverterToGenerate.FullyQualifiedName}} Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
                    {
            #if NETCOREAPP && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0
                        char[]? rentedBuffer = null;
                        var bufferLength = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;

                        var charBuffer = bufferLength <= 128
                            ? stackalloc char[128]
                            : rentedBuffer = global::System.Buffers.ArrayPool<char>.Shared.Rent(bufferLength);

                        var charsWritten = reader.CopyString(charBuffer);
                        global::System.ReadOnlySpan<char> source = charBuffer[..charsWritten];
                        try
                        {
                            if (global::{{fullyQualifiedExtension}}.TryParse(source, out var enumValue, {{(jsonConverterToGenerate.CaseSensitive ? "false" : "true")}}, {{(jsonConverterToGenerate.AllowMatchingMetadataAttribute ? "true))" : "false))")}}
                                return enumValue;

                            throw new global::System.Text.Json.JsonException($"{source.ToString()} is not a valid value.", {{(string.IsNullOrEmpty(propertyName) ? "null" : "PropertyName")}}, null, null);
                        }
                        finally
                        {
                            if (rentedBuffer is not null)
                            {
                                charBuffer[..charsWritten].Clear();
                                global::System.Buffers.ArrayPool<char>.Shared.Return(rentedBuffer);
                            }
                        }
            #else
                        var source = reader.GetString();
                        if (global::{{fullyQualifiedExtension}}.TryParse(source, out var enumValue, {{(jsonConverterToGenerate.CaseSensitive ? "false" : "true")}}, {{(jsonConverterToGenerate.AllowMatchingMetadataAttribute ? "true))" : "false))")}}
                            return enumValue;

                        throw new global::System.Text.Json.JsonException($"{source} is not a valid value.", {{(string.IsNullOrEmpty(propertyName) ? "null" : "PropertyName")}}, null, null);
            #endif
                    }
            """)
            .AppendLine()
            .AppendLine($$"""
                    /// <inheritdoc />
                    public override void Write(global::System.Text.Json.Utf8JsonWriter writer, global::{{jsonConverterToGenerate.FullyQualifiedName}} value, global::System.Text.Json.JsonSerializerOptions options)
                        => writer.WriteStringValue(global::{{fullyQualifiedExtension}}.ToStringFast(value));
            """)
            .AppendLine("    }");

        if (!string.IsNullOrEmpty(jsonConverterToGenerate.ConverterNamespace))
            sb.AppendLine("}");

        return sb.ToString();
    }
}
