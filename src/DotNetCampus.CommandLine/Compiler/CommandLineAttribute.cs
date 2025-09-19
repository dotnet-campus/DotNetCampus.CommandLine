using System.Diagnostics.CodeAnalysis;

namespace DotNetCampus.Cli.Temp40.Compiler;

/// <summary>
/// 为命令行参数与类型属性的关联提供特性基类。
/// </summary>
public abstract class CommandLineAttribute : Attribute
{
    /// <summary>
    /// 只允许内部的类型继承自 <see cref="CommandLineAttribute"/>。
    /// </summary>
    internal CommandLineAttribute()
    {
    }

    /// <summary>
    /// 此命令行类/属性的描述信息（会在命令行输出帮助信息时使用）。
    /// </summary>
    /// <remarks>
    /// 如需支持本地化，请 // TODO: 添加本地化支持。
    /// </remarks>
    [DisallowNull]
    public string? Description { get; set; }

    /// <summary>
    /// 未来不再支持使用本方式的本地化。
    /// </summary>
    [DisallowNull]
    [Obsolete("不再使用单一的本地化方法，请使用即将开发的新方法替代。")]
    public string? LocalizableDescription { get; set; }
}
