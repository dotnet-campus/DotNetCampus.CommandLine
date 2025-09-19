namespace DotNetCampus.Cli.Temp40.Compiler;

/// <summary>
/// 从已解析的命令行参数创建命令数据模型或处理器的委托。
/// </summary>
public delegate object CommandObjectCreator(CommandLine commandLine);
