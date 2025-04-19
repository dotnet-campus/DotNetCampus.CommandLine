using System;
using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 测试各种 AddHandler 重载方法的行为。
/// </summary>
[TestClass]
public class AddHandlerTests
{
    private CommandLineParsingOptions Flexible { get; } = CommandLineParsingOptions.Flexible;

    #region 1. 基本处理器注册测试

    [TestMethod("1.1. 使用 Action 委托处理普通选项类")]
    public void RegisterHandler_WithActionDelegate()
    {
        // Arrange
        string[] args = ["--value", "test-value"];
        string? capturedValue = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<SimpleOptions>(o => capturedValue = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test-value", capturedValue);
    }

    [TestMethod("1.2. 使用 Func<int> 委托处理普通选项类并返回退出代码")]
    public void RegisterHandler_WithFuncIntDelegate()
    {
        // Arrange
        string[] args = ["--value", "test-value"];
        string? capturedValue = null;
        const int expectedExitCode = 42;

        // Act
        int exitCode = CommandLine.Parse(args, Flexible)
            .AddHandler<SimpleOptions>(o =>
            {
                capturedValue = o.Value;
                return expectedExitCode;
            })
            .Run();

        // Assert
        Assert.AreEqual("test-value", capturedValue);
        Assert.AreEqual(expectedExitCode, exitCode);
    }

    [TestMethod("1.3. 使用 Func<Task> 委托处理异步操作")]
    public async Task RegisterHandler_WithFuncTaskDelegate()
    {
        // Arrange
        string[] args = ["--value", "test-value"];
        string? capturedValue = null;
        var taskCompletionSource = new TaskCompletionSource<bool>();

        // Act
        await CommandLine.Parse(args, Flexible)
            .AddHandler<SimpleOptions>(async o =>
            {
                await Task.Delay(10); // 模拟异步操作
                capturedValue = o.Value;
                taskCompletionSource.SetResult(true);
            })
            .RunAsync();

        // Assert
        Assert.IsTrue(await taskCompletionSource.Task);
        Assert.AreEqual("test-value", capturedValue);

        // 确保有返回值，避免警告
        await Task.CompletedTask;
    }

    [TestMethod("1.4. 使用 Func<Task<int>> 委托处理异步操作并返回退出代码")]
    public async Task RegisterHandler_WithFuncTaskIntDelegate()
    {
        // Arrange
        string[] args = ["--value", "test-value"];
        string? capturedValue = null;
        const int expectedExitCode = 42;

        // Act
        int exitCode = await CommandLine.Parse(args, Flexible)
            .AddHandler<SimpleOptions>(async o =>
            {
                await Task.Delay(10); // 模拟异步操作
                capturedValue = o.Value;
                return expectedExitCode;
            })
            .RunAsync();

        // Assert
        Assert.AreEqual("test-value", capturedValue);
        Assert.AreEqual(expectedExitCode, exitCode);

        // 确保有返回值，避免警告
        await Task.CompletedTask;
    }

    [TestMethod("1.5. 注册实现 ICommandHandler 接口的类")]
    public async Task RegisterHandler_WithICommandHandlerImplementation()
    {
        // Arrange
        string[] args = ["--handler-option", "handler-value"];

        // Act
        int exitCode = await CommandLine.Parse(args, Flexible)
            .AddHandler<SimpleCommandHandler>()
            .RunAsync();

        // Assert
        Assert.AreEqual(SimpleCommandHandler.ExpectedExitCode, exitCode);
        Assert.IsTrue(SimpleCommandHandler.WasHandlerCalled);
        Assert.AreEqual("handler-value", SimpleCommandHandler.CapturedValue);

        // Reset static state for other tests
        SimpleCommandHandler.ResetState();

        // 确保有返回值，避免警告
        await Task.CompletedTask;
    }

    #endregion

    #region 2. 命令谓词测试

    [TestMethod("2.1. 使用谓词匹配并处理命令")]
    public void RegisterHandler_WithVerb_MatchesCorrectCommand()
    {
        // Arrange
        string[] args = ["add", "item1"];
        string? capturedAddItem = null;
        string? capturedRemoveItem = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<AddOptions>(o => capturedAddItem = o.ItemToAdd)
            .AddHandler<RemoveOptions>(o => capturedRemoveItem = o.ItemToRemove)
            .Run();

        // Assert
        Assert.AreEqual("item1", capturedAddItem);
        Assert.IsNull(capturedRemoveItem);
    }

    [TestMethod("2.2. 多个谓词时匹配正确的命令")]
    public void RegisterHandler_WithMultipleVerbs_MatchesCorrectCommand()
    {
        // Arrange
        string[] args = ["remove", "item2"];
        string? capturedAddItem = null;
        string? capturedRemoveItem = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<AddOptions>(o => capturedAddItem = o.ItemToAdd)
            .AddHandler<RemoveOptions>(o => capturedRemoveItem = o.ItemToRemove)
            .Run();

        // Assert
        Assert.IsNull(capturedAddItem);
        Assert.AreEqual("item2", capturedRemoveItem);
    }

    [TestMethod("2.3. 未提供谓词时不匹配任何命令抛出CommandVerbNotFoundException")]
    public void RegisterHandler_WithNoVerbProvided_ThrowsCommandVerbNotFoundException()
    {
        // Arrange
        string[] args = ["item3"];
        bool addHandlerCalled = false;
        bool removeHandlerCalled = false;

        // Act & Assert
        var exception = Assert.ThrowsException<CommandVerbNotFoundException>(() => {
            CommandLine.Parse(args, Flexible)
                .AddHandler<AddOptions>(_ => addHandlerCalled = true)
                .AddHandler<RemoveOptions>(_ => removeHandlerCalled = true)
                .Run();
        });

        // 确认异常包含正确的谓词信息
        Assert.IsTrue(exception.Message.Contains("item3"));
        Assert.IsFalse(addHandlerCalled);
        Assert.IsFalse(removeHandlerCalled);
    }

    [TestMethod("2.4. 使用默认处理器处理无谓词命令")]
    public void RegisterHandler_WithDefaultHandler_HandlesNoVerbCommand()
    {
        // Arrange
        string[] args = ["--help"];
        bool defaultHandlerCalled = false;
        bool otherHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<DefaultOptions>(o => {
                defaultHandlerCalled = true;
                Assert.IsTrue(o.ShowHelp);
            })
            .AddHandler<AddOptions>(_ => otherHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsTrue(defaultHandlerCalled);
        Assert.IsFalse(otherHandlerCalled);
    }

    #endregion

    #region 3. 链式调用与处理器顺序测试

    [TestMethod("3.1. 链式调用返回正确类型")]
    public void ChainedCalls_ReturnCorrectBuilderTypes()
    {
        // Arrange
        string[] args = [];
        var commandLine = CommandLine.Parse(args, Flexible);

        // Act
        var syncBuilder = commandLine.AddHandler<SimpleOptions>(_ => { });
        var asyncBuilder = commandLine.AddHandler<SimpleOptions>(async _ => {
            await Task.Delay(1);
            return 0;
        });

        // Assert
        Assert.IsInstanceOfType(syncBuilder, typeof(ICommandRunnerBuilder));
        Assert.IsInstanceOfType(asyncBuilder, typeof(IAsyncCommandRunnerBuilder));
    }

    [TestMethod("3.2. 可以混合链式调用同步和异步处理器")]
    public async Task ChainedCalls_MixedSyncAndAsyncHandlers()
    {
        // Arrange
        string[] args = ["add", "test-item"];
        bool syncHandlerCalled = false;
        bool asyncHandlerCalled = false;
        string? capturedItem = null;

        // Act
        await CommandLine.Parse(args, Flexible)
            .AddHandler<AddOptions>(o => {
                syncHandlerCalled = true;
                capturedItem = o.ItemToAdd;
            })
            .AddHandler<RemoveOptions>(async _ => {
                await Task.Delay(10);
                asyncHandlerCalled = true;
            })
            .RunAsync();

        // Assert
        Assert.IsTrue(syncHandlerCalled);
        Assert.IsFalse(asyncHandlerCalled);
        Assert.AreEqual("test-item", capturedItem);

        // 确保有返回值，避免警告
        await Task.CompletedTask;
    }

    [TestMethod("3.3. 处理器注册顺序不影响谓词匹配")]
    public void HandlerRegistrationOrder_DoesNotAffectVerbMatching()
    {
        // Arrange
        string[] args = ["add", "test-item"];
        bool addHandlerCalled = false;
        bool removeHandlerCalled = false;

        // Act - 先注册remove处理器，再注册add处理器
        CommandLine.Parse(args, Flexible)
            .AddHandler<RemoveOptions>(_ => removeHandlerCalled = true)
            .AddHandler<AddOptions>(_ => addHandlerCalled = true)
            .Run();

        // Assert - 仍然匹配到正确的add处理器
        Assert.IsTrue(addHandlerCalled);
        Assert.IsFalse(removeHandlerCalled);
    }

    #endregion

    #region 4. 错误处理测试

    [TestMethod("4.1. 处理器中抛出的异常会传递给调用者")]
    public void HandlerException_PropagatedToCaller()
    {
        // Arrange
        string[] args = ["--value", "test"];
        var expectedException = new InvalidOperationException("Test exception");        // Act & Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(() => {
            CommandLine.Parse(args, Flexible)
                .AddHandler<SimpleOptions>(new Action<SimpleOptions>(_ => { throw expectedException; }))
                .Run();
        });

        Assert.AreEqual(expectedException.Message, exception.Message);
    }

