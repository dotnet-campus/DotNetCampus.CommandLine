using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using dotnetCampus.Cli.Utils;

namespace dotnetCampus.Cli;

/// <summary>
/// 为应用程序提供统一的命令行参数解析功能。
/// </summary>
public record CommandLine
{
    /// <summary>
    /// 获取此命令行解析类型所关联的命令行参数。
    /// </summary>
    public ImmutableArray<string> CommandLineArguments { get; }

    /// <summary>
    /// 获取命令行参数中猜测的谓词名称。
    /// </summary>
    public string? GuessedVerbName { get; }

    /// <summary>
    /// 从命令行中解析出来的长名称选项。
    /// </summary>
    public ImmutableDictionary<string, ImmutableArray<string>> LongOptionValues { get; }

    /// <summary>
    /// 从命令行中解析出来的短名称选项。
    /// </summary>
    public ImmutableDictionary<char, ImmutableArray<string>> ShortOptionValues { get; }

    /// <summary>
    /// 从命令行中解析出来的位置参数。
    /// </summary>
    public ImmutableArray<string> PositionalArguments { get; }

    private CommandLine(ImmutableArray<string> arguments, CommandLineParsingOptions? parsingOptions = null)
    {
        CommandLineArguments = arguments;
        (GuessedVerbName, LongOptionValues, ShortOptionValues, PositionalArguments) = CommandLineConverter.ParseCommandLineArguments(arguments, parsingOptions);
    }

    /// <summary>
    /// 解析命令行参数，并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    /// <param name="parsingOptions">以此方式解析命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    public static CommandLine Parse(IReadOnlyList<string> args, CommandLineParsingOptions? parsingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(args);
        return new CommandLine([..args], parsingOptions);
    }

    /// <summary>
    /// 解析一整行命令（所有参数被放在了同一个字符串中），并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="singleLineCommandLineArgs">一整行命令。</param>
    /// <param name="parsingOptions">以此方式解析命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    public static CommandLine Parse(string singleLineCommandLineArgs, CommandLineParsingOptions? parsingOptions = null)
    {
        var args = CommandLineConverter.SingleLineCommandLineArgsToArrayCommandLineArgs(singleLineCommandLineArgs);
        return new CommandLine(args, parsingOptions);
    }

    /// <summary>
    /// 尝试获取命令行参数中猜测的谓词名称。
    /// </summary>
    /// <param name="verbName">谓词名称。</param>
    /// <returns>如果命令行参数中包含谓词名称，则返回 <see langword="true" />；否则返回 <see langword="false" />。</returns>
    internal bool TryGuessVerbName([NotNullWhen(true)] out string? verbName)
    {
        verbName = GuessedVerbName;
        return verbName is not null;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。</returns>
    public T? GetOption<T>(string optionName) where T : notnull
    {
        return TryGetOption<T>(optionName, out var result) ? result : default;
    }

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。</returns>
    public T? GetOption<T>(char shortOption) where T : notnull
    {
        return TryGetOption<T>(shortOption, out var result) ? result : default;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <param name="value">返回选项的值。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>如果选项存在，则返回 true；否则返回 false。</returns>
    public bool TryGetOption<T>(string optionName, [NotNullWhen(true)] out T? value) where T : notnull
    {
        if (LongOptionValues.TryGetValue(optionName, out var values))
        {
            value = CommandLineValueConverter.OptionStringsToValue<T>(values);
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <param name="value">返回选项的值。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>如果选项存在，则返回 true；否则返回 false。</returns>
    public bool TryGetOption<T>(char shortOption, [NotNullWhen(true)] out T? value) where T : notnull
    {
        if (ShortOptionValues.TryGetValue(shortOption, out var values))
        {
            value = CommandLineValueConverter.OptionStringsToValue<T>(values);
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <param name="mergeIfSpaceOutOfQuoteExists">如果选项值中包含空格（但用户忘记将其放入引号中），是否将其合并为一个值。</param>
    /// <returns>命令行参数中位置参数的值。</returns>
    public string? GetPositionalArgument(bool mergeIfSpaceOutOfQuoteExists = false)
    {
        return (Values: PositionalArguments, mergeIfSpaceOutOfQuoteExists) switch
        {
            ({ Length: 0 }, _) => null,
            ({ Length: 1 }, _) => PositionalArguments[0],
            (_, true) => string.Join(", ", PositionalArguments),
            _ => PositionalArguments[0],
        };
    }

    /// <summary>
    /// 获取命令行参数中指定位置的位置参数的值。
    /// </summary>
    /// <param name="position">位置参数的位置。</param>
    /// <returns>命令行参数中位置参数的值。如果指定位置处没有参数，则返回 <see langword="null" />。</returns>
    public string? GetPositionalArgument(int position)
    {
        if (position < 0 || position >= PositionalArguments.Length)
        {
            return null;
        }

        return PositionalArguments[position];
    }

    /// <summary>
    /// 获取命令行参数中的位置参数的值的集合。
    /// </summary>
    /// <returns>命令行参数中位置参数的值的集合。</returns>
    public ImmutableArray<string> GetPositionalArguments()
    {
        return PositionalArguments;
    }
}
