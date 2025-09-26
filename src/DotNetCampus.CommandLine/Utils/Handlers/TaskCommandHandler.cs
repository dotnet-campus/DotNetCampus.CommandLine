using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

internal interface IAnonymousCommandHandler : ICommandHandler
{
    object? CreatedCommandOptions { get; }
}

internal sealed class AnonymousCommandHandler<T>(
    CommandLine commandLine,
    CommandObjectFactory factory,
    Action<T> handler) : IAnonymousCommandHandler
    where T : notnull
{
    private T? _options;

    public object? CreatedCommandOptions => _options;

    public Task<int> RunAsync()
    {
        _options ??= (T)factory(commandLine);
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
    CommandObjectFactory factory,
    Func<T, int> handler) : IAnonymousCommandHandler
    where T : notnull
{
    private T? _options;

    public object? CreatedCommandOptions => _options;

    public Task<int> RunAsync()
    {
        _options ??= (T)factory(commandLine);
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(T)} were created.");
        }
        return Task.FromResult(handler(_options));
    }
}

internal sealed class AnonymousTaskCommandHandler<T>(
    CommandLine commandLine,
    CommandObjectFactory factory,
    Func<T, Task> handler) : IAnonymousCommandHandler
    where T : notnull
{
    private T? _options;

    public object? CreatedCommandOptions => _options;

    public async Task<int> RunAsync()
    {
        _options ??= (T)factory(commandLine);
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
    CommandObjectFactory factory,
    Func<T, Task<int>> handler) : IAnonymousCommandHandler
    where T : notnull
{
    private T? _options;

    public object? CreatedCommandOptions => _options;

    public Task<int> RunAsync()
    {
        _options ??= (T)factory(commandLine);
        if (_options is null)
        {
            throw new InvalidOperationException($"No options of type {typeof(T)} were created.");
        }
        return handler(_options);
    }
}
