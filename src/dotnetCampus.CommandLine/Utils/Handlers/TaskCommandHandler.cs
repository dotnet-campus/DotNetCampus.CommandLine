using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Utils.Handlers;

internal sealed class TaskCommandHandler<TOptions>(
    CommandLine commandLine,
    IVerbCreator<TOptions> optionsCreator,
    Func<TOptions, Task<int>> handler) : ICommandHandler, IVerbCreator<TaskCommandHandler<TOptions>>
    where TOptions : class
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
