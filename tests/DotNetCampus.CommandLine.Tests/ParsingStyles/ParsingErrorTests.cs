using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class ParsingErrorTests
{
    [TestMethod]
    [DataRow(new[] { "--=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --=value")]
    [DataRow(new[] { "-=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -=value")]
    [DataRow(new[] { "/=value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /=value")]
    [DataRow(new[] { "--:value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --:value")]
    [DataRow(new[] { "-:value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -:value")]
    [DataRow(new[] { "/:value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /:value")]
    [DataRow(new[] { "--=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --=value")]
    [DataRow(new[] { "--:value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --:value")]
    [DataRow(new[] { "--=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --=value")]
    [DataRow(new[] { "-=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -=value")]
    [DataRow(new[] { "/=value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /=value")]
    [DataRow(new[] { "-:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -:value")]
    [DataRow(new[] { "/:value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /:value")]
    [DataRow(new[] { "test://?=value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://?=value")]
    [DataRow(new[] { "-=value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -=value")]
    [DataRow(new[] { "-:value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -:value")]
    [DataRow(new[] { "-=value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -=value")]
    public void EmptyOptionName(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentParseError, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new[] { "" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new[] { "" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new[] { "" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell]")]
    public void EmptyArgument(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("", options.Value);
    }

    [TestMethod]
    [DataRow(new[] { "test:///" }, TestCommandLineStyle.Url, DisplayName = "[Url] test:///")]
    [DataRow(new[] { "test:////" }, TestCommandLineStyle.Url, DisplayName = "[Url] test:////")]
    public void UrlManySlash(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.IsNull(options.Value);
    }

    public record TestOptions
    {
        [Value(0)]
        public string? Value { get; set; }
    }
}
