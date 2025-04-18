﻿using System.Collections.Immutable;
using DotNetCampus.Cli.Utils;

namespace DotNetCampus.Cli;

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
    /// 在特定的属性不指定时，默认应使用的大小写敏感性。
    /// </summary>
    public bool DefaultCaseSensitive { get; }

    /// <summary>
    /// 获取命令行参数中猜测的谓词名称。
    /// </summary>
    /// <remarks>
    /// <code>
    /// # 对于以下命令：
    /// do something --option value
    /// # 可能存在两种情况：
    /// # 1. do 是位置参数，something 是谓词。
    /// # 2. do 是谓词，something 是位置参数。
    /// </code>
    /// 此属性保存这个 something 的值，待后续决定使用处理器时，根据处理器是否要求有谓词来决定这个词是否是位置参数。
    /// </remarks>
    public string? GuessedVerbName { get; }

    /// <summary>
    /// 如果此命令行是从 Web 请求的 URL 中解析出来的，则此属性保存 URL 的 Scheme 部分。
    /// </summary>
    public string? MatchedUrlScheme { get; }

    /// <summary>
    /// 从命令行中解析出来的长名称选项。根据 <see cref="DefaultCaseSensitive"/> 的值决定是否大小写敏感。
    /// </summary>
    public ImmutableDictionary<string, ImmutableArray<string>> LongOptionValues { get; }

    /// <summary>
    /// 从命令行中解析出来的长名称选项。始终大小写敏感。
    /// </summary>
    public ImmutableDictionary<string, ImmutableArray<string>> RawLongOptionValues { get; }

    /// <summary>
    /// 从命令行中解析出来的短名称选项。根据 <see cref="DefaultCaseSensitive"/> 的值决定是否大小写敏感。
    /// </summary>
    public ImmutableDictionary<char, ImmutableArray<string>> ShortOptionValues { get; }

    /// <summary>
    /// 从命令行中解析出来的短名称选项。始终大小写敏感。
    /// </summary>
    public ImmutableDictionary<char, ImmutableArray<string>> RawShortOptionValues { get; }

    /// <summary>
    /// 从命令行中解析出来的位置参数。
    /// </summary>
    /// <remarks>
    /// 注意，位置参数的第一个值可能是谓词名称；这取决于 <see cref="GuessedVerbName"/> 和实际处理器的谓词。
    /// <code>
    /// # 对于以下命令：
    /// do something --option value
    /// # 可能存在两种情况：
    /// # 1. do 是位置参数，something 是谓词。
    /// # 2. do 是谓词，something 是位置参数。
    /// </code>
    /// 如果处理器决定将 something 作为谓词，那么当需要取出位置参数时，此属性的第一个值需要排除。
    /// </remarks>
    public ImmutableArray<string> PositionalArguments { get; }

    private CommandLine(ImmutableArray<string> arguments, CommandLineParsingOptions? parsingOptions = null)
    {
        DefaultCaseSensitive = parsingOptions?.CaseSensitive ?? false;
        CommandLineArguments = arguments;
        (MatchedUrlScheme, var result) = CommandLineConverter.ParseCommandLineArguments(arguments, parsingOptions);
        (GuessedVerbName, RawLongOptionValues, RawShortOptionValues, PositionalArguments) = result;
        if (DefaultCaseSensitive)
        {
            LongOptionValues = RawLongOptionValues;
            ShortOptionValues = RawShortOptionValues;
        }
        else
        {
            LongOptionValues = RawLongOptionValues
                .DistinctBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToImmutableDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value,
                    StringComparer.OrdinalIgnoreCase);
            ShortOptionValues = RawShortOptionValues
                .DistinctBy(x => x.Key)
                .ToImmutableDictionary(
                    kvp => char.ToLowerInvariant(kvp.Key),
                    kvp => kvp.Value);
        }
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
    /// 尝试将命令行参数转换为指定类型的实例。
    /// </summary>
    /// <typeparam name="T">要转换的类型。</typeparam>
    /// <returns>转换后的实例。</returns>
    public T As<T>() where T : class => CommandRunner.CreateInstance<T>(this);

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOption<T>(string optionName, bool? caseSensitive = null) where T : class
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.Last);

        // 默认的大小写敏感性，使用默认字典匹配。
        if (caseSensitive is null || caseSensitive == DefaultCaseSensitive)
        {
            return LongOptionValues.TryGetValue(optionName, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：敏感。
        if (caseSensitive is true)
        {
            return RawLongOptionValues.TryGetValue(optionName, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：不敏感。
        foreach (var pair in RawLongOptionValues)
        {
            if (string.Equals(pair.Key, optionName, StringComparison.OrdinalIgnoreCase))
            {
                return CommandLineValueConverter.ArgumentStringsToValue<T>(pair.Value, context);
            }
        }

        // 没有找到。
        return null;
    }

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOption<T>(char shortOption, bool? caseSensitive = null) where T : class
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.Last);

        // 默认的大小写敏感性，使用默认字典匹配。
        if (caseSensitive is null || caseSensitive == DefaultCaseSensitive)
        {
            return ShortOptionValues.TryGetValue(shortOption, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：敏感。
        if (caseSensitive is true)
        {
            return RawShortOptionValues.TryGetValue(shortOption, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：不敏感。
        foreach (var pair in RawShortOptionValues)
        {
            if (pair.Key == char.ToLowerInvariant(shortOption))
            {
                return CommandLineValueConverter.ArgumentStringsToValue<T>(pair.Value, context);
            }
        }

        // 没有找到。
        return null;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="shortName">短名称选项。</param>
    /// <param name="longName">选项的名称。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOption<T>(char shortName, string longName, bool? caseSensitive = null) where T : class =>
        // 优先使用短名称（因为长名称可能是根据属性名猜出来的）。
        GetOption<T>(shortName, caseSensitive)
        // 其次使用长名称。
        ?? GetOption<T>(longName, caseSensitive);

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="optionName">选项的名称。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOptionValue<T>(string optionName, bool? caseSensitive = null) where T : struct
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.Last);

        // 默认的大小写敏感性，使用默认字典匹配。
        if (caseSensitive is null || caseSensitive == DefaultCaseSensitive)
        {
            return LongOptionValues.TryGetValue(optionName, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：敏感。
        if (caseSensitive is true)
        {
            return RawLongOptionValues.TryGetValue(optionName, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：不敏感。
        foreach (var pair in RawLongOptionValues)
        {
            if (string.Equals(pair.Key, optionName, StringComparison.OrdinalIgnoreCase))
            {
                return CommandLineValueConverter.ArgumentStringsToValue<T>(pair.Value, context);
            }
        }

        // 没有找到。
        return null;
    }

    /// <summary>
    /// 获取命令行参数中指定短名称的选项的值。
    /// </summary>
    /// <param name="shortOption">短名称选项。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOptionValue<T>(char shortOption, bool? caseSensitive = null) where T : struct
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.Last);

        // 默认的大小写敏感性，使用默认字典匹配。
        if (caseSensitive is null || caseSensitive == DefaultCaseSensitive)
        {
            return ShortOptionValues.TryGetValue(shortOption, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：敏感。
        if (caseSensitive is true)
        {
            return RawShortOptionValues.TryGetValue(shortOption, out var values)
                ? CommandLineValueConverter.ArgumentStringsToValue<T>(values, context)
                : null;
        }

        // 与默认不同的大小写敏感性：不敏感。
        foreach (var pair in RawShortOptionValues)
        {
            if (pair.Key == char.ToLowerInvariant(shortOption))
            {
                return CommandLineValueConverter.ArgumentStringsToValue<T>(pair.Value, context);
            }
        }

        // 没有找到。
        return null;
    }

    /// <summary>
    /// 获取命令行参数中指定名称的选项的值。
    /// </summary>
    /// <param name="shortName">短名称选项。</param>
    /// <param name="longName">选项的名称。</param>
    /// <param name="caseSensitive">单独为此选项设置的大小写敏感性。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>返回选项的值。当命令行未传入此参数时返回 <see langword="null" />。</returns>
    public T? GetOptionValue<T>(char shortName, string longName, bool? caseSensitive = null) where T : struct =>
        // 优先使用短名称（因为长名称可能是根据属性名猜出来的）。
        GetOptionValue<T>(shortName, caseSensitive)
        // 其次使用长名称。
        ?? GetOptionValue<T>(longName, caseSensitive);

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <param name="verbName">因为是否存在谓词会影响到位置参数的序号，所以如果有谓词名称，则需要传入。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>位置参数的值。</returns>
    public T? GetPositionalArgument<T>(string? verbName = null) where T : class
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.SlashAll);
        var shouldSkipVerb = verbName is not null && GuessedVerbName is not null;
        var verbOffset = shouldSkipVerb ? 1 : 0;
        return PositionalArguments.Length <= 0
            ? null
            : CommandLineValueConverter.ArgumentStringsToValue<T>(PositionalArguments.Slice(verbOffset, 1), context);
    }

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <param name="index">获取指定索引处的参数值。</param>
    /// <param name="length">从索引处获取参数值的最长长度。当大于 1 时，会将这些值合并为一个字符串。</param>
    /// <param name="verbName">因为是否存在谓词会影响到位置参数的序号，所以如果有谓词名称，则需要传入。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>位置参数的值。</returns>
    public T? GetPositionalArgument<T>(int index, int length, string? verbName = null) where T : class
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.SlashAll);
        var shouldSkipVerb = verbName is not null && GuessedVerbName is not null;
        var verbOffset = shouldSkipVerb ? 1 : 0;
        return index < 0 || index >= PositionalArguments.Length
            ? null
            : CommandLineValueConverter.ArgumentStringsToValue<T>(
                PositionalArguments.Slice(index + verbOffset,
                    Math.Min(length, PositionalArguments.Length - index - verbOffset)), context);
    }

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <param name="verbName">因为是否存在谓词会影响到位置参数的序号，所以如果有谓词名称，则需要传入。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>位置参数的值。</returns>
    public T? GetPositionalArgumentValue<T>(string? verbName = null) where T : struct
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.SlashAll);
        var shouldSkipVerb = verbName is not null && GuessedVerbName is not null;
        var verbOffset = shouldSkipVerb ? 1 : 0;
        return PositionalArguments.Length <= 0
            ? null
            : CommandLineValueConverter.ArgumentStringsToValue<T>(PositionalArguments.Slice(verbOffset, 1), context);
    }

    /// <summary>
    /// 获取命令行参数中位置参数的值。
    /// </summary>
    /// <param name="index">获取指定索引处的参数值。</param>
    /// <param name="length">从索引处获取参数值的最长长度。当大于 1 时，会将这些值合并为一个字符串。</param>
    /// <param name="verbName">因为是否存在谓词会影响到位置参数的序号，所以如果有谓词名称，则需要传入。</param>
    /// <typeparam name="T">选项的值的类型。</typeparam>
    /// <returns>位置参数的值。</returns>
    public T? GetPositionalArgumentValue<T>(int index, int length, string? verbName = null) where T : struct
    {
        var context = new ConvertingContext(MatchedUrlScheme is null ? MultiValueHandling.First : MultiValueHandling.SlashAll);
        var shouldSkipVerb = verbName is not null && GuessedVerbName is not null;
        var verbOffset = shouldSkipVerb ? 1 : 0;
        return index < 0 || index >= PositionalArguments.Length
            ? null
            : CommandLineValueConverter.ArgumentStringsToValue<T>(
                PositionalArguments.Slice(index + verbOffset,
                    Math.Min(length, PositionalArguments.Length - index - verbOffset)), context);
    }

    /// <summary>
    /// 获取命令行参数中所有位置参数值的集合。
    /// </summary>
    /// <param name="verbName">因为是否存在谓词会影响到位置参数的序号，所以如果有谓词名称，则需要传入。</param>
    /// <returns>命令行参数中位置参数值的集合。</returns>
    public ImmutableArray<string> GetPositionalArguments(string? verbName = null)
    {
        var shouldSkipVerb = verbName is not null && GuessedVerbName is not null;
        return shouldSkipVerb ? PositionalArguments.Slice(1, PositionalArguments.Length - 1) : PositionalArguments;
    }

    /// <summary>
    /// 输出传入的命令行参数字符串。
    /// </summary>
    /// <returns>传入的命令行参数字符串。</returns>
    public override string ToString()
    {
        return MatchedUrlScheme is { } scheme
            ? $"{scheme}://{string.Join("/", PositionalArguments)}?{string.Join("&", LongOptionValues.Select(x => $"{x.Key}={string.Join("&", x.Value)}"))}"
            : string.Join(" ", CommandLineArguments.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
    }
}

internal readonly record struct ConvertingContext(MultiValueHandling MultiValueHandling);

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
