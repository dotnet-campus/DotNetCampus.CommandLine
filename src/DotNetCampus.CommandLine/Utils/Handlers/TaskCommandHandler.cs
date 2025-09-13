using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

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

internal sealed class AnonymousCommandHandler<T>(
    CommandLine commandLine,
    CommandObjectCreator creator,
    Action<T> handler) : ICommandHandler
    where T : notnull
{
    private T? _options;

    public Task<int> RunAsync()
    {
        _options ??= (T)creator(commandLine);
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(T)} were created.");
        }
        handler(_options);
        return Task.FromResult(0);
    }
}

internal sealed class AnonymousInt32CommandHandler<T>(
    CommandLine commandLine,
    CommandObjectCreator creator,
    Func<T, int> handler) : ICommandHandler
    where T : notnull
{
    private T? _options;

    public Task<int> RunAsync()
    {
        _options ??= (T)creator(commandLine);
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(T)} were created.");
        }
        handler(_options);
        return Task.FromResult(0);
    }
}

internal sealed class AnonymousTaskCommandHandler<T>(
    CommandLine commandLine,
    CommandObjectCreator creator,
    Func<T, Task> handler) : ICommandHandler
    where T : notnull
{
    private T? _options;

    public async Task<int> RunAsync()
    {
        _options ??= (T)creator(commandLine);
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(T)} were created.");
        }
        await handler(_options);
        return 0;
    }
}

internal sealed class AnonymousTaskInt32CommandHandler<T>(
    CommandLine commandLine,
    CommandObjectCreator creator,
    Func<T, Task<int>> handler) : ICommandHandler
    where T : notnull
{
    private T? _options;

    public Task<int> RunAsync()
    {
        _options ??= (T)creator(commandLine);
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(T)} were created.");
        }
        return handler(_options);
    }
}
