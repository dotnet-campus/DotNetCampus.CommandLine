using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionCaseSensitiveTests
{
    [TestMethod]
    [DataRow(new[] { "--Option-Name1=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --Option-Name1=value")]
    [DataRow(new[] { "--OPTION-NAME1=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --OPTION-NAME1=value")]
    [DataRow(new[] { "--Option-Name1=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --Option-Name1=value")]
    [DataRow(new[] { "--OPTION-NAME1=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --OPTION-NAME1=value")]
    public void CaseSensitive(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "--Option-Name1=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --Option-Name1=value")]
    [DataRow(new[] { "--OPTION-NAME1=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --OPTION-NAME1=value")]
    [DataRow(new[] { "-optionName1=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -optionName1=value")]
    [DataRow(new[] { "-optionname1=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -optionname1=value")]
    [DataRow(new[] { "-optionname1=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -optionname1=value")]
    [DataRow(new[] { "-OPTIONNAME1=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -OPTIONNAME1=value")]
    [DataRow(new[] { "-optionName1=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -optionName1=value")]
    [DataRow(new[] { "test://?Option-Name1=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://?Option-Name1=value")]
    [DataRow(new[] { "test://?OPTION-NAME1=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://?OPTION-NAME1=value")]
    public void CaseInsensitive(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.OptionName1);
    }

    [TestMethod]
    [DataRow(new[] { "test://?option-name1=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://?option-name1=value")]
    [DataRow(new[] { "Test://?option-name1=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] Test://?option-name1=value")]
    [DataRow(new[] { "TEST://?option-name1=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] TEST://?option-name1=value")]
    public void UrlCaseInsensitive(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.OptionName1);
    }

    public record TestOptions
    {
        [Option("option-name1")]
        public string? OptionName1 { get; set; }

        [Option("OptionName2")]
        public string? OptionName2 { get; set; }
    }
}
