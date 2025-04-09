﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using dotnetCampus.Cli.Utils;

namespace dotnetCampus.Cli;

/// <summary>
/// 为应用程序提供统一的命令行参数解析功能。
/// </summary>
public record CommandLine : ICoreCommandRunnerBuilder
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

    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => new(this);

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
    /// 尝试将命令行参数转换为指定类型的实例。
    /// </summary>
    /// <typeparam name="T">要转换的类型。</typeparam>
    /// <returns>转换后的实例。</returns>
    public T As<T>() where T : class => CommandRunner.CreateInstance<T>(this);

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOption<T>(string optionName)
    {
        return LongOptionValues.TryGetValue(optionName, out var values)
            ? CommandLineValueConverter.ArgumentStringsToValue<T>(values)
            : default;
    }

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOption<T>(char shortOption)
    {
        return ShortOptionValues.TryGetValue(shortOption, out var values)
            ? CommandLineValueConverter.ArgumentStringsToValue<T>(values)
            : default;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="shortName">短名称选项。</param>
    /// <param name="longName">选项的名称。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOption<T>(char shortName, string longName)
    {
        // 优先使用短名称（因为长名称可能是根据属性名猜出来的）。
        if (ShortOptionValues.TryGetValue(shortName, out var shortValues))
        {
            return CommandLineValueConverter.ArgumentStringsToValue<T>(shortValues);
        }
        // 其次使用长名称。
        if (LongOptionValues.TryGetValue(longName, out var longValues))
        {
            return CommandLineValueConverter.ArgumentStringsToValue<T>(longValues);
        }
        // 最后使用默认值（表示没有传入此参数）。
        return default;
    }

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>位置参数的值。</returns>
    public T? GetValue<T>()
    {
        return PositionalArguments.Length <= 0
            ? default
            : CommandLineValueConverter.ArgumentStringsToValue<T>(PositionalArguments.Slice(0, 1));
    }

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <param name="index">获取指定索引处的参数值。</param>
    /// <param name="length">从索引处获取参数值的最长长度。当大于 1 时，会将这些值合并为一个字符串。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>位置参数的值。</returns>
    public T? GetValue<T>(int index, int length)
    {
        return index < 0 || index >= PositionalArguments.Length
            ? default
            : CommandLineValueConverter.ArgumentStringsToValue<T>(PositionalArguments.Slice(index, Math.Min(length, PositionalArguments.Length - index)));
    }

    /// <summary>
    /// 获取命令行参数中所有位置参数值的集合。
    /// </summary>
    /// <returns>命令行参数中位置参数值的集合。</returns>
    public ImmutableArray<string> GetValues()
    {
        return PositionalArguments;
    }
}
