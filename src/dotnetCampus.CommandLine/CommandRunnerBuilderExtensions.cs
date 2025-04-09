using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli;

/// <summary>
/// 辅助创建命令行执行程序。
/// </summary>
public static class CommandRunnerBuilderExtensions
{
    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder)
        where T : class, ICommandHandler
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>();
    }

    /// <inheritdoc cref="AddHandler{T}(ICoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static ICommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Action<T> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(t =>
            {
                handler(t);
                return Task.FromResult(0);
            });
    }

    /// <inheritdoc cref="AddHandler{T}(ICoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Action<T> handler)
        where T : class
    {
        return (IAsyncCommandRunnerBuilder)((ICoreCommandRunnerBuilder)builder).AddHandler<T>(handler);
    }

    /// <inheritdoc cref="AddHandler{T}(ICoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static ICommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, int> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(t => Task.FromResult(handler(t)));
    }

    /// <inheritdoc cref="AddHandler{T}(ICoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, int> handler)
        where T : class
    {
        return (IAsyncCommandRunnerBuilder)((ICoreCommandRunnerBuilder)builder).AddHandler<T>(handler);
    }

    /// <inheritdoc cref="AddHandler{T}(ICoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, Task> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(async t =>
            {
                await handler(t);
                return 0;
            });
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, Task<int>> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(handler);
    }

    /// <summary>
    /// 添加一个命令处理器集合。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <typeparam name="T">命令处理器集合的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static IAsyncCommandRunnerBuilder AddHandlers<T>(this ICoreCommandRunnerBuilder builder)
        where T : ICommandHandlerCollection, new()
    {
        return builder.GetOrCreateRunner()
            .AddHandlers<T>();
    }
}
