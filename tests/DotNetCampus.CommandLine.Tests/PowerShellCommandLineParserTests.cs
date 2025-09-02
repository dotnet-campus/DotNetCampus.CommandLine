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
/// 测试PowerShell风格命令行参数是否正确被解析到了。
/// </summary>
[TestClass]
public class PowerShellCommandLineParserTests
{
    private CommandLineParsingOptions PowerShell { get; } = CommandLineParsingOptions.PowerShell;

    #region 1. 基本参数解析

    [TestMethod("1.1. 单个参数解析，字符串类型，Pascal命名。")]
    public void SingleParameter_StringType_PascalNaming()
    {
        // Arrange
        string[] args = ["-Name", "test"];
        string? name = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS01_BasicOptions>(o => name = o.Name)
            .Run();

        // Assert
        Assert.AreEqual("test", name);
    }

    [TestMethod("1.2. 多个参数解析，混合类型。")]
    public void MultipleParameters_MixedTypes()
    {
        // Arrange
        string[] args = ["-Path", "C:\\temp", "-ItemType", "Directory", "-Force"];
        string? path = null;
        string? itemType = null;
        bool? force = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS02_MultipleOptions>(o =>
            {
                path = o.Path;
                itemType = o.ItemType;
                force = o.Force;
            })
            .Run();

        // Assert
        Assert.AreEqual("C:\\temp", path);
        Assert.AreEqual("Directory", itemType);
        Assert.IsTrue(force);
    }

    [TestMethod("1.3. 参数名使用Camel命名。")]
    public void Parameter_CamelCase_ValueAssigned()
    {
        // Arrange
        string[] args = ["-fileName", "document.txt"];
        string? fileName = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS03_CamelCaseOptions>(o => fileName = o.fileName)
            .Run();

        // Assert
        Assert.AreEqual("document.txt", fileName);
    }

    #endregion

    #region 2. 开关参数处理

    [TestMethod("2.1. 单个开关参数，布尔类型。")]
    public void SwitchParameter_BooleanType_True()
    {
        // Arrange
        string[] args = ["-Verbose"];
        bool? verbose = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS04_SwitchOptions>(o => verbose = o.Verbose)
            .Run();

        // Assert
        Assert.IsTrue(verbose);
    }

    [TestMethod("2.2. 多个开关参数，全部为true。")]
    public void MultipleSwitchParameters_AllTrue()
    {
        // Arrange
        string[] args = ["-Recurse", "-Force", "-WhatIf"];
        bool? recurse = null;
        bool? force = null;
        bool? whatIf = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS05_MultipleSwitchOptions>(o =>
            {
                recurse = o.Recurse;
                force = o.Force;
                whatIf = o.WhatIf;
            })
            .Run();

        // Assert
        Assert.IsTrue(recurse);
        Assert.IsTrue(force);
        Assert.IsTrue(whatIf);
    }

    [TestMethod("2.3. 开关参数与值参数混合。")]
    public void MixedSwitchAndValueParameters()
    {
        // Arrange
        string[] args = ["-Path", "logs.txt", "-Append", "-Encoding", "UTF8"];
        string? path = null;
        bool? append = null;
        string? encoding = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS06_MixedParameterTypes>(o =>
            {
                path = o.Path;
                append = o.Append;
                encoding = o.Encoding;
            })
            .Run();

        // Assert
        Assert.AreEqual("logs.txt", path);
        Assert.IsTrue(append);
        Assert.AreEqual("UTF8", encoding);
    }

    #endregion

    #region 3. 参数名称缩写

    [Ignore("暂时不打算支持 PowerShell 最短缩写功能，如果后面有需要再说。")]
    [TestMethod("3.1. 使用参数的唯一缩写。")]
    public void ParameterAbbreviation_UniquePrefix()
    {
        // Arrange
        string[] args = ["-Com", "Server01"];
        string? computerName = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS07_AbbreviationOptions>(o => computerName = o.ComputerName)
            .Run();

        // Assert
        Assert.AreEqual("Server01", computerName);
    }

