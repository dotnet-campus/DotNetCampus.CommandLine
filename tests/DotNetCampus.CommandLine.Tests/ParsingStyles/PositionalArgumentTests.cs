using System.Collections;
using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class PositionalArgumentTests
{
    [TestMethod]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o option value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o option value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o option value")]
    [DataRow(new[] { "-o", "option", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o option value")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value -o option")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value -o option")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value -o option")]
    [DataRow(new[] { "value", "-o", "option" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value -o option")]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o option -- value")]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o option -- value")]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o option -- value")]
    [DataRow(new[] { "test://value?option=option" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value?option=option")]
    public void Supported(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual("value", options.Value);
    }

    [TestMethod]
    [DataRow(new[] { "-o", "true", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o true value")]
    [DataRow(new[] { "-o", "true", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o true value")]
    [DataRow(new[] { "-o", "true", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o true value")]
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o value")]
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o value")]
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o value")]
    [DataRow(new[] { "-o", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o value")]
    public void Supported_Boolean(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<BooleanTestOptions>();

        // Assert
        Assert.IsTrue(options.Option);
        Assert.AreEqual("value", options.Value);
    }

    [TestMethod]
    [DataRow(new[] { "value", "-o", "a", "b" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value -o a b")]
    [DataRow(new[] { "value", "-o", "a", "b" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value -o a b")]
    [DataRow(new[] { "value", "-o", "a", "b" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value -o a b")]
    [DataRow(new[] { "-o", "a", "b", "--", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o a b -- value")]
    [DataRow(new[] { "-o", "a", "b", "--", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o a b -- value")]
    public void Supported_Collection(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<CollectionTestOptions>();

        // Assert
        CollectionAssert.AreEqual(new[] { "a", "b" }, (ICollection)options.Option!);
        Assert.AreEqual("value", options.Value);
    }

    [TestMethod]
    [DataRow(new[] { "-o", "a", "b", "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -o a b value")]
    [DataRow(new[] { "-o", "a", "b", "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] -o a b value")]
    [DataRow(new[] { "-o", "a", "b", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o a b value")]
    public void NotSupported_Collection(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<CollectionTestOptions>();

        // Assert
        CollectionAssert.AreEqual(new[] { "a", "b", "value" }, (ICollection)options.Option!);
        Assert.IsNull(options.Value);
    }

    [TestMethod]
    [DataRow(new[] { "-o", "option", "--", "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] -o option -- value")]
    public void DoesNotSupportPostPositionalArguments(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<TestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.OptionalArgumentNotFound, exception.Reason);
    }

    // [TestMethod]
    public void MatchPositionalArgumentRange(string[] args, TestCommandLineStyle style)
    {
    }

    [TestMethod]
    [DataRow(new[] { "-o", "true", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o true value")]
    public void DoesNotMatchPositionalArgumentRange_Boolean(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<BooleanTestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
    }

    [TestMethod]
    [DataRow(new[] { "value", "-o", "a", "b" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value -o a b")]
    [DataRow(new[] { "-o", "a", "b", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o a b value")]
    [DataRow(new[] { "-o", "a", "b", "--", "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] -o a b -- value")]
    public void DoesNotMatchPositionalArgumentRange_Collection(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var exception = Assert.Throws<CommandLineParseException>(() => commandLine.As<CollectionTestOptions>());

        // Assert
        Assert.AreEqual(CommandLineParsingError.PositionalArgumentNotFound, exception.Reason);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record BooleanTestOptions
    {
        [Option('o', "option")]
        public bool? Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record CollectionTestOptions
    {
        [Option('o', "option")]
        public IReadOnlyList<string>? Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record MultiplePositionArgumentsOptions
    {
        [Value(0, 2)]
        public IReadOnlyList<string>? Value0 { get; set; }

        [Value(1)]
        public string? Value1 { get; set; }

        [Value(2, int.MaxValue)]
        public IReadOnlyList<string>? Value2 { get; set; }
    }
}
