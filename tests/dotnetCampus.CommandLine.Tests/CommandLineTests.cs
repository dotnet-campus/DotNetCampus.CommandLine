using System.Threading.Tasks;
using dotnetCampus.Cli.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnetCampus.Cli.Tests;

[TestClass]
public class CommandLineTests
{
    [TestMethod("GNU 风格，无参：执行默认处理器。")]
    public async Task Test_GNU_Style_No_Args_Default_Handler()
    {
        var result = await CommandLine.Parse([])
            .AddHandler<FakeVerbCommandHandler>()
            .AddHandler<FakeCommandOptions>(o => 0)
            .AddHandlers<AssemblyCommandHandler>()
            .RunAsync();
        Assert.AreEqual(0, result);
    }
}
