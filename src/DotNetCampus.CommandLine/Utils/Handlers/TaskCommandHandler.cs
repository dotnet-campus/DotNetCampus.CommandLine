namespace DotNetCampus.Cli.Temp40.Utils.Handlers;

internal sealed class TaskCommandHandler<TOptions>(
    Func<TOptions> optionsCreator,
    Func<TOptions, Task<int>> handler) : ICommandHandler
    where TOptions : class
{
    private TOptions? _options;

    public Task<int> RunAsync()
    {
        _options ??= optionsCreator();
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(TOptions)} were created.");
        }

        return handler(_options);
    }
}
