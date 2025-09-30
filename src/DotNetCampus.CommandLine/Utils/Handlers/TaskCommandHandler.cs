using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

internal sealed class AnonymousStatedCommandHandler<TState>(
    ICommandObjectMetadata factory,
    TState state) : ICommandHandlerMetadata
{
    public object Build(CommandRunningContext context)
    {
        return factory.Build(context);
    }

    public Task<int> RunAsync(object createdCommandObject)
    {
        var instance = (ICommandHandler<TState>)createdCommandObject;
        return instance.RunAsync(state);
    }
}

internal sealed class AnonymousActionCommandHandler<T>(
    ICommandObjectMetadata factory,
    Action<T> handler) : ICommandHandlerMetadata
    where T : notnull
{
    public object Build(CommandRunningContext context)
    {
        return factory.Build(context);
    }

    public Task<int> RunAsync(object createdCommandObject)
    {
        var instance = (T)createdCommandObject;
        handler(instance);
        return Task.FromResult(0);
    }
}

internal sealed class AnonymousFuncInt32CommandHandler<T>(
    ICommandObjectMetadata factory,
    Func<T, int> handler) : ICommandHandlerMetadata
    where T : notnull
{
    public object Build(CommandRunningContext context)
    {
        return factory.Build(context);
    }

    public Task<int> RunAsync(object createdCommandObject)
    {
        var instance = (T)createdCommandObject;
        var exitCode = handler(instance);
        return Task.FromResult(exitCode);
    }
}

internal sealed class AnonymousFuncTaskCommandHandler<T>(
    ICommandObjectMetadata factory,
    Func<T, Task> handler) : ICommandHandlerMetadata
    where T : notnull
{
    public object Build(CommandRunningContext context)
    {
        return factory.Build(context);
    }

    public Task<int> RunAsync(object createdCommandObject)
    {
        var instance = (T)createdCommandObject;
        var task = handler(instance);
        return Await(task);
    }

    private static async Task<int> Await(Task task)
    {
        await task.ConfigureAwait(false);
        return 0;
    }
}

internal sealed class AnonymousFuncTaskInt32CommandHandler<T>(
    ICommandObjectMetadata factory,
    Func<T, Task<int>> handler) : ICommandHandlerMetadata
    where T : notnull
{
    public object Build(CommandRunningContext context)
    {
        return factory.Build(context);
    }

    public Task<int> RunAsync(object createdCommandObject)
    {
        var instance = (T)createdCommandObject;
        return handler(instance);
    }
}
