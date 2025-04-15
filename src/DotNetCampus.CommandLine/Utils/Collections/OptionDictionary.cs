using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DotNetCampus.Cli.Utils.Collections;

/// <summary>
/// 为命令行选项特别优化的字典。优化了无值/单值的内存占用和拷贝，优化了多种不同的选项命名风格，优化了大小写敏感性。
/// </summary>
internal class OptionDictionary(bool caseSensitive) : IReadOnlyDictionary<OptionName, IReadOnlyList<string>>
{
    public static OptionDictionary Empty { get; } = new OptionDictionary(CommandLineStyle.Flexible, true);

    private readonly List<KeyValuePair<OptionName, SingleOptimizedList<string>>> _optionValues = [];

    public OptionDictionary(CommandLineStyle style, bool caseSensitive) : this(caseSensitive)
    {
    }

    private OptionDictionary(bool caseSensitive, List<KeyValuePair<OptionName, SingleOptimizedList<string>>> optionValues) : this(caseSensitive)
    {
        _optionValues = optionValues;
    }

    public int Count => _optionValues.Count;

    public IReadOnlyList<string> this[OptionName key]
    {
        get
        {
            foreach (var pair in _optionValues)
            {
                if (pair.Key.Equals(key, caseSensitive))
                {
                    return pair.Value;
                }
            }

            throw new KeyNotFoundException($"Option '{key}' not found.");
        }
    }

    public IEnumerable<OptionName> Keys
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

