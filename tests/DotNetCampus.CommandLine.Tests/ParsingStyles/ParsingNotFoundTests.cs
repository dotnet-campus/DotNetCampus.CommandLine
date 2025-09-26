using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DotNetCampus.Cli.Tests.ParsingStyles.TestCommandLineStyle;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionNotFoundTests
{
    [TestMethod]
    [DataRow(new[] { "--not-exist", "--option", "value" }, Flexible, DisplayName = "[Flexible] --not-exist --option value")]
    [DataRow(new[] { "--not-exist", "--option", "value" }, DotNet, DisplayName = "[DotNet] --not-exist --option value")]
    [DataRow(new[] { "--not-exist", "--option", "value" }, Gnu, DisplayName = "[Gnu] --not-exist --option value")]
    [DataRow(new[] { "-NotExist", "-Option", "value" }, PowerShell, DisplayName = "[PowerShell] -NotExist -Option value")]
    [DataRow(new[] { "--not-exist", "test", "--option", "value" }, Flexible, DisplayName = "[Flexible] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test", "--option", "value" }, DotNet, DisplayName = "[DotNet] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test", "--option", "value" }, Gnu, DisplayName = "[Gnu] --not-exist test --option value")]
    [DataRow(new[] { "-NotExist", "test", "-Option", "value" }, PowerShell, DisplayName = "[PowerShell] -NotExist test -Option value")]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, Flexible, DisplayName = "[Flexible] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, DotNet, DisplayName = "[DotNet] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, Gnu, DisplayName = "[Gnu] --not-exist test --option value")]
    [DataRow(new[] { "-NotExist", "test1", "test2", "-Option", "value" }, PowerShell, DisplayName = "[PowerShell] -NotExist test -Option value")]
    public void OptionNotFound_IgnoreAllUnknownArguments(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions() with
        {
            UnknownArgumentsHandling = UnknownCommandArgumentHandling.IgnoreAllUnknownArguments,
        });

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.Option);
    }

    [TestMethod]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, Flexible, DisplayName = "[Flexible] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, DotNet, DisplayName = "[DotNet] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, Gnu, DisplayName = "[Gnu] --not-exist test --option value")]
    [DataRow(new[] { "-NotExist", "test1", "test2", "-Option", "value" }, PowerShell, DisplayName = "[PowerShell] -NotExist test -Option value")]
    public void OptionNotFound_IgnoreUnknownOptionalArguments(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions() with
        {
            UnknownArgumentsHandling = UnknownCommandArgumentHandling.IgnoreUnknownOptionalArguments,
        });

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, Flexible, DisplayName = "[Flexible] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, DotNet, DisplayName = "[DotNet] --not-exist test --option value")]
    [DataRow(new[] { "--not-exist", "test1", "test2", "--option", "value" }, Gnu, DisplayName = "[Gnu] --not-exist test --option value")]
    [DataRow(new[] { "-NotExist", "test1", "test2", "-Option", "value" }, PowerShell, DisplayName = "[PowerShell] -NotExist test -Option value")]
    public void OptionNotFound_IgnoreUnknownPositionalArguments(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions() with
        {
            UnknownArgumentsHandling = UnknownCommandArgumentHandling.IgnoreUnknownPositionalArguments,
        });

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public string? Option { get; set; }
    }
}
