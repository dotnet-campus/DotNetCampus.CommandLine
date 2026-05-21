using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Help;

namespace DotNetCampus.Cli;

/// <summary>
/// 定制命令行帮助。
/// </summary>
public class HelpConfigurations
{
    /// <summary>
    /// 决定如何处理命令行的帮助请求。
    /// </summary>
    /// <remarks>
    /// 默认实现中，会调用 <see cref="HelpTextLocalizer"/> 获取帮助的本地化文本，然后使用 <see cref="HelpMessageWriter"/> 输出。
    /// </remarks>
    public IHelpHandler? HelpHandler { get; init; }

    /// <summary>
    /// 帮助文本中选项/命令/位置参数名称列的最大宽度。超过此宽度的项，其描述将换到下一行显示。
    /// </summary>
    public int MaxColumnWidth { get; init; } = 30;

    /// <summary>
    /// 提供帮助文本的本地化。
    /// </summary>
    /// <remarks>
    /// 默认情况下，写在命令、选项和位置参数上的 <see cref="CommandLineAttribute.Description"/> 属性会直接作为帮助文本显示，<br/>
    /// 但如果希望进行本地化，可以设置此委托，以 <see cref="CommandLineAttribute.Description"/> 的值为键，返回本地化的文本。
    /// </remarks>
    public Func<string, string>? HelpTextLocalizer { get; init; }

    /// <summary>
    /// 由开发者自行决定如何输出帮助文本。
    /// </summary>
    /// <remarks>
    /// 默认情况下为标准控制台输出。
    /// </remarks>
    public Action<string>? HelpMessageWriter { get; init; }
}
