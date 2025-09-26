using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DotNetCampus.Cli.Tests.ParsingStyles.TestCommandLineStyle;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class OptionNotFoundTests
{
    [TestMethod]
    [DataRow(new[] { "--not-exist", "--option", "value" }, Flexible, DisplayName = "[Flexible] --not-exist --option value")]
    public void OptionNotFound_ButExistedOptionsAssigned(string[] args, TestCommandLineStyle style)
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

    public record TestOptions
    {
        [Option('o', "option")]
        public string? Option { get; set; }
    }
}
