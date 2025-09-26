using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.CommandMatching;

[TestClass]
public class AddHandlerWithStateTests
{
    [TestMethod]
    [DataRow(new[] { "test1", "--option=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] test1 --option=value")]
    [DataRow(new[] { "test1", "--option=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] test1 --option=value")]
    [DataRow(new[] { "test1", "--option=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] test1 --option=value")]
    [DataRow(new[] { "test1", "-Option=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] test1 -Option=value")]
    public async Task AddHandlerWithState1(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = await commandLine.ToRunner()
            .ForState(123).AddHandler<Test1Handler>()
            .ForState("test2").AddHandler<Test2Handler>()
            .ForState().AddHandler<Test0Handler>()
            .RunAsync();
        var state = ((Test1Handler)result.HandledBy!).State;

        // Assert
        Assert.AreEqual(123, state);
    }

    [TestMethod]
    [DataRow(new[] { "test2", "--option=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] test1 --option=value")]
    [DataRow(new[] { "test2", "--option=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] test1 --option=value")]
    [DataRow(new[] { "test2", "--option=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] test1 --option=value")]
    [DataRow(new[] { "test2", "-Option=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] test1 -Option=value")]
    public async Task AddHandlerWithState2(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = await commandLine.ToRunner()
            .ForState(123).AddHandler<Test1Handler>()
            .ForState("test2").AddHandler<Test2Handler>()
            .ForState().AddHandler<Test0Handler>()
            .RunAsync();
        var state = ((Test2Handler)result.HandledBy!).State;

        // Assert
        Assert.AreEqual("test2", state);
    }

    [TestMethod]
    [DataRow(new[] { "test1", "test", "--option=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] test1 --option=value")]
    [DataRow(new[] { "test1", "test", "--option=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] test1 --option=value")]
    [DataRow(new[] { "test1", "test", "--option=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] test1 --option=value")]
    [DataRow(new[] { "test1", "test", "-Option=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] test1 -Option=value")]
    public async Task MultipleAddHandlerWithState1(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = await commandLine.ToRunner()
            .ForState(123).AddHandler<Test1Handler>().AddHandler<Test11Handler>()
            .ForState("test2").AddHandler<Test2Handler>()
            .ForState().AddHandler<Test0Handler>()
            .RunAsync();
        var state = ((Test11Handler)result.HandledBy!).State;

        // Assert
        Assert.AreEqual(123, state);
    }

    public record Test0Handler : ICommandHandler
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        public Task<int> RunAsync()
        {
            return Task.FromResult(0);
        }
    }

    [Command("test1")]
    public record Test1Handler : ICommandHandler<int>
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        public int? State { get; private set; }

        public Task<int> RunAsync(int state)
        {
            State = state;
            return Task.FromResult(0);
        }
    }

    [Command("test1 test")]
    public record Test11Handler : ICommandHandler<int>
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        public int? State { get; private set; }

        public Task<int> RunAsync(int state)
        {
            State = state;
            return Task.FromResult(0);
        }
    }

    [Command("test2")]
    public record Test2Handler : ICommandHandler<string>
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        public string? State { get; private set; }

        public Task<int> RunAsync(string state)
        {
            State = state;
            return Task.FromResult(0);
        }
    }
}

file static class Extensions
{
    // public static StatedCommandRunnerLinkedBuilder<TState> AddHandler<TState, T>(this StatedCommandRunnerBuilder<TState> builder)
    //     where T : class, ICommandHandler<TState>
    // {
    //     throw CommandLine.MethodShouldBeInspected();
    // }
    // public static global::DotNetCampus.Cli.StatedCommandRunnerLinkedBuilder<TState> AddHandler<TState, T>(this global::DotNetCampus.Cli.StatedCommandRunnerBuilder<TState> builder)
    //     where T : class, global::DotNetCampus.Cli.ICommandHandler<TState>
    // {
    //     // 请确保 Test2Handler 类型中至少有一个属性标记了 [Option] 或 [Value] 特性；
    //     // 否则下面的 AddHandlerWithStateTests_Test2HandlerBuilder 类型将不存在，导致编译不通过。
    //     return builder.AddHandler<T>(global::DotNetCampus.Cli.Tests.CommandMatching.AddHandlerWithStateTests_Test2HandlerBuilder.CommandNameGroup, global::DotNetCampus.Cli.Tests.CommandMatching.AddHandlerWithStateTests_Test2HandlerBuilder.CreateInstance);
    // }
}
