namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 要求源生成器判断某个索引处的参数是否为命令（主命令、子命令或多级子命令）。
/// </summary>
/// <param name="argumentIndex">要判断的参数的索引。</param>
/// <returns>如果此参数为命令（主命令、子命令或多级子命令），则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
public delegate bool CheckIsCommandCallback(int argumentIndex);

/// <summary>
/// 要求源生成器匹配长名称，返回此长选项的值类型和追加值的回调。
/// </summary>
/// <param name="longOption">由用户输入的长名称（已去掉前缀符号和后续所带的值，未处理命名法变换）。</param>
/// <param name="defaultCaseSensitive">如果此参数未指定大小写敏感性，则使用此默认值。</param>
/// <param name="allowedNamingPolicies">由开发者配置的允许的命名法。</param>
/// <returns>此长选项的匹配结果。</returns>
public delegate OptionValueHandler LongOptionMatchingCallback(ReadOnlySpan<char> longOption, bool defaultCaseSensitive,
    params ReadOnlySpan<CommandNamingPolicy> allowedNamingPolicies);

/// <summary>
/// 要求源生成器匹配短名称，返回此短选项的值类型和追加值的回调。
/// </summary>
/// <param name="shortOption">由用户输入的短名称（已去掉前缀符号和后续所带的值，包含多个字符时也只允许匹配一个短选项）。</param>
/// <param name="defaultCaseSensitive">如果此参数未指定大小写敏感性，则使用此默认值。</param>
/// <returns>此短选项的匹配结果。</returns>
public delegate OptionValueHandler ShortOptionMatchingCallback(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive);

/// <summary>
/// 要求源生成器匹配位置参数，返回此位置参数的范围和追加值的回调。
/// </summary>
/// <param name="value">由用户输入的位置参数的值。</param>
/// <param name="argumentIndex">位置参数的索引（从 0 开始）。</param>
/// <returns>此位置参数的匹配结果。</returns>
public delegate PositionalArgumentValueHandler PositionalArgumentMatchingCallback(ReadOnlySpan<char> value, int argumentIndex);

/// <summary>
/// 向某个选项或位置参数追加一个值的回调。
/// </summary>
public delegate void AppendValueCallback(ReadOnlySpan<char> value);

/// <summary>
/// 源生成器匹配属性的匹配结果。
/// </summary>
/// <param name="ValueType">此选项的值类型。</param>
/// <param name="AppendValue">向此选项追加一个值的回调。</param>
public readonly record struct OptionValueHandler(OptionValueType ValueType, AppendValueCallback AppendValue);

/// <summary>
/// 源生成器匹配位置参数的匹配结果。
/// </summary>
/// <param name="ValueType">此位置参数的值类型。</param>
/// <param name="AppendValue">向此位置参数追加一个值的回调。</param>
public readonly record struct PositionalArgumentValueHandler(PositionalArgumentValueType ValueType, AppendValueCallback AppendValue);
