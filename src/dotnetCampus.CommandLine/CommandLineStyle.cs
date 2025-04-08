namespace dotnetCampus.Cli;

/// <summary>
/// This version of ps accepts several kinds of options:
/// 1. UNIX options, which may be grouped and must be preceded by a dash.
/// 2. BSD options, which may be grouped and must not be used with a dash.
/// 3. GNU long options, which are preceded by two dashes.
/// </summary>
[Flags]
public enum CommandLineStyle
{
    /// <summary>
    /// 单破折线（-）风格，通过空格赋值。
    /// </summary>
    /// <remarks>
    /// 这种风格既不能和古典的 POSIX/UNIX 风格兼容，也不能和 GNU 风格兼容，所以不应该在新程序中使用，除非遵循老式的X约定看起来价值很高。<br/>
    /// <code>
    /// do -option value
    /// </code>
    /// </remarks>
    [Obsolete("我们提供此风格的选项，仅为兼容老程序使用。")]
    XToolkit = -1,

    /// <summary>
    /// 自动风格。<br/>
    /// 根据实际传入的参数，自动在 <see cref="GNU"/>、XXX、XXX 中选择风格。
    /// </summary>
    Auto,

    /// <summary>
    /// 双破折线（--）风格，通过等号或空格赋值。
    /// </summary>
    /// <remarks>
    /// <code>
    /// do --option=value
    /// do --option value
    /// do -o=value
    /// do -o value
    /// do value1 value2 --option value
    /// do --option value -- value1 value2
    /// </code>
    /// </remarks>
    GNU,

    POSIX,

    /// <summary>
    /// Windows 传统风格。左下划线（/）+ 单个字符 + 选项参数；不使用空格分开。
    /// </summary>
    /// <remarks>
    /// 在此风格下，选项名称仅支持单个字符，其多个字符的本名不起作用。
    /// <code>
    /// do /ovalue
    /// o /o/s
    /// </code>
    /// </remarks>
    [Obsolete("我们提供此风格的选项，仅为兼容老程序使用。")]
    DOS,
}
