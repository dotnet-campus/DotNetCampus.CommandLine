using DotNetCampus.Cli.Tests.ParsingStyles;
using DotNetCampus.CommandLine.FakeObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.CommandMatching;

[TestClass]
public class CommandAttributeTests
{
    [TestMethod]
    [DataRow(new[] { "--option=value" }, nameof(CommandObject0InAnotherAssembly), TestCommandLineStyle.Flexible,
        DisplayName = "[Flexible] --option=value")]
    [DataRow(new[] { "test", "--option=value" }, nameof(CommandObject1InAnotherAssembly), TestCommandLineStyle.Flexible,
        DisplayName = "[Flexible] test --option=value")]
    [DataRow(new[] { "command", "in-another-assembly", "--option=value" }, nameof(CommandObject2InAnotherAssembly), TestCommandLineStyle.Flexible,
        DisplayName = "[Flexible] command in-another-assembly --option=value")]
    public void MatchCommand(string[] args, string expectedCommand, TestCommandLineStyle style)
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
