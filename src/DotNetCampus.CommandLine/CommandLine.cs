using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Utils;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli;

/// <summary>
/// 为应用程序提供统一的命令行参数解析功能。
/// </summary>
public class CommandLine : ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 获取此命令行解析类型所关联的命令行参数。
    /// </summary>
    public IReadOnlyList<string> CommandLineArguments { get; }

    /// <summary>
    /// 获取解析此命令行时所使用的各种选项。
    /// </summary>
    internal CommandLineParsingOptions ParsingOptions { get; }

    /// <summary>
    /// 在特定的属性不指定时，默认应使用的大小写敏感性。
    /// </summary>
    public bool DefaultCaseSensitive { get; }

    /// <summary>
    /// 获取命令行参数中猜测的多级命令名称。
    /// 请注意，此字符串中可能包含空格，表示多级命令名称。也可能比预期的更长，包含后续的一部分位置参数，因为暂时还无法确定那些位置参数是否是命令名称。
    /// </summary>
    /// <remarks>
    /// <code>
    /// # 对于以下命令：
    /// do something --option value
    /// # 本属性的值为 "do something"。
    /// # 对于以下命令：
    /// do something /var/file --option value
    /// # 本属性的值为 "do something"（因为 /var/file 可以提前判断出来不可能是命令）
    /// # 可能存在三种情况：
    /// # 1. do 和 something 都是位置参数。
    /// # 2. do 是命令，something 是位置参数。
    /// # 3. do 和 something 都是命令。
    /// </code>
    /// 此属性保存这个 something 的值，待后续决定使用处理器时，根据处理器是否要求有命令来决定这个词是否是位置参数。<br/>
    /// 另外，**特别强调**，此属性的值可能是命名变体，例如命令行传入 DoSomething 时，此属性则是 Do-Something。
    /// </remarks>
    internal string PossibleCommandNames { get; }

    /// <summary>
    /// 如果此命令行是从 Web 请求的 URL 中解析出来的，则此属性保存 URL 的 Scheme 部分。
    /// </summary>
    private string? MatchedUrlScheme { get; }

    /// <summary>
    /// 适用于选项的多值处理方式。
    /// </summary>
    private MultiValueHandling OptionMultiValueHandling { get; }

    /// <summary>
    /// 适用于位置参数的多值处理方式。
    /// </summary>
    private MultiValueHandling PositionalArgumentsMultiValueHandling { get; }

    /// <summary>
    /// 从命令行中解析出来的长名称选项。始终大小写敏感。
    /// </summary>
    private OptionDictionary LongOptionValuesDefault { get; }

    /// <summary>
    /// 从命令行中解析出来的长名称选项。始终大小写敏感。
    /// </summary>
    private OptionDictionary LongOptionValuesCaseSensitive { get; }

    /// <summary>
    /// 从命令行中解析出来的长名称选项。始终大小写不敏感。
    /// </summary>
    private OptionDictionary LongOptionValuesIgnoreCase { get; }

    /// <summary>
    /// 从命令行中解析出来的短名称选项。始终大小写敏感。
    /// </summary>
    private OptionDictionary ShortOptionValuesDefault { get; }

    /// <summary>
    /// 从命令行中解析出来的短名称选项。始终大小写敏感。
    /// </summary>
    private OptionDictionary ShortOptionValuesCaseSensitive { get; }

    /// <summary>
    /// 从命令行中解析出来的短名称选项。始终大小写不敏感。
    /// </summary>
    private OptionDictionary ShortOptionValuesIgnoreCase { get; }

    /// <summary>
    /// 从命令行中解析出来的位置参数。
    /// </summary>
    /// <remarks>
    /// 注意，位置参数的前几个值可能是命令名称；这取决于 <see cref="PossibleCommandNames"/> 和实际处理器的命令。
    /// <code>
    /// # 对于以下命令：
    /// do something --option value
    /// # 可能存在三种情况：
    /// # 1. do 和 something 都是位置参数。
    /// # 2. do 是命令，something 是位置参数。
    /// # 3. do 和 something 都是命令。
    /// </code>
    /// 如果处理器决定将 something 作为命令名称，那么当需要取出位置参数时，此属性的第一个值需要排除。
    /// </remarks>
    private ReadOnlyListRange<string> PositionalArguments { get; }

    private CommandLine()
    {
        var options = OptionDictionary.Empty;
        var arguments = new ReadOnlyListRange<string>();
        CommandLineArguments = arguments;
        ParsingOptions = CommandLineParsingOptions.Flexible;
        DefaultCaseSensitive = false;
        PossibleCommandNames = "";
        MatchedUrlScheme = null;
        OptionMultiValueHandling = MultiValueHandling.First;
        PositionalArgumentsMultiValueHandling = MultiValueHandling.First;
        LongOptionValuesCaseSensitive = options;
        LongOptionValuesIgnoreCase = options;
        LongOptionValuesDefault = options;
        ShortOptionValuesCaseSensitive = options;
        ShortOptionValuesIgnoreCase = options;
        ShortOptionValuesDefault = options;
        PositionalArguments = arguments;
    }

    private CommandLine(IReadOnlyList<string> arguments, CommandLineParsingOptions? parsingOptions = null)
    {
        CommandLineArguments = arguments;
        ParsingOptions = parsingOptions ?? CommandLineParsingOptions.Flexible;
        DefaultCaseSensitive = parsingOptions?.CaseSensitive ?? false;
        (MatchedUrlScheme, var result) = CommandLineConverter.ParseCommandLineArguments(arguments, parsingOptions);
        PossibleCommandNames = result.PossibleCommandNames;
        OptionMultiValueHandling = MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.Last;
        PositionalArgumentsMultiValueHandling = MatchedUrlScheme is null ? MultiValueHandling.SpaceAll : MultiValueHandling.SlashAll;
        LongOptionValuesCaseSensitive = result.LongOptions.ToOptionLookup(true);
        LongOptionValuesIgnoreCase = result.LongOptions.ToOptionLookup(false);
        LongOptionValuesDefault = DefaultCaseSensitive ? LongOptionValuesCaseSensitive : LongOptionValuesIgnoreCase;
        ShortOptionValuesCaseSensitive = result.ShortOptions.ToOptionLookup(true);
        ShortOptionValuesIgnoreCase = result.ShortOptions.ToOptionLookup(false);
        ShortOptionValuesDefault = DefaultCaseSensitive ? ShortOptionValuesCaseSensitive : ShortOptionValuesIgnoreCase;
        PositionalArguments = result.Arguments;
    }

    /// <summary>
    /// 解析命令行参数，并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    /// <param name="parsingOptions">以此方式解析命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    [Pure]
    public static CommandLine Parse(IReadOnlyList<string> args, CommandLineParsingOptions? parsingOptions = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(args);
#else
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }
#endif
        return args.Count is 0
            ? new CommandLine()
            : new CommandLine(args, parsingOptions);
    }

    /// <summary>
    /// 解析一整行命令（所有参数被放在了同一个字符串中），并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="singleLineCommandLineArgs">一整行命令。</param>
    /// <param name="parsingOptions">以此方式解析命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    [Pure]
    public static CommandLine Parse(string singleLineCommandLineArgs, CommandLineParsingOptions? parsingOptions = null)
    {
        var args = CommandLineConverter.SingleLineCommandLineArgsToArrayCommandLineArgs(singleLineCommandLineArgs);
        return new CommandLine(args, parsingOptions);
    }

    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => new(this);

    /// <summary>
    /// 尝试将命令行参数转换为指定类型的实例。
    /// </summary>
    /// <typeparam name="T">要转换的类型。</typeparam>
    /// <returns>转换后的实例。</returns>
    [Pure]
    public T As<T>() where T : class => CommandRunner.CreateInstance<T>(this);

    /// <summary>
    /// 尝试将命令行参数转换为指定类型的实例。
    /// </summary>
    /// <param name="creator">由拦截器传入的命令处理器创建方法。</param>
    /// <typeparam name="T">要转换的类型。</typeparam>
    /// <returns>转换后的实例。</returns>
    [Pure, EditorBrowsable(EditorBrowsableState.Never)]
    public T As<T>(CommandObjectCreator creator) where T : class => CommandRunner.CreateInstance<T>(this, creator);

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetOption(char shortOption) => GetShortOption(shortOption.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetShortOption(string shortOption)
    {
        return ShortOptionValuesDefault.TryGetValue(shortOption, out var defaultValues)
            ? new CommandLinePropertyValue(defaultValues, OptionMultiValueHandling)
            : null;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetOption(string optionName)
    {
        return LongOptionValuesDefault.TryGetValue(optionName, out var defaultValues)
            ? new CommandLinePropertyValue(defaultValues, OptionMultiValueHandling)
            : null;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="shortName">短名称选项。</param>
    /// <param name="longName">选项的名称。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetOption(char shortName, string longName) =>
        // 优先使用短名称（因为长名称可能是根据属性名猜出来的）。
        GetOption(shortName)
        // 其次使用长名称。
        ?? GetOption(longName);

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetOption(char optionName, bool caseSensitive) =>
        GetShortOption(optionName.ToString(CultureInfo.InvariantCulture), caseSensitive);

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetShortOption(string shortOption, bool caseSensitive)
    {
        var optionValues = caseSensitive
            ? ShortOptionValuesCaseSensitive
            : ShortOptionValuesIgnoreCase;
        return optionValues.TryGetValue(shortOption, out var defaultValues)
            ? new CommandLinePropertyValue(defaultValues, OptionMultiValueHandling)
            : null;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetOption(string optionName, bool caseSensitive)
    {
        var optionValues = caseSensitive
            ? LongOptionValuesCaseSensitive
            : LongOptionValuesIgnoreCase;
        return optionValues.TryGetValue(optionName, out var defaultValues)
            ? new CommandLinePropertyValue(defaultValues, OptionMultiValueHandling)
            : null;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="shortName">短名称选项。</param>
    /// <param name="longName">选项的名称。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    [Pure]
    public CommandLinePropertyValue? GetOption(char shortName, string longName, bool caseSensitive) =>
        // 优先使用短名称（因为长名称可能是根据属性名猜出来的）。
        GetOption(shortName, caseSensitive)
        // 其次使用长名称。
        ?? GetOption(longName, caseSensitive);

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <param name="index">获取指定索引处的参数值。</param>
    /// <param name="length">从索引处获取参数值的最长长度。当大于 1 时，会将这些值合并为一个字符串。</param>
    /// <param name="commandNames">因为子命令会影响到位置参数的序号，所以如果存在命令和子命令，则需要传入所有多级命令共同组成的字符串。</param>
    /// <returns>位置参数的值。</returns>
    [Pure]
    public CommandLinePropertyValue? GetPositionalArgument(int index, int length, string? commandNames = null)
    {
        var commandLevel = GetCommandLevel(commandNames);
        var positionalArgumentsStartIndex = Math.Max(0, commandLevel);
        positionalArgumentsStartIndex = Math.Min(positionalArgumentsStartIndex, commandLevel);
        var realIndex = index + positionalArgumentsStartIndex;
        return realIndex < 0 || realIndex >= PositionalArguments.Count
            ? null
            : new CommandLinePropertyValue(
                PositionalArguments.Slice(realIndex,
                    Math.Min(length, PositionalArguments.Count - realIndex)), PositionalArgumentsMultiValueHandling);
    }

    /// <summary>
    /// 获取命令行参数中所有位置参数值的集合。
    /// </summary>
    /// <param name="commandNames">因为子命令会影响到位置参数的序号，所以如果存在命令和子命令，则需要传入所有多级命令共同组成的字符串。</param>
    /// <returns>命令行参数中位置参数值的集合。</returns>
    [Pure]
    public IReadOnlyList<string> GetPositionalArguments(string? commandNames = null)
    {
        var commandLevel = GetCommandLevel(commandNames);
        var positionalArgumentsStartIndex = Math.Max(0, commandLevel);
        positionalArgumentsStartIndex = Math.Min(positionalArgumentsStartIndex, commandLevel);
        return PositionalArguments.Slice(positionalArgumentsStartIndex, PositionalArguments.Count - 1);
    }

    /// <summary>
    /// 根据某个特定的命令名称字符串，获取此字符串中包含了多少级命令。
    /// </summary>
    /// <param name="commandNames">命令名称字符串。</param>
    /// <returns>命令的层级数。</returns>
    private int GetCommandLevel(string? commandNames)
    {
        var possibleCommandNames = PossibleCommandNames;

        // 如果没有命令，则不需要排除任何位置参数。
        if (string.IsNullOrEmpty(commandNames) || string.IsNullOrEmpty(possibleCommandNames))
        {
            return 0;
        }
#if !NETCOREAPP3_1_OR_GREATER
        if (commandNames is null)
        {
            return 0;
        }
#endif
        if (commandNames.Length > possibleCommandNames.Length)
        {
            return 0;
        }
        if (!possibleCommandNames.StartsWith(commandNames, DefaultCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }
        // 计算 possibleCommandNames 中有多少个空格。
        var commandLevel = 1;
        for (var i = 0; i < commandNames.Length; i++)
        {
            if (possibleCommandNames[i] == ' ')
            {
                commandLevel++;
            }
        }
        return commandLevel;
    }

    /// <summary>
    /// 输出传入的命令行参数字符串。
    /// </summary>
    /// <returns>传入的命令行参数字符串。</returns>
    [Pure]
    public override string ToString()
    {
        return MatchedUrlScheme is { } scheme
            ? $"{scheme}://{string.Join("/", PositionalArguments)}?{string.Join("&", LongOptionValuesCaseSensitive.Select(x => $"{x.Key}={string.Join("&", x.Value)}"))}"
            : string.Join(" ", CommandLineArguments.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
    }
}
