#if NETCOREAPP3_1_OR_GREATER
using System.Collections.Immutable;
#endif
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 专门解析来自命令行的布尔类型，并辅助赋值给属性。
/// </summary>
[DebuggerDisplay("Boolean: {Value,nq}")]
public readonly record struct BooleanArgument
{
    /// <summary>
    /// 存储解析到的布尔值。
    /// </summary>
    private bool? Value { get; init; }

    /// <summary>
    /// 当命令行直接或间接输入了一个布尔参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的布尔值。</param>
    public BooleanArgument Assign(ReadOnlySpan<char> value)
    {
        return value switch
        {
            // 因为解析器已经保证了布尔参数只可能出现以下三种值：
            // - []:  表示 true
            // - ['1', ..]: 表示 true
            // - ['0', ..]: 表示 false
            [] => new BooleanArgument { Value = true },
            ['1', ..] => new BooleanArgument { Value = true },
            _ => new BooleanArgument { Value = false },
        };
    }

    /// <summary>
    /// 将解析到的值转换为布尔值。
    /// </summary>
    public bool? ToBoolean()
    {
        return Value;
    }
}

/// <summary>
/// 专门解析来自命令行的数值类型，并辅助赋值给属性。
/// </summary>
[DebuggerDisplay("Number: {Value,nq}")]
public readonly record struct NumberArgument
{
    /// <summary>
    /// 指示在解析失败时是否忽略异常并保持未初始化的状态。
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// 存储解析到的数值。
    /// </summary>
    private decimal? Value { get; init; }

    /// <summary>
    /// 当命令行输入了一个数值参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的数值字符串。</param>
    public NumberArgument Assign(ReadOnlySpan<char> value)
    {
        if (decimal.TryParse(value
#if !NETCOREAPP3_1_OR_GREATER
                    .ToString()
#endif
                , out var doubleValue))
        {
            return this with { Value = doubleValue };
        }
        if (!IgnoreExceptions)
        {
            throw new FormatException($"无法将 \"{value.ToString()}\" 转换为数值。");
        }
        return this;
    }

    /// <summary>
    /// 将解析到的值转换为字节。
    /// </summary>
    public byte? ToByte() => (byte?)Value;

    /// <summary>
    /// 将解析到的值转换为有符号字节。
    /// </summary>
    public sbyte? ToSByte() => (sbyte?)Value;

    /// <summary>
    /// 将解析到的值转换为高精度浮点数。
    /// </summary>
    public decimal? ToDecimal() => Value;

    /// <summary>
    /// 将解析到的值转换为双精度浮点数。
    /// </summary>
    public double? ToDouble() => (double?)Value;

    /// <summary>
    /// 将解析到的值转换为单精度浮点数。
    /// </summary>
    public float? ToSingle() => (float?)Value;

    /// <summary>
    /// 将解析到的值转换为 32 位整数。
    /// </summary>
    public int? ToInt32() => (int?)Value;

    /// <summary>
    /// 将解析到的值转换为无符号 32 位整数。
    /// </summary>
    public uint? ToUInt32() => (uint?)Value;

    /// <summary>
    /// 将解析到的值转换为指针大小的整数。
    /// </summary>
    public nint? ToIntPtr() => (nint?)Value;

    /// <summary>
    /// 将解析到的值转换为无符号指针大小的整数。
    /// </summary>
    public nuint? ToUIntPtr() => (nuint?)Value;

    /// <summary>
    /// 将解析到的值转换为 64 位整数。
    /// </summary>
    public long? ToInt64() => (long?)Value;

    /// <summary>
    /// 将解析到的值转换为无符号 64 位整数。
    /// </summary>
    public ulong? ToUInt64() => (ulong?)Value;

    /// <summary>
    /// 将解析到的值转换为 16 位整数。
    /// </summary>
    public short? ToInt16() => (short?)Value;

    /// <summary>
    /// 将解析到的值转换为无符号 16 位整数。
    /// </summary>
    public ushort? ToUInt16() => (ushort?)Value;
}

/// <summary>
/// 专门解析来自命令行的字符串类型，并辅助赋值给属性。
/// </summary>
[DebuggerDisplay("String: {Value,nq}")]
public readonly record struct StringArgument
{
    /// <summary>
    /// 指示在解析失败时是否忽略异常并保持未初始化的状态。
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// 存储解析到的字符串值。
    /// </summary>
    private string? Value { get; init; }

    /// <summary>
    /// 当命令行输入了一个字符串参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的字符串值。</param>
    public StringArgument Assign(ReadOnlySpan<char> value)
    {
        return this with { Value = value.ToString() };
    }

    /// <summary>
    /// 将解析到的值转换为字符。
    /// </summary>
    /// <returns>如果字符串长度为 1，则返回该字符；否则返回 null。</returns>
    public char? ToChar() => Value switch
    {
        null => null,
        { Length: 1 } => Value[0],
        _ when IgnoreExceptions => null,
        _ => throw new FormatException($"无法将 \"{Value}\" 转换为字符，因为它的长度不为 1。"),
    };

    /// <summary>
    /// 将解析到的值转换为字符串。
    /// </summary>
    public override string? ToString()
    {
        return Value;
    }
}

