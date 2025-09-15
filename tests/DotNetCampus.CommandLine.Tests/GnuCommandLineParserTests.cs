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
/// 测试GNU风格命令行参数是否正确被解析到了。
/// </summary>
[TestClass]
public class GnuCommandLineParserTests
{
    private CommandLineParsingOptions GNU { get; } = CommandLineParsingOptions.Gnu;

    #region 1. 选项识别与解析

    [TestMethod("1.1. 长选项，字符串类型，可正常赋值。")]
    public void LongOption_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["--value", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.2. 短选项，字符串类型，可正常赋值。")]
    public void ShortOption_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["-v", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU02_ShortOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.3. 长选项带等号，字符串类型，可正常赋值。")]
    public void LongOptionWithEquals_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["--value=test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU01_StringOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.4.1 短选项无空格，字符串类型，可正常赋值。")]
    public void ShortOptionNoSpace_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["-vtest.txt"];
        string? value = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU02_ShortOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test.txt", value);
    }

    [Ignore("目前先 Parse 后 As 的两个步骤，会使得第 1 步的 Parse 无法区分这种短选项无空格的值。1.4.1 因为带了非字母的符号所以还能勉强区分。除非我们未来在 CommandLine 对象里对同一个短选项存两种值才可能。")]
    [TestMethod("1.4.2 短选项无空格，但难以与缩写区分，字符串类型，可正常赋值。")]
    public void ShortOptionNoSpace2_StringType_ValueAssigned()
    {
        // Arrange
        string[] args = ["-vtest"];
        string? value = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU02_ShortOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("1.5. 多个选项混合使用，全部正确解析。")]
    public void MixedOptions_MultipleParsed_AllAssigned()
    {
        // Arrange
        string[] args =
        [
            "-n", "42", "-u", "11", "--text", "hello", "--nullable-text", "hello null", "--nullable-list", "a", "--nullable-nullable-list", "b", "-b"
        ];
        int? number = null;
        int? nullableNumber = null;
        string? text = null;
        string? nullableText = null;
        IReadOnlyList<string?>? nullableList = null;
        IReadOnlyList<string?>? nullableNullableList = null;
        bool? flag = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU03_MixedOptions>(o =>
            {
                nullableNumber = o.NullableNumber;
                number = o.Number;
                text = o.Text;
                nullableText = o.NullableText;
                nullableList = o.NullableList;
                nullableNullableList = o.NullableNullableList;
                flag = o.Flag;
            })
            .Run();

        // Assert
        Assert.AreEqual(11, nullableNumber);
        Assert.AreEqual(42, number);
        Assert.AreEqual("hello", text);
        Assert.AreEqual("hello null", nullableText);
        CollectionAssert.AreEqual(new[] { "a" }, nullableList?.ToList());
        CollectionAssert.AreEqual(new[] { "b" }, nullableNullableList?.ToList());
        Assert.IsTrue(flag);
    }

    #endregion

    #region 2. 类型转换

    [TestMethod("2.1. 整数类型，赋值成功。")]
    public void IntegerOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--number", "42"];
        int? number = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU04_IntegerOptions>(o => number = o.Number)
            .Run();

