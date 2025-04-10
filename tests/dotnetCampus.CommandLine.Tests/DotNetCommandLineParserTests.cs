using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace dotnetCampus.Cli.Tests;

/// <summary>
/// 测试 DotNet 风格命令行参数是否正确被解析。
/// </summary>
[TestClass]
public class DotNetCommandLineParserTests
{
    private CommandLineParsingOptions DotNet { get; } = new CommandLineParsingOptions { Style = CommandLineStyle.DotNet };

    #region 1. 选项识别与解析

    [TestMethod("1.1. 短选项冒号形式 (-option:value)，字符串类型，可正常赋值。")]
    public void ShortOption_WithColon_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["-value:test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.2. 长选项冒号形式 (--option:value)，字符串类型，可正常赋值。")]
    public void LongOption_WithColon_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["--value:test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.3. 斜杠前缀形式 (/option:value)，字符串类型，可正常赋值。")]
    public void SlashPrefix_WithColon_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["/value:test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.4. 多个选项混合使用，全部正确解析。")]
    public void MixedOptions_MultipleParsed_AllAssigned()
    {
        // Arrange
        string[] args = ["-number:42", "--text:hello", "/flag:true"];
        int? number = null;
        string? text = null;
        bool? flag = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet03_MixedOptions>(o =>
            {
                number = o.Number;
                text = o.Text;
                flag = o.Flag;
            })
            .Run();

        // Assert
        Assert.AreEqual(42, number);
        Assert.AreEqual("hello", text);
        Assert.IsTrue(flag);
    }

    #endregion

    #region 2. 类型转换

    [TestMethod("2.1. 整数类型，赋值成功。")]
    public void IntegerOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--number:42"];
        int? number = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet04_IntegerOptions>(o => number = o.Number)
            .Run();

        // Assert
        Assert.AreEqual(42, number);
    }

    [TestMethod("2.2. 布尔类型，不带值赋为true。")]
    public void BooleanOption_NoValue_SetTrue()
    {
        // Arrange
        string[] args = ["--flag"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet05_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsTrue(flag);
    }

    [TestMethod("2.2.1. 布尔类型，带值true赋为true。")]
    public void BooleanOption_TrueValue_SetTrue()
    {
        // Arrange
        string[] args = ["--flag:true"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet05_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsTrue(flag);
    }

    [TestMethod("2.2.2. 布尔类型，带值false赋为false。")]
    public void BooleanOption_FalseValue_SetFalse()
    {
        // Arrange
        string[] args = ["--flag:false"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet05_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsFalse(flag);
    }

    [TestMethod("2.3. 枚举类型，赋值成功。")]
    public void EnumOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--log-level:Warning"];
        LogLevel? logLevel = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet06_EnumOptions>(o => logLevel = o.LogLevel)
            .Run();

        // Assert
        Assert.AreEqual(LogLevel.Warning, logLevel);
    }

    [TestMethod("2.4. 字符串数组，赋值成功。")]
    public void StringArrayOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--files:file1.txt", "--files:file2.txt", "--files:file3.txt"];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet07_ArrayOptions>(o => files = o.Files)
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(3, files.Length);
        CollectionAssert.AreEqual(new[] { "file1.txt", "file2.txt", "file3.txt" }, files);
    }

    [TestMethod("2.5. 列表类型，赋值成功。")]
    public void ListOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--tags:tag1", "--tags:tag2", "--tags:tag3"];
        List<string>? tags = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet08_ListOptions>(o => tags = o.Tags.ToList())
            .Run();

        // Assert
        Assert.IsNotNull(tags);
        Assert.AreEqual(3, tags.Count);
        CollectionAssert.AreEqual(new[] { "tag1", "tag2", "tag3" }, tags);
    }

    #endregion

    #region 3. 边界情况处理

    [TestMethod("3.1. 缺失必需选项，抛出异常。")]
    public void MissingRequiredOption_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsException<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, DotNet)
                .AddHandler<DotNet09_RequiredOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("3.2. 无效格式选项，抛出异常。")]
    public void InvalidOption_ThrowsException()
    {
        // Arrange
        string[] args = ["---invalid:value"]; // 三个破折号是无效的

        // Act & Assert
        Assert.ThrowsException<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, DotNet)
                .AddHandler<DotNet01_StringOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("3.3. 类型不匹配，抛出异常。")]
    public void TypeMismatch_ThrowsException()
    {
        // Arrange
        string[] args = ["--number:not-a-number"];

        // Act & Assert
        Assert.ThrowsException<CommandLineParseValueException>(() =>
        {
            CommandLine.Parse(args, DotNet)
                .AddHandler<DotNet04_IntegerOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("3.4. 大小写不敏感，识别正确。")]
    public void CaseInsensitive_CorrectOptionParsed()
    {
        // Arrange
        string[] args = ["--Ignore-Case:value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet11_CaseInsensitiveOptions>(o => value = o.IgnoreCase)
            .Run();

        // Assert
        Assert.AreEqual("value", value);
    }

    #endregion

    #region 4. 特殊特性

    [TestMethod("4.1. 选项别名，识别正确。")]
    public void OptionAliases_CorrectOptionParsed()
    {
        // Arrange
        string[] args = ["--alt:value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet12_AliasOptions>(o => value = o.OptionWithAlias)
            .Run();

        // Assert
        Assert.AreEqual("value", value);
    }

    [TestMethod("4.2. 终止选项解析符号，识别正确。")]
    public void OptionTerminator_FollowingArgsAreValues()
    {
        // Arrange
        string[] args = ["--option:value", "--", "--not-an-option", "-x"];
        string? option = null;
        string[]? values = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet14_TerminatorOptions>(o =>
            {
                option = o.Option;
                values = o.Values;
            })
            .Run();

        // Assert
        Assert.AreEqual("value", option);
        Assert.IsNotNull(values);
        Assert.AreEqual(2, values.Length);
        Assert.AreEqual("--not-an-option", values[0]);
        Assert.AreEqual("-x", values[1]);
    }

    #endregion

    #region 5. 位置参数处理

    [TestMethod("5.1. 单个位置参数，赋值成功。")]
    public void SinglePositionalValue_ValueAssigned()
    {
        // Arrange
        string[] args = ["positional-value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet15_SingleValueOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("positional-value", value);
    }

    [TestMethod("5.2. 多个位置参数，赋值成功。")]
    public void MultiplePositionalValues_AllAssigned()
    {
        // Arrange
        string[] args = ["value1", "value2", "value3"];
        string[]? values = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet16_MultipleValueOptions>(o => values = o.Values)
            .Run();

        // Assert
        Assert.IsNotNull(values);
        Assert.AreEqual(3, values.Length);
        CollectionAssert.AreEqual(new[] { "value1", "value2", "value3" }, values);
    }

    [TestMethod("5.3. 位置参数与选项混合，识别正确。")]
    public void MixedPositionalAndOptions_AllParsedCorrectly()
    {
        // Arrange
        string[] args = ["value1", "--option:opt-val", "value2"];
        string? option = null;
        string? value1 = null;
        string? value2 = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet17_MixedValueOptions>(o =>
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

    [TestMethod("5.4. 指定索引位置参数，识别正确。")]
    public void IndexedPositionalValues_CorrectAssignment()
    {
        // Arrange
        string[] args = ["first", "second", "third"];
        string? first = null;
        string? third = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet18_IndexedValueOptions>(o =>
            {
                first = o.First;
                third = o.Third;
            })
            .Run();

        // Assert
        Assert.AreEqual("first", first);
        Assert.AreEqual("third", third);
    }

    #endregion

    #region 6. Required 和 Nullable 组合测试

    [TestMethod("6.1. Non-required, Non-nullable, 无CLI参数，使用默认值。")]
    public void NonRequiredNonNullable_NoCli_UsesDefault()
    {
        // Arrange
        string[] args = [];
        string? value = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet27_NonRequiredNonNullableOption>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual(string.Empty, value); // 使用初始化时的默认值
    }

    [TestMethod("6.2. Required, Non-nullable, 无CLI参数，抛出异常。")]
    public void RequiredNonNullable_NoCli_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsException<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, DotNet)
                .AddHandler<DotNet19_RequiredNonNullableOption>(_ => { })
                .Run();
        });
    }

    [TestMethod("6.3. Non-required, Nullable, 无CLI参数，赋默认值(null)。")]
    public void NonRequiredNullable_NoCli_DefaultNull()
    {
        // Arrange
        string[] args = [];
        string? value = "not-null";

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet20_NonRequiredNullableOption>(o => value = o.Value)
            .Run();

        // Assert
        Assert.IsNull(value);
    }

    [TestMethod("6.4. Required, Nullable, 无CLI参数，抛出异常。")]
    public void RequiredNullable_NoCli_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsException<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, DotNet)
                .AddHandler<DotNet21_RequiredNullableOption>(_ => { })
                .Run();
        });
    }

    [TestMethod("6.5. 各种组合都提供CLI参数，全部赋值成功。")]
    public void AllCombinations_WithCli_AllAssigned()
    {
        // Arrange
        string[] args =
        [
            "--req-non-null:value1", "--non-req-null:value2",
            "--req-null:value3", "--non-req-non-null:value4"
        ];
        string? reqNonNull = null;
        string? nonReqNull = null;
        string? reqNull = null;
        string? nonReqNonNull = null;

        // Act
        CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet22_AllCombinationsOption>(o =>
            {
                reqNonNull = o.ReqNonNull;
                nonReqNull = o.NonReqNull;
                reqNull = o.ReqNull;
                nonReqNonNull = o.NonReqNonNull;
            })
            .Run();

        // Assert
        Assert.AreEqual("value1", reqNonNull);
        Assert.AreEqual("value2", nonReqNull);
        Assert.AreEqual("value3", reqNull);
        Assert.AreEqual("value4", nonReqNonNull);
    }

    #endregion

    #region 7. 异步处理测试

    [TestMethod("7.1. 异步处理方法，正确执行。")]
    public async Task AsyncHandler_ExecutesCorrectly()
    {
        // Arrange
        string[] args = ["--value:async-test"];
        string? value = null;

        // Act
        await CommandLine.Parse(args, DotNet)
            .AddHandler<DotNet01_StringOptions>(async o =>
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

internal record DotNet01_StringOptions
{
    [Option]
    public required string Value { get; init; }
}

internal record DotNet03_MixedOptions
{
    [Option]
    public int Number { get; init; }

    [Option]
    public required string Text { get; init; }

    [Option]
    public bool Flag { get; init; }
}

internal record DotNet04_IntegerOptions
{
    [Option]
    public int Number { get; init; }
}

internal record DotNet05_BooleanOptions
{
    [Option]
    public bool Flag { get; init; }
}

internal record DotNet06_EnumOptions
{
    [Option("log-level")]
    public LogLevel LogLevel { get; init; }
}

internal record DotNet07_ArrayOptions
{
    [Option]
    public string[] Files { get; init; } = [];
}

internal record DotNet08_ListOptions
{
    [Option]
    public IReadOnlyList<string> Tags { get; init; } = [];
}

internal record DotNet09_RequiredOptions
{
    [Option]
    public required string RequiredValue { get; init; }
}

internal record DotNet11_CaseInsensitiveOptions
{
    [Option("ignore-case")]
    public string IgnoreCase { get; init; } = string.Empty;
}

internal record DotNet12_AliasOptions
{
    [Option("option-with-alias", Aliases = ["alt", "alternate"])]
    public string OptionWithAlias { get; init; } = string.Empty;
}

internal record DotNet14_TerminatorOptions
{
    [Option]
    public string Option { get; init; } = string.Empty;

    [Value(Length = int.MaxValue)]
    public string[] Values { get; init; } = [];
}

internal record DotNet15_SingleValueOptions
{
    [Value]
    public string Value { get; init; } = string.Empty;
}

internal record DotNet16_MultipleValueOptions
{
    [Value(Length = int.MaxValue)]
    public string[] Values { get; init; } = [];
}

internal record DotNet17_MixedValueOptions
{
    [Value(0)]
    public string Value1 { get; init; } = string.Empty;

    [Option]
    public string Option { get; init; } = string.Empty;

    [Value(1)]
    public string Value2 { get; init; } = string.Empty;
}

internal record DotNet18_IndexedValueOptions
{
    [Value(0)]
    public string First { get; init; } = string.Empty;

    [Value(2)]
    public string Third { get; init; } = string.Empty;
}

internal record DotNet19_RequiredNonNullableOption
{
    [Option]
    public required string Value { get; init; }
}

internal record DotNet20_NonRequiredNullableOption
{
    [Option]
    public string? Value { get; init; }
}

internal record DotNet21_RequiredNullableOption
{
    [Option]
    public required string? Value { get; init; }
}

internal record DotNet22_AllCombinationsOption
{
    [Option("req-non-null")]
    public required string ReqNonNull { get; init; }

    [Option("non-req-null")]
    public string? NonReqNull { get; init; }

    [Option("req-null")]
    public required string? ReqNull { get; init; }

    [Option("non-req-non-null")]
    public string NonReqNonNull { get; init; } = string.Empty;
}

internal record DotNet27_NonRequiredNonNullableOption
{
    [Option]
    public string Value { get; init; } = string.Empty;
}

#endregion
