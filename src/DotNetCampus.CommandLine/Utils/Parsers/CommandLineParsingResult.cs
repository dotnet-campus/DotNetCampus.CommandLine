using System.Diagnostics;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 命令行参数解析结果。
/// </summary>
/// <param name="ErrorType">如果解析失败，此处包含错误类型；否则为 <see cref="CommandLineParsingError.None"/>。</param>
/// <param name="ErrorMessage">如果解析失败，此处包含错误消息；否则为 <see langword="null"/>。</param>
public readonly record struct CommandLineParsingResult(CommandLineParsingError ErrorType, string? ErrorMessage)
{
    /// <summary>
    /// 获取一个值，指示此解析是否成功。
    /// </summary>
    public bool IsSuccess => ErrorMessage is null;

    /// <summary>
    /// 将另一个解析结果与当前实例合并。合并后，如果全部成功，则结果为成功；如果有任何一个失败，则结果为失败，并包含第一个失败的错误信息。
    /// </summary>
    /// <param name="other">
    /// 两个都失败，则会使用此实例的错误信息。
    /// </param>
    public CommandLineParsingResult Combine(CommandLineParsingResult other)
    {
        return (IsSuccess, other.IsSuccess) switch
        {
            (true, true) => Success,
            (false, true) => this,
            (true, false) => other,
            (false, false) => other,
        };
    }

    /// <summary>
    /// 处理解析错误。如果解析结果表示失败，则调用此方法来处理错误。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    public void WithFallback(CommandLine commandLine)
    {
        // 如果解析成功，则不需要处理错误。
        if (IsSuccess)
        {
            return;
        }

        // 根据命令行参数解析选项时，指定的未知参数处理方式，决定是否忽略某些错误。
        var unknownHandling = commandLine.ParsingOptions.UnknownArgumentsHandling;
        var ignoreOptionalArguments = unknownHandling is
            UnknownCommandArgumentHandling.IgnoreAllUnknownArguments or UnknownCommandArgumentHandling.IgnoreUnknownOptionalArguments;
        var ignorePositionalArguments = unknownHandling is
            UnknownCommandArgumentHandling.IgnoreAllUnknownArguments or UnknownCommandArgumentHandling.IgnoreUnknownPositionalArguments;
        if (ignoreOptionalArguments && ErrorType is CommandLineParsingError.OptionalArgumentNotFound
            || ignorePositionalArguments && ErrorType is CommandLineParsingError.PositionalArgumentNotFound)
        {
            return;
        }

        // 尝试使用命令行参数解析器的回调来处理错误。
        var runner = ((ICoreCommandRunnerBuilder)commandLine).GetOrCreateRunner();
        if (runner.RunFallback(this))
        {
            return;
        }

        // 最终还是没有被处理，则抛出异常。
        ThrowIfError();
    }

    /// <summary>
    /// 如果解析结果表示失败，则抛出一个异常，包含错误信息。
    /// </summary>
    public void ThrowIfError()
    {
        if (IsSuccess)
        {
            return;
        }

        throw ErrorType switch
        {
            CommandLineParsingError.OptionalArgumentNotFound => new CommandLineParseException(ErrorType, ErrorMessage!),
            CommandLineParsingError.OptionalArgumentSeparatorNotSupported => new CommandLineParseException(ErrorType, ErrorMessage!),
            CommandLineParsingError.MultiCharShortOptionalArgumentNotSupported => new CommandLineParseException(ErrorType, ErrorMessage!),
            CommandLineParsingError.OptionalArgumentParseError => new CommandLineParseException(ErrorType, ErrorMessage!),
            CommandLineParsingError.PositionalArgumentNotFound => new CommandLineParseException(ErrorType, ErrorMessage!),
            CommandLineParsingError.ArgumentCombinationIsNotBoolean => new CommandLineParseException(ErrorType, ErrorMessage!),
            CommandLineParsingError.BooleanValueParseError => new CommandLineParseValueException(ErrorType, ErrorMessage!),
            CommandLineParsingError.DictionaryValueParseError => new CommandLineParseValueException(ErrorType, ErrorMessage!),
            CommandLineParsingError.None => throw new CommandLineException("解析过程中没有发生任何错误。"),
            _ => throw new CommandLineException("未知的命令行解析错误类型。"),
        };
    }

    /// <summary>
    /// 隐式转换运算符，允许将 <see cref="CommandLineParsingResult"/> 直接转换为布尔值，表示解析是否成功。
    /// </summary>
    /// <param name="result">要转换的解析结果。</param>
    /// <returns>如果解析成功，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public static implicit operator bool(CommandLineParsingResult result) => result.IsSuccess;

    /// <summary>
    /// 获取一个表示成功的解析结果。
    /// </summary>
    public static CommandLineParsingResult Success => new(CommandLineParsingError.None, null);

    /// <summary>
    /// 创建一个表示选项未找到的解析结果。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    /// <param name="index">当前正在解析的参数索引。</param>
    /// <param name="commandObjectName">正在解析此参数的命令对象的名称。</param>
    /// <param name="optionName">确定没有找到的选项名称。</param>
    /// <param name="isLongOption">指示此选项名称是否为长选项名称。如果为 <see langword="null"/>，表示无法确定是长选项还是短选项。</param>
    /// <returns>表示选项未找到的解析结果。</returns>
    public static CommandLineParsingResult OptionalArgumentNotFound(CommandLine commandLine, int index, string commandObjectName,
        ReadOnlySpan<char> optionName, bool? isLongOption)
    {
        var isUrl = commandLine.MatchedUrlScheme is not null;
        var possibleSeparatorIndex = optionName.IndexOfAnyPossibleSeparators();
        var reason = (isLongOption, possibleSeparatorIndex) switch
        {
            (_, < 0) => CommandLineParsingError.OptionalArgumentNotFound,
            (_, 0) => CommandLineParsingError.OptionalArgumentParseError,
            (_, 1) => CommandLineParsingError.OptionalArgumentSeparatorNotSupported,
            (false, _) => CommandLineParsingError.MultiCharShortOptionalArgumentNotSupported,
            _ => CommandLineParsingError.OptionalArgumentSeparatorNotSupported,
        };
        var message = reason switch
        {
            CommandLineParsingError.OptionalArgumentNotFound when isUrl =>
                $"命令行对象 {commandObjectName} 没有任何属性的选项名为 {optionName.ToString()}，请注意解析 URL 时不支持短选项参数。URL={commandLine.ToRawString()}",
            CommandLineParsingError.OptionalArgumentNotFound =>
                $"命令行对象 {commandObjectName} 没有任何属性的选项名为 {optionName.ToString()}。参数列表：{commandLine}，索引 {index}，参数 {commandLine.CommandLineArguments[index]}。",
            CommandLineParsingError.OptionalArgumentParseError =>
                $"命令行参数 {commandLine.CommandLineArguments[index]} 中不包含选项名称，解析失败。参数列表：{commandLine}，索引 {index}。",
            CommandLineParsingError.OptionalArgumentSeparatorNotSupported =>
                $"当前解析风格 {commandLine.ParsingOptions.Style.Name} 不支持选项值分隔符 '{optionName[possibleSeparatorIndex]}'，因此无法识别参数 {commandLine.CommandLineArguments[index]}。参数列表：{commandLine}，索引 {index}，参数 {commandLine.CommandLineArguments[index]}。",
            CommandLineParsingError.MultiCharShortOptionalArgumentNotSupported =>
                $"当前解析风格 {commandLine.ParsingOptions.Style.Name} 不支持多字符短选项，因此无法识别参数 {commandLine.CommandLineArguments[index]}。参数列表：{commandLine}，索引 {index}，参数 {commandLine.CommandLineArguments[index]}。",
            _ => throw new CommandLineException("Unreachable code."),
        };
        return new CommandLineParsingResult(reason, message);
    }

    /// <summary>
    /// 创建一个表示选项组合不支持非布尔类型的解析结果。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    /// <param name="index">当前正在解析的参数索引。</param>
    /// <param name="commandObjectName">正在解析此参数的命令对象的名称。</param>
    /// <param name="optionName">类型为非布尔类型的选项名称。</param>
    /// <returns>表示选项组合不支持非布尔类型的解析结果。</returns>
    public static CommandLineParsingResult OptionalArgumentCombinationIsNotBoolean(CommandLine commandLine, int index, string commandObjectName,
        ReadOnlySpan<char> optionName)
    {
        var message =
            $"命令行对象 {commandObjectName} 中，选项 {optionName.ToString()} 的类型不是布尔类型，因此不支持使用短布尔选项组合的方式来表示此选项。参数列表：{commandLine}，索引 {index}，参数 {commandLine.CommandLineArguments[index]}。";
        return new CommandLineParsingResult(CommandLineParsingError.ArgumentCombinationIsNotBoolean, message);
    }

    /// <summary>
    /// 创建一个表示选项未找到的解析结果。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    /// <param name="index">当前正在解析的参数索引。</param>
    /// <param name="commandObjectName">正在解析此参数的命令对象的名称。</param>
    /// <returns>表示选项未找到的解析结果。</returns>
    public static CommandLineParsingResult OptionalArgumentParseError(CommandLine commandLine, int index, string commandObjectName)
    {
        var message = $"命令行参数 {commandLine.CommandLineArguments[index]} 中不包含选项名称，解析失败。参数列表：{commandLine}，索引 {index}。";
        return new CommandLineParsingResult(CommandLineParsingError.OptionalArgumentParseError, message);
    }

    /// <summary>
    /// 创建一个表示位置参数未找到的解析结果。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    /// <param name="index">当前正在解析的参数索引。</param>
    /// <param name="commandObjectName">正在解析此参数的命令对象的名称。</param>
    /// <param name="positionalArgumentIndex">要查找的位置参数的索引。</param>
    /// <returns>表示位置参数未找到的解析结果。</returns>
    public static CommandLineParsingResult PositionalArgumentNotFound(CommandLine commandLine, int index, string commandObjectName, int positionalArgumentIndex)
    {
        var message =
            $"命令行对象 {commandObjectName} 位置参数范围不包含索引 {positionalArgumentIndex}。参数列表：{commandLine}，索引 {index}，参数 {commandLine.CommandLineArguments[index]}。";
        return new CommandLineParsingResult(CommandLineParsingError.PositionalArgumentNotFound, message);
    }

    /// <summary>
    /// 创建一个表示无法将值解析为布尔值的解析结果。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    /// <param name="value">要解析的值。</param>
    /// <returns>表示无法将值解析为布尔值的解析结果。</returns>
    public static CommandLineParsingResult BooleanValueParseError(CommandLine commandLine, ReadOnlySpan<char> value)
    {
        var message = $"无法将 {value.ToString()} 解析为布尔值。参数列表：{commandLine}。";
        return new CommandLineParsingResult(CommandLineParsingError.BooleanValueParseError, message);
    }

    /// <summary>
    /// 创建一个表示无法将值解析为键值对的解析结果。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    /// <param name="value">要解析的值。</param>
    /// <returns>表示无法将值解析为键值对的解析结果。</returns>
    public static CommandLineParsingResult DictionaryValueParseError(CommandLine commandLine, ReadOnlySpan<char> value)
    {
        var message = $"无法将 {value.ToString()} 解析为键值对。参数列表：{commandLine}。";
        return new CommandLineParsingResult(CommandLineParsingError.DictionaryValueParseError, message);
    }
}

