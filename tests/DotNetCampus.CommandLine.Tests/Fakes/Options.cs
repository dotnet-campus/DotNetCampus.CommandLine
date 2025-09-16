using System.ComponentModel;

namespace DotNetCampus.Cli.Tests.Fakes;

/// <summary>
/// 表示此程序在被启动的时候使用的参数信息。此类型是不可变类型，所有实例都是线程安全的。
/// </summary>
public class Options
{
    /// <summary>
    /// 表示通过打开的文件路径。此属性可能为 null，但绝不会是空字符串或空白字符串。
    /// </summary>
    [DotNetCampus.Cli.Compiler.Value(0), DotNetCampus.Cli.Compiler.Option('f', "file")]
    [dotnetCampus.Cli.Value(0), dotnetCampus.Cli.Option('f', "File")]
    public string? FilePath { get; set; }

    /// <summary>
    /// 当此参数值为 true 时，表示此进程是从 Cloud 端启动的 Shell 进程。此属性默认值是 false。
    /// </summary>
    [DotNetCampus.Cli.Compiler.Option("cloud")]
    [dotnetCampus.Cli.Option("Cloud"), DefaultValue(false)]
    public bool IsFromCloud { get; init; }

    /// <summary>
    /// 表示 Shell 端启动的模式。此属性可能为 null，但绝不会是空字符串或空白字符串。
    /// </summary>
    [DotNetCampus.Cli.Compiler.Option('m', "mode")]
    [dotnetCampus.Cli.Option('m', "Mode")]
    public string? StartupMode { get; init; }

    /// <summary>
    /// 表示当前是否是静默方式启动，通常由 Shell 启动 Cloud 时使用。此属性默认值是 false。
    /// </summary>
    [DotNetCampus.Cli.Compiler.Option('s', "silence")]
    [dotnetCampus.Cli.Option('s', "Silence"), DefaultValue(false)]
    public bool IsSilence { get; init; }

    /// <summary>
    /// 表示当前启动时需要针对 IWB 进行处理。此属性默认值是 false。
    /// </summary>
    [DotNetCampus.Cli.Compiler.Option("iwb")]
    [dotnetCampus.Cli.Option("Iwb"), DefaultValue(false)]
    public bool IsIwb { get; init; }

    /// <summary>
    /// 表示当前窗口启动时应该安放的位置。此属性可能为 null，但绝不会是空字符串或空白字符串。
    /// </summary>
    [DotNetCampus.Cli.Compiler.Option('p', "placement")]
    [dotnetCampus.Cli.Option('p', "Placement")]
    public string? Placement { get; init; }

    /// <summary>
    /// 表示一个启动会话 Id，用于在多个进程间同步一些信息。此属性可能为 null，但绝不会是空字符串或空白字符串。
    /// </summary>
    [DotNetCampus.Cli.Compiler.Option("startup-session")]
    [dotnetCampus.Cli.Option("StartupSession")]
    public string? StartupSession { get; init; }

    /// <summary>
    /// 创建 <see cref="Options"/> 类的新实例。
    /// </summary>
    public Options()
    {
    }

    /// <summary>
    /// 创建 <see cref="Options"/> 类的新实例。
    /// </summary>
    public Options(
        string? filePath,
        bool isFromCloud,
        string? startupMode,
        bool isSilence,
        bool isIwb,
        string? placement,
        string? startupSession)
    {
        FilePath = filePath;
        IsFromCloud = isFromCloud;
        StartupMode = startupMode;
        IsSilence = isSilence;
        IsIwb = isIwb;
        Placement = placement;
        StartupSession = startupSession;
    }
}
