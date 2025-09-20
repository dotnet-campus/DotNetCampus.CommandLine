using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionValueSeparatorTests
{
    [TestMethod]
    // option=value
    [DataRow(new[] { "--option=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option=value")]
    [DataRow(new[] { "--option=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option=value")]
    [DataRow(new[] { "--option=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option=value")]
    [DataRow(new[] { "-Option=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option=value")]
    [DataRow(new[] { "-option=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option=value")]
    [DataRow(new[] { "/Option=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option=value")]
    [DataRow(new[] { "/option=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option=value")]
    [DataRow(new[] { "test://?option=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] option=value")]
    // o=value
    [DataRow(new[] { "-o=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o=value")]
    [DataRow(new[] { "-o=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o=value")]
    [DataRow(new[] { "-o=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o=value")]
    [DataRow(new[] { "/o=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o=value")]
    // option:value
    [DataRow(new[] { "--option:value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option:value")]
    [DataRow(new[] { "--option:value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option:value")]
    [DataRow(new[] { "-Option:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option:value")]
    [DataRow(new[] { "-option:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option:value")]
    [DataRow(new[] { "/Option:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option:value")]
    [DataRow(new[] { "/option:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option:value")]
    // o:value
    [DataRow(new[] { "-o:value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o:value")]
    [DataRow(new[] { "-o:value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:value")]
    [DataRow(new[] { "-o:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o:value")]
    [DataRow(new[] { "/o:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o:value")]
    // option value
    [DataRow(new[] { "--option", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option value")]
    [DataRow(new[] { "--option", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option value")]
    [DataRow(new[] { "--option", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option value")]
    [DataRow(new[] { "-Option", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option value")]
    [DataRow(new[] { "-option", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option value")]
    [DataRow(new[] { "/Option", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option value")]
    [DataRow(new[] { "/option", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option value")]
    // o value
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o value")]
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o value")]
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o value")]
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o value")]
    [DataRow(new[] { "/o", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o value")]
    // ovalue
    [DataRow(new[] { "-ovalue" }, TestCommandLineStyle.Gnu, DisplayName = "[Flexible] -ovalue")]
    public void Supported(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.Option);
    }

    [TestMethod]
    [DataRow(new[] { "--option:value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option:value")]
    [DataRow(new[] { "test://?option:value" }, TestCommandLineStyle.Url, DisplayName = "[Url] option:value")]
    [DataRow(new[] { "test://?o:value" }, TestCommandLineStyle.Url, DisplayName = "[Url] o:value")]
    [DataRow(new[] { "test://?option%20value" }, TestCommandLineStyle.Url, DisplayName = "[Url] option value")]
    [DataRow(new[] { "test://?o%20value" }, TestCommandLineStyle.Url, DisplayName = "[Url] o value")]
    public void NotSupported(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentSeparatorNotSupported, exception.Reason);
    }


    [TestMethod]
    [DataRow(new[] { "-o=value" }, "=value", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o=value (预期值为 '=value')")]
    [DataRow(new[] { "-o:value" }, ":value", TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o:value (预期值为 ':value')")]
    public void GnuDoesNotSupportShortOptionSeparator(string[] args, string value, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual(value, options.Option);
    }

    [TestMethod]
    [DataRow(new[] { "test://?o=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] o=value")]
    public void UrlStyleDoesNotSupportShortOption(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
        Assert.Contains("URL", exception.Message);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public string? Option { get; set; }
    }
}
