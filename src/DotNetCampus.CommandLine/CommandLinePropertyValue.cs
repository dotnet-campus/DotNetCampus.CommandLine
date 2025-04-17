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
        { Count: 0 } => Array.Empty<string>(),
        { } values => values.ToArray(),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为不可变字符串数组。
    /// </summary>
    public static implicit operator ImmutableArray<string>(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => ImmutableArray<string>.Empty,
        { } values => values.ToImmutableArray(),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为不可变字符串哈希集合。
    /// </summary>
    public static implicit operator ImmutableHashSet<string>(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => ImmutableHashSet<string>.Empty,
        { } values => values.ToImmutableHashSet(),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串集合。
    /// </summary>
    public static implicit operator Collection<string>(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => [],
        { } values => [..values],
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串列表。
    /// </summary>
    public static implicit operator List<string>(CommandLinePropertyValue propertyValue) => propertyValue._values switch
    {
        { Count: 0 } => [],
        { } values => values.ToList(),
    };

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串键值对。
    /// </summary>
    public static implicit operator KeyValuePair<string, string>(CommandLinePropertyValue propertyValue) => propertyValue.ToDictionary().FirstOrDefault();

    /// <summary>
    /// 将从命令行解析出来的属性值转换为字符串字典。
    /// </summary>
    public static implicit operator Dictionary<string, string>(CommandLinePropertyValue propertyValue) => propertyValue.ToDictionary();

    /// <summary>
    /// 将从命令行解析出来的属性值以只读列表的形式访问。
    /// </summary>
    public IReadOnlyList<string> AsReadOnlyList() => _values;

    /// <summary>
    /// 将从命令行解析出来的属性值转换为枚举值。
    /// </summary>
    public T ToEnum<T>() where T : unmanaged => _values switch
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
        { } values => values.ToList(),
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
