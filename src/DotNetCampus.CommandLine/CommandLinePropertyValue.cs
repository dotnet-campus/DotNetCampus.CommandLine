using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli;

/// <summary>
/// 包含从命令行解析出来的属性值，可供转换为各种常见类型。
/// </summary>
public readonly struct CommandLinePropertyValue : IReadOnlyList<string>
{
    private readonly IReadOnlyList<string> _values;
    private readonly MultiValueHandling _multiValueHandling;

    internal CommandLinePropertyValue(IReadOnlyList<string> values, MultiValueHandling multiValueHandling)
    {
        _values = values;
        _multiValueHandling = multiValueHandling;
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator() => _values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
    int IReadOnlyCollection<string>.Count => _values.Count;
    string IReadOnlyList<string>.this[int index] => _values[index];

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="bool"/>。
    /// </summary>
    public static implicit operator bool(CommandLinePropertyValue propertyValue)
    {
        return propertyValue._values switch
        {
            // 没传选项时，相当于传了 false。
            null => false,
            // 传了选项时，相当于传了 true。
            { Count: 0 } => true,
            // 传了选项，后面还带了参数时，取第一个参数的值作为 true/false。
            { } values => ParseBoolean(values[0]) ?? throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid boolean value. Available values are: 1, true, yes, on, 0, false, no, off."),
        };

        static bool? ParseBoolean(string value)
        {
            var isTrue = value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("on", StringComparison.OrdinalIgnoreCase);
            if (isTrue)
            {
                return true;
            }
            var isFalse = value.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                          value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                          value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                          value.Equals("off", StringComparison.OrdinalIgnoreCase);
            if (isFalse)
            {
                return false;
            }
            return null;
        }
    }

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="byte"/>。
    /// </summary>
    public static implicit operator byte(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => byte.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid byte value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="sbyte"/>。
    /// </summary>
    public static implicit operator sbyte(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => sbyte.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid sbyte value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="char"/>。
    /// </summary>
    public static implicit operator char(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => char.TryParse(values[0], out var result) ? result : '\0',
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="decimal"/>。
    /// </summary>
    public static implicit operator decimal(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => decimal.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid decimal value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="double"/>。
    /// </summary>
    public static implicit operator double(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => double.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid double value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="float"/>。
    /// </summary>
    public static implicit operator float(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => float.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid float value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="int"/>。
    /// </summary>
    public static implicit operator int(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => int.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid int value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="uint"/>。
    /// </summary>
    public static implicit operator uint(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => uint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid uint value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="nint"/>。
    /// </summary>
    public static implicit operator nint(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => nint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid nint value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="nuint"/>。
    /// </summary>
    public static implicit operator nuint(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => nuint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid unint value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="long"/>。
    /// </summary>
    public static implicit operator long(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => long.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid long value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="ulong"/>。
    /// </summary>
    public static implicit operator ulong(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => ulong.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid ulong value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="short"/>。
    /// </summary>
    public static implicit operator short(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => short.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid short value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="ushort"/>。
    /// </summary>
    public static implicit operator ushort(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => default,
        { } values => ushort.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid ushort value."),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为 <see cref="string"/>。
    /// </summary>
    public static implicit operator string(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => "",
        { } values => propertyValue._multiValueHandling switch
        {
            MultiValueHandling.First => values[0],
            MultiValueHandling.Last => values[^1],
            MultiValueHandling.SpaceAll => string.Join(' ', values),
            MultiValueHandling.SlashAll => string.Join('/', values),
            _ => values[0],
        },
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串数组。
    /// </summary>
    public static implicit operator string[](CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => [],
        { } values => [..SplitValues(values)],
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为不可变字符串数组。
    /// </summary>
    public static implicit operator ImmutableArray<string>(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => [],
        { } values => [..SplitValues(values)],
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为不可变字符串哈希集合。
    /// </summary>
    public static implicit operator ImmutableHashSet<string>(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => [],
        { } values => [..SplitValues(values)],
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串集合。
    /// </summary>
    public static implicit operator Collection<string>(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => [],
        { } values => [..SplitValues(values)],
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串列表。
    /// </summary>
    public static implicit operator List<string>(CommandLinePropertyValue propertyValue) => propertyValue.ToList();

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串键值对。
    /// </summary>
    public static implicit operator KeyValuePair<string, string>(CommandLinePropertyValue propertyValue) => propertyValue.ToDictionary().FirstOrDefault();

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串字典。
    /// </summary>
    public static implicit operator Dictionary<string, string>(CommandLinePropertyValue propertyValue) => propertyValue.ToDictionary();

    /// <summary>
    /// 将从命令行解析出来的属性值转换为枚举值。
    /// </summary>
    public T ToEnum<T>() where T : unmanaged, Enum => _values switch
    {
        { Count: 0 } => default,
        { } values => Enum.TryParse(typeof(T), values[0], true, out var result) ? (T)result : default!,
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串列表。
    /// </summary>
    public List<string> ToList() => _values switch
    {
        { Count: 0 } => [],
        { } values => [..SplitValues(values)],
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串字典。
    /// </summary>
    public Dictionary<string, string> ToDictionary() => _values switch
    {
        { Count: 0 } => new Dictionary<string, string>(),
        { } values => values
            .SelectMany(x => x.Split(';', StringSplitOptions.RemoveEmptyEntries))
            .Select(x =>
            {
                var parts = x.Split('=');
                if (parts.Length is not 2)
                {
                    throw new CommandLineParseValueException(
                        $"Value [{x}] is not a valid dictionary. Expected format is key1=value1;key2=value2.");
                }
                return new KeyValuePair<string, string>(parts[0], parts[1]);
            })
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Last().Value),
    };

    private static IEnumerable<string> SplitValues(IReadOnlyList<string> commandLineValues)
    {
        for (var commandLineValueIndex = 0; commandLineValueIndex < commandLineValues.Count; commandLineValueIndex++)
        {
            var optionValue = commandLineValues[commandLineValueIndex];
            var lastPart = ListValueParsingType.Start;
            var thisPartStartIndex = 0;
            for (var index = 0; index < optionValue.Length; index++)
            {
                var c = optionValue[index];

                // 引号
                if (c is '"')
                {
                    if (lastPart is ListValueParsingType.Start)
                    {
                        // 开始的引号
                        lastPart = ListValueParsingType.QuoteStart;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuoteStart)
                    {
                        // 连续出现的引号
                        yield return "";
                        lastPart = ListValueParsingType.QuoteEnd;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuotedValue)
                    {
                        // 引号中值后的引号
                        yield return optionValue[thisPartStartIndex..index];
                        lastPart = ListValueParsingType.QuoteEnd;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuotedSeparator)
                    {
                        // 引号中分割符后的引号
                        yield return "";
                        lastPart = ListValueParsingType.QuotedValue;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuoteEnd)
                    {
                        // 引号结束后的引号
                        lastPart = ListValueParsingType.QuoteStart;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.Value)
                    {
                        // 正常值后的引号
                        throw new CommandLineParseValueException(
                            $"Invalid value format at index [{index}]: {optionValue}");
                    }
                    if (lastPart is ListValueParsingType.Separator)
                    {
                        // 正常分隔符后的引号
                        lastPart = ListValueParsingType.QuoteStart;
                        continue;
                    }
                }

                // 分割符
                if (c is ';' or ',')
                {
                    if (lastPart is ListValueParsingType.Start)
                    {
                        // 开始的分割符
                        yield return "";
                        lastPart = ListValueParsingType.Separator;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuoteStart)
                    {
                        // 引号后紧跟着的分割符（等同于正常字符）
                        lastPart = ListValueParsingType.QuotedValue;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuotedValue)
                    {
                        // 引号中值后的分割符（等同于正常字符）
                        lastPart = ListValueParsingType.QuotedValue;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuotedSeparator)
                    {
                        // 引号中连续出现的分割符（等同于正常字符）
                        lastPart = ListValueParsingType.QuotedValue;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.QuoteEnd)
                    {
                        // 引号结束后的分割符
                        lastPart = ListValueParsingType.Separator;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.Value)
                    {
                        // 正常值后的分割符
                        yield return optionValue[thisPartStartIndex..index];
                        lastPart = ListValueParsingType.Separator;
                        continue;
                    }
                    if (lastPart is ListValueParsingType.Separator)
                    {
                        // 连续出现的分割符
                        yield return "";
                        lastPart = ListValueParsingType.Separator;
                        continue;
                    }
                }

                // 其他字符
                if (lastPart is ListValueParsingType.Start)
                {
                    // 开始的值
                    thisPartStartIndex = index;
                    lastPart = ListValueParsingType.Value;
                    continue;
                }
                if (lastPart is ListValueParsingType.QuoteStart)
                {
                    // 引号后紧跟着的值
                    thisPartStartIndex = index;
                    lastPart = ListValueParsingType.QuotedValue;
                    continue;
                }
                if (lastPart is ListValueParsingType.QuotedValue)
                {
                    // 引号中值后的值
                    lastPart = ListValueParsingType.QuotedValue;
                    continue;
                }
                if (lastPart is ListValueParsingType.QuotedSeparator)
                {
                    // 引号中分割符（实际上就是正常值）后的值
                    lastPart = ListValueParsingType.QuotedValue;
                    continue;
                }
                if (lastPart is ListValueParsingType.QuoteEnd)
                {
                    // 引号结束后的值
                    throw new CommandLineParseValueException(
                        $"Invalid value format at index [{index}]: {optionValue}");
                }
                if (lastPart is ListValueParsingType.Value)
                {
                    // 正常值后的值
                    lastPart = ListValueParsingType.Value;
                    continue;
                }
                if (lastPart is ListValueParsingType.Separator)
                {
                    // 正常分割符后的值
                    thisPartStartIndex = index;
                    lastPart = ListValueParsingType.Value;
                    continue;
                }
            }

            // 处理最后一个值
            if (lastPart is ListValueParsingType.Start)
            {
                // 一开始就结束了（字符串里就没有值）
                yield return "";
            }
            else if (lastPart is ListValueParsingType.QuoteStart or ListValueParsingType.QuotedValue or ListValueParsingType.QuotedSeparator)
            {
                // 引号还没结束，字符串就结束了
                throw new CommandLineParseValueException(
                    $"Missing quote end at index [{optionValue.Length}]: {optionValue}");
            }
            else if (lastPart is ListValueParsingType.QuoteEnd)
            {
                // 引号结束后字符串正常结束
            }
            else if (lastPart is ListValueParsingType.Value)
            {
                // 正常值结束的字符串
                yield return optionValue[thisPartStartIndex..];
            }
            else if (lastPart is ListValueParsingType.Separator)
            {
                // 正常分割符后就结束了字符串
                yield return "";
            }
        }
    }
}

file enum ListValueParsingType
{
    /// <summary>
    /// 尚未开始分割。
    /// </summary>
    Start,

    /// <summary>
    /// 引号开始。
    /// </summary>
    QuoteStart,

    /// <summary>
    /// 引号中的值。
    /// </summary>
    QuotedValue,

    /// <summary>
    /// 引号中的分割符。
    /// </summary>
    QuotedSeparator,

    /// <summary>
    /// 引号结束。
    /// </summary>
    QuoteEnd,

    /// <summary>
    /// 正常值。
    /// </summary>
    Value,

    /// <summary>
    /// 正常分割符。
    /// </summary>
    Separator,
}

internal enum MultiValueHandling
{
    /// <summary>
    /// 仅返回第一个值。
    /// </summary>
    First,

    /// <summary>
    /// 返回最后一个值。
    /// </summary>
    Last,

    /// <summary>
    /// 用空格连接返回所有值。
    /// </summary>
    SpaceAll,

    /// <summary>
    /// 用斜杠 '/' 连接返回所有值。
    /// </summary>
    SlashAll,
}
