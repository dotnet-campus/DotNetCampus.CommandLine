using System.ComponentModel;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Utils.Handlers;

namespace DotNetCampus.Cli;

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
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this CommandLine builder)
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 为命令执行器指定一个状态对象，后续添加的命令处理器如果被执行，将会收到这个状态对象。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="state">状态对象。</param>
    /// <typeparam name="TState">状态对象的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static StatedCommandRunnerBuilder<TState> ForState<TState>(this CommandLine builder, TState state)
    {
        return new StatedCommandRunnerBuilder<TState>(((ICommandRunnerBuilder)builder).AsRunner(), state);
    }

    /// <inheritdoc cref="AddHandler{T}(CommandLine,Func{T, Task{int}})" />
    public static ICommandRunnerBuilder AddHandler<T>(this CommandLine builder, Action<T> handler)
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(CommandLine,Func{T, Task{int}})" />
    public static ICommandRunnerBuilder AddHandler<T>(this CommandLine builder, Func<T, int> handler)
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(CommandLine,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this CommandLine builder, Func<T, Task> handler)
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this CommandLine builder, Func<T, Task<int>> handler)
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder)
        where T : class, ICommandHandler
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 为命令执行器指定一个状态对象，后续添加的命令处理器如果被执行，将会收到这个状态对象。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="state">状态对象。</param>
    /// <typeparam name="TState">状态对象的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static StatedCommandRunnerBuilder<TState> ForState<TState>(this ICommandRunnerBuilder builder, TState state)
    {
        return new StatedCommandRunnerBuilder<TState>(builder.AsRunner(), state);
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}})" />
    public static ICommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Action<T> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}})" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ICommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Func<T, int> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Func<T, Task> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Func<T, Task<int>> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder)" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder)
        where T : class, ICommandHandler
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 为命令执行器指定一个状态对象，后续添加的命令处理器如果被执行，将会收到这个状态对象。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="state">状态对象。</param>
    /// <typeparam name="TState">状态对象的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    public static StatedCommandRunnerBuilder<TState> ForState<TState>(this IAsyncCommandRunnerBuilder builder, TState state)
    {
        return new StatedCommandRunnerBuilder<TState>(builder.AsRunner(), state);
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Action<T> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, int> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, Task> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}})" />
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, Task<int>> handler)
        where T : class
    {
        throw CommandLine.MethodShouldBeInspected();
    }

    /// <summary>
    /// 由拦截器调用，用于添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="command">由拦截器传入的的命令处理器的命令，<see langword="null"/> 或空字符串表示此处理器没有命令名称。</param>
    /// <param name="metadata">由拦截器传入的包含命令对象如何创建和运行的元数据。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class, ICommandHandler
    {
        return builder.AsRunner()
            .AddHandlerCore(command, metadata);
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class, ICommandHandler
    {
        return builder.AsRunner()
            .AddHandlerCore(command, metadata);
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}},NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ICommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Action<T> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousActionCommandHandler<T>(metadata, handler));
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}},NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ICommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Func<T, int> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousFuncInt32CommandHandler<T>(metadata, handler));
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}},NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Func<T, Task> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousFuncTaskCommandHandler<T>(metadata, handler));
    }

    /// <summary>
    /// 由拦截器调用，用于添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <param name="command">由拦截器传入的的命令处理器的命令，<see langword="null"/> 或空字符串表示此处理器没有命令名称。</param>
    /// <param name="metadata">由拦截器传入的包含命令对象如何创建和运行的元数据。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this ICommandRunnerBuilder builder, Func<T, Task<int>> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousFuncTaskInt32CommandHandler<T>(metadata, handler));
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}},NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Action<T> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousActionCommandHandler<T>(metadata, handler));
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}},NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, int> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousFuncInt32CommandHandler<T>(metadata, handler));
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}},NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, Task> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousFuncTaskCommandHandler<T>(metadata, handler));
    }

    /// <inheritdoc cref="AddHandler{T}(ICommandRunnerBuilder,Func{T, Task{int}},NamingPolicyNameGroup,ICommandObjectMetadata)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, Task<int>> handler,
        NamingPolicyNameGroup command, ICommandObjectMetadata metadata)
        where T : class
    {
        return builder.AsRunner()
            .AddHandlerCore(command, new AnonymousFuncTaskInt32CommandHandler<T>(metadata, handler));
    }

    /// <summary>
    /// 添加一个命令处理器集合。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <typeparam name="T">命令处理器集合的类型。</typeparam>
    /// <returns>命令行执行器构造的链式调用。</returns>
    [Obsolete("我们正在考虑更好的实现方式。此前这个依赖于模块初始化器，但我们正在用拦截器替换它。", true)]
    public static IAsyncCommandRunnerBuilder AddHandlers<T>(this ICommandRunnerBuilder builder)
        // where T : class, ICommandHandlerCollection, new()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 添加支持 GNU 标准的命令行通用参数。这将在无参数，带 --help 参数和带 --version 参数时得到通用的响应。<br/>
    /// 考虑到几乎没有开发者认为这个方法的行为符合预期，我们移除了这个功能。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <returns>命令行执行器构造的链式调用。</returns>
    /// <exception cref="NotSupportedException">任何时候调用这个方法都会抛出这个异常。</exception>
    [Obsolete("Considering that almost no developer thinks the behavior of this method meets expectations, we removed this feature.", true)]
    public static IAsyncCommandRunnerBuilder AddStandardHandlers(this ICommandRunnerBuilder builder)
    {
        throw new NotSupportedException("Considering that almost no developer thinks the behavior of this method meets expectations, we removed this feature.");
    }
}
