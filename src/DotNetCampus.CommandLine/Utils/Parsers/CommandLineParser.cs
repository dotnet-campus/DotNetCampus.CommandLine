using Cat = DotNetCampus.Cli.Utils.Parsers.CommandArgumentType;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 通用的命令行参数解析器。（此解析器不可解析 URL 类型的参数。）
/// </summary>
public readonly ref struct CommandLineParser
{
    private readonly CommandLine _commandLine;
    private readonly string _commandObjectName;
    private readonly int _commandCount;
    private readonly bool _caseSensitive;
    private readonly CommandNamingPolicy _namingPolicy;

    /// <summary>
    /// 通用的命令行参数解析器。（此解析器不可解析 URL 类型的参数。）
    /// </summary>
    /// <param name="commandLine">要解析的命令行参数。</param>
    /// <param name="commandObjectName">正在解析此参数的命令对象的名称。</param>
    /// <param name="commandCount">主命令/子命令/多级子命令的数量。在解析时，要跳过这些命令。</param>
    public CommandLineParser(CommandLine commandLine, string commandObjectName, int commandCount)
    {
        _commandLine = commandLine;
        _commandObjectName = commandObjectName;
        _commandCount = commandCount;
        Style = commandLine.ParsingOptions.Style;
        _namingPolicy = Style.NamingPolicy;
        OptionPrefix = Style.OptionPrefix;
        _caseSensitive = Style.CaseSensitive;
        SupportsLongOption = Style.SupportsLongOption;
        SupportsShortOption = Style.SupportsShortOption;
        SupportsShortOptionCombination = Style.SupportsShortOptionCombination;
        SupportsShortOptionValueWithoutSeparator = Style.SupportsShortOptionValueWithoutSeparator;
    }

    /// <summary>
    /// 获取解析命令行时所使用的各种选项。
    /// </summary>
    internal CommandLineStyleDetails Style { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.OptionPrefix"/>
    internal CommandOptionPrefix OptionPrefix { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsLongOption"/>
    internal bool SupportsLongOption { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsShortOption"/>
    internal bool SupportsShortOption { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsShortOptionCombination"/>
    internal bool SupportsShortOptionCombination { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsShortOptionValueWithoutSeparator"/>
    internal bool SupportsShortOptionValueWithoutSeparator { get; }

    /// <summary>
    /// 要求源生成器匹配长名称，返回此长选项的值类型。
    /// </summary>
    public required LongOptionMatchingCallback MatchLongOption { get; init; }

    /// <summary>
    /// 要求源生成器匹配短名称，返回此短选项的值类型。
    /// </summary>
    public required ShortOptionMatchingCallback MatchShortOption { get; init; }

    /// <summary>
    /// 要求源生成器匹配位置参数，返回位置参数的范围。
    /// </summary>
    public required PositionalArgumentMatchingCallback MatchPositionalArguments { get; init; }

    /// <summary>
    /// 要求源生成器将解析到的值赋值给指定索引处的属性。
    /// </summary>
    public required AssignPropertyValueCallback AssignPropertyValue { get; init; }

    /// <summary>
    /// 获取默认的选项值处理器（默认的选项处理器仅为了避免代码错误产生误用，实际永远不会被使用）。
    /// </summary>
    private static OptionValueMatch DefaultOptionValueHandler => new OptionValueMatch("", -1, OptionValueType.Normal);

    /// <summary>
    /// 解析命令行参数，并返回解析结果。
    /// </summary>
    /// <returns>命令行参数解析结果。</returns>
    public CommandLineParsingResult Parse()
    {
        var arguments = _commandLine.CommandLineArguments;
        var currentOptionName = new OptionName(false, []);
        var currentOption = new OptionValueMatch("", -1, OptionValueType.Normal);
        var currentPositionArgumentIndex = 0;
        var lastState = Cat.Start;

        for (var index = _commandCount; index < arguments.Count; index++)
        {
            var argument = arguments[index];

            // 状态机状态转移。
            var part = new CommandArgumentPart(this, argument, lastState, currentOption.ValueType);
            part.Parse();
            var (state, optionName, value) = part;
            lastState = state;

            // 应用新状态下的值。
            switch (state)
            {
                case Cat.LongOption or Cat.ShortOption or Cat.Option:
                {
                    // 如果当前是一个选项，则记录下来，供后面解析选项值时使用。
                    currentOptionName = optionName;
                    var optionMatch = state switch
                    {
                        Cat.LongOption => MatchLongOption(optionName.Name, _caseSensitive, _namingPolicy),
                        Cat.ShortOption => MatchShortOption(optionName.Name, _caseSensitive),
                        _ => MatchLongOption(optionName.Name, _caseSensitive, _namingPolicy) switch
                        {
                            { ValueType: OptionValueType.NotExist } => MatchShortOption(optionName.Name, _caseSensitive),
                            var t => t,
                        },
                    };
                    if (optionMatch.ValueType is OptionValueType.NotExist)
                    {
                        // 如果选项不存在，则报告错误。
                        return CommandLineParsingResult.OptionNotFound(_commandLine, index, _commandObjectName, optionName.Name);
                    }
                    currentOption = optionMatch;
                    break;
                }
                case Cat.OptionValue:
                {
                    AssignOptionValue(currentOption, value);
                    if (currentOption.ValueType is not OptionValueType.Collection)
                    {
                        // 如果不是集合，那么此选项已经结束。
                        // 清空上一个选项，避免误用。
                        currentOptionName = new OptionName(false, []);
                        currentOption = DefaultOptionValueHandler;
                    }
                    break;
                }
                case Cat.PositionalArgument or Cat.PostPositionalArgument:
                {
                    var positionalArgumentMatch = MatchPositionalArguments(value, currentPositionArgumentIndex);
                    if (positionalArgumentMatch.ValueType is PositionalArgumentValueType.NotExist)
                    {
                        // 如果位置参数不存在，则报告错误。
                        return CommandLineParsingResult.PositionalArgumentNotFound(_commandLine, index, _commandObjectName, currentPositionArgumentIndex);
                    }
                    currentPositionArgumentIndex++;
                    AssignPositionalArgumentValue(positionalArgumentMatch, value);
                    break;
                }
                case Cat.LongOptionWithValue or Cat.ShortOptionWithValue or Cat.OptionWithValue:
                {
                    AssignOptionValue(currentOption, value);
                    break;
                }
                case Cat.ErrorOption:
                {
                    // 如果当前参数疑似选项但解析失败，则报告错误。
                    return CommandLineParsingResult.OptionParseError(_commandLine, index);
                }
                case Cat.MultiShortOptions:
                {
                    // 逐个处理多个短选项。
                    for (var i = 0; i < optionName.Name.Length; i++)
                    {
                        var n = optionName.Name[i..(i + 1)];
                        var optionMatch = MatchShortOption(n, _caseSensitive);
                        if (optionMatch.ValueType is OptionValueType.NotExist)
                        {
                            // 如果选项不存在，则报告错误。
                            return CommandLineParsingResult.OptionNotFound(_commandLine, index, _commandObjectName, n);
                        }
                        AssignOptionValue(optionMatch, []);
                    }
                    break;
                }
                case Cat.MultiShortOptionsOrShortOptionConcatWithValue:
                {
                    // 先看看是否是一个多字符短选项，如果不是，再看看是否是单个字符无分隔符带值的短选项。
                    var m = optionName.Name;
                    var optionMatch = MatchShortOption(m, _caseSensitive);
                    if (optionMatch.ValueType is not OptionValueType.NotExist)
                    {
                        // 是一个多字符短选项。
                        AssignOptionValue(optionMatch, []);
                        break;
                    }
                    // 不是一个多字符短选项，尝试解析为单个字符无分隔符带值的短选项。
                    var n = m[..1];
                    var v = m[1..];
                    optionMatch = MatchShortOption(n, _caseSensitive);
                    if (optionMatch.ValueType is OptionValueType.NotExist)
                    {
                        // 如果选项不存在，则报告错误。
                        return CommandLineParsingResult.OptionNotFound(_commandLine, index, _commandObjectName, n);
                    }
                    AssignOptionValue(optionMatch, v);
                    break;
                }
                default:
                {
                    // 其他状态要么已经处理过了，要不还未处理，要么不需要处理，所以不需要做任何事情。
                    break;
                }
            }
        }

        return CommandLineParsingResult.Success;
    }

    private void AssignOptionValue(OptionValueMatch match, ReadOnlySpan<char> value)
    {
        SplitKeyValue(match.ValueType is OptionValueType.Dictionary, value, out var k, out var v);
        AssignPropertyValue(match.PropertyName, match.PropertyIndex, k, v);
    }

    private void AssignPositionalArgumentValue(PositionalArgumentValueMatch match, ReadOnlySpan<char> value)
    {
        SplitKeyValue(false, value, out var k, out var v);
        AssignPropertyValue(match.PropertyName, match.PropertyIndex, k, v);
    }

    private static void SplitKeyValue(bool isDictionary, ReadOnlySpan<char> item,
        out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        if (!isDictionary)
        {
            key = [];
            value = item;
            return;
        }

        // 截至目前，所有的字典类型都使用 key=value 形式，如果将来新增的风格有其他符号，我们再用一样的分隔符方式来配置。
        var index = item.IndexOf('=');
        if (index < 0)
        {
            key = item;
            value = [];
            return;
        }
        key = item[..index];
        value = item[(index + 1)..];
    }
}

/// <summary>
/// 这是命令行解析状态机中的其中一个状态。当调用 <see cref="Parse"/> 方法后，此对象会被修改以转移到新的状态。
/// </summary>
internal ref struct CommandArgumentPart
{
    private readonly CommandLineParser _parser;
    private readonly string _argument;
    private readonly Cat _lastType;
    private readonly OptionValueType _lastOptionType;

    /// <summary>
    /// 辅助解析命令行参数中的其中一个参数。
    /// </summary>
    /// <param name="parser">正在使用的命令行参数解析器。</param>
    /// <param name="argument">要解析的参数。</param>
    /// <param name="lastType">上一个参数的类型，初始为 <see cref="CommandArgumentType.Start"/>。</param>
    /// <param name="lastOptionType">上一个参数的选项值类型，如果上一个参数不是选项，则为默认值。</param>
    public CommandArgumentPart(CommandLineParser parser, string argument, Cat lastType, OptionValueType lastOptionType)
    {
        _parser = parser;
        _argument = argument;
        _lastType = lastType;
        _lastOptionType = lastOptionType;
    }

    /// <summary>
    /// 解析完成后，发现此参数的类型。
    /// </summary>
    public Cat Type { get; private set; }

    /// <summary>
    /// 如果此参数是一个选项（长选项或短选项），则为此选项的名称；否则为默认值。
    /// </summary>
    public OptionName Option { get; private set; }

    /// <summary>
    /// 如果此参数包含值（位置参数或选项值），则为此值；否则为默认值。
    /// </summary>
    public ReadOnlySpan<char> Value { get; private set; }

    /// <summary>
    /// 将此参数解构为各个部分。
    /// </summary>
    /// <param name="type">此参数的类型。</param>
    /// <param name="optionName">如果此参数是一个选项（长选项或短选项），则为此选项的名称；否则为默认值。</param>
    /// <param name="value">如果此参数包含值（位置参数或选项值），则为此值；否则为默认值。</param>
    public void Deconstruct(out Cat type, out OptionName optionName, out ReadOnlySpan<char> value)
    {
        type = Type;
        optionName = Option;
        value = Value;
    }

    /// <summary>
    /// 以上一个状态为基准，解析当前参数，并转移到新的状态。
    /// </summary>
    /// <returns>
    /// 返回值没有意义，纯粹为了使用 switch 表达式。
    /// </returns>
    public void Parse() => _ = _lastType switch
    {
        Cat.Start or Cat.Command => ParseCommandRegion(),
        Cat.PositionalArgumentSeparator or Cat.PostPositionalArgument => ParsePostPositionalArgumentRegion(),
        _ => ParseOptionAndPositionalArgumentRegion(),
    };

    /// <summary>
    /// 起点/命令/子命令区 --> 命令/子命令区
    /// 起点/命令/子命令区 --> 选项和位置参数混合区
    /// </summary>
    private bool ParseCommandRegion()
    {
        // 由于命令已提前跳过，所以这里直接进入选项和位置参数混合区。
        return ParseOptionAndPositionalArgumentRegion();
    }

    /// <summary>
    /// 选项和位置参数混合区 --> 后置位置参数区
    /// 选项和位置参数混合区 --> 选项和位置参数混合区
    /// </summary>
    private bool ParseOptionAndPositionalArgumentRegion()
    {
        var isPostPositionalArgument = string.Equals(_argument, "--", StringComparison.Ordinal);
        if (isPostPositionalArgument)
        {
            Type = Cat.PositionalArgumentSeparator;
            return true;
        }

        return _lastType switch
        {
            // 值已经被上一个选项消费掉了，必须是新的选项或位置参数。
            Cat.PositionalArgument or Cat.LongOptionWithValue or Cat.ShortOptionWithValue or Cat.OptionWithValue => ParseOptionOrPositionalArgument(),
            // 多个短选项，后面不允许带值。
            Cat.MultiShortOptions => ParseOptionOrPositionalArgument(),
            // 上一个是选项：
            Cat.LongOption or Cat.ShortOption or Cat.Option => (_lastOptionType switch
            {
                // 如果是布尔选项，则后面只能跟布尔值，否则只能是新的选项或位置参数。
                OptionValueType.Boolean => ParseBooleanOptionValueOrNewOptionOrPositionalArgument(),
                // 如果是集合选项，则后面可以跟多个值，直到遇到新的选项或位置参数分隔符为止。
                OptionValueType.Collection or OptionValueType.Dictionary => ParseCollectionOptionValueOrNewOptionOrPositionalArgument(),
                // 如果是普通选项，则后面只能是选项值。
                _ => ParseOptionValue(_argument.AsSpan()),
            }),
            _ => throw new InvalidOperationException($"解析上一个参数时已进入错误的状态：{_lastType}。"),
        };
    }

    /// <summary>
    /// 后置位置参数区 --> 后置位置参数区
    /// </summary>
    private bool ParsePostPositionalArgumentRegion()
    {
        Type = Cat.PostPositionalArgument;
        Value = _argument.AsSpan();
        return true;
    }

    /// <summary>
    /// 选项和位置参数混合区（状态内部）
    /// 起点 --> 位置参数
    /// 起点 --> 选项
    /// </summary>
    /// <returns></returns>
    private bool ParseOptionOrPositionalArgument()
    {
        var argument = _argument.AsSpan();
        if (argument.Length is 0 or 1)
        {
            // TODO 针对单独有选项分隔符的，要报错。
            // 空参数或单个字符（无法组成选项），视为位置参数。
            Type = Cat.PositionalArgument;
            Value = argument;
            return true;
        }

        return _parser.OptionPrefix switch
        {
            CommandOptionPrefix.DoubleDash => (argument[0], argument[1]) switch
            {
                ('-', '-') => ParseLongOptionOrLongOptionWithValue(argument[2..]),
                ('-', _) => ParseShortOptionOrMultiShortOptions(argument[1..]),
                _ => ParsePositionalArgument(argument),
            },
            CommandOptionPrefix.SingleDash => argument[0] switch
            {
                '-' => ParseLongShortOptionOrLongShortOptionWithValue(argument[1..]),
                _ => ParsePositionalArgument(argument),
            },
            CommandOptionPrefix.Slash => argument[0] switch
            {
                '/' => ParseLongShortOptionOrLongShortOptionWithValue(argument[1..]),
                _ => ParsePositionalArgument(argument),
            },
            CommandOptionPrefix.Any => (argument[0], argument[1]) switch
            {
                ('-', '-') => ParseLongOptionOrLongOptionWithValue(argument[2..]),
                ('-', _) or ('/', _) => ParseLongShortOptionOrLongShortOptionWithValue(argument[1..]),
                _ => ParsePositionalArgument(argument),
            },
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private bool ParseLongOptionOrLongOptionWithValue(ReadOnlySpan<char> argument)
    {
        Span<char> separators = stackalloc char[4];
        _parser.Style.OptionValueSeparators.CopyTo(separators, out var length);
        separators = separators[..length];

        var index = argument.IndexOfAny(separators);
        if (index is 0)
        {
            // 没有选项名，报告错误。
            Type = Cat.ErrorOption;
            return true;
        }
        if (index > 0)
        {
            // 带值的长选项。
            Type = Cat.LongOptionWithValue;
            Option = new OptionName(true, argument[..index]);
            Value = argument[(index + 1)..];
            return true;
        }
        // 不带值的长选项。
        Type = Cat.LongOption;
        Option = new OptionName(true, argument);
        return true;
    }

    private bool ParseShortOptionOrMultiShortOptions(ReadOnlySpan<char> argument)
    {
        Span<char> separators = stackalloc char[4];
        _parser.Style.OptionValueSeparators.CopyTo(separators, out var length);
        separators = separators[..length];

        var supportsCombination = _parser.SupportsShortOptionCombination;
        var supportsNoSeparator = _parser.SupportsShortOptionValueWithoutSeparator;

        var index = argument.IndexOfAny(separators);
        if (index is 0)
        {
            // 没有选项名，报告错误。
            Type = Cat.ErrorOption;
            return true;
        }
        if (index > 0)
        {
            // 带值的短选项。
            Type = Cat.ShortOptionWithValue;
            Option = new OptionName(false, argument[..index]);
            Value = argument[(index + 1)..];
            return true;
        }
        if (argument.Length is 1 || !supportsCombination)
        {
            // 单独的短选项。
            Type = Cat.ShortOption;
            Option = new OptionName(false, argument);
            return true;
        }
        // TODO 不可能三种都存在
        // -abc -a -b -c
        // -abc
        // -abc -a bc
        if (supportsNoSeparator)
        {
            // 不确定是多个短选项，还是一个无分隔符的带值短选项。
            Type = Cat.MultiShortOptionsOrShortOptionConcatWithValue;
            Option = new OptionName(false, argument);
            return true;
        }
        // 多个短选项。
        Type = Cat.MultiShortOptions;
        Option = new OptionName(false, argument);
        return true;
    }

    private bool ParseLongShortOptionOrLongShortOptionWithValue(ReadOnlySpan<char> argument)
    {
        Span<char> separators = stackalloc char[4];
        _parser.Style.OptionValueSeparators.CopyTo(separators, out var length);
        separators = separators[..length];

        var index = argument.IndexOfAny(separators);
        if (index is 0)
        {
            // 没有选项名，报告错误。
            Type = Cat.ErrorOption;
            return true;
        }
        if (index > 0)
        {
            // 带值的选项。
            Type = Cat.OptionWithValue;
            Option = new OptionName(true, argument[..index]);
            Value = argument[(index + 1)..];
            return true;
        }
        // 不带值的选项。
        Type = Cat.Option;
        Option = new OptionName(true, argument);
        return true;
    }

    /// <summary>
    /// 尝试解析布尔值。解析成功则视为选项值，失败则视为新的选项或位置参数。
    /// </summary>
    /// <returns></returns>
    private bool ParseBooleanOptionValueOrNewOptionOrPositionalArgument()
    {
        var argument = _argument;
        if (argument.Length <= 4 && (
                argument.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                argument is ""))
        {
            Type = Cat.OptionValue;
            Value = "true".AsSpan();
            return true;
        }
        if (argument.Length is > 0 and <= 5 && (
                argument.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("0", StringComparison.OrdinalIgnoreCase)))
        {
            Type = Cat.OptionValue;
            Value = "false".AsSpan();
            return true;
        }
        return ParseOptionOrPositionalArgument();
    }

    private bool ParseCollectionOptionValueOrNewOptionOrPositionalArgument()
    {
        var argument = _argument.AsSpan();
        if (argument.Length is 0 or 1)
        {
            // 空参数或单个字符（无法组成选项），视为选项值。
            Type = Cat.OptionValue;
            Value = argument;
            return true;
        }

        var optionPrefix = _parser.OptionPrefix;
        return optionPrefix switch
        {
            CommandOptionPrefix.DoubleDash => (argument[0], argument[1]) switch
            {
                ('-', '-') => ParseLongOptionOrLongOptionWithValue(argument[2..]),
                ('-', _) => ParseShortOptionOrMultiShortOptions(argument[1..]),
                _ => ParseOptionValue(argument),
            },
            CommandOptionPrefix.SingleDash => argument[0] switch
            {
                '-' => ParseLongShortOptionOrLongShortOptionWithValue(argument[1..]),
                _ => ParseOptionValue(argument),
            },
            CommandOptionPrefix.Slash => argument[0] switch
            {
                '/' => ParseLongShortOptionOrLongShortOptionWithValue(argument[1..]),
                _ => ParseOptionValue(argument),
            },
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private bool ParseOptionValue(ReadOnlySpan<char> argument)
    {
        Type = Cat.OptionValue;
        Value = argument;
        return true;
    }

    private bool ParsePositionalArgument(ReadOnlySpan<char> argument)
    {
        Type = Cat.PositionalArgument;
        Value = argument;
        return true;
    }
}
