using dotnetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotnetCampus.Cli.Tests;

/// <summary>
/// 测试命令行参数是否正确被解析到了。
/// </summary>
[TestClass]
public class GnuCommandLineParserTests
{
    [TestMethod("单个单词的长名称参数，字符串类型，可正常赋值。")]
    public void NoArgs_NoValueAssigned()
    {
        // Arrange
        string[] args = ["--value", "test"];
        string? value = null;

        // Act
        var result = CommandLine.Parse([
            ])
            .AddHandler<T01_Options>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }
}

internal record T01_Options
{
    [Option]
    public required string Value { get; set; }
}
