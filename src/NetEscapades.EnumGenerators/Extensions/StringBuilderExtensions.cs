using System.Text;

namespace NetEscapades.EnumGenerators.Extensions;

internal static class StringBuilderExtensions
{
    internal static StringBuilder AppendNamespaceOpening(this StringBuilder stringBuilder, in string nameSpace)
    {
        if (!string.IsNullOrEmpty(nameSpace))
        {
            stringBuilder.Append($@"
namespace {nameSpace}
{{");
        }

        return stringBuilder;
    }

    internal static StringBuilder AppendNamespaceEnding(this StringBuilder stringBuilder, in string nameSpace)
    {
        if (!string.IsNullOrEmpty(nameSpace))
        {
            stringBuilder.Append(@"
}");
        }

        return stringBuilder;
    }

    internal static StringBuilder AppendClassOpening(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append(@"
    ").Append(enumToGenerate.IsPublic ? "public" : "internal").Append(@" static partial class ").Append(enumToGenerate.Name).Append(@"
    {");

        return stringBuilder;
    }

    internal static StringBuilder AppendClassEnding(this StringBuilder stringBuilder)
    {
        return stringBuilder.Append(@"
    }");
    }

    internal static StringBuilder AppendToStringFastMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append(@"
        public static string ToStringFast(this ").Append(enumToGenerate.FullyQualifiedName).Append(@" value)
            => value switch
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append(@"
                ").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key)
                .Append(" => nameof(")
                .Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append("),");
        }

        stringBuilder.Append(@"
                _ => value.ToString(),
            };");

        return stringBuilder;
    }

    internal static StringBuilder AppendHasFlagsMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        if (enumToGenerate.HasFlags)
        {
            stringBuilder
                .AppendLine()
                .Append(@"
        public static bool HasFlag(this ").Append(enumToGenerate.FullyQualifiedName).Append(@" value, ").Append(enumToGenerate.FullyQualifiedName).Append(@" flag)
            => value switch
            {
                0  => flag.Equals(0),
                _ => (value & flag) != 0,
            };");
        }

        return stringBuilder;
    }

    internal static StringBuilder AppendIsDefinedMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append(@"
       public static bool IsDefined(").Append(enumToGenerate.FullyQualifiedName).Append(@" value)
            => value switch
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append(@"
                ").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key)
                .Append(" => true,");
        }
        stringBuilder.Append(@"
                _ => false,
            };

        public static bool IsDefined(string name)
            => name switch
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append(@"
                nameof(").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append(@") => true,");
        }

        stringBuilder.Append(@"
                _ => false,
            };");

        return stringBuilder;
    }

    internal static StringBuilder AppendTryParseMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append(@"
        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            bool ignoreCase, 
            out ").Append(enumToGenerate.FullyQualifiedName).Append(@" value)
            => ignoreCase ? TryParseIgnoreCase(name, out value) : TryParse(name, out value);

        private static bool TryParseIgnoreCase(
#if NETCOREAPP3_0_OR_GREATER
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            out ").Append(enumToGenerate.FullyQualifiedName).Append(@" value)
        {
            switch (name)
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append(@"
                case { } s when s.Equals(nameof(").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append(@"), System.StringComparison.OrdinalIgnoreCase):
                    value = ").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append(@";
                    return true;");
        }

        stringBuilder.Append(@"
                case { } s when ").Append(enumToGenerate.UnderlyingType).Append(@".TryParse(name, out var val):
                    value = (").Append(enumToGenerate.FullyQualifiedName).Append(@")val;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }

        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            out ").Append(enumToGenerate.FullyQualifiedName).Append(@" value)
        {
            switch (name)
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append(@"
                case nameof(").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append(@"):
                    value = ").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append(@";
                    return true;");
        }

        stringBuilder.Append(@"
                case { } s when ").Append(enumToGenerate.UnderlyingType).Append(@".TryParse(name, out var val):
                    value = (").Append(enumToGenerate.FullyQualifiedName).Append(@")val;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }");

        return stringBuilder;
    }

    internal static StringBuilder AppendGetValuesMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append(@"
        public static ").Append(enumToGenerate.FullyQualifiedName).Append(@"[] GetValues()
        {
            return new[]
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append(@"
                ").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append(',');
        }
        stringBuilder.Append(@"
            };
        }");

        return stringBuilder;
    }

    internal static StringBuilder AppendGetNamesMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append(@"
        public static string[] GetNames()
        {
            return new[]
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append(@"
                nameof(").Append(enumToGenerate.FullyQualifiedName).Append('.').Append(member.Key).Append("),");
        }
        stringBuilder.Append(@"
            };
        }");

        return stringBuilder;
    }
}
