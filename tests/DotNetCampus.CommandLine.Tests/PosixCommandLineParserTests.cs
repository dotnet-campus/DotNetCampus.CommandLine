using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 测试POSIX风格命令行参数是否正确被解析。
/// </summary>
[TestClass]
public class PosixCommandLineParserTests
{
    private CommandLineParsingOptions POSIX { get; } = CommandLineParsingOptions.Posix;

    #region 1. 基本短选项解析

    [TestMethod("1.1. 单个短选项，字符串类型，可正常赋值。")]
    public void SingleShortOption_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["-v", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX01_ShortOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.2. 带参数的短选项，数值类型，可正常赋值。")]
    public void ShortOptionWithValue_IntType_ValueAssigned()
    {
        // Arrange
        string[] args = ["-n", "42"];
        int? number = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX02_IntegerOptions>(o => number = o.Number)
            .Run();

        // Assert
        Assert.AreEqual(42, number);
    }

    [TestMethod("1.3. 多个短选项，全部正确解析。")]
    public void MultipleShortOptions_AllParsed()
    {
        // Arrange
        string[] args = ["-v", "text", "-n", "42", "-f"];
        string? value = null;
        int? number = null;
        bool? flag = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX03_MixedOptions>(o =>
            {
                value = o.Value;
                number = o.Number;
                flag = o.Flag;
            })
            .Run();

        // Assert
        Assert.AreEqual("text", value);
        Assert.AreEqual(42, number);
        Assert.IsTrue(flag);
    }

    [Ignore("POSIX风格不支持短选项无空格跟参数的语法")]
    [TestMethod("1.4. 短选项无空格跟参数 (不支持) 。")]
    public void ShortOptionNoSpace_NotSupported_ThrowsException()
    {
        // Arrange
        string[] args = ["-vtest"];

        // Act & Assert
        Assert.ThrowsException<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, POSIX)
                .AddHandler<POSIX01_ShortOptions>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 2. 组合短选项

    [TestMethod("2.1. 组合布尔短选项，全部正确解析。")]
    public void CombinedShortOptions_BooleanFlags_AllAssigned()
    {
        // Arrange
        string[] args = ["-abc"];
        bool? optionA = null;
        bool? optionB = null;
        bool? optionC = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX04_CombinedOptions>(o =>
            {
                optionA = o.OptionA;
                optionB = o.OptionB;
                optionC = o.OptionC;
            })
            .Run();

        // Assert
        Assert.IsTrue(optionA);
        Assert.IsTrue(optionB);
        Assert.IsTrue(optionC);
    }

    [TestMethod("2.2. 组合短选项中，最后一个带参数会抛异常。")]
    public void CombinedShortOptions_LastWithParam_ThrowsException()
    {
        // Arrange
        string[] args = ["-abc", "value"];

        // Act & Assert
        Assert.ThrowsException<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, POSIX)
                .AddHandler<POSIX05_CombinedWithValueOptions>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 3. 选项终止符(--)

    [TestMethod("3.1. 终止符后的参数被当作位置参数处理。")]
    public void OptionTerminator_FollowingArgsAreValues()
    {
        // Arrange
        string[] args = ["-o", "value", "--", "-x", "-y"];
        string? option = null;
        string[]? values = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX06_TerminatorOptions>(o =>
            {
                option = o.Option;
                values = o.Values;
            })
            .Run();

        // Assert
        Assert.AreEqual("value", option);
        Assert.IsNotNull(values);
        Assert.AreEqual(2, values.Length);
        Assert.AreEqual("-x", values[0]);
        Assert.AreEqual("-y", values[1]);
    }

    #endregion

    #region 4. 位置参数处理

    [TestMethod("4.1. 单个位置参数，赋值成功。")]
    public void SinglePositionalValue_ValueAssigned()
    {
        // Arrange
        string[] args = ["positional-value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX07_SingleValueOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("positional-value", value);
    }

    [TestMethod("4.2. 多个位置参数，赋值成功。")]
    public void MultiplePositionalValues_AllAssigned()
    {
        // Arrange
        string[] args = ["value1", "value2", "value3"];
        string[]? values = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX08_MultipleValueOptions>(o => values = o.Values)
            .Run();

        // Assert
        Assert.IsNotNull(values);
        Assert.AreEqual(3, values.Length);
        CollectionAssert.AreEqual(new[] { "value1", "value2", "value3" }, values);
    }

    [TestMethod("4.3. 位置参数与选项混合，识别正确。")]
    public void MixedPositionalAndOptions_AllParsedCorrectly()
    {
        // Arrange
        string[] args = ["value1", "-o", "opt-val", "value2"];
        string? option = null;
        string? value1 = null;
        string? value2 = null;

        // Act
        CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX09_MixedValueOptions>(o =>
            {
                option = o.Option;
                value1 = o.Value1;
                value2 = o.Value2;
            })
            .Run();

        // Assert
        Assert.AreEqual("opt-val", option);
        Assert.AreEqual("value1", value1);
        Assert.AreEqual("value2", value2);
    }

    #endregion

    #region 5. 边界情况测试

    [TestMethod("5.1. 缺失必需选项，抛出异常。")]
    public void MissingRequiredOption_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsException<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, POSIX)
                .AddHandler<POSIX10_RequiredOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("5.2. 无效选项格式，抛出异常。")]
    public void InvalidOption_ThrowsException()
    {
        // Arrange
        string[] args = ["-invalid-format"];

        // Act & Assert
        Assert.ThrowsException<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, POSIX)
                .AddHandler<POSIX01_ShortOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("5.3. 类型不匹配，抛出异常。")]
    public void TypeMismatch_ThrowsException()
    {
        // Arrange
        string[] args = ["-n", "not-a-number"];

        // Act & Assert
        Assert.ThrowsException<CommandLineParseValueException>(() =>
        {
            CommandLine.Parse(args, POSIX)
                .AddHandler<POSIX02_IntegerOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("5.4. 不允许长选项，抛出异常。")]
    public void LongOption_NotSupported_ThrowsException()
    {
        // Arrange
        string[] args = ["--option", "value"];

        // Act & Assert
        Assert.ThrowsException<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, POSIX)
                .AddHandler<POSIX11_LongOptionTest>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 6. 异步处理测试

    [TestMethod("6.1. 异步处理方法，正确执行。")]
    public async Task AsyncHandler_ExecutesCorrectly()
    {
        // Arrange
        string[] args = ["-v", "async-test"];
        string? value = null;

        // Act
        await CommandLine.Parse(args, POSIX)
            .AddHandler<POSIX01_ShortOptions>(async o =>
            {
                await Task.Delay(10); // 模拟异步操作
                value = o.Value;
            })
            .RunAsync();

        // Assert
        Assert.AreEqual("async-test", value);
    }

    #endregion
}

#region 测试用数据模型

internal record POSIX01_ShortOptions
{
    [Option('v')]
    public required string Value { get; init; }
}

internal record POSIX02_IntegerOptions
{
    [Option('n')]
    public int Number { get; init; }
}

internal record POSIX03_MixedOptions
{
    [Option('v')]
    public required string Value { get; init; }

    [Option('n')]
    public int Number { get; init; }

    [Option('f')]
    public bool Flag { get; init; }
}

internal record POSIX04_CombinedOptions
{
    [Option('a')]
    public bool OptionA { get; init; }

    [Option('b')]
    public bool OptionB { get; init; }

    [Option('c')]
    public bool OptionC { get; init; }
}

internal record POSIX05_CombinedWithValueOptions
{
    [Option('a')]
    public bool OptionA { get; init; }

    [Option('b')]
    public bool OptionB { get; init; }

    [Option('c')]
    public required string OptionC { get; init; }
}

internal record POSIX06_TerminatorOptions
{
    [Option('o')]
    public string Option { get; init; } = string.Empty;

    [Value(Length = int.MaxValue)]
    public string[] Values { get; init; } = [];
}

internal record POSIX07_SingleValueOptions
{
    [Value]
    public string Value { get; init; } = string.Empty;
}

internal record POSIX08_MultipleValueOptions
{
    [Value(Length = int.MaxValue)]
    public string[] Values { get; init; } = [];
}

internal record POSIX09_MixedValueOptions
{
    [Value(0)]
    public string Value1 { get; init; } = string.Empty;

    [Option('o')]
    public string Option { get; init; } = string.Empty;

    [Value(1)]
    public string Value2 { get; init; } = string.Empty;
}

internal record POSIX10_RequiredOptions
{
    [Option('r')]
    public required string RequiredValue { get; init; }
}

internal record POSIX11_LongOptionTest
{
    [Option("option")] // 这个会被POSIX风格拒绝，因为POSIX不支持长选项
    public string LongOption { get; init; } = string.Empty;
}

#endregion