    [TestMethod("3.2. 使用完整参数名。")]
    public void ParameterAbbreviation_FullName()
    {
        // Arrange
        string[] args = ["-ComputerName", "Server01"];
        string? computerName = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS07_AbbreviationOptions>(o => computerName = o.ComputerName)
            .Run();

        // Assert
        Assert.AreEqual("Server01", computerName);
    }

    [Ignore("暂时不打算支持 PowerShell 最短缩写功能，如果后面有需要再说。")]
    [TestMethod("3.3. 使用最短唯一缩写。")]
    public void ParameterAbbreviation_ShortestUniquePrefix()
    {
        // Arrange
        string[] args = ["-C", "Server01"];
        string? computerName = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS07_AbbreviationOptions>(o => computerName = o.ComputerName)
            .Run();

        // Assert
        Assert.AreEqual("Server01", computerName);
    }

    [Ignore("暂时不打算支持 PowerShell 最短缩写功能，如果后面有需要再说。")]
    [TestMethod("3.3. 缩写歧义处理。")]
    public void ParameterAbbreviation_AmbiguousPrefix_ThrowsException()
    {
        // Arrange
        string[] args = ["-Co", "value"]; // 可能是 ComputerName 或 Count

        // Act & Assert
        Assert.ThrowsExactly<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, PowerShell)
                .AddHandler<PS08_AmbiguousOptions>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 4. 位置参数

