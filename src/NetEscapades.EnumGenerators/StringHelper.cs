namespace NetEscapades.EnumGenerators
{
    public static class StringHelper
    {
        public static string? ToCamelCase(this string? value)
        {
            if (string.IsNullOrEmpty(value) || !char.IsUpper(value![0]))
            {
                return value;
            }

            char[] chars = value.ToCharArray();
            FixCasing(chars);
            return new string(chars);
        }

        private static void FixCasing(Span<char> chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);

                // Stop when next char is already lowercase.
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // If the next char is a space, lowercase current char before exiting.
                    if (chars[i + 1] == ' ')
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }
        }
    }
}