using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class UrlCommandTests
{
    [TestMethod]
    // 空格
    [DataRow(new[] { "test://?option=value%20with%20space" }, "value with space", DisplayName = "[Uri] test://?option=value%20with%20space")]
    // 特殊字符（# & % 等）
    [DataRow(new[] { "test://?option=special%23chars%26more%25" }, "special#chars&more%", DisplayName = "[Uri] test://?option=special%23chars%26more%25")]
    // 保留字符（/ ? : @ 等）
    [DataRow(new[] { "test://?option=reserved%2Fchars%3F%3A%40" }, "reserved/chars?:@", DisplayName = "[Uri] test://?option=reserved%2Fchars%3F%3A%40")]
    // 中文和其他非 ASCII 字符
    [DataRow(new[] { "test://?option=%E4%B8%AD%E6%96%87" }, "中文", DisplayName = "[Uri] test://?option=%E4%B8%AD%E6%96%87")]
    // emoji 字符
    [DataRow(new[] { "test://?option=%F0%9F%98%81" }, "😁", DisplayName = "[Uri] test://?option=%F0%9F%98%81")]
    public void Escape(string[] args, string expectedValue)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, TestCommandLineStyle.Url.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual(expectedValue, options.Option);
    }

    [TestMethod]
    [DataRow(new[] { "test://#anchor" }, "anchor", DisplayName = "[Uri] test://?option=value#anchor")]
    [DataRow(new[] { "test://?option=value#anchor" }, "anchor", DisplayName = "[Uri] test://?option=value#anchor")]
    public void Fragment(string[] args, string expectedValue)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, TestCommandLineStyle.Url.ToParsingOptions());

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.AreEqual(expectedValue, options.Fragment);
    }

    public record TestOptions
    {
        [Option('o', "option")]
        public string? Option { get; set; }

        [Option("fragment")]
        public string? Fragment { get; set; }
    }
}
