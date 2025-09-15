#if NETCOREAPP3_1_OR_GREATER
using System.Collections.Immutable;
#endif
using System.Collections.ObjectModel;

namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 专门解析来自命令行的布尔类型，并辅助赋值给属性。
/// </summary>
public struct BooleanArgument
{
    /// <summary>
    /// 存储解析到的布尔值。
    /// </summary>
    private bool? _value;

    /// <summary>
    /// 当命令行直接或间接输入了一个布尔参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的布尔值。</param>
    public void Assign(bool value)
    {
        _value = value;
    }

    /// <summary>
    /// 将解析到的值转换为布尔值。
    /// </summary>
    public bool? ToBoolean()
    {
        return _value;
    }
}

/// <summary>
/// 专门解析来自命令行的数值类型，并辅助赋值给属性。
/// </summary>
public struct NumberArgument
{
    /// <summary>
    /// 指示在解析失败时是否忽略异常并保持未初始化的状态。
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// 存储解析到的数值。
    /// </summary>
    private decimal? _value;

    /// <summary>
    /// 当命令行输入了一个数值参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的数值字符串。</param>
    public void Assign(ReadOnlySpan<char> value)
    {
        if (decimal.TryParse(value
#if !NETCOREAPP3_1_OR_GREATER
                    .ToString()
#endif
                , out var doubleValue))
        {
            _value = doubleValue;
        }
        else if (!IgnoreExceptions)
        {
            throw new FormatException($"无法将 \"{value.ToString()}\" 转换为数值。");
        }
    }

    /// <summary>
    /// 将解析到的值转换为字节。
    /// </summary>
    public byte? ToByte() => (byte?)_value;

    /// <summary>
    /// 将解析到的值转换为有符号字节。
    /// </summary>
    public sbyte? ToSByte() => (sbyte?)_value;

    /// <summary>
    /// 将解析到的值转换为高精度浮点数。
    /// </summary>
    public decimal? ToDecimal() => _value;

    /// <summary>
    /// 将解析到的值转换为双精度浮点数。
    /// </summary>
    public double? ToDouble() => (double?)_value;

    /// <summary>
    /// 将解析到的值转换为单精度浮点数。
    /// </summary>
    public float? ToSingle() => (float?)_value;

    /// <summary>
    /// 将解析到的值转换为 32 位整数。
    /// </summary>
    public int? ToInt32() => (int?)_value;

    /// <summary>
    /// 将解析到的值转换为无符号 32 位整数。
    /// </summary>
    public uint? ToUInt32() => (uint?)_value;

    /// <summary>
    /// 将解析到的值转换为指针大小的整数。
    /// </summary>
    public nint? ToIntPtr() => (nint?)_value;

    /// <summary>
    /// 将解析到的值转换为无符号指针大小的整数。
    /// </summary>
    public nuint? ToUIntPtr() => (nuint?)_value;

    /// <summary>
    /// 将解析到的值转换为 64 位整数。
    /// </summary>
    public long? ToInt64() => (long?)_value;

    /// <summary>
    /// 将解析到的值转换为无符号 64 位整数。
    /// </summary>
    public ulong? ToUInt64() => (ulong?)_value;

    /// <summary>
    /// 将解析到的值转换为 16 位整数。
    /// </summary>
    public short? ToInt16() => (short?)_value;

    /// <summary>
    /// 将解析到的值转换为无符号 16 位整数。
    /// </summary>
    public ushort? ToUInt16() => (ushort?)_value;
}

/// <summary>
/// 专门解析来自命令行的字符串类型，并辅助赋值给属性。
/// </summary>
public struct StringArgument
{
    /// <summary>
    /// 指示在解析失败时是否忽略异常并保持未初始化的状态。
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// 存储解析到的字符串值。
    /// </summary>
    private string? _text;

    /// <summary>
    /// 当命令行输入了一个字符串参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的字符串值。</param>
    public void Assign(ReadOnlySpan<char> value)
    {
        _text = value.ToString();
    }

    /// <summary>
    /// 将解析到的值转换为字符。
    /// </summary>
    /// <returns>如果字符串长度为 1，则返回该字符；否则返回 null。</returns>
    public char? ToChar() => _text switch
    {
        null => null,
        { Length: 1 } => _text[0],
        _ when IgnoreExceptions => null,
        _ => throw new FormatException($"无法将 \"{_text}\" 转换为字符，因为它的长度不为 1。"),
    };

    /// <summary>
    /// 将解析到的值转换为字符串。
    /// </summary>
    public override string? ToString()
    {
        return _text;
    }
}

