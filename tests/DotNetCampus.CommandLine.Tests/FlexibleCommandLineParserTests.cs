using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 测试 Flexible 风格命令行参数是否正确被解析。
/// </summary>
[TestClass]
public class FlexibleCommandLineParserTests
{
    private CommandLineParsingOptions Flexible { get; } = CommandLineParsingOptions.Flexible;

    #region 1. 参数前缀支持多种形式

    [TestMethod("1.1. 支持双破折线(--) + 字符串类型参数")]
    public void DoubleHyphen_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["--value", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.2. 支持单破折线(-) + 字符串类型参数")]
    public void SingleHyphen_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["-value", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.3. 支持斜杠(/) + 字符串类型参数")]
    public void Slash_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["/value", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    #endregion

    #region 2. 参数值分隔符兼容多种形式

    [TestMethod("2.1. 支持空格作为分隔符")]
    public void SpaceSeparator_ValueAssigned()
    {
        // Arrange
        string[] args = ["--value", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("2.2. 支持等号(=)作为分隔符")]
    public void EqualSeparator_ValueAssigned()
    {
        // Arrange
        string[] args = ["--value=test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("2.3. 支持冒号(:)作为分隔符")]
    public void ColonSeparator_ValueAssigned()
    {
        // Arrange
        string[] args = ["--value:test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [Ignore("只有 GNU 风格支持。Flexible 包容万象，但包容不下这种偏门功能。")]
    [TestMethod("2.4. 短选项支持无分隔符直接跟参数(GNU风格)")]
    public void ShortOption_NoSeparator_ValueAssigned()
    {
        // Arrange
        string[] args = ["-vtest"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible02_ShortOption>(o => value = o.V)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("2.5. 短选项与分隔符混合使用")]
    public void ShortOption_MixedSeparators_AllAssigned()
    {
        // Arrange
        string[] args = ["-a", "value1", "-b=value2", "-c:value3"];
        string? valueA = null;
        string? valueB = null;
        string? valueC = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible03_MultipleShortOptions>(o =>
            {
                valueA = o.A;
                valueB = o.B;
                valueC = o.C;
            })
            .Run();

        // Assert
        Assert.AreEqual("value1", valueA);
        Assert.AreEqual("value2", valueB);
        Assert.AreEqual("value3", valueC);
    }

    #endregion

    #region 3. 参数命名风格兼容性

    [TestMethod("3.1. 支持kebab-case命名风格")]
    public void KebabCase_OptionName_ValueAssigned()
    {
        // Arrange
        string[] args = ["--parameter-name", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible04_KebabCaseOptions>(o => value = o.ParameterName)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("3.2. 支持PascalCase命名风格")]
    public void PascalCase_OptionName_ValueAssigned()
    {
        // Arrange
        string[] args = ["-ParameterName", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible04_KebabCaseOptions>(o => value = o.ParameterName)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("3.3. 支持camelCase命名风格")]
    public void CamelCase_OptionName_ValueAssigned()
    {
        // Arrange
        string[] args = ["--parameterName", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible04_KebabCaseOptions>(o => value = o.ParameterName)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    #endregion

    #region 4. 大小写不敏感测试

    [TestMethod("4.1. 选项名大小写不敏感")]
    public void CaseInsensitive_OptionName_ValueAssigned()
    {
        // Arrange
        string[] args = ["--VALUE", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("4.2. 短选项大小写不敏感")]
    public void CaseInsensitive_ShortOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["-V", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible02_ShortOption>(o => value = o.V)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    #endregion

    #region 5. 短选项和长选项测试

    [TestMethod("5.1. 短选项与长选项对应相同属性")]
    public void ShortAndLongOption_SameProperty_ValueAssigned()
    {
        // Arrange
        string[] args1 = ["--output", "file.txt"];
        string[] args2 = ["-o", "file.txt"];
        string? value1 = null;
        string? value2 = null;

        // Act
        CommandLine.Parse(args1, Flexible)
            .AddHandler<Flexible05_ShortLongOptions>(o => value1 = o.Output)
            .Run();

        CommandLine.Parse(args2, Flexible)
            .AddHandler<Flexible05_ShortLongOptions>(o => value2 = o.Output)
            .Run();

        // Assert
        Assert.AreEqual("file.txt", value1);
        Assert.AreEqual("file.txt", value2);
    }

    [Ignore("自动形式经过讨论，不支持短选项组合；如果需要，请改用 GNU 风格。")]
    [TestMethod("5.2. 支持有限的短选项组合")]
    public void ShortOptionCombination_AllAssigned()
    {
        // Arrange
        string[] args = ["-abc"];
        bool? flagA = null;
        bool? flagB = null;
        bool? flagC = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible06_BooleanShortOptions>(o =>
            {
                flagA = o.A;
                flagB = o.B;
                flagC = o.C;
            })
            .Run();

        // Assert
        Assert.IsTrue(flagA);
        Assert.IsTrue(flagB);
        Assert.IsTrue(flagC);
    }

    #endregion

    #region 6. 布尔开关参数测试

    [TestMethod("6.1. 不带值的布尔参数默认为true")]
    public void BooleanFlag_NoValue_DefaultTrue()
    {
        // Arrange
        string[] args = ["--flag"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible07_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsTrue(flag);
    }

    [TestMethod("6.2. 布尔参数支持true/false值")]
    public void BooleanFlag_ExplicitValue_Assigned()
    {
        // Arrange
        string[] args = ["--flag=false"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible07_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsFalse(flag);
    }

    [TestMethod("6.3. 布尔参数支持yes/no值")]
    public void BooleanFlag_YesNoValue_Assigned()
    {
        // Arrange
        string[] args = ["--flag=yes"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible07_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsTrue(flag);
    }

    [TestMethod("6.4. 布尔参数支持on/off值")]
    public void BooleanFlag_OnOffValue_Assigned()
    {
        // Arrange
        string[] args = ["--flag=off"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible07_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsFalse(flag);
    }

    [Ignore("否定形式(no-prefix)计划以后再实现。")]
    [TestMethod("6.5. 支持否定形式(no-prefix)的布尔参数")]
    public void BooleanFlag_NegatePrefix_Assigned()
    {
        // Arrange
        string[] args = ["--no-feature"];
        bool? feature = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible08_NegatedBooleanOptions>(o => feature = o.Feature)
            .Run();

        // Assert
        Assert.IsFalse(feature);
    }

    #endregion

    #region 7. 位置参数测试

    [TestMethod("7.1. 单个位置参数解析正确")]
    public void SinglePositionalParameter_ValueAssigned()
    {
        // Arrange
        string[] args = ["positional-value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible09_PositionalOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("positional-value", value);
    }

    [TestMethod("7.2. 多个位置参数解析正确")]
    public void MultiplePositionalParameters_AllAssigned()
    {
        // Arrange
        string[] args = ["value1", "value2", "value3"];
        string[]? values = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible10_MultiplePositionalOptions>(o => values = o.Values)
            .Run();

        // Assert
        Assert.IsNotNull(values);
        Assert.AreEqual(3, values.Length);
        CollectionAssert.AreEqual(new[] { "value1", "value2", "value3" }, values);
    }

    [TestMethod("7.3. 双破折号(--)后的内容作为位置参数")]
    public void DoubleHyphen_TreatsFollowingAsValues()
    {
        // Arrange
        string[] args = ["--option", "value", "--", "--not-an-option", "-x"];
        string? option = null;
        string[]? values = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible11_TerminatorOptions>(o =>
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

    [TestMethod("7.4. 选项与位置参数混合使用")]
    public void MixedOptionsAndValues_AllAssigned()
    {
        // Arrange
        string[] args = ["value1", "--option=test", "value2"];
        string? option = null;
        string? value1 = null;
        string? value2 = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible12_MixedOptions>(o =>
            {
                option = o.Option;
                value1 = o.Value1;
                value2 = o.Value2;
            })
            .Run();

        // Assert
        Assert.AreEqual("test", option);
        Assert.AreEqual("value1", value1);
        Assert.AreEqual("value2", value2);
    }

    #endregion

    #region 8. 混合风格测试

    [TestMethod("8.1. 混合使用多种风格的选项前缀")]
    public void MixedPrefixStyles_AllAssigned()
    {
        // Arrange
        string[] args = ["--option1", "value1", "-option2", "value2", "/option3", "value3"];
        string? value1 = null;
        string? value2 = null;
        string? value3 = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible13_MixedPrefixOptions>(o =>
            {
                value1 = o.Option1;
                value2 = o.Option2;
                value3 = o.Option3;
            })
            .Run();

        // Assert
        Assert.AreEqual("value1", value1);
        Assert.AreEqual("value2", value2);
        Assert.AreEqual("value3", value3);
    }

    [TestMethod("8.2. 混合使用多种分隔符")]
    public void MixedSeparatorStyles_AllAssigned()
    {
        // Arrange
        string[] args = ["--option1", "value1", "--option2=value2", "--option3:value3"];
        string? value1 = null;
        string? value2 = null;
        string? value3 = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible13_MixedPrefixOptions>(o =>
            {
                value1 = o.Option1;
                value2 = o.Option2;
                value3 = o.Option3;
            })
            .Run();

        // Assert
        Assert.AreEqual("value1", value1);
        Assert.AreEqual("value2", value2);
        Assert.AreEqual("value3", value3);
    }

    [TestMethod("8.3. 混合使用多种命名风格")]
    public void MixedNamingStyles_AllAssigned()
    {
        // Arrange
        string[] args = ["--kebab-case", "value1", "-PascalCase", "value2", "--camelCase", "value3"];
        string? kebabValue = null;
        string? pascalValue = null;
        string? camelValue = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible14_MixedNamingOptions>(o =>
            {
                kebabValue = o.KebabCase;
                pascalValue = o.PascalCase;
                camelValue = o.CamelCase;
            })
            .Run();

        // Assert
        Assert.AreEqual("value1", kebabValue);
        Assert.AreEqual("value2", pascalValue);
        Assert.AreEqual("value3", camelValue);
    }

    #endregion

    #region 9. 边界情况和错误处理

    [TestMethod("9.1. 未知选项，抛出异常")]
    public void UnknownOption_ThrowsException()
    {
        // Arrange
        string[] args = ["--non-existent", "value"];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, Flexible)
                .AddHandler<Flexible01_StringOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("9.2. 选项名称拼写错误时，抛出异常并提示近似选项")]
    public void MisspelledOption_ThrowsExceptionWithHint()
    {
        // Arrange
        string[] args = ["--valu", "test"]; // 应该是 --value

        // Act
        var exception = Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, Flexible)
                .AddHandler<Flexible01_StringOptions>(_ => { })
                .Run();
        });

        // Assert
        StringAssert.Contains(exception.Message, "value"); // 确保消息中包含近似的正确选项
    }

    [TestMethod("9.3. 类型不匹配时，抛出异常")]
    public void TypeMismatch_ThrowsException()
    {
        // Arrange
        string[] args = ["--number=not-a-number"];

        // Act & Assert
        Assert.ThrowsExactly<CommandLineParseValueException>(() =>
        {
            CommandLine.Parse(args, Flexible)
                .AddHandler<Flexible15_TypedOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("9.4. 缺失必需参数时，抛出异常")]
    public void MissingRequiredOption_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, Flexible)
                .AddHandler<Flexible16_RequiredOptions>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 10. 异步处理测试

    [TestMethod("10.1. 异步处理方法，正确执行")]
    public async Task AsyncHandler_ExecutesCorrectly()
    {
        // Arrange
        string[] args = ["--value=async-test"];
        string? value = null;

        // Act
        await CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible01_StringOptions>(async o =>
            {
                await Task.Delay(10); // 模拟异步操作
                value = o.Value;
            })
            .RunAsync();

        // Assert
        Assert.AreEqual("async-test", value);
    }

    #endregion

    #region 11. 列表参数测试

    [TestMethod("11.1. 支持空列表")]
    public void EmptyList_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["--files"];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                files = o.Files;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(0, files.Length);
    }

    [TestMethod("11.3. 支持分号分隔的列表")]
    public void SemicolonSeparatedList_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["--names:John;Jane;Doe"];
        IEnumerable<string>? names = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                names = o.Names;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(names);
        Assert.AreEqual(3, names.Count());
        CollectionAssert.AreEqual(new[] { "John", "Jane", "Doe" }, names.ToArray());
    }

    [TestMethod("11.4. 支持混合分隔符的列表")]
    public void MixedSeparatorList_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["--files:file1.txt,file2.txt;file3.txt"];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                files = o.Files;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(3, files.Length);
        CollectionAssert.AreEqual(new[] { "file1.txt", "file2.txt", "file3.txt" }, files);
    }

    [TestMethod("11.5. 支持带引号的列表参数")]
    public void QuotedListElements_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["--files:\"file with spaces.txt\",\"another file.txt\""];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                files = o.Files;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(2, files.Length);
        CollectionAssert.AreEqual(new[] { "file with spaces.txt", "another file.txt" }, files);
    }

    [TestMethod("11.6. 带引号的列表参数，多次指定")]
    public void QuotedListElements_MultipleOptions()
    {
        // Arrange
        string[] args = ["--files", "\"file with spaces.txt\"", "--files", "normal.txt", "--files", "\"another file.txt\""];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                files = o.Files;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(3, files.Length);
        CollectionAssert.AreEqual(new[] { "file with spaces.txt", "normal.txt", "another file.txt" }, files);
    }

    [TestMethod("11.7. 带引号的列表参数，通过分隔符")]
    public void QuotedListElements_WithSeparators()
    {
        // Arrange
        string[] args = ["--names:\"John Doe\";\"Jane Smith\";Anonymous"];
        IEnumerable<string>? names = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                names = o.Names;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(names);
        Assert.AreEqual(3, names.Count());
        CollectionAssert.AreEqual(new[] { "John Doe", "Jane Smith", "Anonymous" }, names.ToArray());
    }

    [TestMethod("11.9. 单选项后接带引号且引号内有逗号或分号的多个值")]
    public void SingleOption_QuotedMultipleValuesWithColonOrSemicolon()
    {
        // Arrange
        string[] args = ["--files", "\"tag1,with,colon\",\"tag2,with,colon\"", "--tags", "\"tag1;with;semicolon\";\"tag2;with;semicolon\""];
        string[]? files = null;
        IReadOnlyList<string>? tags = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                files = o.Files;
                tags = o.Tags;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.IsNotNull(tags);
        Assert.AreEqual(2, files.Length);
        Assert.AreEqual(2, tags.Count);
        CollectionAssert.AreEqual(new[] { "tag1,with,colon", "tag2,with,colon" }, files.ToArray());
        CollectionAssert.AreEqual(new[] { "tag1;with;semicolon", "tag2;with;semicolon" }, tags.ToArray());
    }

    [TestMethod("11.10. 单选项后接带引号且引号内有逗号或分号的多个值，其中部分引号和分隔符含空字符串")]
    public void SingleOption_QuotedMultipleValuesWithColonOrSemicolonAndEmpty()
    {
        // Arrange
        string[] args = ["--files", "\"tag1,with,colon\",,\"tag2,with,colon\"", "--tags", "\"tag1;with;semicolon\";;;\"\";\"tag2;with;semicolon\""];
        string[]? files = null;
        IReadOnlyList<string>? tags = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<Flexible16_ListOptions>(o =>
            {
                files = o.Files;
                tags = o.Tags;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.IsNotNull(tags);
        Assert.AreEqual(3, files.Length);
        Assert.AreEqual(5, tags.Count);
        CollectionAssert.AreEqual(new[] { "tag1,with,colon", "", "tag2,with,colon" }, files.ToArray());
        CollectionAssert.AreEqual(new[] { "tag1;with;semicolon", "", "", "", "tag2;with;semicolon" }, tags.ToArray());
    }

    #endregion
}

#region 测试用数据模型

internal record Flexible01_StringOptions
{
    [Option]
    public required string Value { get; init; }
}

internal record Flexible02_ShortOption
{
    [Option('v')]
    public required string V { get; init; }
}

internal record Flexible03_MultipleShortOptions
{
    [Option('a')]
    public required string A { get; init; }

    [Option('b')]
    public required string B { get; init; }

    [Option('c')]
    public required string C { get; init; }
}

internal record Flexible04_KebabCaseOptions
{
    [Option("parameter-name")]
    public required string ParameterName { get; init; }
}

internal record Flexible05_ShortLongOptions
{
    [Option('o', "output")]
    public required string Output { get; init; }
}

internal record Flexible06_BooleanShortOptions
{
    [Option('a')]
    public bool A { get; init; }

    [Option('b')]
    public bool B { get; init; }

    [Option('c')]
    public bool C { get; init; }
}

internal record Flexible07_BooleanOptions
{
    [Option]
    public bool Flag { get; init; }
}

internal record Flexible08_NegatedBooleanOptions
{
    [Option("feature", Aliases = ["no-feature"])]
    public bool Feature { get; init; } = true;
}

internal record Flexible09_PositionalOptions
{
    [Value]
    public required string Value { get; init; }
}

internal record Flexible10_MultiplePositionalOptions
{
    [Value(Length = int.MaxValue)]
    public required string[] Values { get; init; }
}

internal record Flexible11_TerminatorOptions
{
    [Option]
    public required string Option { get; init; }

    [Value(Length = int.MaxValue)]
    public string[] Values { get; init; } = [];
}

internal record Flexible12_MixedOptions
{
    [Value(0)]
    public required string Value1 { get; init; }

    [Option]
    public required string Option { get; init; }

    [Value(1)]
    public required string Value2 { get; init; }
}

internal record Flexible13_MixedPrefixOptions
{
    [Option]
    public required string Option1 { get; init; }

    [Option]
    public required string Option2 { get; init; }

    [Option]
    public required string Option3 { get; init; }
}

internal record Flexible14_MixedNamingOptions
{
    [Option("kebab-case")]
    public required string KebabCase { get; init; }

    [Option("PascalCase")]
    public required string PascalCase { get; init; }

    [Option("camelCase")]
    public required string CamelCase { get; init; }
}

internal record Flexible15_TypedOptions
{
    [Option]
    public int Number { get; init; }
}

internal record Flexible16_ListOptions
{
    [Option]
    public string[] Files { get; init; } = [];

    [Option]
    public IReadOnlyList<string> Tags { get; init; } = [];

    [Option]
    public IEnumerable<string> Names { get; init; } = [];
}

internal record Flexible16_RequiredOptions
{
    [Option]
    public required string RequiredValue { get; init; }
}

#endregion

#region 代码清理

// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore InconsistentNaming

#endregion
