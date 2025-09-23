using DotNetCampus.Cli.Tests.ParsingStyles;
using DotNetCampus.CommandLine.FakeObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S = DotNetCampus.Cli.Tests.ParsingStyles.TestCommandLineStyle;

namespace DotNetCampus.Cli.Tests.CommandMatching;

[TestClass]
public class CommandAttributeTests
{
    [TestMethod]
    [DataRow(new[] { "--option=value" }, nameof(CommandObject0InAnotherAssembly), S.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new[] { "test", "--option=value" }, nameof(CommandObject1InAnotherAssembly), S.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new[] { "command", "in-another-assembly", "--option=value" }, nameof(CommandObject2InAnotherAssembly), S.Flexible, DisplayName = "[Flexible]")]
    public void MatchCommand(string[] args, string expectedCommand, S style)
    {
        // Arrange
        (string? TypeName, string? Value) matched = default;
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        commandLine
            .AddHandler<CommandObject0InAnotherAssembly>(o => matched = (o.GetType().Name, o.Option))
            .AddHandler<CommandObject1InAnotherAssembly>(o => matched = (o.GetType().Name, o.Option))
            .AddHandler<CommandObject2InAnotherAssembly>(o => matched = (o.GetType().Name, o.Option))
            .Run();

        // Assert
        Assert.AreEqual(expectedCommand, matched.TypeName);
        Assert.AreEqual("value", matched.Value);
    }
}
