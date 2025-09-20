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
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] No command")]
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] No command")]
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] No command")]
    [DataRow(new string[] { }, nameof(DefaultOptions), "Default", TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] No command")]
    [DataRow(new[] { "test://" }, nameof(DefaultOptions), "Default", TestCommandLineStyle.Url, DisplayName = "[Url] No command")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] foo")]
    [DataRow(new[] { "test://foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.Url, DisplayName = "[Url] test://foo")]
    [DataRow(new[] { "foo" }, nameof(FooOptions), "Foo", TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] foo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] fooo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] fooo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] fooo")]
    [DataRow(new[] { "fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] fooo")]
    [DataRow(new[] { "test://fooo" }, nameof(DefaultOptions), "fooo", TestCommandLineStyle.Url, DisplayName = "[Url] test://fooo")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] bar baz")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] bar baz")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] bar baz")]
    [DataRow(new[] { "bar", "baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] bar baz")]
    [DataRow(new[] { "test://bar/baz" }, nameof(BarBazOptions), "BarBaz", TestCommandLineStyle.Url, DisplayName = "[Url] test://bar/baz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.Flexible, DisplayName = "[Flexible] bar bazz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.DotNet, DisplayName = "[DotNet] bar bazz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] bar bazz")]
    [DataRow(new[] { "bar", "bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] bar bazz")]
    [DataRow(new[] { "test://bar/bazz" }, nameof(BarOptions), "bazz", TestCommandLineStyle.Url, DisplayName = "[Url] test://bar/bazz")]
    [DataRow(new[] { "another", "sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Flexible,
        DisplayName = "[Flexible] another sub-command")]
    [DataRow(new[] { "another", "sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.DotNet,
        DisplayName = "[DotNet] another sub-command")]
    [DataRow(new[] { "another", "sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Gnu,
        DisplayName = "[Gnu] another sub-command")]
    [DataRow(new[] { "Another", "SubCommand" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.PowerShell,
        DisplayName = "[PowerShell] Another SubCommand")]
    [DataRow(new[] { "another", "subCommand" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.PowerShell,
        DisplayName = "[PowerShell] another subCommand")]
    [DataRow(new[] { "test://another/sub-command" }, nameof(SubCommandOptions), "AnotherSubCommand", TestCommandLineStyle.Url,
        DisplayName = "[Url] test://another/sub-command")]
    public void MatchCommand(string[] args, string expectedCommand, string expectedValue, TestCommandLineStyle style)
    {
        // Arrange
        (string? TypeName, string? Value) matched = default;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHandler<DefaultOptions>(o => matched = (o.GetType().Name, o.Value))
            .AddHandler<FooOptions>(o => matched = (o.GetType().Name, o.Value))
            .AddHandler<BarOptions>(o => matched = (o.GetType().Name, o.Value))
            .AddHandler<BarBazOptions>(o => matched = (o.GetType().Name, o.Value))
            .AddHandler<SubCommandOptions>(o => matched = (o.GetType().Name, o.Value))
            .Run();

        // Assert
        Assert.AreEqual(expectedCommand, matched.TypeName);
        Assert.AreEqual(expectedValue, matched.Value);
    }

    [TestMethod]
    [DataRow(new[] { "another", "sub-command" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] another sub-command")]
    public void MatchCommand_PositionalArgumentNotMatch(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var matched = "";
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine
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
