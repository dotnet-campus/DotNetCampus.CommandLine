using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Tests.ParsingStyles;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.CommandMatching;

[TestClass]
public class MatchCommandTests
{
    [TestMethod]
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.Windows, DisplayName = "[Windows]")]
    [DataRow(new[] { "test://" }, nameof(DefaultOptions), "Default", TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "unknown://" }, nameof(DefaultOptions), "unknown://", TestCommandLineStyle.Url, DisplayName = "[Url] unknown://")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "test://foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Url, DisplayName = "[Url] test://foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Windows, DisplayName = "[Windows] foo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] fooo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] fooo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] fooo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.Windows, DisplayName = "[Windows] fooo")]
    [DataRow(new[] { "test://fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.Url, DisplayName = "[Url] test://fooo")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] bar baz")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] bar baz")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] bar baz")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.Windows, DisplayName = "[Windows] bar baz")]
    [DataRow(new[] { "test://bar/baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.Url, DisplayName = "[Url] test://bar/baz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] bar bazz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] bar bazz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] bar bazz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.Windows, DisplayName = "[Windows] bar bazz")]
    [DataRow(new[] { "test://bar/bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.Url, DisplayName = "[Url] test://bar/bazz")]
    [DataRow(new[] { "another", "sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Flexible,
        DisplayName = "[Flexible] another sub-command")]
    [DataRow(new[] { "another", "sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.DotNet,
        DisplayName = "[DotNet] another sub-command")]
    [DataRow(new[] { "another", "sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Gnu,
        DisplayName = "[Gnu] another sub-command")]
    [DataRow(new[] { "Another", "SubCommand" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Windows,
        DisplayName = "[Windows] Another SubCommand")]
    [DataRow(new[] { "another", "subCommand" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Windows,
        DisplayName = "[Windows] another subCommand")]
    [DataRow(new[] { "test://another/sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Url,
        DisplayName = "[Url] test://another/sub-command")]
    public void MatchCommand(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        string? matched = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine.ToRunner()
            .AddHandler<DefaultOptions>(o => matched = o.Value)
            .AddHandler<FooOptions>(o => matched = o.Value)
            .AddHandler<BarOptions>(o => matched = o.Value)
            .AddHandler<BarBazOptions>(o => matched = o.Value)
            .AddHandler<SubCommandOptions>(o => matched = o.Value)
            .Run();
        var matchedTypeName = result.HandledBy!.GetType().Name;

        // Assert
        Assert.AreEqual(expectedCommand, matchedTypeName);
        Assert.AreEqual(expectedValue, matched);
    }

    [TestMethod]
    [DataRow(new[] { "another", "sub-command" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] another sub-command")]
    public void MatchCommand_PositionalArgumentNotMatch(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var matched = "";
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.ToRunner()
            .AddHandler<DefaultOptions>(o => matched = o.Value)
            .AddHandler<FooOptions>(o => matched = o.Value)
            .AddHandler<BarBazOptions>(o => matched = o.Value)
            .AddHandler<SubCommandOptions>(o => matched = o.Value)
            .Run());

        // Assert
        Assert.IsEmpty(matched);
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
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

    [Command("bar baz")]
    public record BarBazOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "BarBaz";
    }

    [Command("bar qux")]
    public record BarQuxOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "BarQux";
    }

    [Command("another sub-command")]
    public record SubCommandOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "AnotherSubCommand";
    }
}
