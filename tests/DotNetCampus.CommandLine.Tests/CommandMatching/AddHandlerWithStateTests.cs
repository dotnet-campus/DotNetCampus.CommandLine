using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Tests.ParsingStyles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.CommandMatching;

[TestClass]
public class AddHandlerWithStateTests
{
    [TestMethod]
    [DataRow(new[] { "a", "b", "c" }, TestCommandLineStyle.Flexible, DisplayName = "AddHandlerWithState")]
    public async Task AddHandlerWithState(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var result = await commandLine.ToRunner()
            .ForState(123).AddHandler<Test1Handler>()
            .ForState("test1").AddHandler<Test2Handler>()
            .ForState().AddHandler<Test0Handler>()
            .RunAsync();
        var state = ((Test1Handler)result.HandledBy!).State;

        // Assert
        Assert.AreEqual(123, state);
    }

    public record Test0Handler : ICommandHandler
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        public Task<int> RunAsync()
        {
            return Task.FromResult(0);
        }
    }

    [Command("test1")]
    public record Test1Handler : ICommandHandler<int>
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        public int? State { get; private set; }

        public Task<int> RunAsync(int state)
        {
            State = state;
            return Task.FromResult(0);
        }
    }

    [Command("test1")]
    public record Test2Handler : ICommandHandler<string>
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        public string? State { get; private set; }

        public Task<int> RunAsync(string state)
        {
            State = state;
            return Task.FromResult(0);
        }
    }
}
