using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Tests.ParsingStyles;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.Issues;

[TestClass]
public class SlashPrefixValueTests
{
    [TestMethod]
    [DataRow(new[] { "--option", "/var/log" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option /var/log")]
    [DataRow(new[] { "-option", "/var/log" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -option /var/log")]
    [DataRow(new[] { "/option", "/var/log" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /option /var/log")]
    [DataRow(new[] { "--option", "/var/log" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option /var/log")]
    [DataRow(new[] { "--option", "/var/log" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option /var/log")]
    [DataRow(new[] { "-Option", "/var/log" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option /var/log")]
    [DataRow(new[] { "/option", "/var/log" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option /var/log")]
    public void LinuxPathAsOptionValue(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("/var/log", options.Option);
    }

    [TestMethod]
    [DataRow(new[] { "/var/log" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] /var/log")]
    [DataRow(new[] { "/var/log" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] /var/log")]
    public void LinuxPathAsPositionalArgumentValue_Supported(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestValues>();

        // Assert
        Assert.AreEqual("/var/log", options.Value);
    }

    [TestMethod]
    [DataRow(new[] { "/var/log" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /var/log")]
    [DataRow(new[] { "/var/log" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /var/log")]
    public void LinuxPathAsPositionalArgumentValue_NotSupported1(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestValues>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentSeparatorNotSupported, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "/var" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /var/log")]
    [DataRow(new[] { "/var" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /var/log")]
    public void LinuxPathAsPositionalArgumentValue_NotSupported2(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestValues>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public string? Option { get; set; }
    }

    public record TestValues
    {
        [Value(0)]
        public string? Value { get; set; }
    }
}
