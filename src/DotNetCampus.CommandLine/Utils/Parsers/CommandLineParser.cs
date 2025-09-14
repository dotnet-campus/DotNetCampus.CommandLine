using Cat = DotNetCampus.Cli.Utils.Parsers.CommandArgumentType;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 通用的命令行参数解析器。（此解析器不可解析 URL 类型的参数。）
/// </summary>
public readonly ref struct CommandLineParser
{
    private readonly CommandLine _commandLine;
    private readonly string _commandObjectName;

    /// <summary>
    /// 通用的命令行参数解析器。（此解析器不可解析 URL 类型的参数。）
    /// </summary>
    /// <param name="commandLine">要解析的命令行参数。</param>
    /// <param name="commandObjectName">正在解析此参数的命令对象的名称。</param>
    public CommandLineParser(CommandLine commandLine, string commandObjectName)
    {
        _commandLine = commandLine;
        _commandObjectName = commandObjectName;
        Style = commandLine.ParsingOptions.Style;
        NamingPolicy = Style.NamingPolicy;
        OptionPrefix = Style.OptionPrefix;
        CaseSensitive = Style.CaseSensitive;
        SupportsLongOption = Style.SupportsLongOption;
        SupportsShortOption = Style.SupportsShortOption;
        SupportsShortOptionCombination = Style.SupportsShortOptionCombination;
        SupportsShortOptionValueWithoutSeparator = Style.SupportsShortOptionValueWithoutSeparator;
    }

    internal CommandLineStyleDetails Style { get; }

    internal CommandNamingPolicy NamingPolicy { get; }

    internal CommandOptionPrefix OptionPrefix { get; }

    internal bool CaseSensitive { get; }

    internal bool SupportsLongOption { get; }

    internal bool SupportsShortOption { get; }

    internal bool SupportsShortOptionCombination { get; }

    internal bool SupportsShortOptionValueWithoutSeparator { get; }

    /// <summary>
    /// 要求源生成器判断某个索引处的参数是否为命令（主命令、子命令或多级子命令）。
    /// </summary>
    public required CheckIsCommandCallback IsCommand { get; init; }

    /// <summary>
    /// 要求源生成器匹配长名称，返回此长选项的值类型。
    /// </summary>
    public required LongOptionMatchingCallback MatchLongOption { get; init; }

    /// <summary>
    /// 要求源生成器匹配短名称，返回此短选项的值类型。
    /// </summary>
    public required ShortOptionMatchingCallback MatchShortOption { get; init; }

    public CommandLineParsingResult Parse()
    {
        var arguments = _commandLine.CommandLineArguments;
        var lastOptionName = new OptionName(false, []);
        var lastOptionType = OptionValueType.Normal;
        var lastType = Cat.Start;

        for (var index = 0; index < arguments.Count; index++)
        {
            // 跳过命令（主命令、子命令、多级子命令）。
            var argument = arguments[index];
            if (IsCommand(index))
            {
                continue;
            }

            // 解析当前参数。
            var part = new CommandArgumentPart(this, index, argument, lastType, lastOptionType);
            part.Parse();
            var (currentType, optionName, value) = part;

            // 更新状态。
            lastType = currentType;
            if (currentType is Cat.LongOption or Cat.ShortOption or Cat.Option)
            {
                // 如果当前是一个选项，则记录下来，供后面解析选项值时使用。
                lastOptionName = optionName;
                var optionType = currentType switch
                {
                    Cat.LongOption => MatchLongOption(optionName.Name, CaseSensitive, NamingPolicy),
                    Cat.ShortOption => MatchShortOption(optionName.Name, CaseSensitive),
                    _ => MatchLongOption(optionName.Name, CaseSensitive, NamingPolicy) switch
                    {
                        OptionValueType.NotExist => MatchShortOption(optionName.Name, CaseSensitive),
                        var t => t,
                    },
                };
                if (optionType is OptionValueType.NotExist)
                {
                    // 如果选项不存在，则报告错误。
                    return CommandLineParsingResult.OptionNotFound(_commandLine, index, _commandObjectName, optionName.Name);
                }
                lastOptionType = optionType;
            }
            else if (currentType is Cat.OptionValue && lastOptionType is OptionValueType.Collection)
            {
                // 如果当前是选项值，且上个选项是一个集合值，则继续使用上个选项。
            }
            else
            {
                // 其他情况，都需要清空上一个选项，避免误用。
                lastOptionName = new OptionName(false, []);
                lastOptionType = OptionValueType.Normal;
            }

            // 处理解析结果。
        }
    }
}

/// <summary>
/// 辅助解析命令行参数中的其中一个参数。
/// </summary>
internal ref struct CommandArgumentPart
{
    private readonly CommandLineParser _parser;
    private readonly int _index;
    private readonly string _argument;
    private readonly Cat _lastType;
    private readonly OptionValueType _lastOptionType;

    /// <summary>
    /// 辅助解析命令行参数中的其中一个参数。
    /// </summary>
    /// <param name="parser">正在使用的命令行参数解析器。</param>
    /// <param name="index">正在解析的参数的索引。</param>
    /// <param name="argument">要解析的参数。</param>
    /// <param name="lastType">上一个参数的类型，初始为 <see cref="CommandArgumentType.Start"/>。</param>
    /// <param name="lastOptionType">上一个参数的选项值类型，如果上一个参数不是选项，则为默认值。</param>
    public CommandArgumentPart(CommandLineParser parser, int index, string argument, Cat lastType, OptionValueType lastOptionType)
    {
        _parser = parser;
        _index = index;
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
    /// 开始解析这个参数，可通过解构获得解析结果。
    /// </summary>
    /// <returns>
    /// 返回值没有意义，纯粹为了使用 switch 表达式。
    /// </returns>
    public bool Parse() => _lastType switch
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
                OptionValueType.Collection => ParseCollectionOptionValueOrNewOptionOrPositionalArgument(),
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
            // 没有选项名，视为位置参数。
            Type = Cat.PositionalArgument;
            Value = argument;
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
            // 没有选项名，视为位置参数。
            Type = Cat.PositionalArgument;
            Value = argument;
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
            Value = "true";
            return true;
        }
        if (argument.Length is > 0 and <= 5 && (
                argument.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                argument.Equals("0", StringComparison.OrdinalIgnoreCase)))
        {
            Type = Cat.OptionValue;
            Value = "false";
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
