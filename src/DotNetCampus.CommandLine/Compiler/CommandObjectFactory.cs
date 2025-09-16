namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 从已解析的命令行参数创建命令数据模型或处理器的委托。
/// </summary>
public delegate object LegacyCommandObjectCreator(LegacyCommandLine commandLine);

/// <summary>
/// 从已解析的命令行参数创建命令数据模型或处理器的委托。
/// </summary>
public delegate object CommandObjectFactory(CommandLine commandLine);
