using System.ComponentModel;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli;

/// <summary>
/// 命令行执行器构造器，用于链式创建命令行执行器。
/// </summary>
public interface ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 获取或创建一个命令行执行器。
    /// </summary>
    /// <returns>命令行执行器。</returns>
    internal CommandRunner AsRunner();
}

/// <summary>
/// 命令行执行器构造器，用于链式创建命令行执行器。
/// </summary>
public interface ICommandRunnerBuilder : ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 以同步方式运行命令行处理器。
    /// </summary>
    /// <returns>将被执行的命令行处理器的返回值。</returns>
    CommandRunningResult Run();
}

/// <summary>
/// 命令行执行器构造器，用于链式创建命令行执行器。
/// </summary>
public interface IAsyncCommandRunnerBuilder : ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 以异步方式运行命令行处理器。
    /// </summary>
    /// <returns>将被执行的命令行处理器的返回值。</returns>
    Task<CommandRunningResult> RunAsync();
}

/// <summary>
/// 带有状态的命令行执行器构造器，专门用来添加执行时需要额外状态的命令处理器。
/// </summary>
/// <typeparam name="TState">状态对象的类型。</typeparam>
public readonly ref struct StatedCommandRunnerBuilder<TState>
{
    private readonly IAsyncCommandRunnerBuilder _builder;
    private readonly TState _state;

    /// <summary>
    /// 创建一个带有状态的命令行执行器构造器。
    /// </summary>
    /// <param name="builder">命令行执行器构造器。</param>
    /// <param name="state">状态对象。</param>
    public StatedCommandRunnerBuilder(IAsyncCommandRunnerBuilder builder, TState state)
    {
        _builder = builder;
        _state = state;
    }

    /// <summary>
    /// 为命令执行器指定一个新的状态对象，后续添加的命令处理器如果被执行，将会收到这个新的状态对象。
    /// </summary>
    /// <param name="state">新的状态对象。</param>
    /// <typeparam name="TAnotherState">新的状态对象的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public StatedCommandRunnerBuilder<TAnotherState> ForState<TAnotherState>(TAnotherState state)
    {
        return new StatedCommandRunnerBuilder<TAnotherState>(_builder, state);
    }

    /// <summary>
    /// 返回不带状态的命令行执行器构造器，后续添加的命令处理器将不会收到状态对象。
    /// </summary>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public IAsyncCommandRunnerBuilder ForState()
    {
        return _builder;
    }

    /// <summary>
    /// 添加一个带有状态的命令处理器。
    /// </summary>
    /// <typeparam name="T">带有状态的命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public StatedCommandRunnerBuilder<TState> AddHandler<T>()
        where T : class, ICommandHandler<TState>
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 由拦截器调用，用于添加一个带有状态的命令处理器。
    /// </summary>
    /// <param name="command">由拦截器传入的的命令处理器的命令，<see langword="null"/> 或空字符串表示此处理器没有命令名称。</param>
    /// <param name="factory">由拦截器传入的命令处理器创建方法。</param>
    /// <typeparam name="T">带有状态的命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public StatedCommandRunnerBuilder<TState> AddHandler<T>(
        NamingPolicyNameGroup command, CommandObjectFactory factory
    )
        where T : class, ICommandHandler<TState>
    {
        var state = _state;
        _builder.AsRunner()
            .AddHandlerCore(command, factory, o => ((T)o).RunAsync(state));
        return this;
    }

    /// <summary>
    /// 以异步方式运行命令行处理器。
    /// </summary>
    /// <returns>将被执行的命令行处理器的返回值。</returns>
    public Task<CommandRunningResult> RunAsync()
    {
        return _builder.RunAsync();
    }
}