/// <summary>
/// 专门解析来自命令行的字符串集合类型，并辅助赋值给属性。
/// </summary>
[DebuggerDisplay("String: {Value,nq}")]
public readonly record struct StringListArgument
{
    /// <summary>
    /// 存储解析到的字符串列表。
    /// </summary>
    private List<string>? Value { get; init; }

    /// <summary>
    /// 当命令行输入了一个字符串参数时，调用此方法追加值。
    /// </summary>
    /// <param name="value">解析到的字符串值。</param>
    public StringListArgument Append(ReadOnlySpan<char> value)
    {
        var list = Value;
        list ??= [];
        list.Add(value.ToString());
        return new StringListArgument { Value = list };
    }

    /// <summary>
    /// 将解析到的值转换为字符串数组。
    /// </summary>
    public string[] ToArray() => Value switch
    {
        null or { Count: 0 } => [],
        { } values => [..values],
    };

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// 将解析到的值转换为不可变数组。
    /// </summary>
    public ImmutableArray<string> ToImmutableArray() => Value switch
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
    public ImmutableHashSet<string> ToImmutableHashSet() => Value switch
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
    public Collection<string> ToCollection() => Value switch
    {
        null or { Count: 0 } => [],
        { } values => [..values],
    };

    /// <summary>
    /// 将解析到的值转换为列表。
    /// </summary>
    public List<string> ToList() => Value switch
    {
        null or { Count: 0 } => [],
        { } values => values,
    };
}

/// <summary>
/// 专门解析来自命令行的字典类型，并辅助赋值给属性。
/// </summary>
[DebuggerDisplay("String: {Value,nq}")]
public readonly record struct StringDictionaryArgument
{
    /// <summary>
    /// 存储解析到的字符串字典。
    /// </summary>
    private Dictionary<string, string> Value { get; init; }

    /// <summary>
    /// 当命令行输入了一个键值对参数时，调用此方法追加值。
    /// </summary>
    /// <param name="key">解析到的键。</param>
    /// <param name="value">解析到的值。</param>
    public StringDictionaryArgument Append(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        var dictionary = Value;
        dictionary ??= [];
        dictionary[key.ToString()] = value.ToString();
        return new StringDictionaryArgument { Value = dictionary };
    }

    /// <summary>
    /// 将解析到的值转换为键值对。
    /// </summary>
    public KeyValuePair<string, string>? ToKeyValuePair()
    {
        if (Value is null || Value.Count == 0)
        {
            return null;
        }

        if (Value.Count > 1)
        {
            throw new InvalidOperationException("字典包含多个元素，无法转换为 KeyValuePair。");
        }

        using var enumerator = Value.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.Current;
    }

    /// <summary>
    /// 将解析到的值转换为字典。
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        return Value ?? [];
    }
}

/// <summary>
/// 在运行时解析来自命令行的枚举类型，并辅助赋值给属性。
/// </summary>
/// <remarks>
/// 源生成器会为各个枚举生成专门的编译时类型来处理枚举的赋值。<br/>
/// 此类型是为那些在运行时才知道枚举类型的场景准备的。
/// </remarks>
public readonly record struct RuntimeEnumArgument<T> where T : unmanaged, Enum
{
    /// <summary>
    /// 指示在解析失败时是否忽略异常并保持未初始化的状态。
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// 存储解析到的枚举值。
    /// </summary>
    private T? Value { get; init; }

    /// <summary>
    /// 当命令行输入了一个数值参数时，调用此方法赋值。
    /// </summary>
    /// <param name="value">解析到的数值字符串。</param>
    public RuntimeEnumArgument<T> Assign(ReadOnlySpan<char> value)
    {
        if (Enum.TryParse<T>(value
#if !NET6_0_OR_GREATER
                    .ToString()
#endif
                , ignoreCase: true, out var enumValue))
        {
            return this with { Value = enumValue };
        }
        if (!IgnoreExceptions)
        {
            throw new FormatException($"无法将 \"{value.ToString()}\" 转换为 {typeof(T).FullName} 枚举。");
        }
        return this;
    }

    /// <summary>
    /// 将解析到的值转换为枚举。
    /// </summary>
    public T? ToEnum() => Value;
}