    public bool ContainsKey(OptionName key)
    {
        foreach (var pair in _optionValues)
        {
            if (pair.Key.Equals(key, caseSensitive))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryGetValue(OptionName key, [MaybeNullWhen(false)] out IReadOnlyList<string> value)
    {
        foreach (var pair in _optionValues)
        {
            if (pair.Key.Equals(key, caseSensitive))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    public void AddOption(OptionName optionName)
    {
        var index = _optionValues.FindIndex(p => p.Key.Equals(optionName, caseSensitive));
        if (index < 0)
        {
            _optionValues.Add(new KeyValuePair<OptionName, SingleOptimizedList<string>>(optionName, []));
        }
    }

    public void AddValue(OptionName optionName, string value)
    {
        var index = _optionValues.FindIndex(p => p.Key.Equals(optionName, caseSensitive));
        if (index >= 0)
        {
            _optionValues[index] = new KeyValuePair<OptionName, SingleOptimizedList<string>>(optionName, _optionValues[index].Value.Add(value));
        }
        else
        {
            _optionValues.Add(new KeyValuePair<OptionName, SingleOptimizedList<string>>(optionName, new SingleOptimizedList<string>(value)));
        }
    }

    public void UpdateValue(OptionName optionName, string value)
    {
        var index = _optionValues.FindIndex(p => p.Key.Equals(optionName, caseSensitive));
        if (index >= 0)
        {
            _optionValues[index] = new KeyValuePair<OptionName, SingleOptimizedList<string>>(optionName, new SingleOptimizedList<string>(value));
        }
        else
        {
            _optionValues.Add(new KeyValuePair<OptionName, SingleOptimizedList<string>>(optionName, new SingleOptimizedList<string>(value)));
        }
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

        return new OptionDictionary(newCaseSensitive, _optionValues);
    }

    public IEnumerator<KeyValuePair<OptionName, IReadOnlyList<string>>> GetEnumerator()
    {
        foreach (var pair in _optionValues)
        {
            yield return new KeyValuePair<OptionName, IReadOnlyList<string>>(pair.Key, pair.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal readonly record struct OptionName(string Argument, Range Range) : IEnumerable<char>
{
    public char this[int index]
    {
        get
        {
            var (offset, length) = Range.GetOffsetAndLength(Argument.Length);
            var realIndex = index + offset;
            if (realIndex < offset || realIndex >= offset + length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range.");
            }
            return Argument[realIndex];
        }
    }

#if NET8_0_OR_GREATER
    public ReadOnlySpan<char> AsSpan() => Argument.AsSpan(Range);
#else
    public ReadOnlySpan<char> AsSpan()
    {
        var (offset, length) = Range.GetOffsetAndLength(Argument.Length);
        return Argument.AsSpan(offset, length);
    }
#endif

    public bool Equals(OptionName? other)
    {
        if (other is null)
        {
            return false;
        }

        var (thisOffset, thisLength) = Range.GetOffsetAndLength(Argument.Length);
        var (thatOffset, thatLength) = other.Value.Range.GetOffsetAndLength(other.Value.Argument.Length);
        if (thisLength != thatLength)
        {
            return false;
        }

        for (var i = 0; i < thisLength; i++)
        {
            if (Argument[thisOffset + i] != other.Value.Argument[thatOffset + i])
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(OptionName other, bool caseSensitive)
    {
        var (thisOffset, thisLength) = Range.GetOffsetAndLength(Argument.Length);
        var (thatOffset, thatLength) = other.Range.GetOffsetAndLength(other.Argument.Length);
        if (thisLength != thatLength)
        {
            return false;
        }

        for (var i = 0; i < thisLength; i++)
        {
            var thisChar = Argument[thisOffset + i];
            var thatChar = other.Argument[thatOffset + i];
            if (thisChar != thatChar)
            {
                if (caseSensitive)
                {
                    return false;
                }
                if (char.ToLowerInvariant(thisChar) != char.ToLowerInvariant(thatChar))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public IEnumerator<char> GetEnumerator()
    {
        var (offset, length) = Range.GetOffsetAndLength(Argument.Length);
        for (var i = offset; i < offset + length; i++)
        {
            yield return Argument[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => AsSpan().ToString();

    public static implicit operator OptionName(string optionName) => new OptionName(optionName, Range.All);

    public static implicit operator OptionName(char optionName) => new OptionName(optionName.ToString(), Range.All);

    public static string MakeKebabCase(ReadOnlySpan<char> span)
    {
        Span<char> builder = stackalloc char[span.Length * 2];
        var isWordStart = true;
        var actualBuilderCount = 0;
        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];
            if (char.IsUpper(c))
            {
                // 大写字母。
                if (!isWordStart)
                {
                    // 单词的中间，添加分隔符。
                    builder[actualBuilderCount++] = '-';
                }
                builder[actualBuilderCount++] = char.ToLowerInvariant(c);
                isWordStart = false;
            }
            else if (char.IsLetterOrDigit(c))
            {
                // 无大小写，但可作为标识符的字符（对 char 来说也视为字母）。
                builder[actualBuilderCount++] = c;
                isWordStart = false;
            }
            else
            {
                // 其他字符，直接添加。
                builder[actualBuilderCount++] = c;
            }
        }
        if (actualBuilderCount == 0)
        {
            return string.Empty;
        }
        if (actualBuilderCount == builder.Length)
        {
            return builder.ToString();
        }
        return new string(builder[..actualBuilderCount]);
    }
}

/// <summary>
/// 为命令行选项特别优化的字典。优化了无值/单值的内存占用和拷贝，优化了多种不同的选项命名风格，优化了大小写敏感性。
/// </summary>
internal class OptionDictionary1(CommandLineStyle namingStyle, bool caseSensitive) : IReadOnlyDictionary<string, IReadOnlyList<string>>
{
    public static OptionDictionary1 Empty { get; } = new OptionDictionary1(CommandLineStyle.Flexible, false);

    private readonly List<KeyValuePair<string, SingleOptimizedList<string>>> _optionValues = [];

    private OptionDictionary1(CommandLineStyle namingStyle, bool caseSensitive, List<KeyValuePair<string, SingleOptimizedList<string>>> optionValues)
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
        CommandLineStyle.DotNet or CommandLineStyle.Gnu => MatchKebabCase(existedOptionName, comparingOptionName),
        CommandLineStyle.PowerShell => MatchPascalCase(existedOptionName, comparingOptionName),
        CommandLineStyle.Posix => MatchSingleLetter(existedOptionName, comparingOptionName),
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
    public OptionDictionary1 ToOptionLookup(bool newCaseSensitive)
    {
        if (newCaseSensitive == caseSensitive)
        {
            return this;
        }

        return new OptionDictionary1(namingStyle, newCaseSensitive, _optionValues);
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