    [TestMethod("4.2. 未找到匹配的处理器时抛出CommandVerbNotFoundException")]
    public void NoMatchingHandler_ThrowsCommandVerbNotFoundException()
    {
        // Arrange
        string[] args = ["unknown-verb"];

        // Act & Assert
        var exception = Assert.ThrowsException<CommandVerbNotFoundException>(() => {
            CommandLine.Parse(args, Flexible)
                .AddHandler<AddOptions>(_ => { })
                .AddHandler<RemoveOptions>(_ => { })
                .Run();
        });

        // 确认异常包含正确的谓词信息
        Assert.IsTrue(exception.Message.Contains("unknown-verb"));
    }

    [TestMethod("4.3. 必需属性未赋值时抛出异常")]
    public void RequiredPropertyNotAssigned_ThrowsException()
    {
        // Arrange
        string[] args = ["add"]; // 缺少 ItemToAdd 参数

        // Act & Assert
        Assert.ThrowsException<RequiredPropertyNotAssignedException>(() => {
            CommandLine.Parse(args, Flexible)
                .AddHandler<AddOptions>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 5. 自动发现命令处理器测试

    // 注意：由于有其他大量测试类污染源生成器搜集到的命令处理器，所以目前暂时无法测试。
    // 这里保留两个测试方法的结构，但在实际运行时可能需要跳过。

    [TestMethod("5.1. 通过程序集自动发现并添加命令处理器")]
    [Ignore("需要源生成器支持，在单元测试环境中可能无法正常运行")]
    public void AddHandlers_DiscoverCommandHandlers()
    {
        // 由于有其他大量测试类污染源生成器搜集到的命令处理器，所以目前暂时无法测试。
    }

    [TestMethod("5.2. 自动发现的默认处理器在没有匹配谓词时被调用")]
    [Ignore("需要源生成器支持，在单元测试环境中可能无法正常运行")]
    public void AddHandlers_DefaultHandlerCalled_WhenNoVerbMatched()
    {
        // 由于有其他大量测试类污染源生成器搜集到的命令处理器，所以目前暂时无法测试。
    }

    #endregion
}

#region 测试用数据模型

// 简单选项类
internal class SimpleOptions
{
    [Option("value")]
    public string? Value { get; init; }
}

// AddOptions 类 - 用于测试谓词命令
[Verb("add")]
internal class AddOptions
{
    [Value(0)]
    public required string ItemToAdd { get; init; }
}

// RemoveOptions 类 - 用于测试谓词命令
[Verb("remove")]
internal class RemoveOptions
{
    [Value(0)]
    public required string ItemToRemove { get; init; }
}

// 默认选项类 - 用于测试无谓词命令
internal class DefaultOptions
{
    [Option('h', "help")]
    public bool ShowHelp { get; init; }
}

// 实现 ICommandHandler 接口的处理器
internal class SimpleCommandHandler : ICommandHandler
{
    public static bool WasHandlerCalled { get; private set; }
    public static string? CapturedValue { get; private set; }
    public const int ExpectedExitCode = 42;

    [Option("handler-option")]
    public required string HandlerOption { get; init; }

    public Task<int> RunAsync()
    {
        WasHandlerCalled = true;
        CapturedValue = HandlerOption;
        return Task.FromResult(ExpectedExitCode);
    }

    public static void ResetState()
    {
        WasHandlerCalled = false;
        CapturedValue = null;
    }
}

// 注释掉自动发现命令处理器的相关代码，因为有其他大量测试类污染源生成器搜集到的命令处理器，所以目前暂时无法测试。
/*
// 用于测试自动发现命令处理器的类
[CollectCommandHandlersFromThisAssembly]
internal partial class TestCommandHandlerCollection : ICommandHandlerCollection
{
}

// 自动发现的示例命令处理器
[Verb("sample")]
internal class SampleCommandHandler : ICommandHandler
{
    public static bool WasHandlerCalled { get; private set; }
    public static string? CapturedOption { get; private set; }

    [Option("sample-property")]
    public required string Option { get; init; }

    public Task<int> RunAsync()
    {
        WasHandlerCalled = true;
        CapturedOption = Option;
        return Task.FromResult(0);
    }

    public static void ResetState()
    {
        WasHandlerCalled = false;
        CapturedOption = null;
    }
}

// 自动发现的默认命令处理器
internal class DefaultCommandHandler : ICommandHandler
{
    public static bool WasHandlerCalled { get; private set; }
    public static bool HelpRequested { get; private set; }

    [Option('h', "help")]
    public bool ShowHelp { get; init; }

    public Task<int> RunAsync()
    {
        WasHandlerCalled = true;
        HelpRequested = ShowHelp;
        return Task.FromResult(0);
    }

    public static void ResetState()
    {
        WasHandlerCalled = false;
        HelpRequested = false;
    }
}
*/
#endregion
