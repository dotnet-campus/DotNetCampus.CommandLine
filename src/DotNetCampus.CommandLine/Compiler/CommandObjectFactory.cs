namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 从已解析的命令行参数创建命令数据模型或处理器的委托。
/// </summary>
/// <param name="commandLine">已解析的命令行参数。</param>
/// <returns>命令数据模型或处理器。</returns>
public delegate object CommandObjectFactory(CommandLine commandLine);

/// <summary>
/// 运行命令处理器的委托。
/// </summary>
/// <param name="state">传递给命令处理器的状态对象。</param>
/// <returns>命令处理器的返回值。</returns>
public delegate Task<int> CommandHandlerRunner(object state);
