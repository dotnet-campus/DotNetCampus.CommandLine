using System.Collections;
using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionCollectionValueTests
{
    [TestMethod]
    // option a option b
    [DataRow(new[] { "--option", "a", "--option", "b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option a --option b")]
    [DataRow(new[] { "--option", "a", "--option", "b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option a --option b")]
    [DataRow(new[] { "--option", "a", "--option", "b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option a --option b")]
    [DataRow(new[] { "-Option", "a", "-Option", "b" }, TestCommandLineStyle.PowerShell, DisplayName = "[Gnu] -Option a -Option b")]
    [DataRow(new[] { "-o", "a", "-o", "b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o a -o b")]
    [DataRow(new[] { "-o", "a", "-o", "b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o a -o b")]
    [DataRow(new[] { "-o", "a", "-o", "b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o a -o b")]
    [DataRow(new[] { "-o", "a", "-o", "b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o a -o b")]
    // option a b
    [DataRow(new[] { "--option", "a", "b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option a b")]
    [DataRow(new[] { "--option", "a", "b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option a b")]
    [DataRow(new[] { "-Option", "a", "b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option a b")]
    [DataRow(new[] { "-o", "a", "b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o a b")]
    [DataRow(new[] { "-o", "a", "b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o a b")]
    [DataRow(new[] { "-o", "a", "b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o a b")]
    // option a,b
    [DataRow(new[] { "--option", "a,b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option a,b")]
    [DataRow(new[] { "--option", "a,b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option a,b")]
    [DataRow(new[] { "--option", "a,b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option a,b")]
    [DataRow(new[] { "-Option", "a,b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option a,b")]
    [DataRow(new[] { "-o", "a,b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o a,b")]
    [DataRow(new[] { "-o", "a,b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o a,b")]
    [DataRow(new[] { "-o", "a,b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o a,b")]
    [DataRow(new[] { "-o", "a,b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o a,b")]
    // option a;b
    [DataRow(new[] { "--option", "a;b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option a;b")]
    [DataRow(new[] { "--option", "a;b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option a;b")]
    [DataRow(new[] { "--option", "a;b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option a;b")]
    [DataRow(new[] { "-Option", "a;b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option a;b")]
    [DataRow(new[] { "-o", "a;b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o a;b")]
    [DataRow(new[] { "-o", "a;b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o a;b")]
    [DataRow(new[] { "-o", "a;b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o a;b")]
    [DataRow(new[] { "-o", "a;b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o a;b")]
    // option=a option=b
    [DataRow(new[] { "--option=a", "--option=b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option=a --option=b")]
    [DataRow(new[] { "--option=a", "--option=b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option=a --option=b")]
    [DataRow(new[] { "--option=a", "--option=b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option=a --option=b")]
    [DataRow(new[] { "-Option=a", "-Option=b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option=a -Option=b")]
    [DataRow(new[] { "test://?option=a&option=b" }, TestCommandLineStyle.Url, DisplayName = "[Uri] test://?option=a&option=b")]
    [DataRow(new[] { "-o=a", "-o=b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o=a -o=b")]
    [DataRow(new[] { "-o=a", "-o=b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o=a -o=b")]
    [DataRow(new[] { "-o=a", "-o=b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o=a -o=b")]
    // option=a,b
    [DataRow(new[] { "--option=a,b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option=a,b")]
    [DataRow(new[] { "--option=a,b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option=a,b")]
    [DataRow(new[] { "--option=a,b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option=a,b")]
    [DataRow(new[] { "-Option=a,b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option=a,b")]
    [DataRow(new[] { "test://?option=a,b" }, TestCommandLineStyle.Url, DisplayName = "[Uri] test://?option=a,b")]
    [DataRow(new[] { "-o=a,b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o=a,b")]
    [DataRow(new[] { "-o=a,b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o=a,b")]
    [DataRow(new[] { "-o=a,b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o=a,b")]
    // option=a;b
    [DataRow(new[] { "--option=a;b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option=a;b")]
    [DataRow(new[] { "--option=a;b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option=a;b")]
    [DataRow(new[] { "--option=a;b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option=a;b")]
    [DataRow(new[] { "-Option=a;b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option=a;b")]
    [DataRow(new[] { "test://?option=a;b" }, TestCommandLineStyle.Url, DisplayName = "[Uri] test://?option=a;b")]
    [DataRow(new[] { "-o=a;b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o=a;b")]
    [DataRow(new[] { "-o=a;b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o=a;b")]
    [DataRow(new[] { "-o=a;b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o=a;b")]
    // oa,b
    [DataRow(new[] { "-oa,b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -oa,b")]
    // oa;b
    [DataRow(new[] { "-oa;b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -oa;b")]
    public void Supported_Collection(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.IsNotNull(options.Option);
        CollectionAssert.AreEqual(new[] { "a", "b" }, (ICollection)options.Option);
    }

    [TestMethod]
    // option a b
    [DataRow(new[] { "--option", "a", "b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option a b")]
    [DataRow(new[] { "-o", "a", "b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o a b")]
    public void NotSupported_Collection(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
    }

    [TestMethod]
    // o=a o=b
    [DataRow(new[] { "-o=a", "-o=b" }, new[] { "=a", "=b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o=a -o=b (预期值为 '=a, =b')")]
    [DataRow(new[] { "-o:a", "-o:b" }, new[] { ":a", ":b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o:a -o:b (预期值为 ':a, :b')")]
    public void GnuDoesNotSupportShortOptionSeparator(string[] args, string[] expectedValues, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.IsNotNull(options.Option);
        CollectionAssert.AreEqual(expectedValues, (ICollection)options.Option);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public IReadOnlyList<string>? Option { get; set; }
    }
}
