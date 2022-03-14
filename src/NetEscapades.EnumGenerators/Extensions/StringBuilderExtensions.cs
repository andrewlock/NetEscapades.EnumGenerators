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

    internal static StringBuilder AppendClassOpening(this StringBuilder stringBuilder, in string accesibility, bool isStatic, in string name)
    {
        string staticKeyword = isStatic ? "static " : string.Empty;

        stringBuilder.Append($@"
    {accesibility} {staticKeyword}partial class {name}
    {{");

        return stringBuilder;
    }

    internal static StringBuilder AppendClassEnding(this StringBuilder stringBuilder)
    {
        return stringBuilder.Append($@"
    }}");
    }

    internal static StringBuilder AppendToStringFastMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append($@"
    {{
        public static string ToStringFast(this {enumToGenerate.FullyQualifiedName} value)
            => value switch
            {{");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                {enumToGenerate.FullyQualifiedName}.{member.Key} => nameof({enumToGenerate.FullyQualifiedName}.{member.Key}),");
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
            stringBuilder.Append($@"

        public static bool HasFlag(this {enumToGenerate.FullyQualifiedName} value, {enumToGenerate.FullyQualifiedName} flag)
            => value switch
            {{
                0  => flag.Equals(0),
                _ => (value & flag) != 0,
            }};");
        }

        return stringBuilder;
    }

    internal static StringBuilder AppendIsDefinedMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append($@"

       public static bool IsDefined({enumToGenerate.FullyQualifiedName} value)
            => value switch
            {{");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                {enumToGenerate.FullyQualifiedName}.{member.Key} => true,");
        }
        stringBuilder.Append($@"
                _ => false,
            }};

        public static bool IsDefined(string name)
            => name switch
            {{");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                nameof({enumToGenerate.FullyQualifiedName}.{member.Key}) => true,");
        }

        stringBuilder.Append(@"
                _ => false,
            };");

        return stringBuilder;
    }

    internal static StringBuilder AppendTryParseMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append($@"

        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            bool ignoreCase, 
            out {enumToGenerate.FullyQualifiedName} value)
            => ignoreCase ? TryParseIgnoreCase(name, out value) : TryParse(name, out value);

        private static bool TryParseIgnoreCase(
#if NETCOREAPP3_0_OR_GREATER
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            out {enumToGenerate.FullyQualifiedName} value)
        {{
            switch (name)
            {{");

        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                case {{ }} s when s.Equals(nameof({enumToGenerate.FullyQualifiedName}.{member.Key}), System.StringComparison.OrdinalIgnoreCase):
                    value = {enumToGenerate.FullyQualifiedName}.{member.Key}
                    return true;");
        }

        stringBuilder.Append($@"
                case {{ }} s when {enumToGenerate.UnderlyingType}.TryParse(name, out var val):
                    value = ({enumToGenerate.FullyQualifiedName})val;
                    return true;
                default:
                    value = default;
                    return false;
            }}
        }}

        public static bool TryParse(
#if NETCOREAPP3_0_OR_GREATER
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
            string? name, 
            out {enumToGenerate.FullyQualifiedName} value)
        {{
            switch (name)
            {{");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                case nameof({enumToGenerate.FullyQualifiedName}).{member.Key}
                    value = {enumToGenerate.FullyQualifiedName}.{ member.Key}
                    return true;");
        }

        stringBuilder.Append($@"
                case {{ }} s when {enumToGenerate.UnderlyingType}.TryParse(name, out var val):
                    value = ({enumToGenerate.FullyQualifiedName})val;
                    return true;
                default:
                    value = default;
                    return false;
            }}
        }}

        public static {enumToGenerate.FullyQualifiedName}[] GetValues()
        {{
            return new[]
            {{");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                {enumToGenerate.FullyQualifiedName}.{member.Key},");
        }
        stringBuilder.Append(@"
            };
        }");

        return stringBuilder;
    }


    internal static StringBuilder AppendGetNamesMethod(this StringBuilder stringBuilder, in EnumToGenerate enumToGenerate)
    {
        stringBuilder.Append($@"

        public static {enumToGenerate.FullyQualifiedName}[] GetValues()
        {{
            return new[]
            {{");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                {enumToGenerate.FullyQualifiedName}.{member.Key},");
        }
        stringBuilder.Append(@"
            };
        }

        public static string[] GetNames()
        {
            return new[]
            {");
        foreach (var member in enumToGenerate.Values)
        {
            stringBuilder.Append($@"
                nameof({enumToGenerate.FullyQualifiedName}.{member.Key}),");
        }
        stringBuilder.Append(@"
            };
        }
    }");

        return stringBuilder;
    }
}
