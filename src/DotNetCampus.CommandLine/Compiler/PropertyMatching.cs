namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 选项值的类型。此枚举中的选项值类型会影响到选项值的解析方式。
/// </summary>
public enum OptionValueType : byte
{
    /// <summary>
    /// 普通值。只解析一个参数。
    /// </summary>
    Normal,

    /// <summary>
    /// 布尔值。会尝试解析一个参数，如果无法解析，则视为 <see langword="true"/>。
    /// </summary>
    Boolean,

    /// <summary>
    /// 集合值。会尝试解析多个参数，直到遇到下一个选项或位置参数分隔符为止。
    /// </summary>
    List,

    /// <summary>
    /// 字典值。会尝试解析多个键值对，直到遇到下一个选项或位置参数分隔符为止。
    /// </summary>
    Dictionary,

    /// <summary>
    /// 用户输入的选项没有命中到任何已知的选项。
    /// </summary>
    NotExist,
}

/// <summary>
/// 位置参数值的类型。此枚举中的位置参数值类型会影响到位置参数值的解析方式。
/// </summary>
public enum PositionalArgumentValueType : byte
{
    /// <summary>
    /// 正常的位置参数。
    /// </summary>
    Normal,

    /// <summary>
    /// 指定位置处的位置参数没有匹配到任何位置参数范围。
    /// </summary>
    NotExist,
}
