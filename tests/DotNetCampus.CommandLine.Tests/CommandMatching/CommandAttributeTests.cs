using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Tests.ParsingStyles;
using DotNetCampus.CommandLine.FakeObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.CommandMatching;

/// <summary>
/// 我们所有的特性（包括 <see cref="CommandAttribute"/>、<see cref="OptionAttribute"/>、<see cref="ValueAttribute"/> 都标记了
/// [Conditional("FOR_SOURCE_GENERATION_ONLY")]。由于我们一般不会标记条件编译符 FOR_SOURCE_GENERATION_ONLY，所以这些特性在编译完成后都会消失。
/// 源生成器可以看见源代码中的这些特性，看不见已编译好的程序集中的这些特性；
/// 所以源生成器只能生成本项目（程序集）中与这些特性相关的代码，无法生成其他程序集中的相关代码；这就可能导致无法生成跨程序集对象的命令。<br/>
/// 为了解决这个问题，我们要求所有与这些特性相关的代码必须在本项目中生成，跨程序集的必须直接引用原项目中生成的代码。<br/>
/// 本单元测试旨在测试以确保这种情况的正确性。
/// </summary>
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
        commandLine.ToRunner()
            .AddHandler<CommandObject0InAnotherAssembly>(o => matched = (o.GetType().Name, o.Option))
            .AddHandler<CommandObject1InAnotherAssembly>(o => matched = (o.GetType().Name, o.Option))
            .AddHandler<CommandObject2InAnotherAssembly>(o => matched = (o.GetType().Name, o.Option))
            .Run();

        // Assert
        Assert.AreEqual(expectedCommand, matched.TypeName);
        Assert.AreEqual("value", matched.Value);
    }
}
