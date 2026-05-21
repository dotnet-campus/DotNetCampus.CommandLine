using System;
using System.IO;
using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.Help;

[TestClass]
public class AddHelpHandlerTests
{
    [TestMethod]
    public void AddHelpHandler_WithHelp_ReturnsExitCode0()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var result = CommandLine.Parse(["--help"])
                .AddHandler<SimpleOptions>(o => { })
                .AddHelpHandler()
                .Run();
            Assert.AreEqual(0, result.ExitCode);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [TestMethod]
    public void AddHelpHandler_WithHelp_WritesToConsoleOut()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            CommandLine.Parse(["--help"])
                .AddHandler<SimpleOptions>(o => { })
                .AddHelpHandler()
                .Run();
            var output = writer.ToString();
            Assert.IsTrue(output.Contains("用法:"));
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [TestMethod]
    public void AddHelpHandler_WithoutHelp_RunsNormally()
    {
        var handled = false;
        var result = CommandLine.Parse(["-n", "test"])
            .AddHandler<SimpleOptions>(o => handled = true)
            .AddHelpHandler()
            .Run();
        Assert.IsTrue(handled);
    }

    [TestMethod]
    public void WithoutAddHelpHandler_HelpArgIsNormal()
    {
        // 没有调用 AddHelpHandler()，--help 被当作普通选项
        // 这里期望正常解析流程不做帮助检测
        var handled = false;
        var result = CommandLine.Parse(["--help"], CommandLineParsingOptions.Flexible)
            .AddHandler<SimpleOptions>(o => handled = true)
            .Run();
        Assert.IsTrue(handled);
    }

    [TestMethod]
    public void AddHelpHandler_CommandSpecificHelp()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            CommandLine.Parse(["sub", "--help"])
                .AddHandler<SimpleOptions>(o => { })
                .AddHandler<SubOptions>(o => { })
                .AddHelpHandler()
                .Run();
            var output = writer.ToString();
            Assert.IsTrue(output.Contains("sub"));
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [TestMethod]
    public void AddHelpHandler_RootHelp_ListsAllCommands()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            CommandLine.Parse(["--help"])
                .AddHandler<SubOptions>(o => { })
                .AddHandler<AnotherSubOptions>(o => { })
                .AddHelpHandler()
                .Run();
            var output = writer.ToString();
            Assert.IsTrue(output.Contains("命令:"));
            Assert.IsTrue(output.Contains("sub"));
            Assert.IsTrue(output.Contains("another"));
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [TestMethod]
    public void AddHelpHandler_OnAsyncBuilder()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var result = CommandLine.Parse(["--help"])
                .AddHandler<SimpleHandler>()
                .AddHelpHandler()
                .RunAsync().Result;
            Assert.AreEqual(0, result.ExitCode);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [TestMethod]
    public void AddHelpHandler_OnCommandLineDirectly()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var result = CommandLine.Parse(["--help"])
                .AddHelpHandler()
                .AddHandler<SimpleOptions>(o => { })
                .Run();
            Assert.AreEqual(0, result.ExitCode);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [TestMethod]
    public void AddHelpHandler_DotNetStyle_DashH()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var result = CommandLine.Parse(["-h"], CommandLineParsingOptions.DotNet)
                .AddHandler<SimpleOptions>(o => { })
                .AddHelpHandler()
                .Run();
            Assert.AreEqual(0, result.ExitCode);
            Assert.IsTrue(writer.ToString().Contains("用法:"));
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [TestMethod]
    public void AddHelpHandler_WindowsStyle_SlashQuestion()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var result = CommandLine.Parse(["/?"], CommandLineParsingOptions.Windows)
                .AddHandler<SimpleOptions>(o => { })
                .AddHelpHandler()
                .Run();
            Assert.AreEqual(0, result.ExitCode);
            Assert.IsTrue(writer.ToString().Contains("用法:"));
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    public record SimpleOptions
    {
        [Option('n', "name", Description = "名称")]
        public string? Name { get; init; }
    }

    [Command("sub", Description = "子命令")]
    public record SubOptions
    {
        [Option("count", Description = "次数")]
        public int Count { get; init; }
    }

    [Command("another", Description = "另一个子命令")]
    public record AnotherSubOptions
    {
        [Option("flag")]
        public bool Flag { get; init; }
    }

    [Command]
    public class SimpleHandler : ICommandHandler
    {
        public Task<int> RunAsync()
        {
            return Task.FromResult(0);
        }
    }
}
