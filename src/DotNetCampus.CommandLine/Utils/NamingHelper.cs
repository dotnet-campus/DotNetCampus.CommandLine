using System.Text;

namespace DotNetCampus.Cli.Utils;

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

    /// <summary>
    /// Check if the specified <paramref name="value"/> is a kebab-case string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool CheckIsKebabCase(string value)
    {
        var testName = MakeKebabCase(value, true, true);
        return string.Equals(value, testName, StringComparison.Ordinal);
    }

    internal static string MakePascalCase(string oldName)
    {
        var builder = new StringBuilder();

        var isFirstLetter = true;
        var isWordStart = true;
        for (var i = 0; i < oldName.Length; i++)
        {
            var c = oldName[i];
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

    /// <summary>
    /// 从其他命名法转换为 kebab-case 命名法。
    /// </summary>
    /// <param name="oldName">其他命名法的名称。</param>
    /// <param name="isUpperSeparator">大写字母是否是单词分隔符。例如 SampleName_Text -> Sample-Name-Text | SampleName-Text。</param>
    /// <param name="toLower">是否将所有字母转换为小写形式。例如 Sample-Name -> sample-name | Sample-Name。</param>
    /// <returns>kebab-case 命名法的字符串。</returns>
    internal static string MakeKebabCase(string oldName, bool isUpperSeparator = true, bool toLower = true)
    {
        var builder = new StringBuilder();

        var isFirstLetter = true;
        var isUpperLetter = false;
        var isSeparator = false;
        for (var i = 0; i < oldName.Length; i++)
        {
            var c = oldName[i];
            if (!char.IsLetterOrDigit(c))
            {
                isUpperLetter = false;
                // Append nothing because kebab-case has no continuous special characters.
                if (!isFirstLetter)
                {
                    isSeparator = true;
                }
                continue;
            }

            if (isFirstLetter)
            {
                if (char.IsDigit(c))
                {
                    // kebab-case does not support digital as the first letter.
                    isSeparator = false;
                }
                else if (char.IsUpper(c))
                {
                    // 大写字母。
                    isFirstLetter = false;
                    isUpperLetter = true;
                    isSeparator = false;
                    builder.Append(toLower ? char.ToLowerInvariant(c) : c);
                }
                else if (char.IsLower(c))
                {
                    // 小写字母。
                    isFirstLetter = false;
                    isUpperLetter = false;
                    isSeparator = false;
                    builder.Append(c);
                }
                else
                {
                    isFirstLetter = false;
                    isUpperLetter = false;
                    builder.Append(c);
                }
            }
            else
            {
                if (char.IsDigit(c))
                {
                    isUpperLetter = false;
                    isSeparator = false;
                    builder.Append(c);
                }
                else if (char.IsUpper(c))
                {
                    if (!isUpperLetter && (isUpperSeparator || isSeparator))
                    {
                        builder.Append('-');
                    }
                    isUpperLetter = true;
                    isSeparator = false;
                    builder.Append(toLower ? char.ToLowerInvariant(c) : c);
                }
                else if (char.IsLower(c))
                {
                    if (isSeparator)
                    {
                        builder.Append('-');
                    }
                    isUpperLetter = false;
                    isSeparator = false;
                    builder.Append(c);
                }
                else
                {
                    if (isSeparator)
                    {
                        builder.Append('-');
                    }
                    builder.Append(c);
                }
            }
        }

        return builder.ToString();
    }
}
