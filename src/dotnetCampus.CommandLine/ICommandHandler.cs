using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli;

/// <summary>
/// 表示一个可以接收命令行参数的对象。
/// </summary>
public interface ICommandOptions
{
}

/// <summary>
/// 表示可以接收命令行参数，然后处理一条命令。
/// </summary>
public interface ICommandHandler : ICommandOptions
{
    /// <summary>
    /// 处理一条命令。
    /// </summary>
    /// <returns>返回处理结果。</returns>
    Task<int> RunAsync();
}

internal sealed class TaskCommandHandler<TOptions>(
    CommandLine commandLine,
    IVerbCreator<TOptions> optionsCreator,
    Func<TOptions, Task<int>> handler) : ICommandHandler, IVerbCreator<TaskCommandHandler<TOptions>>
    where TOptions : class, ICommandOptions
{
    private TOptions? _options;

    public TaskCommandHandler<TOptions> CreateInstance(CommandLine cl)
    {
        return this;
    }

    public Task<int> RunAsync()
    {
        _options ??= optionsCreator.CreateInstance(commandLine);
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(TOptions)} were created.");
        }

        return handler(_options);
    }
}
