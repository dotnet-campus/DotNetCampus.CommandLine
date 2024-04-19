using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetCampus.Cli.Utils
{
    internal static class NamingHelper
    {
        /// <summary>
        /// Check if the specified <paramref name="value"/> is a PascalCase string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool CheckIsPascalCase(string value)
        {
            var first = value[0];
            if (char.IsLower(first))
            {
                return false;
            }

            var testName = MakePascalCase(value);
            return string.Equals(value, testName, StringComparison.Ordinal);
        }

        internal static string MakePascalCase(string oldName)
        {
            var builder = new StringBuilder();

            var isFirstLetter = true;
            var isWordStart = true;
            foreach (char c in oldName)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    // Append nothing because PascalCase has no special characters.
                    isWordStart = true;
                    continue;
                }

                if (isFirstLetter)
                {
                    if (char.IsDigit(c))
                    {
                        // PascalCase does not support digital as the first letter.
                        isWordStart = true;
                        continue;
                    }
                    else if (char.IsLower(c))
                    {
                        // 小写字母。
                        isFirstLetter = false;
                        isWordStart = false;
                        builder.Append(char.ToUpperInvariant(c));
                    }
                    else if (char.IsUpper(c))
                    {
                        // 大写字母。
                        isFirstLetter = false;
                        isWordStart = false;
                        builder.Append(c);
                    }
                    else
                    {
                        // 无大小写，但可作为标识符的字符（对 char 来说也视为字母）。
                        isFirstLetter = false;
                        isWordStart = true;
                        builder.Append(c);
                    }
                }
                else
                {
                    if (char.IsDigit(c))
                    {
                        // PascalCase does not support digital as the first letter.
                        isWordStart = true;
                        builder.Append(c);
                    }
                    else if (char.IsLower(c))
                    {
                        // 小写字母。
                        builder.Append(isWordStart
                            ? char.ToUpperInvariant(c)
                            : c);
                        isWordStart = false;
                    }
                    else if (char.IsUpper(c))
                    {
                        // 大写字母。
                        isWordStart = false;
                        builder.Append(c);
                    }
                    else
                    {
                        // 无大小写，但可作为标识符的字符（对 char 来说也视为字母）。
                        isWordStart = true;
                        builder.Append(c);
                    }
                }
            }

            return builder.ToString();
        }

        internal static string MakeKebabCase(string oldName)
        {
            var builder = new StringBuilder();

            var isFirstLetter = true;
            var isUpperOrLower = false;
            foreach (char c in oldName)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    // Append nothing because kebab-case has no special characters.
                    isUpperOrLower = false;
                    continue;
                }

                if (isFirstLetter)
                {
                    if (char.IsDigit(c))
                    {
                        // kebab-case does not support digital as the first letter.
                        isUpperOrLower = false;
                        continue;
                    }
                    else if (char.IsUpper(c))
                    {
                        // 大写字母。
                        isFirstLetter = false;
                        isUpperOrLower = true;
                        builder.Append(char.ToLowerInvariant(c));
                    }
                    else if (char.IsLower(c))
                    {
                        // 小写字母。
                        isFirstLetter = false;
                        isUpperOrLower = true;
                        builder.Append(c);
                    }
                    else
                    {
                        isFirstLetter = false;
                        isUpperOrLower = false;
                        builder.Append(c);
                    }
                }
                else
                {
                    if (char.IsDigit(c))
                    {
                        // kebab-case does not support digital as the first letter.
                        isUpperOrLower = false;
                        builder.Append(c);
                    }
                    else if (char.IsUpper(c))
                    {
                        builder.Append('-');
                        builder.Append(char.ToLowerInvariant(c));
                        isUpperOrLower = true;
                    }
                    else if (char.IsLower(c))
                    {
                        builder.Append(c);
                        isUpperOrLower = true;
                    }
                    else
                    {
                        if (isUpperOrLower)
                        {
                            builder.Append('-');
                        }
                        builder.Append(c);
                        isUpperOrLower = false;
                    }
                }
            }

            return builder.ToString();
        }
    }
}
