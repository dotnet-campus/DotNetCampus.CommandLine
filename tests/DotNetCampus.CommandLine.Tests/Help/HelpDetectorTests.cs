using DotNetCampus.Cli.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.Help;

[TestClass]
public class HelpDetectorTests
{
    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    [DataRow(new[] { "-h" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -h")]
    [DataRow(new[] { "/help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /help")]
    [DataRow(new[] { "/h" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /h")]
    [DataRow(new[] { "/?" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /?")]
    [DataRow(new[] { "-?" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -?")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --help")]
    [DataRow(new[] { "-h" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -h")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --help")]
    [DataRow(new[] { "-h" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -h")]
    [DataRow(new[] { "/help" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] /help")]
    [DataRow(new[] { "/h" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] /h")]
    [DataRow(new[] { "/?" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] /?")]
    [DataRow(new[] { "-?" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] -?")]
    [DataRow(new[] { "foo", "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] foo --help")]
    [DataRow(new[] { "foo", "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] foo --help")]
    public void IsHelpRequested(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLineStyle = style.ToParsingOptions().Style;

        // Act
        var result = HelpDetector.IsHelpRequested(args, commandLineStyle);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow(new[] { "--file" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --file")]
    [DataRow(new[] { "-f" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -f")]
    [DataRow(new[] { "help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] help (no prefix)")]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] empty")]
    [DataRow(new[] { "--file" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --file")]
    [DataRow(new[] { "help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] help (no prefix)")]
    [DataRow(new[] { "--helper" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --helper")]
    [DataRow(new[] { "/file" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] /file")]
    public void IsHelpRequested_NotTriggered(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLineStyle = style.ToParsingOptions().Style;

        // Act
        var result = HelpDetector.IsHelpRequested(args, commandLineStyle);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow(new[] { "-vh" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -vh")]
    [DataRow(new[] { "-abh" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -abh")]
    [DataRow(new[] { "-ha" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -ha")]
    public void IsHelpRequested_ShortOptionCombination(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLineStyle = style.ToParsingOptions().Style;

        // Act
        var result = HelpDetector.IsHelpRequested(args, commandLineStyle);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow(new[] { "--Help" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --Help (case sensitive, not triggered)")]
    [DataRow(new[] { "-H" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -H (case sensitive, not triggered)")]
    public void IsHelpRequested_CaseSensitive_NotTriggered(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLineStyle = style.ToParsingOptions().Style;

        // Act
        var result = HelpDetector.IsHelpRequested(args, commandLineStyle);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [DataRow(new[] { "--Help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --Help (case insensitive, triggered)")]
    [DataRow(new[] { "-H" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -H (case insensitive, triggered)")]
    [DataRow(new[] { "/HELP" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] /HELP (case insensitive, triggered)")]
    public void IsHelpRequested_CaseInsensitive_Triggered(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLineStyle = style.ToParsingOptions().Style;

        // Act
        var result = HelpDetector.IsHelpRequested(args, commandLineStyle);

        // Assert
        Assert.IsTrue(result);
    }
}
