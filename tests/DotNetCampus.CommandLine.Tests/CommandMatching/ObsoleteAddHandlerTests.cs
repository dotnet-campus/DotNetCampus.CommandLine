using System;
using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.CommandMatching;

[TestClass]
public class ObsoleteAddHandlerTests
{
    [TestMethod]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    [Obsolete("此单元测试仅为确保开发者旧代码未完成迁移时，至少也能正常工作。")]
    public void AddHandler(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultHandler>()
            .RunAsync();
        var exitCode = result.Result.ExitCode;

        // Assert
        Assert.AreEqual(1, exitCode);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    [Obsolete("此单元测试仅为确保开发者旧代码未完成迁移时，至少也能正常工作。")]
    public void AddHandler_Action(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        string? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHandler<DefaultOptions>(o => matched = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("foo", matched);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    [Obsolete("此单元测试仅为确保开发者旧代码未完成迁移时，至少也能正常工作。")]
    public void AddHandler_FuncInt32(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        string? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultOptions>(o => RunWithExitCode(ref matched, o.Value))
            .Run();
        var exitCode = result.ExitCode;

        // Assert
        Assert.AreEqual("foo", matched);
        Assert.AreEqual(1, exitCode);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    [Obsolete("此单元测试仅为确保开发者旧代码未完成迁移时，至少也能正常工作。")]
    public void AddHandler_FuncTask(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        Task<string?>? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHandler<DefaultOptions>(o => matched = Task.FromResult(o.Value))
            .RunAsync();

        // Assert
        Assert.AreEqual("foo", matched?.Result);
    }

    [TestMethod]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "foo" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    [Obsolete("此单元测试仅为确保开发者旧代码未完成迁移时，至少也能正常工作。")]
    public void AddHandler_FuncTaskInt32(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        Task<string?>? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHandler<DefaultOptions>(o => RunWithExitCode(ref matched, o.Value))
            .RunAsync();
        var exitCode = result.Result.ExitCode;

        // Assert
        Assert.AreEqual("foo", matched?.Result);
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

    public record DefaultOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "Default";
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
}
