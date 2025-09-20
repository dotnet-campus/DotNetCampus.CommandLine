using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class PositionalArgumentTests
{
    [TestMethod]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o option value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o option value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o option value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o option value")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value -o option")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value -o option")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value -o option")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value -o option")]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o option -- value")]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o option -- value")]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o option -- value")]
    [DataRow(new[] { "test://value?option=option" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value?option=option")]
    public void Supported(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.Value);
    }

    [TestMethod]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o option -- value")]
    public void DoesNotSupportPostPositionalArguments(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }
}
