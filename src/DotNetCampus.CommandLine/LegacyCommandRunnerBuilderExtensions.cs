using System.ComponentModel;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli;

/// <summary>
/// 辅助创建命令行执行程序。
/// </summary>
public static class LegacyCommandRunnerBuilderExtensions
{
    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder)
        where T : class, ICommandHandler
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>();
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="command">由拦截器传入的的命令处理器的命令名称。</param>
    /// <param name="creator">由拦截器传入的命令处理器创建方法。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder,
        string? command, LegacyCommandObjectCreator creator)
        where T : class, ICommandHandler
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(command, creator);
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static ILegacyCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder, Action<T> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(t =>
            {
                handler(t);
                return Task.FromResult(0);
            });
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ILegacyCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder,
        string? command, LegacyCommandObjectCreator creator, Action<T> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(command, creator, t =>
            {
                handler(t);
                return Task.FromResult(0);
            });
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyAsyncCommandRunnerBuilder builder, Action<T> handler)
        where T : class
    {
        return (ILegacyAsyncCommandRunnerBuilder)((ILegacyCoreCommandRunnerBuilder)builder).AddHandler<T>(handler);
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyAsyncCommandRunnerBuilder builder,
        string? command, LegacyCommandObjectCreator creator, Action<T> handler)
        where T : class
    {
        return (ILegacyAsyncCommandRunnerBuilder)((ILegacyCoreCommandRunnerBuilder)builder).AddHandler<T>(command, creator, handler);
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static ILegacyCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder, Func<T, int> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(t => Task.FromResult(handler(t)));
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ILegacyCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder,
        string? command, LegacyCommandObjectCreator creator, Func<T, int> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(command, creator, t => Task.FromResult(handler(t)));
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyAsyncCommandRunnerBuilder builder, Func<T, int> handler)
        where T : class
    {
        return (ILegacyAsyncCommandRunnerBuilder)((ILegacyCoreCommandRunnerBuilder)builder).AddHandler<T>(handler);
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyAsyncCommandRunnerBuilder builder,
        string? command, LegacyCommandObjectCreator creator, Func<T, int> handler)
        where T : class
    {
        return (ILegacyAsyncCommandRunnerBuilder)((ILegacyCoreCommandRunnerBuilder)builder).AddHandler<T>(command, creator, handler);
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder, Func<T, Task> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(async t =>
            {
                await handler(t);
                return 0;
            });
    }

    /// <inheritdoc cref="AddHandler{T}(ILegacyCoreCommandRunnerBuilder,Func{T, Task{int}})" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder,
        string? command, LegacyCommandObjectCreator creator, Func<T, Task> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(command, creator, async t =>
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
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder, Func<T, Task<int>> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(handler);
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="command">由拦截器传入的的命令处理器的命令名称。</param>
    /// <param name="creator">由拦截器传入的命令处理器创建方法。</param>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ILegacyAsyncCommandRunnerBuilder AddHandler<T>(this ILegacyCoreCommandRunnerBuilder builder,
        string? command, LegacyCommandObjectCreator creator, Func<T, Task<int>> handler)
        where T : class
    {
        return builder.GetOrCreateRunner()
            .AddHandler<T>(command, creator, handler);
    }

    /// <summary>
    /// 添加一个命令处理器集合。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <typeparam name="T">命令处理器集合的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static ILegacyAsyncCommandRunnerBuilder AddHandlers<T>(this ILegacyCoreCommandRunnerBuilder builder)
        where T : ICommandHandlerCollection, new()
    {
        return builder.GetOrCreateRunner()
            .AddHandlers<T>();
    }

    /// <summary>
    /// 添加支持 GNU 标准的命令行通用参数。这将在无参数，带 --help 参数和带 --version 参数时得到通用的响应。<br/>
    /// 考虑到几乎没有开发者认为这个方法的行为符合预期，我们移除了这个功能。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <returns>命令行执行器构造的链式调用。</returns>
    /// <exception cref="NotSupportedException">任何时候调用这个方法都会抛出这个异常。</exception>
    [Obsolete("Considering that almost no developer thinks the behavior of this method meets expectations, we removed this feature.", true)]
    public static ILegacyAsyncCommandRunnerBuilder AddStandardHandlers(this ILegacyCoreCommandRunnerBuilder builder)
    {
        throw new NotSupportedException("Considering that almost no developer thinks the behavior of this method meets expectations, we removed this feature.");
    }
}
