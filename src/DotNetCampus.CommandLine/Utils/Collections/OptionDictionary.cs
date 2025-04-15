using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DotNetCampus.Cli.Utils.Collections;

/// <summary>
/// 为命令行选项特别优化的字典。优化了无值/单值的内存占用和拷贝，优化了多种不同的选项命名风格，优化了大小写敏感性。
/// </summary>
internal class OptionDictionary(CommandLineStyle namingStyle, bool caseSensitive) : IReadOnlyDictionary<string, IReadOnlyList<string>>
{
    public static OptionDictionary Empty { get; } = new OptionDictionary(CommandLineStyle.Flexible, false);

    private readonly List<KeyValuePair<string, SingleOptimizedList<string>>> _optionValues = [];

    private OptionDictionary(CommandLineStyle namingStyle, bool caseSensitive, List<KeyValuePair<string, SingleOptimizedList<string>>> optionValues)
        : this(namingStyle, caseSensitive)
    {
        _optionValues = optionValues;
    }

    public int Count => _optionValues.Count;

    public IReadOnlyList<string> this[string key]
    {
        get
        {
            foreach (var pair in _optionValues)
            {
                if (Match(pair.Key, key))
                {
                    return pair.Value;
                }
            }

            throw new KeyNotFoundException($"Option '{key}' not found.");
        }
    }

    public IEnumerable<string> Keys
    {
        get
        {
            foreach (var pair in _optionValues)
            {
                yield return pair.Key;
            }
        }
    }

    public IEnumerable<IReadOnlyList<string>> Values
    {
        get
        {
            foreach (var pair in _optionValues)
            {
                yield return pair.Value;
            }
        }
    }

    public void AddOption(char optionName) => AddOption(optionName.ToString());

    public void AddOption(string optionName)
    {
        var index = _optionValues.FindIndex(p => Match(p.Key, optionName));
        if (index < 0)
        {
            _optionValues.Add(new KeyValuePair<string, SingleOptimizedList<string>>(optionName, []));
        }
    }

    public void AddValue(char optionName, string value) => AddValue(optionName.ToString(), value);

    public void AddValue(string optionName, string value)
    {
        var index = _optionValues.FindIndex(p => Match(p.Key, optionName));
        if (index >= 0)
        {
            _optionValues[index] = new KeyValuePair<string, SingleOptimizedList<string>>(optionName, _optionValues[index].Value.Add(value));
        }
        else
        {
            _optionValues.Add(new KeyValuePair<string, SingleOptimizedList<string>>(optionName, new SingleOptimizedList<string>(value)));
        }
    }

    public void UpdateValue(char optionName, string value) => UpdateValue(optionName.ToString(), value);

    public void UpdateValue(string optionName, string value)
    {
        var index = _optionValues.FindIndex(p => Match(p.Key, optionName));
        if (index >= 0)
        {
            _optionValues[index] = new KeyValuePair<string, SingleOptimizedList<string>>(optionName, new SingleOptimizedList<string>(value));
        }
        else
        {
            _optionValues.Add(new KeyValuePair<string, SingleOptimizedList<string>>(optionName, new SingleOptimizedList<string>(value)));
        }
    }

