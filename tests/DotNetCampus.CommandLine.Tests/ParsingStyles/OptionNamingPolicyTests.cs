using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionNamingPolicyTests
{
    [TestMethod]
    // --kebab-case
    [DataRow(new[] { "--option-name1=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option-name1=value")]
    [DataRow(new[] { "--option-name1=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option-name1=value")]
    [DataRow(new[] { "--option-name1=value" }, TestCommandLineStyle.Gnu, DisplayName = "[GNU] --option-name1=value")]
    // -PascalCase
    [DataRow(new[] { "-OptionName1=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -OptionName1=value")]
    [DataRow(new[] { "-OptionName1=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -OptionName1=value")]
    // --PascalCase (Strange but supported in Flexible)
    [DataRow(new[] { "--OptionName1=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --OptionName1=value")]
    public void Supported1(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.OptionName1);
    }

    [TestMethod]
    // ordinal
    [DataRow(new[] { "--OptionName2=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --OptionName2=value")]
    [DataRow(new[] { "-OptionName2=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -OptionName2=value")]
    [DataRow(new[] { "--OptionName2=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --OptionName2=value")]
    [DataRow(new[] { "--OptionName2=value" }, TestCommandLineStyle.Gnu, DisplayName = "[GNU] --OptionName2=value")]
    [DataRow(new[] { "-OptionName2=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -OptionName2=value")]
    public void Supported2(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.OptionName2);
    }

    [TestMethod]
    [DataRow(new[] { "--OptionName1=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --OptionName1=value")]
    [DataRow(new[] { "--OptionName1=value" }, TestCommandLineStyle.Gnu, DisplayName = "[GNU] --OptionName1=value")]
    [DataRow(new[] { "-option-name1=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option-name1=value")]
    [DataRow(new[] { "--option-name2=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option-name2=value")]
    [DataRow(new[] { "--option-name2=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option-name2=value")]
    [DataRow(new[] { "--option-name2=value" }, TestCommandLineStyle.Gnu, DisplayName = "[GNU] --option-name2=value")]
    [DataRow(new[] { "-option-name2=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option-name2=value")]
    public void NotSupported(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
    }

    public record TestOptions
    {
        [Option("option-name1")]
        public string? OptionName1 { get; set; }

        [Option("OptionName2")]
        public string? OptionName2 { get; set; }
    }
}