/// <summary>
/// 专门解析来自命令行的字符串集合类型，并辅助赋值给属性。
/// </summary>
public struct StringListArgument
{
    /// <summary>
    /// 存储解析到的字符串列表。
    /// </summary>
    private List<string>? _list;

    /// <summary>
    /// 当命令行输入了一个字符串参数时，调用此方法追加值。
    /// </summary>
    /// <param name="value">解析到的字符串值。</param>
    public void Append(ReadOnlySpan<char> value)
    {
        _list ??= [];
        _list.Add(value.ToString());
    }

    /// <summary>
    /// 将解析到的值转换为字符串数组。
    /// </summary>
    public string[] ToArray() => _list switch
    {
        null or { Count: 0 } => [],
        { } values => [..values],
    };

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// 将解析到的值转换为不可变数组。
    /// </summary>
    public ImmutableArray<string> ToImmutableArray() => _list switch
    {
#if NET8_0_OR_GREATER
        null or { Count: 0 } => [],
        { } values => [..values],
#else
        null or { Count: 0 } => ImmutableArray<string>.Empty,
        { } values => values.ToImmutableArray(),
#endif
    };

    /// <summary>
    /// 将解析到的值转换为不可变哈希集合。
    /// </summary>
    public ImmutableHashSet<string> ToImmutableHashSet() => _list switch
    {
#if NET8_0_OR_GREATER
        null or { Count: 0 } => [],
        { } values => [..values],
#else
        null or { Count: 0 } => ImmutableHashSet<string>.Empty,
        { } values => values.ToImmutableHashSet(),
#endif
    };

#endif

    /// <summary>
    /// 将解析到的值转换为集合。
    /// </summary>
    public Collection<string> ToCollection() => _list switch
    {
        null or { Count: 0 } => [],
        { } values => [..values],
    };

    /// <summary>
    /// 将解析到的值转换为列表。
    /// </summary>
    public List<string> ToList() => _list switch
    {
        null or { Count: 0 } => [],
        { } values => values,
    };
}

/// <summary>
/// 专门解析来自命令行的字典类型，并辅助赋值给属性。
/// </summary>
public struct DictionaryArgument
{
    /// <summary>
    /// 存储解析到的字符串字典。
    /// </summary>
    private Dictionary<string, string> _dictionary;

    /// <summary>
    /// 当命令行输入了一个键值对参数时，调用此方法追加值。
    /// </summary>
    /// <param name="key">解析到的键。</param>
    /// <param name="value">解析到的值。</param>
    public void Append(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        _dictionary ??= [];
        _dictionary[key.ToString()] = value.ToString();
    }

    /// <summary>
    /// 将解析到的值转换为键值对。
    /// </summary>
    public KeyValuePair<string, string>? ToKeyValuePair()
    {
        if (_dictionary is null || _dictionary.Count == 0)
        {
            return null;
        }

        if (_dictionary.Count > 1)
        {
            throw new InvalidOperationException("字典包含多个元素，无法转换为 KeyValuePair。");
        }

        using var enumerator = _dictionary.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.Current;
    }

    /// <summary>
    /// 将解析到的值转换为字典。
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        return _dictionary ?? [];
    }
}

/// <summary>
/// 在运行时解析来自命令行的枚举类型，并辅助赋值给属性。
/// </summary>
/// <remarks>
/// 源生成器会为各个枚举生成专门的编译时类型来处理枚举的赋值。<br/>
/// 此类型是为那些在运行时才知道枚举类型的场景准备的。
/// </remarks>
public struct RuntimeEnumArgument<T> where T : unmanaged, Enum
{
    /// <summary>
    /// 指示在解析失败时是否忽略异常并保持未初始化的状态。
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// 存储解析到的枚举值。
    /// </summary>
    private T? _value;

    /// <summary>
    /// 当命令行输入了一个数值参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的数值字符串。</param>
    public void Assign(ReadOnlySpan<char> value)
    {
        if (Enum.TryParse<T>(value
#if !NET6_0_OR_GREATER
                    .ToString()
#endif
                , ignoreCase: true, out var enumValue))
        {
            _value = enumValue;
        }
        else if (!IgnoreExceptions)
        {
            throw new FormatException($"无法将 \"{value.ToString()}\" 转换为 {typeof(T).FullName} 枚举。");
        }
    }

    /// <summary>
    /// 将解析到的值转换为枚举。
    /// </summary>
    public T? ToEnum() => _value;
}
