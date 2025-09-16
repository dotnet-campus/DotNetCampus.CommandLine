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
        SupportsMultiCharShortOption = Style.SupportsMultiCharShortOption;
        SupportsShortOptionValueWithoutSeparator = Style.SupportsShortOptionValueWithoutSeparator;
        SupportsSpaceSeparatedOptionValue = Style.SupportsSpaceSeparatedOptionValue;
        SupportsSpaceSeparatedCollectionValues = Style.SupportsSpaceSeparatedCollectionValues;
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

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsMultiCharShortOption"/>
    internal bool SupportsMultiCharShortOption { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsShortOptionValueWithoutSeparator"/>
    internal bool SupportsShortOptionValueWithoutSeparator { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsSpaceSeparatedOptionValue"/>
    internal bool SupportsSpaceSeparatedOptionValue { get; }

    /// <inheritdoc cref="CommandLineStyleDetails.SupportsSpaceSeparatedCollectionValues"/>
    internal bool SupportsSpaceSeparatedCollectionValues { get; }

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
        var result = CommandLineParsingResult.Success;
        var arguments = _commandLine.CommandLineArguments;
        var currentOption = OptionValueMatch.NotMatch;
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
                        return CommandLineParsingResult.OptionalArgumentNotFound(_commandLine, index, _commandObjectName, optionName.Name);
                    }
                    if (optionMatch.ValueType is OptionValueType.Boolean)
                    {
                        // 布尔选项必须立即赋值，因为后面是不一定需要跟值的。
                        result = AssignOptionValue(optionMatch, []).Combine(result);
                    }
                    currentOption = optionMatch;
                    break;
                }
                case Cat.OptionValue:
                {
                    result = AssignOptionValue(currentOption, value).Combine(result);
                    if (currentOption.ValueType is not OptionValueType.List)
                    {
                        // 如果不是集合，那么此选项已经结束。
                        // 清空上一个选项，避免误用。
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
                    var optionMatch = state switch
                    {
                        Cat.LongOptionWithValue => MatchLongOption(optionName.Name, _caseSensitive, _namingPolicy),
                        Cat.ShortOptionWithValue => MatchShortOption(optionName.Name, _caseSensitive),
                        _ => MatchLongOption(optionName.Name, _caseSensitive, _namingPolicy) switch
                        {
                            { ValueType: OptionValueType.NotExist } => MatchShortOption(optionName.Name, _caseSensitive),
                            var t => t,
                        },
                    };
                    if (optionMatch.ValueType is OptionValueType.NotExist)
                    {
                        // 如果选项不存在，则报告错误。
                        return CommandLineParsingResult.OptionalArgumentNotFound(_commandLine, index, _commandObjectName, optionName.Name);
                    }
                    result = AssignOptionValue(optionMatch, value).Combine(result);
                    break;
                }
                case Cat.ErrorOption:
                {
                    // 如果当前参数疑似选项但解析失败，则报告错误。
                    return CommandLineParsingResult.OptionalArgumentParseError(_commandLine, index);
                }
                case Cat.MultiShortOptions:
                {
                    // 如果支持多字符短选项，则优先作为多字符短选项处理。
                    if (SupportsMultiCharShortOption)
                    {
                        var m = optionName.Name;
                        var optionMatch = MatchShortOption(m, _caseSensitive);
                        if (optionMatch.ValueType is not OptionValueType.NotExist)
                        {
                            // 是一个多字符短选项。
                            result = AssignOptionValue(optionMatch, []).Combine(result);
                            break;
                        }
                    }
                    // 随后，尝试逐个处理多个短选项。
                    for (var i = 0; i < optionName.Name.Length; i++)
                    {
                        var n = optionName.Name[i..(i + 1)];
                        var optionMatch = MatchShortOption(n, _caseSensitive);
                        if (optionMatch.ValueType is OptionValueType.NotExist)
                        {
                            // 如果选项不存在，则报告错误。
                            return CommandLineParsingResult.OptionalArgumentNotFound(_commandLine, index, _commandObjectName, n);
                        }
                        result = AssignOptionValue(optionMatch, []).Combine(result);
                    }
                    break;
                }
                case Cat.MultiShortOptionsOrShortOptionConcatWithValue:
                {
                    var name = optionName.Name;
                    // 如果支持多字符短选项，则优先作为多字符短选项处理。
                    if (SupportsMultiCharShortOption)
                    {
                        var optionMatch = MatchShortOption(name, _caseSensitive);
                        if (optionMatch.ValueType is not OptionValueType.NotExist)
                        {
                            // 是一个多字符短选项。
                            result = AssignOptionValue(optionMatch, []).Combine(result);
                            break;
                        }
                    }
                    // 随后，尝试解析为单个字符无分隔符带值的短选项。
                    {
                        var o = name[..1];
                        var v = name[1..];
                        var optionMatch = MatchShortOption(o, _caseSensitive);
                        if (optionMatch.ValueType is OptionValueType.NotExist)
                        {
                            // 如果选项不存在，则报告错误。
                            return CommandLineParsingResult.OptionalArgumentNotFound(_commandLine, index, _commandObjectName, o);
                        }
                        result = AssignOptionValue(optionMatch, v).Combine(result);
                    }
                    break;
                }
                // 其他状态要么已经处理过了，要不还未处理，要么不需要处理，所以不需要做任何事情。
            }
        }

        return result;
    }

    /// <summary>
    /// 配合源生成器生成的匹配结果，将选项值赋值给指定索引处的属性。
    /// </summary>
    /// <param name="match">源生成器生成的匹配结果。</param>
    /// <param name="value">选项值。</param>
    /// <returns>命令行参数解析结果。</returns>
    private CommandLineParsingResult AssignOptionValue(OptionValueMatch match, ReadOnlySpan<char> value)
    {
        var result = CommandLineParsingResult.Success;
        if (match.ValueType is OptionValueType.List)
        {
            Span<char> separators = stackalloc char[4];
            Style.CollectionValueSeparators.CopyTo(separators, out var length);
            separators = separators[..length];

            var start = 0;
            while (start < value.Length)
            {
                var index = value[start..].IndexOfAny(separators);
                if (index < 0)
                {
                    // 剩余部分没有分隔符，全部作为一个值。
                    AssignPropertyValue(match.PropertyName, match.PropertyIndex, [], value[start..]);
                    break;
                }
                if (index > 0)
                {
                    // 截取分隔符前的部分作为一个值。
                    AssignPropertyValue(match.PropertyName, match.PropertyIndex, [], value.Slice(start, index));
                }
                // 跳过分隔符，继续处理后续部分。
                start += index + 1;
            }
        }
        else if (match.ValueType is OptionValueType.Dictionary)
        {
            Span<char> separators = stackalloc char[4];
            Style.CollectionValueSeparators.CopyTo(separators, out var length);
            separators = separators[..length];

            var start = 0;
            while (start < value.Length)
            {
                var index = value[start..].IndexOfAny(separators);
                if (index < 0)
                {
                    // 剩余部分没有分隔符，全部作为一个值。
                    result = SplitKeyValue(value[start..], out var k, out var v).Combine(result);
                    AssignPropertyValue(match.PropertyName, match.PropertyIndex, k, v);
                    break;
                }
                if (index > 0)
                {
                    // 截取分隔符前的部分作为一个值。
                    result = SplitKeyValue(value.Slice(start, index), out var k, out var v).Combine(result);
                    AssignPropertyValue(match.PropertyName, match.PropertyIndex, k, v);
                }
                // 跳过分隔符，继续处理后续部分。
                start += index + 1;
            }
        }
        else if (match.ValueType is OptionValueType.Boolean)
        {
            var booleanValue = ParseBoolean(value);
            if (booleanValue is null)
            {
                result = CommandLineParsingResult.BooleanValueParseError(_commandLine, value).Combine(result);
            }
            ReadOnlySpan<char> finalValue = booleanValue switch
            {
                // 用户输入明确指定为 true。
                true => ['1'],
                // 用户输入明确指定为 false。
                false => ['0'],
                // 无法识别。
                _ => ['0'],
            };
            AssignPropertyValue(match.PropertyName, match.PropertyIndex, [], finalValue);
        }
        else
        {
            // 普通值。
            AssignPropertyValue(match.PropertyName, match.PropertyIndex, [], value);
        }
        return result;
    }

    private void AssignPositionalArgumentValue(PositionalArgumentValueMatch match, ReadOnlySpan<char> value)
    {
        AssignPropertyValue(match.PropertyName, match.PropertyIndex, [], value);
    }

    private CommandLineParsingResult SplitKeyValue(ReadOnlySpan<char> item,
        out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        // 截至目前，所有的字典类型都使用 key=value 形式，如果将来新增的风格有其他符号，我们再用一样的分隔符方式来配置。
        var index = item.IndexOf('=');
        if (index < 0)
        {
            key = item;
            value = [];
            return CommandLineParsingResult.DictionaryValueParseError(_commandLine, item);
        }
        key = item[..index];
        value = item[(index + 1)..];
        return CommandLineParsingResult.Success;
    }

    internal static bool? ParseBoolean(ReadOnlySpan<char> value)
    {
        if (value.Length <= 4 && (
                value.Equals("true".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Equals("1".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Length is 0))
        {
            return true;
        }
        if (value.Length is > 0 and <= 5 && (
                value.Equals("false".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Equals("no".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Equals("off".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                value.Equals("0".AsSpan(), StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }
        return null;
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
            // 上一个是起点或命令，后面只能是新的选项或位置参数。
            Cat.Start or Cat.Command => ParseOptionOrPositionalArgument(),
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
                OptionValueType.List or OptionValueType.Dictionary => ParseCollectionOptionValueOrNewOptionOrPositionalArgument(),
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
        if (argument.Length is 0)
        {
            // 空字符串，视为位置参数。
            Type = Cat.PositionalArgument;
            Value = argument;
            return true;
        }
        if (argument.Length is 1)
        {
            // 单个字符，确定一下是否是选项分隔符，如果是则要报错。
            Span<char> separators = stackalloc char[4];
            _parser.Style.OptionValueSeparators.CopyTo(separators, out var length);
            separators = separators[..length];
            if (argument.IndexOfAny(separators) >= 0)
            {
                // 仅包含分隔符，视为错误选项。
                Type = Cat.ErrorOption;
                return true;
            }
            // 单个字符（无法组成选项），视为位置参数。
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

        var index = argument.IndexOfAny(separators);
        if (index is 0)
        {
            // 没有选项名，报告错误。
            Type = Cat.ErrorOption;
            return true;
        }
        if (argument.Length is 1)
        {
            // 单独的短选项。
            Type = Cat.ShortOption;
            Option = new OptionName(false, argument);
            return true;
        }
        if (index > 0)
        {
            if (index is 1 || _parser.SupportsMultiCharShortOption)
            {
                // 带值的短选项。
                Type = Cat.ShortOptionWithValue;
                Option = new OptionName(false, argument[..index]);
                Value = argument[(index + 1)..];
                return true;
            }
            // 分隔符出现在第二个字符之后，但不支持多字符短选项，报告错误。
            Type = Cat.ErrorOption;
            return true;
        }

        // 对于不带值的短选项，存在以下三种情况：
        // 1. -abc 表示 -a -b -c 三个布尔短选项。
        // 2. -abc 表示 -a 选项的值为 bc。
        // 3. -abc 表示一个名为 abc 的多字符短选项。
        // 目前不存在任何一种命令行风格同时支持上述三种情况，所以我们可以消除一些不确定性。
        var supportsCombination = _parser.SupportsShortOptionCombination;
        var supportsNoSeparator = _parser.SupportsShortOptionValueWithoutSeparator;
        switch (supportsCombination, supportsNoSeparator)
        {
            // 支持短选项组合，也支持无分隔符带值的短选项。（上述 1 和 2，从实际考虑消除了 3）
            case (true, true):
                Type = Cat.MultiShortOptionsOrShortOptionConcatWithValue;
                Option = new OptionName(false, argument);
                return true;
            case (true, false):
                // 支持短选项组合，不支持无分隔符带值的短选项。（上述 1 和 3）
                Type = Cat.MultiShortOptions;
                Option = new OptionName(false, argument);
                return true;
            case (false, true):
                // 不支持短选项组合，但支持无分隔符带值的短选项。（上述 2，从实际考虑消除了 3）
                Type = Cat.ShortOptionWithValue;
                Option = new OptionName(false, argument[..1]);
                Value = argument[1..];
                return true;
            case (false, false):
                // 既不支持短选项组合，也不支持无分隔符带值的短选项。（上述 3）
                Type = Cat.ShortOption;
                Option = new OptionName(false, argument);
                return true;
        }
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
        var booleanValue = CommandLineParser.ParseBoolean(argument.AsSpan());
        if (booleanValue is true)
        {
            Type = Cat.OptionValue;
            Value = "1".AsSpan();
            return true;
        }
        if (booleanValue is false)
        {
            Type = Cat.OptionValue;
            Value = "0".AsSpan();
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
