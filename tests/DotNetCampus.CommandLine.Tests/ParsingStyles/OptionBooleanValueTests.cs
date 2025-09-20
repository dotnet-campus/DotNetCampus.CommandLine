using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionBooleanValueTests
{
    [TestMethod]
    // option
    [DataRow(new[] { "--option" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option")]
    [DataRow(new[] { "--option" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option")]
    [DataRow(new[] { "--option" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option")]
    [DataRow(new[] { "-Option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option")]
    [DataRow(new[] { "-option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option")]
    [DataRow(new[] { "/Option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option")]
    [DataRow(new[] { "/option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option")]
    // o
    [DataRow(new[] { "-o" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o")]
    [DataRow(new[] { "-o" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o")]
    [DataRow(new[] { "-o" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o")]
    [DataRow(new[] { "-o" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o")]
    [DataRow(new[] { "/o" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o")]
    // option=true
    [DataRow(new[] { "--option=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option=true")]
    [DataRow(new[] { "--option=true" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option=true")]
    [DataRow(new[] { "--option=true" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option=true")]
    [DataRow(new[] { "-Option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option=true")]
    [DataRow(new[] { "-option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option=true")]
    [DataRow(new[] { "/Option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option=true")]
    [DataRow(new[] { "/option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option=true")]
    // o=true
    [DataRow(new[] { "-o=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o=true")]
    [DataRow(new[] { "-o=true" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o=true")]
    [DataRow(new[] { "-o=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o=true")]
    [DataRow(new[] { "/o=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o=true")]
    // option  true
    [DataRow(new[] { "--option", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option true")]
    [DataRow(new[] { "--option", "true" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option true")]
    [DataRow(new[] { "-Option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option true")]
    [DataRow(new[] { "-option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option true")]
    [DataRow(new[] { "/Option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option true")]
    [DataRow(new[] { "/option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option true")]
    // o true
    [DataRow(new[] { "-o", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o true")]
    [DataRow(new[] { "-o", "true" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o true")]
    [DataRow(new[] { "-o", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o true")]
    [DataRow(new[] { "/o", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o true")]
    // otrue
    [DataRow(new[] { "-otrue" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -otrue")]
    public void Supported_True(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.IsTrue(options.Option);
    }

    [TestMethod]
    [DataRow(new[] { "--option", "true" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option true")]
    [DataRow(new[] { "-o", "true" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o true")]
    public void GnuDoesNotSupportExplicitBooleanValue(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public bool? Option { get; set; }
    }
}