    public bool ContainsKey(char optionName)
    {
        foreach (var pair in _optionValues)
        {
            if (Match(pair.Key, optionName))
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsKey(string optionName)
    {
        foreach (var pair in _optionValues)
        {
            if (Match(pair.Key, optionName))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryGetValue(char shortOptionName, [MaybeNullWhen(false)] out IReadOnlyList<string> value)
    {
        foreach (var pair in _optionValues)
        {
            if (Match(pair.Key, shortOptionName))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    public bool TryGetValue(string optionName, [MaybeNullWhen(false)] out IReadOnlyList<string> value)
    {
        foreach (var pair in _optionValues)
        {
            if (Match(pair.Key, optionName))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    private bool Match(string existedOptionName, char comparingOptionName)
    {
        return MatchSingleLetter(existedOptionName, comparingOptionName);
    }

    private bool Match(string existedOptionName, string comparingOptionName) => namingStyle switch
    {
        CommandLineStyle.DotNet or CommandLineStyle.GNU => MatchKebabCase(existedOptionName, comparingOptionName),
        CommandLineStyle.PowerShell => MatchPascalCase(existedOptionName, comparingOptionName),
        CommandLineStyle.POSIX => MatchSingleLetter(existedOptionName, comparingOptionName),
        _ => MatchFlexible(existedOptionName, comparingOptionName),
    };

    private bool MatchSingleLetter(string existedOptionName, char comparingOptionName)
    {
        if (existedOptionName.Length is not 1)
        {
            return false;
        }
        return caseSensitive
            ? existedOptionName[0] == comparingOptionName
            : char.ToLowerInvariant(existedOptionName[0]) == char.ToLowerInvariant(comparingOptionName);
    }

    private bool MatchSingleLetter(string existedOptionName, string comparingOptionName)
    {
        if (existedOptionName.Length is not 1 || comparingOptionName.Length is not 1)
        {
            return false;
        }
        return caseSensitive
            ? existedOptionName[0] == comparingOptionName[0]
            : char.ToLowerInvariant(existedOptionName[0]) == char.ToLowerInvariant(comparingOptionName[0]);
    }

    private bool MatchPascalCase(string existedOptionName, string comparingOptionName)
    {
        if (existedOptionName.Length <= 0 || comparingOptionName.Length <= 0)
        {
            return false;
        }

        // existedOptionName: --OptionName, -OptionName, /OptionName
        // comparingOptionName: --OptionName, -OptionName, /OptionName, option-name, OptionName, optionname

        int existedIndex = 0, comparingIndex = 0;
        while (true)
        {
            if (existedIndex >= existedOptionName.Length && comparingIndex >= comparingOptionName.Length)
            {
                // 二者同时到达末尾，说明匹配成功。
                return true;
            }
            if (existedIndex < existedOptionName.Length && comparingIndex < comparingOptionName.Length)
            {
                var ec = existedOptionName[existedIndex];
                var cc = comparingOptionName[comparingIndex];
                if (!char.IsLetterOrDigit(ec))
                {
                    existedIndex++;
                    continue;
                }
                if (!char.IsLetterOrDigit(cc))
                {
                    comparingIndex++;
                    continue;
                }

                existedIndex++;
                comparingIndex++;

                // 二者都没有到达末尾，继续匹配。
                if (caseSensitive)
                {
                    if (ec != cc)
                    {
                        return false;
                    }
                }
                else
                {
                    if (char.ToLowerInvariant(ec) != char.ToLowerInvariant(cc))
                    {
                        return false;
                    }
                }
            }
            else
            {
                // 其中一个到达末尾，另一个没有到达末尾，说明匹配失败。
                return false;
            }
        }
    }

    private bool MatchKebabCase(string existedOptionName, string comparingOptionName)
    {
        if (existedOptionName.Length <= 0 || comparingOptionName.Length <= 0)
        {
            return false;
        }

        // existedOptionName: --Option-Name, -O
        // comparingOptionName: --Option-Name, -O, --option-name, -o, option-name, o

        int existedIndex = 0, comparingIndex = 0;
        bool isExistedWordStart = true, isComparingWordStart = true;
        while (true)
        {
            if (isExistedWordStart && isComparingWordStart || !isExistedWordStart && !isComparingWordStart)
            {
                // 二者都是单词的开始，或二者都是单词的中间，继续匹配。

                if (existedIndex >= existedOptionName.Length && comparingIndex >= comparingOptionName.Length)
                {
                    // 二者同时到达末尾，说明匹配成功。
                    return true;
                }
                if (existedIndex < existedOptionName.Length && comparingIndex < comparingOptionName.Length)
                {
                    var ec = existedOptionName[existedIndex];
                    var cc = comparingOptionName[comparingIndex];
                    if (char.IsUpper(ec))
                    {
                        isExistedWordStart = true;
                    }
                    if (char.IsUpper(cc))
                    {
                        isComparingWordStart = true;
                    }
                    if (!char.IsLetterOrDigit(ec))
                    {
                        existedIndex++;
                        isExistedWordStart = true;
                        continue;
                    }
                    if (!char.IsLetterOrDigit(cc))
                    {
                        comparingIndex++;
                        isComparingWordStart = true;
                        continue;
                    }

                    existedIndex++;
                    comparingIndex++;

                    // 二者都没有到达末尾，继续匹配。
                    if (caseSensitive)
                    {
                        if (ec != cc)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (char.ToLowerInvariant(ec) != char.ToLowerInvariant(cc))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // 其中一个到达末尾，另一个没有到达末尾，说明匹配失败。
                    return false;
                }
            }
            else
            {
                // 只有一个是单词的开始，另一个是单词的中间，说明匹配失败。
                return false;
            }
        }
    }

    private bool MatchFlexible(string existedOptionName, string comparingOptionName)
    {
        return MatchPascalCase(existedOptionName, comparingOptionName);
    }

    /// <summary>
    /// 保留当前字典的所有内容，但返回一个新字典，使用指定的大小写敏感性来查询选项的值。
    /// </summary>
    /// <param name="newCaseSensitive">新的大小写敏感性。</param>
    /// <returns>原有字典内容但新的查询方式的字典。</returns>
    public OptionDictionary ToOptionLookup(bool newCaseSensitive)
    {
        if (newCaseSensitive == caseSensitive)
        {
            return this;
        }

        return new OptionDictionary(namingStyle, newCaseSensitive, _optionValues);
    }

    public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
    {
        foreach (var pair in _optionValues)
        {
            yield return new KeyValuePair<string, IReadOnlyList<string>>(pair.Key, pair.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
