using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NetEscapades.EnumGenerators.Benchmarks;

internal static class EnumHelper<T> where T : struct
{
    internal static bool TryParseByDisplayName(string name, bool ignoreCase, out T enumValue)
    {
        enumValue = default;

        var stringComparisonOption = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var enumValues = (T[])Enum.GetValues(typeof(T));
        foreach (var value in enumValues)
        {
            if (TryGetDisplayName(value.ToString(), out var displayName) && displayName.Equals(name, stringComparisonOption))
            {
                enumValue = value;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetDisplayName(
        string? value,
#if NETCOREAPP3_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? displayName)
#else
        out string? displayName)
#endif
    {
        displayName = default;

        if (typeof(T).IsEnum)
        {
            // Prevent: Warning CS8604  Possible null reference argument for parameter 'name' in 'MemberInfo[] Type.GetMember(string name)'
            if (value is not null)
            {
                var memberInfo = typeof(T).GetMember(value);
                if (memberInfo.Length > 0)
                {
                    displayName = memberInfo[0].GetCustomAttribute<DisplayAttribute>()?.GetName();
                    if (displayName is null)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    internal static string GetDisplayName(T value)
    {
        if (typeof(T).IsEnum)
        {
            var memberInfo = typeof(T).GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                var displayName = memberInfo[0].GetCustomAttribute<DisplayAttribute>().GetName();
                if (displayName is null)
                {
                    return string.Empty;
                }

                return displayName;
            }
        }

        return string.Empty;
    }
}
