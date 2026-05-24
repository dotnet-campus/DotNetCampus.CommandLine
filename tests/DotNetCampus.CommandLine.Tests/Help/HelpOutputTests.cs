using System;
using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.Help;

[TestClass]
public class HelpOutputTests
{
    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    [DataRow(new[] { "-h" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] -h")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --help")]
    [DataRow(new[] { "/?" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] /?")]
    public void HelpReturnsExitCode0(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHelpHandler()
            .AddHandler<DefaultOptions>(_ => { })
            .Run();

        // Assert
        Assert.AreEqual(0, result.ExitCode);
    }

    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --help")]
    [DataRow(new[] { "/?" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] /?")]
    public void HelpNotEnabled_ThrowsParseException(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act & Assert
        Assert.ThrowsExactly<CommandLineParseException>(() => commandLine
            .AddHandler<DefaultOptions>(_ => { })
            .Run());
    }

    [TestMethod]
    [DataRow(new[] { "sub", "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] sub --help")]
    [DataRow(new[] { "sub", "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] sub --help")]
    [DataRow(new[] { "sub", "--help" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] sub --help")]
    public void HelpWithSubCommand_OutputContainsCommandName(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        string? helpText = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = commandLine
            .AddHelpHandler(new HelpConfigurations
            {
                HelpMessageWriter = text => helpText = text,
            })
            .AddHandler<DefaultOptions>(_ => { })
            .AddHandler<SubCommandOptions>(_ => { })
            .Run();

        // Assert
        Assert.AreEqual(0, result.ExitCode);
        Assert.IsNotNull(helpText);
        Assert.IsTrue(helpText.Contains("sub"), $"Help text should contain command name 'sub'. Actual: {helpText}");
        Assert.IsTrue(helpText.Contains("Sub command description"), $"Help text should contain description. Actual: {helpText}");
    }

    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] --help")]
    public void HelpOutput_ContainsOptionNames(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        string? helpText = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHelpHandler(new HelpConfigurations
            {
                HelpMessageWriter = text => helpText = text,
            })
            .AddHandler<OptionsWithDescription>(_ => { })
            .Run();

        // Assert
        Assert.IsNotNull(helpText);
        Assert.IsTrue(helpText.Contains("--output"), $"Help text should contain '--output'. Actual: {helpText}");
        Assert.IsTrue(helpText.Contains("-o"), $"Help text should contain '-o'. Actual: {helpText}");
    }

    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --help")]
    public void HelpOutput_ContainsPositionalArgumentName(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        string? helpText = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHelpHandler(new HelpConfigurations
            {
                HelpMessageWriter = text => helpText = text,
            })
            .AddHandler<OptionsWithPositionalArg>(_ => { })
            .Run();

        // Assert
        Assert.IsNotNull(helpText);
        Assert.IsTrue(helpText.Contains("input_file"), $"Help text should contain positional arg name 'input_file'. Actual: {helpText}");
    }

    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --help")]
    public void HelpOutput_Localization(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        string? helpText = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHelpHandler(new HelpConfigurations
            {
                HelpMessageWriter = text => helpText = text,
                HelpTextLocalizer = key => key == "OptionDescription" ? "LOCALIZED_OPTION_DESCRIPTION" : key,
            })
            .AddHandler<LocalizableOptions>(_ => { })
            .Run();

        // Assert
        Assert.IsNotNull(helpText);
        Assert.IsTrue(helpText.Contains("LOCALIZED_OPTION_DESCRIPTION"), $"Help text should contain localized description. Actual: {helpText}");
        Assert.IsFalse(helpText.Contains("OptionDescription") && !helpText.Contains("LOCALIZED_OPTION_DESCRIPTION"),
            "Help text should not contain raw key when localizer is provided.");
    }

    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    public void HelpOutput_CustomHelpHandler(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        MatchedCommand? capturedMatched = null;
        ICommandObjectMetadata? capturedDefault = null;
        IReadOnlyList<ICommandObjectMetadata>? capturedSubCommands = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHelpHandler(new HelpConfigurations
            {
                HelpHandler = new TestHelpHandler((matched, defaultMetadata, subCommands) =>
                {
                    capturedMatched = matched;
                    capturedDefault = defaultMetadata;
                    capturedSubCommands = subCommands;
                }),
            })
            .AddHandler<DefaultOptions>(_ => { })
            .AddHandler<SubCommandOptions>(_ => { })
            .Run();

        // Assert
        Assert.IsNotNull(capturedMatched);
        Assert.IsNotNull(capturedDefault);
        Assert.IsNotNull(capturedSubCommands);
        Assert.IsTrue(capturedSubCommands!.Count > 0);
    }

    [TestMethod]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] --help")]
    [DataRow(new[] { "--help" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] --help")]
    public void HelpOutput_RequiredOptionMarked(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        string? helpText = null;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHelpHandler(new HelpConfigurations
            {
                HelpMessageWriter = text => helpText = text,
            })
            .AddHandler<RequiredOptionOptions>(_ => { })
            .Run();

        // Assert
        Assert.IsNotNull(helpText);
        Assert.IsTrue(helpText.Contains("--name"), $"Help text should contain '--name'. Actual: {helpText}");
    }

    #region Test Types

    public record DefaultOptions
    {
        [Value(0)]
        public string? Value { get; set; } = "Default";
    }

    [Command("sub", Description = "Sub command description")]
    public record SubCommandOptions
    {
        [Option('v', "verbose", Description = "Enable verbose output")]
        public bool Verbose { get; set; }
    }

    public record OptionsWithDescription
    {
        [Option('o', "output", Description = "The output path")]
        public string? Output { get; set; }
    }

    public record OptionsWithPositionalArg
    {
        [Value(0, Description = "The input file")]
        public string? InputFile { get; set; }
    }

    public record LocalizableOptions
    {
        [Option('n', "name", Description = "OptionDescription")]
        public string? Name { get; set; }
    }

    public record RequiredOptionOptions
    {
        [Option('n', "name", Description = "The name")]
        public required string Name { get; init; }
    }

    private class TestHelpHandler(
        Action<MatchedCommand, ICommandObjectMetadata?, IReadOnlyList<ICommandObjectMetadata>> callback) : IHelpHandler
    {
        public void Handle(MatchedCommand matchedCommand, ICommandObjectMetadata? defaultCommandMetadata,
            IReadOnlyList<ICommandObjectMetadata> subCommandMetadataList)
        {
            callback(matchedCommand, defaultCommandMetadata, subCommandMetadataList);
        }
    }

    #endregion
}
