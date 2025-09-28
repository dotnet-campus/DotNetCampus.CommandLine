using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.CommandMatching;

[TestClass]
public class AddHandlerTests
{
    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(DefaultHandler), "DefaultHandler", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(DefaultHandler), "DefaultHandler", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(DefaultHandler), "DefaultHandler", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(DefaultHandler), "DefaultHandler", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultHandler>()
            .RunAsync();
        var matchedTypeName = result.Result.HandledBy!.GetType().Name;
        var exitCode = result.Result.ExitCode;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(1, exitCode);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler_Action(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        string? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultOptions>(o => matched = o.Value)
            .AddHandler<FooOptions>(o => matched = o.Value)
            .AddHandler<BarOptions>(o => matched = o.Value)
            .Run();
        var matchedTypeName = result.HandledBy!.GetType().Name;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler_FuncInt32(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        string? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultOptions>(o => RunWithExitCode(ref matched, o.Value))
            .AddHandler<FooOptions>(o => RunWithExitCode(ref matched, o.Value))
            .AddHandler<BarOptions>(o => RunWithExitCode(ref matched, o.Value))
            .Run();
        var matchedTypeName = result.HandledBy!.GetType().Name;
        var exitCode = result.ExitCode;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched);
        Assert.AreEqual(1, exitCode);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler_FuncTask(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        Task<string?>? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultOptions>(o => matched = Task.FromResult(o.Value))
            .AddHandler<FooOptions>(o => matched = Task.FromResult(o.Value))
            .AddHandler<BarOptions>(o => matched = Task.FromResult(o.Value))
            .RunAsync();
        var matchedTypeName = result.Result.HandledBy!.GetType().Name;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched?.Result);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler_FuncTaskInt32(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        Task<string?>? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultOptions>(o => RunWithExitCode(ref matched, o.Value))
            .AddHandler<FooOptions>(o => RunWithExitCode(ref matched, o.Value))
            .AddHandler<BarOptions>(o => RunWithExitCode(ref matched, o.Value))
            .RunAsync();
        var matchedTypeName = result.Result.HandledBy!.GetType().Name;
        var exitCode = result.Result.ExitCode;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched?.Result);
        Assert.AreEqual(1, exitCode);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler_Mix1(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        Task<string?>? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<FooOptions>(o => RunSyncWithExitCode(ref matched, o.Value))
            .AddHandler<BarOptions>(o => RunWithExitCode(ref matched, o.Value))
            .AddHandler<DefaultHandler>()
            .RunAsync();
        var matchedTypeName = result.Result.HandledBy!.GetType().Name;
        var exitCode = result.Result.ExitCode;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched?.Result);
        Assert.AreEqual(1, exitCode);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler_Mix2(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        Task<string?>? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultOptions>(o => RunWithExitCode(ref matched, o.Value))
            .AddHandler<FooOptions>(o => RunSyncWithExitCode(ref matched, o.Value))
            .AddHandler<BarOptions>(o => RunSyncVoidWithExitCode(ref matched, o.Value))
            .RunAsync();
        var matchedTypeName = result.Result.HandledBy!.GetType().Name;
        var exitCode = result.Result.ExitCode;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched?.Result);
        Assert.AreEqual(1, exitCode);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    public void AddHandler_Mix3(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        Task<string?>? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<FooOptions>(o => RunSyncWithExitCode(ref matched, o.Value))
            .AddHandler<DefaultHandler>()
            .RunAsync();
        var matchedTypeName = result.Result.HandledBy!.GetType().Name;
        var exitCode = result.Result.ExitCode;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched?.Result);
        Assert.AreEqual(1, exitCode);
    }

    // ReSharper disable once RedundantAssignment
    private int RunWithExitCode<T>(ref T field, T value)
    {
        field = value;
        return 1;
    }

    // ReSharper disable once RedundantAssignment
    private Task<int> RunWithExitCode<T>(ref Task<T>? field, T value)
    {
        field = Task.FromResult(value);
        return Task.FromResult(1);
    }

    // ReSharper disable once RedundantAssignment
    private int RunSyncWithExitCode<T>(ref Task<T>? field, T value)
    {
        field = Task.FromResult(value);
        return 1;
    }

    // ReSharper disable once RedundantAssignment
    private void RunSyncVoidWithExitCode<T>(ref Task<T>? field, T value)
    {
        field = Task.FromResult(value);
    }

    public record DefaultOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "Default";
    }

    [Command("foo")]
    public record FooOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "Foo";
    }

    [Command("bar")]
    public record BarOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "Bar";
    }

    public record DefaultHandler : ICommandHandler
    {
        [Value(0)]
        public string? Value { get; set; } = "DefaultHandler";

        public Task<int> RunAsync()
        {
            return Task.FromResult(1);
        }
    }

    [Command("foo")]
    public record FooHandler : ICommandHandler
    {
        [Value(0)]
        public string? Value { get; set; } = "FooHandler";

        public Task<int> RunAsync()
        {
            return Task.FromResult(2);
        }
    }
}
