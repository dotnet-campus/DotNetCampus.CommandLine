using System.Collections;
using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionDictionaryValueTests
{
    [TestMethod]
    // option key1=value1 option key2=value2
    [DataRow(new[] { "--option", "a=x", "--option", "b=y" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option a=x --option b=y")]
    [DataRow(new[] { "--option", "a=x", "--option", "b=y" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option a=x --option b=y")]
    [DataRow(new[] { "--option", "a=x", "--option", "b=y" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option a=x --option b=y")]
    [DataRow(new[] { "-Option", "a=x", "-Option", "b=y" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] -Option a=x -Option b=y")]
    // option:key1=value1;key2=value2
    [DataRow(new[] { "--option:a=x;b=y" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option:a=x;b=y")]
    [DataRow(new[] { "--option:a=x;b=y" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option:a=x;b=y")]
    [DataRow(new[] { "-Option:a=x;b=y" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] -Option:a=x;b=y")]
    [DataRow(new[] { "-o:a=x;b=y" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o:a=x;b=y")]
    [DataRow(new[] { "-o:a=x;b=y" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:a=x;b=y")]
    [DataRow(new[] { "-o:a=x;b=y" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] -o:a=x;b=y")]
    public void Supported_Collection(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.IsNotNull(options.Option);
        CollectionAssert.AreEquivalent(new Dictionary<string, string>
        {
            ["a"] = "x",
            ["b"] = "y",
        }, (ICollection)options.Option);
    }

    [TestMethod]
    // option=key1=value1;key2=value2
    [DataRow(new[] { "--option=a=x;b=y" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option=a=x;b=y")]
    [DataRow(new[] { "--option=a=x;b=y" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option=a=x;b=y")]
    [DataRow(new[] { "--option=a=x;b=y" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option=a=x;b=y")]
    [DataRow(new[] { "-Option=a=x;b=y" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] -Option=a=x;b=y")]
    public void SupportedButStrange_Collection(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.IsNotNull(options.Option);
        CollectionAssert.AreEquivalent(new Dictionary<string, string>
        {
            ["a"] = "x",
            ["b"] = "y",
        }, (ICollection)options.Option);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public IReadOnlyDictionary<string, string>? Option { get; set; }
    }
}