        // Assert
        Assert.AreEqual(42, number);
    }

    [TestMethod("2.2. 布尔类型，赋值成功。")]
    public void BooleanOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--flag"];
        bool? flag = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU05_BooleanOptions>(o => flag = o.Flag)
            .Run();

        // Assert
        Assert.IsTrue(flag);
    }

    [TestMethod("2.3. 枚举类型，赋值成功。")]
    public void EnumOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--log-level", "Warning", "--nullable-log-level", "Error"];
        LogLevel? logLevel = null;
        LogLevel? nullableLogLevel = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU06_EnumOptions>(o =>
            {
                logLevel = o.LogLevel;
                nullableLogLevel = o.NullableLogLevel;
            })
            .Run();

        // Assert
        Assert.AreEqual(LogLevel.Warning, logLevel);
        Assert.AreEqual(LogLevel.Error, nullableLogLevel);
    }

    [TestMethod("2.4. 字符串数组，赋值成功。")]
    public void StringArrayOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--files", "file1.txt", "--files", "file2.txt", "--files", "file3.txt"];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU07_ArrayOptions>(o => files = o.Files)
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
        string[] args = ["--tags", "tag1", "--tags", "tag2", "--tags", "tag3"];
        List<string>? tags = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU08_ListOptions>(o => tags = o.Tags.ToList())
            .Run();

        // Assert
        Assert.IsNotNull(tags);
        Assert.AreEqual(3, tags.Count);
        CollectionAssert.AreEqual(new[] { "tag1", "tag2", "tag3" }, tags);
    }

    [TestMethod("2.6. 使用等号分隔的列表选项，通过分号划分")]
    public void SemicolonSeparatedList_ValueAssigned()
    {
        // Arrange
        string[] args = ["--files=file1.txt;file2.txt;file3.txt"];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU07_ArrayOptions>(o => files = o.Files)
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(3, files.Length);
        CollectionAssert.AreEqual(new[] { "file1.txt", "file2.txt", "file3.txt" }, files);
    }

    [TestMethod("2.7. 使用等号分隔的列表选项，通过逗号划分")]
    public void CommaSeparatedList_ValueAssigned()
    {
        // Arrange
        string[] args = ["--files=file1.txt,file2.txt,file3.txt"];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU07_ArrayOptions>(o => files = o.Files)
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(3, files.Length);
        CollectionAssert.AreEqual(new[] { "file1.txt", "file2.txt", "file3.txt" }, files);
    }

    [TestMethod("2.8. 带引号的列表参数，赋值成功。")]
    public void QuotedArrayOption_ValueAssigned()
    {
        // Arrange
        string[] args = ["--files", "\"file with spaces.txt\"", "--files", "normal.txt", "--files", "\"another file.txt\""];
        string[]? files = null;        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU14_QuotedArrayOptions>(o =>
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

    [TestMethod("2.9. 等号方式带引号的列表参数，赋值成功。")]
    public void QuotedArrayWithEquals_ValueAssigned()
    {
        // Arrange
        string[] args = ["--paths=\"path with spaces\",regular-path,\"another path\""];
        string[]? paths = null;        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU14_QuotedArrayOptions>(o =>
            {
                paths = o.Paths;
                return 0;
            })
            .Run();

        // Assert
        Assert.IsNotNull(paths);
        Assert.AreEqual(3, paths.Length);
        CollectionAssert.AreEqual(new[] { "path with spaces", "regular-path", "another path" }, paths);
    }

    #endregion

    #region 3. 边界情况处理

    [TestMethod("3.1. 缺失必需选项，抛出异常。")]
    public void MissingRequiredOption_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, GNU)
                .AddHandler<GNU09_RequiredOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("3.2. 无效格式选项，抛出异常。")]
    public void InvalidOption_ThrowsException()
    {
        // Arrange
        string[] args = ["---invalid"];

        // Act & Assert
        Assert.ThrowsExactly<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, GNU)
                .AddHandler<GNU01_StringOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("3.3. 类型不匹配，抛出异常。")]
    public void TypeMismatch_ThrowsException()
    {
        // Arrange
        string[] args = ["--number", "not-a-number"];

        // Act & Assert
        Assert.ThrowsExactly<CommandLineParseValueException>(() =>
        {
            CommandLine.Parse(args, GNU)
                .AddHandler<GNU04_IntegerOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("3.4. 大小写敏感，识别正确。")]
    public void CaseSensitive_CorrectOptionParsed()
    {
        // Arrange
        string[] args = ["--case-sensitive", "lower", "--CASE-SENSITIVE", "upper"];
        string? lowerValue = null;
        string? upperValue = null;

        // Act
        CommandLine.Parse(args, GNU with { CaseSensitive = true })
            .AddHandler<GNU10_CaseSensitiveOptions>(o =>
            {
                lowerValue = o.CaseSensitive;
                upperValue = o.CASESENSITIVE;
            })
            .Run();

        // Assert
        Assert.AreEqual("lower", lowerValue);
        Assert.AreEqual("upper", upperValue);
    }

    [TestMethod("3.5. 大小写不敏感，识别正确。")]
    public void CaseInsensitive_CorrectOptionParsed()
    {
        // Arrange
        string[] args = ["--Ignore-Case", "value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, GNU with { CaseSensitive = false })
            .AddHandler<GNU11_CaseInsensitiveOptions>(o => value = o.IgnoreCase)
            .Run();

        // Assert
        Assert.AreEqual("value", value);
    }

    [TestMethod("3.6. 单个选项设置大小写敏感，全局默认不敏感，识别正确。")]
    public void SingleOptionCaseSensitive_GlobalInsensitive_CorrectlyParsed()
    {
        // Arrange
        string[] args = ["--Case-Option", "value1", "--case-option", "value2"];
        string? sensitiveValue = null;
        string? insensitiveValue = null;

        // Act
        CommandLine.Parse(args, GNU) // 默认大小写不敏感
            .AddHandler<GNU23_MixedCaseOptions>(o =>
            {
                sensitiveValue = o.CaseSensitiveOption;
                insensitiveValue = o.CaseInsensitiveOption;
            })
            .Run();

        // Assert
        Assert.AreEqual("value1", sensitiveValue); // 大小写敏感，匹配第一个 Case-Option
        Assert.AreEqual("value2", insensitiveValue); // 大小写不敏感，匹配第二个 case-option
    }

    [TestMethod("3.7. 单个选项设置大小写不敏感，全局设置为敏感，识别正确。")]
    public void OptionCaseInsensitive_OverridesGlobalSensitive()
    {
        // Arrange
        string[] args = ["--option-one", "value1", "--option-TWO", "value2"];
        string? option1Value = null;
        string? option2Value = null;

        // Act
        CommandLine.Parse(args, GNU with { CaseSensitive = true })
            .AddHandler<GNU24_OverrideCaseOptions>(o =>
            {
                option1Value = o.OptionOne;
                option2Value = o.OptionTwo;
            })
            .Run();

        // Assert
        Assert.IsNull(option1Value); // 全局大小写敏感，--option-one 不匹配 --Option-One
        Assert.AreEqual("value2", option2Value); // 选项明确指定为大小写不敏感，所以匹配成功
    }

    [TestMethod("3.8. 全局大小写敏感时，未指定大小写设置的选项不匹配。")]
    public void GlobalCaseSensitive_DefaultOption_NotMatched()
    {
        // Arrange
        string[] args = ["--global-sensitive", "value1"];
        string? globalSensitiveValue = null;

        // Act
        CommandLine.Parse(args, GNU with { CaseSensitive = true })
            .AddHandler<GNU25_ComplexCaseOptions>(o =>
            {
                globalSensitiveValue = o.GlobalSensitive;
            })
            .Run();

        // Assert
        Assert.IsNull(globalSensitiveValue); // 全局大小写敏感，--global-sensitive 不匹配 --GLOBAL-SENSITIVE
    }

    [TestMethod("3.9. 选项设置大小写敏感时，大小写不匹配无效。")]
    public void OptionCaseSensitive_CaseMismatch_NotMatched()
    {
        // Arrange
        string[] args = ["--local-sensitive", "value"];
        string? localSensitiveValue = null;

        // Act
        CommandLine.Parse(args, GNU with { CaseSensitive = true })
            .AddHandler<GNU25_ComplexCaseOptions>(o =>
            {
                localSensitiveValue = o.LocalSensitive;
            })
            .Run();

        // Assert
        Assert.IsNull(localSensitiveValue); // 局部大小写敏感，--local-sensitive 不匹配 --local-SENSITIVE
    }

    [TestMethod("3.10. 选项设置大小写不敏感时，无论全局设置，都能匹配。")]
    public void OptionCaseInsensitive_GlobalSensitive_StillMatched()
    {
        // Arrange
        string[] args = ["--LOCAL-insensitive", "value"];
        string? localInsensitiveValue = null;

        // Act
        CommandLine.Parse(args, GNU with { CaseSensitive = true })
            .AddHandler<GNU25_ComplexCaseOptions>(o =>
            {
                localInsensitiveValue = o.LocalInsensitive;
            })
            .Run();

        // Assert
        Assert.AreEqual("value", localInsensitiveValue); // 明确指定大小写不敏感，匹配成功
    }

    [TestMethod("3.11. 选项值大小写测试，枚举值不敏感，识别正确。")]
    public void EnumValueCaseInsensitive_CorrectlyParsed()
    {
        // Arrange
        string[] args = ["--log-level", "warning", "--second-level", "ERROR"];
        LogLevel? logLevel = null;
        LogLevel? secondLevel = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU26_EnumCaseOptions>(o =>
            {
                logLevel = o.LogLevel;
                secondLevel = o.SecondLevel;
            })
            .Run();

        // Assert
        Assert.AreEqual(LogLevel.Warning, logLevel); // 枚举值大小写不敏感
        Assert.AreEqual(LogLevel.Error, secondLevel); // 枚举值大小写不敏感
    }

    #endregion

    #region 4. 特殊特性

    [TestMethod("4.1. 选项别名，识别正确。")]
    public void OptionAliases_CorrectOptionParsed()
    {
        // Arrange
        string[] args = ["--alt", "value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU12_AliasOptions>(o => value = o.OptionWithAlias)
            .Run();

        // Assert
        Assert.AreEqual("value", value);
    }

    [TestMethod("4.2. 组合短选项，识别正确。")]
    public void CombinedShortOptions_AllParsedCorrectly()
    {
        // Arrange
        string[] args = ["-abc"];
        bool? optionA = null;
        bool? optionB = null;
        bool? optionC = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU13_CombinedOptions>(o =>
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

    [TestMethod("4.3. 终止选项解析符号，识别正确。")]
    public void OptionTerminator_FollowingArgsAreValues()
    {
        // Arrange
        string[] args = ["--option", "value", "--", "--not-an-option", "-x"];
        string? option = null;
        string[]? values = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU14_TerminatorOptions>(o =>
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
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU15_SingleValueOptions>(o => value = o.Value)
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
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU16_MultipleValueOptions>(o => values = o.Values)
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
        string[] args = ["value1", "--option", "opt-val", "value2"];
        string? option = null;
        string? value1 = null;
        string? value2 = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU17_MixedValueOptions>(o =>
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
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU18_IndexedValueOptions>(o =>
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
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU27_NonRequiredNonNullableOption>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual(null, value); // 使用初始化时的默认值
    }

    [TestMethod("6.2. Required, Non-nullable, 无CLI参数，抛出异常。")]
    public void RequiredNonNullable_NoCli_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, GNU)
                .AddHandler<GNU19_RequiredNonNullableOption>(_ => { })
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
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU20_NonRequiredNullableOption>(o => value = o.Value)
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
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, GNU)
                .AddHandler<GNU21_RequiredNullableOption>(_ => { })
                .Run();
        });
    }

    [TestMethod("6.5. 各种组合都提供CLI参数，全部赋值成功。")]
    public void AllCombinations_WithCli_AllAssigned()
    {
        // Arrange
        string[] args =
        [
            "--req-non-null", "value1", "--non-req-null", "value2",
            "--req-null", "value3", "--non-req-non-null", "value4"
        ];
        string? reqNonNull = null;
        string? nonReqNull = null;
        string? reqNull = null;
        string? nonReqNonNull = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU22_AllCombinationsOption>(o =>
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

    [TestMethod("6.6. 可空枚举类型，无CLI参数，赋默认值(null)。")]
    public void NullableEnumOption_NoCli_DefaultNull()
    {
        // Arrange
        string[] args = ["--log-level", "Warning"]; // 只提供非可空枚举，不提供可空枚举
        LogLevel? logLevel = null;
        LogLevel? nullableLogLevel = LogLevel.Error; // 初始化为非null值，验证会被设置为null

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU06_EnumOptions>(o =>
            {
                logLevel = o.LogLevel;
                nullableLogLevel = o.NullableLogLevel;
            })
            .Run();

        // Assert
        Assert.AreEqual(LogLevel.Warning, logLevel);
        Assert.IsNull(nullableLogLevel, $"可空枚举应该是null，但实际值是: {nullableLogLevel}"); // 可空枚举应该是null
    }

    [TestMethod("6.7. 可空值类型，无CLI参数，赋默认值(null)。")]
    public void NullableValueTypes_NoCli_DefaultNull()
    {
        // Arrange
        string[] args = ["--provided-int", "42"]; // 只提供一个必需的参数，不提供其他可空参数
        int? nullableInt = 123; // 初始化为非null值，验证会被设置为null
        bool? nullableBool = true; // 初始化为非null值，验证会被设置为null
        double? nullableDouble = 3.14; // 初始化为非null值，验证会被设置为null
        decimal? nullableDecimal = 100.5m; // 初始化为非null值，验证会被设置为null
        int? providedInt = null;

        // Act
        CommandLine.Parse(args, GNU)
            .AddHandler<GNU28_NullableValueTypesOptions>(o =>
            {
                nullableInt = o.NullableInt;
                nullableBool = o.NullableBool;
                nullableDouble = o.NullableDouble;
                nullableDecimal = o.NullableDecimal;
                providedInt = o.ProvidedInt;
            })
            .Run();

        // Assert
        Assert.AreEqual(42, providedInt); // 提供的参数应该正确解析
        Assert.IsNull(nullableInt, $"可空int应该是null，但实际值是: {nullableInt}");
        Assert.IsNull(nullableBool, $"可空bool应该是null，但实际值是: {nullableBool}");
        Assert.IsNull(nullableDouble, $"可空double应该是null，但实际值是: {nullableDouble}");
        Assert.IsNull(nullableDecimal, $"可空decimal应该是null，但实际值是: {nullableDecimal}");
    }

    #endregion

    #region 7. 异步处理测试

    [TestMethod("7.1. 异步处理方法，正确执行。")]
    public async Task AsyncHandler_ExecutesCorrectly()
    {
        // Arrange
        string[] args = ["--value", "async-test"];
        string? value = null;

        // Act
        await CommandLine.Parse(args, GNU)
            .AddHandler<GNU01_StringOptions>(async o =>
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

internal record GNU01_StringOptions
{
    [Option]
    public required string Value { get; init; }
}

internal record GNU02_ShortOptions
{
    [Option('v')]
    public required string Value { get; init; }
}

internal record GNU03_MixedOptions
{
    [Option('n')]
    public int Number { get; init; }

    [Option('u')]
    public int? NullableNumber { get; init; }

    [Option]
    public required string Text { get; init; }

    [Option]
    public required string? NullableText { get; init; }

    [Option]
    public required IReadOnlyList<string?> NullableList { get; init; }

    [Option]
    public required IReadOnlyList<string?>? NullableNullableList { get; init; }

    [Option('b')]
    public bool Flag { get; init; }
}

internal record GNU04_IntegerOptions
{
    [Option]
    public int Number { get; init; }
}

internal record GNU05_BooleanOptions
{
    [Option]
    public bool Flag { get; init; }
}

internal record GNU06_EnumOptions
{
    [Option("log-level")]
    public LogLevel LogLevel { get; init; }

    [Option("nullable-log-level")]
    public LogLevel? NullableLogLevel { get; init; }
}

internal record GNU07_ArrayOptions
{
    [Option]
    public string[] Files { get; init; } = [];
}

internal record GNU08_ListOptions
{
    [Option]
    public IReadOnlyList<string> Tags { get; init; } = [];
}

internal record GNU09_RequiredOptions
{
    [Option]
    public required string RequiredValue { get; init; }
}

internal record GNU10_CaseSensitiveOptions
{
    [Option("case-sensitive", CaseSensitive = true)]
    public string CaseSensitive { get; init; } = string.Empty;

    [Option("CASE-SENSITIVE", CaseSensitive = true)]
    public string CASESENSITIVE { get; init; } = string.Empty;
}

internal record GNU11_CaseInsensitiveOptions
{
    [Option("ignore-case")]
    public string IgnoreCase { get; init; } = string.Empty;
}

internal record GNU12_AliasOptions
{
    [Option([], ["option-with-alias", "alt", "alternate"])]
    public string OptionWithAlias { get; init; } = string.Empty;
}

internal record GNU13_CombinedOptions
{
    [Option('a')]
    public bool OptionA { get; init; }

    [Option('b')]
    public bool OptionB { get; init; }

    [Option('c')]
    public bool OptionC { get; init; }
}

internal record GNU14_TerminatorOptions
{
    [Option]
    public string Option { get; init; } = string.Empty;

    [Value(Length = int.MaxValue)]
    public string[] Values { get; init; } = [];
}

internal record GNU15_SingleValueOptions
{
    [Value]
    public string Value { get; init; } = string.Empty;
}

internal record GNU16_MultipleValueOptions
{
    [Value(Length = int.MaxValue)]
    public string[] Values { get; init; } = [];
}

internal record GNU17_MixedValueOptions
{
    [Value(0)]
    public string Value1 { get; init; } = string.Empty;

    [Option]
    public string Option { get; init; } = string.Empty;

    [Value(1)]
    public string Value2 { get; init; } = string.Empty;
}

internal record GNU18_IndexedValueOptions
{
    [Value(0)]
    public string First { get; init; } = string.Empty;

    [Value(2)]
    public string Third { get; init; } = string.Empty;
}

internal record GNU19_RequiredNonNullableOption
{
    [Option]
    public required string Value { get; init; }
}

internal record GNU20_NonRequiredNullableOption
{
    [Option]
    public string? Value { get; init; }
}

internal record GNU21_RequiredNullableOption
{
    [Option]
    public required string? Value { get; init; }
}

internal record GNU22_AllCombinationsOption
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

internal record GNU23_MixedCaseOptions
{
    [Option("Case-Option", CaseSensitive = true)]
    public string CaseSensitiveOption { get; init; } = string.Empty;

    [Option("case-option")]
    public string CaseInsensitiveOption { get; init; } = string.Empty;
}

internal record GNU24_OverrideCaseOptions
{
    [Option("Option-One")]
    public string OptionOne { get; init; } = string.Empty;

    [Option("option-TWO", CaseSensitive = false)]
    public string OptionTwo { get; init; } = string.Empty;
}

internal record GNU25_ComplexCaseOptions
{
    [Option("GLOBAL-SENSITIVE")]
    public string GlobalSensitive { get; init; } = string.Empty;

    [Option("local-SENSITIVE", CaseSensitive = true)]
    public string LocalSensitive { get; init; } = string.Empty;

    [Option("Local-Insensitive", CaseSensitive = false)]
    public string LocalInsensitive { get; init; } = string.Empty;
}

internal record GNU26_EnumCaseOptions
{
    [Option("log-level")]
    public LogLevel LogLevel { get; init; }

    [Option("second-level")]
    public LogLevel SecondLevel { get; init; }
}

internal record GNU27_NonRequiredNonNullableOption
{
    [Option]
    public string Value { get; init; } = string.Empty;
}

internal record GNU28_NullableValueTypesOptions
{
    [Option("nullable-int")]
    public int? NullableInt { get; init; }

    [Option("nullable-bool")]
    public bool? NullableBool { get; init; }

    [Option("nullable-double")]
    public double? NullableDouble { get; init; }

    [Option("nullable-decimal")]
    public decimal? NullableDecimal { get; init; }

    [Option("provided-int")]
    public int ProvidedInt { get; init; }
}

internal record GNU14_QuotedArrayOptions
{
    [Option]
    public string[] Files { get; init; } = [];

    [Option]
    public string[] Paths { get; init; } = [];
}

#endregion