    [TestMethod("4.1. 单个位置参数。")]
    public void SinglePositionalParameter()
    {
        // Arrange
        string[] args = ["value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS09_PositionalOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("value", value);
    }

    [TestMethod("4.2. 多个位置参数。")]
    public void MultiplePositionalParameters()
    {
        // Arrange
        string[] args = ["source.txt", "destination.txt"];
        string? source = null;
        string? destination = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS10_MultiplePositionalOptions>(o =>
            {
                source = o.Source;
                destination = o.Destination;
            })
            .Run();

        // Assert
        Assert.AreEqual("source.txt", source);
        Assert.AreEqual("destination.txt", destination);
    }

    [TestMethod("4.3. 位置参数与命名参数混合。")]
    public void MixedPositionalAndNamedParameters()
    {
        // Arrange
        string[] args = ["source.txt", "-Destination", "dest.txt", "-Force"];
        string? source = null;
        string? destination = null;
        bool? force = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS11_MixedParameterOptions>(o =>
            {
                source = o.Source;
                destination = o.Destination;
                force = o.Force;
            })
            .Run();

        // Assert
        Assert.AreEqual("source.txt", source);
        Assert.AreEqual("dest.txt", destination);
        Assert.IsTrue(force);
    }

    #endregion

    #region 5. 数组参数

    [TestMethod("5.1. 逗号分隔的数组参数。")]
    public void CommaSeparatedArrayParameter()
    {
        // Arrange
        string[] args = ["-Processes", "chrome,firefox,edge"];
        string[]? processes = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS12_ArrayOptions>(o => processes = o.Processes)
            .Run();

        // Assert
        Assert.IsNotNull(processes);
        Assert.AreEqual(3, processes.Length);
        CollectionAssert.AreEqual(new[] { "chrome", "firefox", "edge" }, processes);
    }

    [TestMethod("5.2. 多次指定同一参数形成数组。")]
    public void RepeatedParameterAsArray()
    {
        // Arrange
        string[] args = ["-ComputerName", "server1", "-ComputerName", "server2", "-ComputerName", "server3"];
        IReadOnlyList<string>? computerNames = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS13_RepeatedParameterOptions>(o => computerNames = o.ComputerName)
            .Run();

        // Assert
        Assert.IsNotNull(computerNames);
        Assert.AreEqual(3, computerNames.Count);
        CollectionAssert.AreEqual(new[] { "server1", "server2", "server3" }, computerNames.ToArray());
    }

    [TestMethod("5.3. 单个选项后接多个值形成数组。")]
    public void SingleOption_MultipleValues_FormArray()
    {
        // Arrange
        string[] args = ["-Tags", "tag1", "tag2", "tag3"];
        string[]? tags = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS19_ArrayMultiValueOptions>(o => tags = o.Tags)
            .Run();

        // Assert
        Assert.IsNotNull(tags);
        Assert.AreEqual(3, tags.Length);
        CollectionAssert.AreEqual(new[] { "tag1", "tag2", "tag3" }, tags);
    }

    [TestMethod("5.4. 分号分隔的数组参数。")]
    public void SemicolonSeparatedArrayParameter()
    {
        // Arrange
        string[] args = ["-Processes", "chrome;firefox;edge"];
        string[]? processes = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS12_ArrayOptions>(o => processes = o.Processes)
            .Run();

        // Assert
        Assert.IsNotNull(processes);
        Assert.AreEqual(3, processes.Length);
        CollectionAssert.AreEqual(new[] { "chrome", "firefox", "edge" }, processes);
    }

    [TestMethod("5.5. 带引号的数组元素。")]
    public void QuotedArrayElements()
    {
        // Arrange
        string[] args = ["-Files", "\"file with spaces.txt\"", "\"another file.txt\"", "normal.txt"];
        string[]? files = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS20_QuotedArrayOptions>(o => files = o.Files)
            .Run();

        // Assert
        Assert.IsNotNull(files);
        Assert.AreEqual(3, files.Length);
        CollectionAssert.AreEqual(new[] { "file with spaces.txt", "another file.txt", "normal.txt" }, files);
    }

    [TestMethod("5.6. 逗号分隔的带引号数组元素。")]
    public void CommaSeparatedQuotedArrayElements()
    {
        // Arrange
        string[] args = ["-ComputerNames", "\"server one\",\"server two\",server3"];
        string[]? computerNames = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS20_QuotedArrayOptions>(o => computerNames = o.ComputerNames)
            .Run();

        // Assert
        Assert.IsNotNull(computerNames);
        Assert.AreEqual(3, computerNames.Length);
        CollectionAssert.AreEqual(new[] { "server one", "server two", "server3" }, computerNames);
    }

    #endregion

    #region 6. 边界条件处理

    [TestMethod("6.1. 必选参数缺失，抛出异常。")]
    public void MissingRequiredParameter_ThrowsException()
    {
        // Arrange
        string[] args = [];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, PowerShell)
                .AddHandler<PS14_RequiredOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("6.2. 类型转换错误，抛出异常。")]
    public void TypeConversionError_ThrowsException()
    {
        // Arrange
        string[] args = ["-Count", "not-a-number"];

        // Act & Assert
        Assert.ThrowsExactly<CommandLineParseValueException>(() =>
        {
            CommandLine.Parse(args, PowerShell)
                .AddHandler<PS15_TypeConversionOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("6.3. 参数大小写不敏感。")]
    public void ParameterCaseInsensitive()
    {
        // Arrange
        string[] args = ["-NAME", "test", "-path", "C:\\temp"];
        string? name = null;
        string? path = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS16_CaseInsensitiveOptions>(o =>
            {
                name = o.Name;
                path = o.Path;
            })
            .Run();

        // Assert
        Assert.AreEqual("test", name);
        Assert.AreEqual("C:\\temp", path);
    }

    #endregion

    #region 7. 特殊场景

    [TestMethod("7.1. 引号包围的参数值。")]
    public void QuotedParameterValues()
    {
        // Arrange
        string[] args = ["-Message", "\"Hello World\""];
        string? message = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS17_QuotedValueOptions>(o => message = o.Message)
            .Run();

        // Assert
        Assert.AreEqual("\"Hello World\"", message);
    }

    [TestMethod("7.2. 参数别名支持。")]
    public void ParameterAliases()
    {
        // Arrange
        string[] args = ["-Alias", "test"];
        string? value = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS18_AliasOptions>(o => value = o.ParameterWithAlias)
            .Run();

        // Assert
        Assert.AreEqual("test", value);
    }

    [TestMethod("7.3. 枚举类型参数。")]
    public void EnumTypeParameter()
    {
        // Arrange
        string[] args = ["-LogLevel", "Warning"];
        LogLevel? logLevel = null;

        // Act
        CommandLine.Parse(args, PowerShell)
            .AddHandler<PS19_EnumOptions>(o => logLevel = o.LogLevel)
            .Run();

        // Assert
        Assert.AreEqual(LogLevel.Warning, logLevel);
    }

    #endregion

    #region 8. 异步处理测试

    [TestMethod("8.1. 异步处理方法，正确执行。")]
    public async Task AsyncHandler_ExecutesCorrectly()
    {
        // Arrange
        string[] args = ["-Name", "async-test"];
        string? name = null;

        // Act
        await CommandLine.Parse(args, PowerShell)
            .AddHandler<PS01_BasicOptions>(async o =>
            {
                await Task.Delay(10); // 模拟异步操作
                name = o.Name;
            })
            .RunAsync();

        // Assert
        Assert.AreEqual("async-test", name);
    }

    #endregion
}

#region 测试用数据模型

internal record PS01_BasicOptions
{
    [Option("Name")]
    public string Name { get; init; } = string.Empty;
}

internal record PS02_MultipleOptions
{
    [Option("Path")]
    public string Path { get; init; } = string.Empty;

    [Option("ItemType")]
    public string ItemType { get; init; } = string.Empty;

    [Option("Force")]
    public bool Force { get; init; }
}

internal record PS03_CamelCaseOptions
{
    [Option("fileName")]
    public string fileName { get; init; } = string.Empty;
}

internal record PS04_SwitchOptions
{
    [Option("Verbose")]
    public bool Verbose { get; init; }
}

internal record PS05_MultipleSwitchOptions
{
    [Option("Recurse")]
    public bool Recurse { get; init; }

    [Option("Force")]
    public bool Force { get; init; }

    [Option("WhatIf")]
    public bool WhatIf { get; init; }
}

internal record PS06_MixedParameterTypes
{
    [Option("Path")]
    public string Path { get; init; } = string.Empty;

    [Option("Append")]
    public bool Append { get; init; }

    [Option("Encoding")]
    public string Encoding { get; init; } = string.Empty;
}

internal record PS07_AbbreviationOptions
{
    [Option("ComputerName")]
    public string ComputerName { get; init; } = string.Empty;
}

internal record PS08_AmbiguousOptions
{
    [Option("ComputerName")]
    public string ComputerName { get; init; } = string.Empty;

    [Option("Count")]
    public int Count { get; init; }
}

internal record PS09_PositionalOptions
{
    [Value]
    public string Value { get; init; } = string.Empty;
}

internal record PS10_MultiplePositionalOptions
{
    [Value(0)]
    public string Source { get; init; } = string.Empty;

    [Value(1)]
    public string Destination { get; init; } = string.Empty;
}

internal record PS11_MixedParameterOptions
{
    [Value(0)]
    public string Source { get; init; } = string.Empty;

    [Option("Destination")]
    public string Destination { get; init; } = string.Empty;

    [Option("Force")]
    public bool Force { get; init; }
}

internal record PS12_ArrayOptions
{
    [Option("Processes")]
    public string[] Processes { get; init; } = [];
}

internal record PS13_RepeatedParameterOptions
{
    [Option("ComputerName")]
    public IReadOnlyList<string> ComputerName { get; init; } = [];
}

internal record PS14_RequiredOptions
{
    [Option("Name")]
    public required string Name { get; init; }
}

internal record PS15_TypeConversionOptions
{
    [Option("Count")]
    public int Count { get; init; }
}

internal record PS16_CaseInsensitiveOptions
{
    [Option("Name")]
    public string Name { get; init; } = string.Empty;

    [Option("Path")]
    public string Path { get; init; } = string.Empty;
}

internal record PS17_QuotedValueOptions
{
    [Option("Message")]
    public string Message { get; init; } = string.Empty;
}

internal record PS18_AliasOptions
{
    [Option("ParameterWithAlias", Aliases = ["Alias", "Alt"])]
    public string ParameterWithAlias { get; init; } = string.Empty;
}

internal record PS19_EnumOptions
{
    [Option("LogLevel")]
    public LogLevel LogLevel { get; init; }
}

internal record PS19_ArrayMultiValueOptions
{
    [Option("Tags")]
    public string[] Tags { get; init; } = [];
}

internal record PS20_QuotedArrayOptions
{
    [Option("Files")]
    public string[] Files { get; init; } = [];

    [Option("ComputerNames")]
    public string[] ComputerNames { get; init; } = [];
}

#endregion
