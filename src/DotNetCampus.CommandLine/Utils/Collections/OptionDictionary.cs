using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DotNetCampus.Cli.Utils.Collections;

/// <summary>
/// 为命令行选项特别优化的字典。优化了无值/单值的内存占用和拷贝，优化了多种不同的选项命名风格，优化了大小写敏感性。
/// </summary>
internal class OptionDictionary(bool caseSensitive) : IReadOnlyDictionary<OptionName, IReadOnlyList<string>>
{
    public static OptionDictionary Empty { get; } = new OptionDictionary(true);

    private readonly List<KeyValuePair<OptionName, SingleOptimizedList<string>>> _optionValues = [];

    private OptionDictionary(bool caseSensitive, List<KeyValuePair<OptionName, SingleOptimizedList<string>>> optionValues) : this(caseSensitive)
    {
        _optionValues = optionValues;
    }

    public int Count => _optionValues.Count;

    public IReadOnlyList<string> this[OptionName key]
    {
        get
        {
            for (var i = 0; i < _optionValues.Count; i++)
            {
                var pair = _optionValues[i];
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
            for (var i = 0; i < _optionValues.Count; i++)
            {
                var pair = _optionValues[i];
                yield return pair.Key;
            }
        }
    }

    public IEnumerable<IReadOnlyList<string>> Values
    {
        get
        {
            for (var i = 0; i < _optionValues.Count; i++)
            {
                var pair = _optionValues[i];
                yield return pair.Value;
            }
        }
    }

    public bool ContainsKey(OptionName key)
    {
        for (var i = 0; i < _optionValues.Count; i++)
        {
            var pair = _optionValues[i];
            if (pair.Key.Equals(key, caseSensitive))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryGetValue(OptionName key, [MaybeNullWhen(false)] out IReadOnlyList<string> value)
    {
        for (var i = 0; i < _optionValues.Count; i++)
        {
            var pair = _optionValues[i];
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

    public void AddValues(OptionName optionName, IReadOnlyList<string> values)
    {
        var index = _optionValues.FindIndex(p => p.Key.Equals(optionName, caseSensitive));
        if (index >= 0)
        {
            _optionValues[index] = new KeyValuePair<OptionName, SingleOptimizedList<string>>(optionName, _optionValues[index].Value.AddRange(values));
        }
        else
        {
            _optionValues.Add(new KeyValuePair<OptionName, SingleOptimizedList<string>>(optionName, new SingleOptimizedList<string>().AddRange(values)));
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
        for (var i = 0; i < _optionValues.Count; i++)
        {
            var pair = _optionValues[i];
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
        var needSeparator = false;
        var actualBuilderCount = 0;
        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];
            if (char.IsUpper(c))
            {
                // 大写字母。
                if (needSeparator)
                {
                    // 需要使用分隔符。
                    builder[actualBuilderCount++] = '-';
                }
                builder[actualBuilderCount++] = char.ToLowerInvariant(c);
            }
            else if (char.IsLetterOrDigit(c))
            {
                // 无大小写，但可作为标识符的字符（对 char 来说也视为字母）。
                builder[actualBuilderCount++] = c;
                needSeparator = i + 1 < span.Length && char.IsUpper(span[i + 1]);
            }
            else
            {
                // 其他字符，直接添加。
                builder[actualBuilderCount++] = c;
            }
        }
        if (actualBuilderCount == 0)
        {
            return "";
        }
        if (actualBuilderCount == builder.Length)
        {
            return builder.ToString();
        }
        return new string(builder[..actualBuilderCount]);
    }
}
