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
    [DataRow(new[] { "-Option" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -Option")]
    [DataRow(new[] { "-option" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -option")]
    [DataRow(new[] { "/Option" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /Option")]
    [DataRow(new[] { "/option" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /option")]
    [DataRow(new[] { "--option" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option")]
    [DataRow(new[] { "--option" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option")]
    [DataRow(new[] { "-Option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option")]
    [DataRow(new[] { "-option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option")]
    [DataRow(new[] { "/Option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option")]
    [DataRow(new[] { "/option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option")]
    // o
    [DataRow(new[] { "-o" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o")]
    [DataRow(new[] { "/o" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /o")]
    [DataRow(new[] { "-o" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o")]
    [DataRow(new[] { "-o" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o")]
    [DataRow(new[] { "-o" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o")]
    [DataRow(new[] { "/o" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o")]
    // option=true
    [DataRow(new[] { "--option=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option=true")]
    [DataRow(new[] { "-Option=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -Option=true")]
    [DataRow(new[] { "-option=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -option=true")]
    [DataRow(new[] { "/Option=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /Option=true")]
    [DataRow(new[] { "/option=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /option=true")]
    [DataRow(new[] { "--option=true" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option=true")]
    [DataRow(new[] { "--option=true" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option=true")]
    [DataRow(new[] { "-Option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option=true")]
    [DataRow(new[] { "-option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option=true")]
    [DataRow(new[] { "/Option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option=true")]
    [DataRow(new[] { "/option=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option=true")]
    // o=true
    [DataRow(new[] { "-o=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o=true")]
    [DataRow(new[] { "/o=true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /o=true")]
    [DataRow(new[] { "-o=true" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o=true")]
    [DataRow(new[] { "-o=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o=true")]
    [DataRow(new[] { "/o=true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /o=true")]
    // option  true
    [DataRow(new[] { "--option", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --option true")]
    [DataRow(new[] { "-Option", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -Option true")]
    [DataRow(new[] { "-option", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -option true")]
    [DataRow(new[] { "/Option", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /Option true")]
    [DataRow(new[] { "/option", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /option true")]
    [DataRow(new[] { "--option", "true" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --option true")]
    [DataRow(new[] { "-Option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -Option true")]
    [DataRow(new[] { "-option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -option true")]
    [DataRow(new[] { "/Option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /Option true")]
    [DataRow(new[] { "/option", "true" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /option true")]
    // o true
    [DataRow(new[] { "-o", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o true")]
    [DataRow(new[] { "/o", "true" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /o true")]
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
    [DataRow(new[] { "-o:true" }, true, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:true")]
    [DataRow(new[] { "-o:yes" }, true, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:yes")]
    [DataRow(new[] { "-o:on" }, true, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:on")]
    [DataRow(new[] { "-o:1" }, true, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:1")]
    [DataRow(new[] { "-o:false" }, false, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:false")]
    [DataRow(new[] { "-o:no" }, false, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:no")]
    [DataRow(new[] { "-o:off" }, false, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:off")]
    [DataRow(new[] { "-o:0" }, false, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o:0")]
    [DataRow(new[] { "-otrue" }, true, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -otrue")]
    [DataRow(new[] { "-oyes" }, true, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -oyes")]
    [DataRow(new[] { "-oon" }, true, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -oon")]
    [DataRow(new[] { "-o1" }, true, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o1")]
    [DataRow(new[] { "-ofalse" }, false, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -ofalse")]
    [DataRow(new[] { "-ono" }, false, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -ono")]
    [DataRow(new[] { "-ooff" }, false, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -ooff")]
    [DataRow(new[] { "-o0" }, false, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o0")]
    public void BooleanValue(string[] args, bool value, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual(value, options.Option);
    }

    [TestMethod]
    [DataRow(new[] { "--option", "true" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --option true")]
    [DataRow(new[] { "-o", "true" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o true")]
    public void GnuDoesNotSupportExplicitBooleanValue(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "-ab" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -ab")]
    public void BooleanOptionCombination(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestCombinationOptions>();

        // Assert
        Assert.IsTrue(options.OptionA);
        Assert.IsTrue(options.OptionB);
        Assert.IsNull(options.OptionC);
    }

    [TestMethod]
    [DataRow(new[] { "-abc" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -abc")]
    public void OptionCombinationMustAllBoolean(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestCombinationOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.ArgumentCombinationIsNotBoolean, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "-ab" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -ab")]
    [DataRow(new[] { "/ab" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /ab")]
    [DataRow(new[] { "-ab" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -ab")]
    [DataRow(new[] { "-ab" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -ab")]
    [DataRow(new[] { "/ab" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /ab")]
    public void DoesNotSupportBooleanOptionCombination(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<TestCombinationOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "-ab" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -ab")]
    [DataRow(new[] { "/ab" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /ab")]
    [DataRow(new[] { "-ab" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -ab")]
    [DataRow(new[] { "-ab" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -ab")]
    [DataRow(new[] { "/ab" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /ab")]
    public void SupportMultiCharShortOptions(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<MultiCharShortOptions>();

        // Assert
        Assert.IsTrue(options.OptionA);
        Assert.IsNull(options.OptionB);
    }

    [TestMethod]
    [DataRow(new[] { "-ab", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -ab value")]
    [DataRow(new[] { "/ab", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] /ab value")]
    [DataRow(new[] { "-ab", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -ab value")]
    [DataRow(new[] { "-ab", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -ab value")]
    [DataRow(new[] { "/ab", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] /ab value")]
    public void MultiCharShortOptionsDoesNotSupportValue(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.ThrowsExactly<CommandLineParseException>(() => commandLine.As<MultiCharShortOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public bool? Option { get; set; }
    }

    public record TestCombinationOptions
    {
        [Option('a', "option-a")]
        public bool? OptionA { get; set; }

        [Option('b', "option-b")]
        public bool? OptionB { get; set; }

        [Option('c', "option-c")]
        public string? OptionC { get; set; }
    }

    public record MultiCharShortOptions
    {
        [Option("ab", "option-ab")]
        public bool? OptionA { get; set; }

        [Option('b', "option-b")]
        public bool? OptionB { get; set; }
    }
}