file static class Extensions
{
    internal static int IndexOfAnyPossibleSeparators(this ReadOnlySpan<char> span)
    {
        for (var i = 0; i < span.Length; i++)
        {
            if (!char.IsLetterOrDigit(span[i]) && span[i] is not '-' and not '_' and not '.')
            {
                return i;
            }
        }

        return -1;
    }
}

/// <summary>
/// 命令行参数解析错误类型。
/// </summary>
public enum CommandLineParsingError : byte
{
    /// <summary>
    /// 没有错误。
    /// </summary>
    None,

    /// <summary>
    /// 没有任何选项能够匹配当前的命令行参数。
    /// </summary>
    OptionalArgumentNotFound,

    /// <summary>
    /// 没有任何选项能够匹配当前的命令行参数，可能是因为当前的命令行参数使用了不被支持的选项值分隔符。
    /// </summary>
    OptionalArgumentSeparatorNotSupported,

    /// <summary>
    /// 当前命令行风格不支持多字符短选项。
    /// </summary>
    MultiCharShortOptionalArgumentNotSupported,

    /// <summary>
    /// 当前的命令行参数正试图使用短布尔选项组合的方式来表示一个非布尔类型的选项。
    /// </summary>
    ArgumentCombinationIsNotBoolean,

    /// <summary>
    /// 当前的命令行参数无法解析出选项名。
    /// </summary>
    OptionalArgumentParseError,

    /// <summary>
    /// 没有任何位置参数的范围能够匹配当前的命令行参数。
    /// </summary>
    PositionalArgumentNotFound,

    /// <summary>
    /// 无法将值解析为布尔值。
    /// </summary>
    BooleanValueParseError,

    /// <summary>
    /// 无法将值解析为键值对。
    /// </summary>
    DictionaryValueParseError,
}
