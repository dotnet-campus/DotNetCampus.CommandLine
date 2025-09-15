namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 要求源生成器匹配长名称，返回此长选项的值类型和追加值的回调。
/// </summary>
/// <param name="longOption">由用户输入的长名称（已去掉前缀符号和后续所带的值，未处理命名法变换）。</param>
/// <param name="defaultCaseSensitive">如果此参数未指定大小写敏感性，则使用此默认值。</param>
/// <param name="namingPolicy">由开发者配置的允许的命名法。</param>
/// <returns>此长选项的匹配结果。</returns>
public delegate OptionValueMatch LongOptionMatchingCallback(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, CommandNamingPolicy namingPolicy);

/// <summary>
/// 要求源生成器匹配短名称，返回此短选项的值类型和追加值的回调。
/// </summary>
/// <param name="shortOption">由用户输入的短名称（已去掉前缀符号和后续所带的值，包含多个字符时也只允许匹配一个短选项）。</param>
/// <param name="defaultCaseSensitive">如果此参数未指定大小写敏感性，则使用此默认值。</param>
/// <returns>此短选项的匹配结果。</returns>
public delegate OptionValueMatch ShortOptionMatchingCallback(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive);

/// <summary>
/// 要求源生成器匹配位置参数，返回此位置参数的范围和追加值的回调。
/// </summary>
/// <param name="value">由用户输入的位置参数的值。</param>
/// <param name="argumentIndex">位置参数的索引（从 0 开始）。</param>
/// <returns>此位置参数的匹配结果。</returns>
public delegate PositionalArgumentValueMatch PositionalArgumentMatchingCallback(ReadOnlySpan<char> value, int argumentIndex);

/// <summary>
/// 向某个选项或位置参数追加一个值的回调。
/// </summary>
/// <param name="key">要追加的键（对于字典类型的选项有效，其他类型永远为空）。</param>
/// <param name="value">要追加的值。</param>
public delegate void AppendValueCallback(ReadOnlySpan<char> key, ReadOnlySpan<char> value);

/// <summary>
/// 向指定索引处的属性赋值。
/// </summary>
/// <param name="propertyName">要赋值的属性名称（调试追踪用）。</param>
/// <param name="propertyIndex">要赋值的属性索引（源生成器生成的索引）。</param>
/// <param name="key">要赋值的键（对于字典类型的选项有效，其他类型永远为空）。</param>
/// <param name="value">要赋值的值。</param>
public delegate void AssignPropertyValueCallback(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value);

/// <summary>
/// 源生成器匹配属性的匹配结果。
/// </summary>
/// <param name="PropertyName">此选项对应的属性名称。</param>
/// <param name="PropertyIndex">此选项对应的属性索引。</param>
/// <param name="ValueType">此选项的值类型。</param>
public readonly record struct OptionValueMatch(string PropertyName, int PropertyIndex, OptionValueType ValueType);

/// <summary>
/// 源生成器匹配位置参数的匹配结果。
/// </summary>
/// <param name="PropertyName">此选项对应的属性名称。</param>
/// <param name="PropertyIndex">此选项对应的属性索引。</param>
/// <param name="ValueType">此位置参数的值类型。</param>
public readonly record struct PositionalArgumentValueMatch(string PropertyName, int PropertyIndex, PositionalArgumentValueType ValueType);
