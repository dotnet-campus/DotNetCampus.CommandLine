using System;
using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 测试命名规则（Naming Convention）功能，包括 kebab-case、PascalCase、camelCase 等多种命名风格的支持。
/// 基于 CommandAttribute、OptionAttribute 和 ValueAttribute 的命名规则要求。
/// </summary>
[TestClass]
public class NamingConventionTests
{
    private CommandLineParsingOptions Flexible { get; } = CommandLineParsingOptions.Flexible;

    private CommandLineParsingOptions CaseSensitive { get; } = new CommandLineParsingOptions
    {
        Style = CommandLineParsingOptions.Flexible.Style with { CaseSensitive = false },
    };

    #region 1. CommandAttribute 命名规则测试

    [TestMethod("1.1. kebab-case 命令名称 - 基本情况")]
    public void Command_KebabCase_BasicCase()
    {
        // Arrange
        string[] args = ["build-project", "--verbose"];
        bool handlerCalled = false;
        bool verboseFlag = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<BuildProjectCommand>(o =>
            {
                handlerCalled = true;
                verboseFlag = o.Verbose;
            })
            .Run();

        // Assert
        Assert.IsTrue(handlerCalled);
        Assert.IsTrue(verboseFlag);
    }

    [TestMethod("1.2. kebab-case 多级子命令")]
    public void Command_KebabCase_MultiLevel()
    {
        // Arrange
        string[] args = ["user-management", "create-account", "--username", "john"];
        string? capturedUsername = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<UserManagementCreateAccountCommand>(o =>
            {
                capturedUsername = o.Username;
            })
            .Run();

        // Assert
        Assert.AreEqual("john", capturedUsername);
    }

    [TestMethod("1.3. 空命令名称 - 默认命令")]
    public void Command_EmptyName_DefaultCommand()
    {
        // Arrange
        string[] args = ["--help"];
        bool handlerCalled = false;
        bool helpFlag = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<DefaultCommand>(o =>
            {
                handlerCalled = true;
                helpFlag = o.Help;
            })
            .Run();

        // Assert
        Assert.IsTrue(handlerCalled);
        Assert.IsTrue(helpFlag);
    }

    [TestMethod("1.4. 单一命令名称")]
    public void Command_SingleName()
    {
        // Arrange
        string[] args = ["build", "--output", "bin"];
        string? capturedOutput = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<BuildCommand>(o =>
            {
                capturedOutput = o.Output;
            })
            .Run();

        // Assert
        Assert.AreEqual("bin", capturedOutput);
    }

    [TestMethod("1.5. 命令名称大小写不敏感（默认）")]
    public void Command_CaseInsensitive_Default()
    {
        // Arrange
        string[] args = ["BUILD-PROJECT", "--verbose"]; // 大写命令
        bool handlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible) // Flexible 默认大小写不敏感
            .AddHandler<BuildProjectCommand>(_ => handlerCalled = true)
            .Run();

        // Assert
        Assert.IsTrue(handlerCalled);
    }

    [TestMethod("1.6. 命令名称大小写敏感")]
    public void Command_CaseSensitive()
    {
        // Arrange
        string[] args = ["BUILD-PROJECT", "--verbose"]; // 大写命令

        // Act & Assert - 大小写敏感模式下应该抛出异常
        Assert.ThrowsExactly<CommandNameNotFoundException>(() =>
        {
            CommandLine.Parse(args, CaseSensitive)
                .AddHandler<BuildProjectCommand>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 2. OptionAttribute 命名规则测试

    [TestMethod("2.1. 无参数 OptionAttribute - 自动使用属性名")]
    public void Option_NoParameter_UsePropertyName()
    {
        // Arrange
        string[] args = ["--verbose"]; // 属性名 Verbose 转为 kebab-case
        bool verboseFlag = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<DefaultCommand>(o =>
            {
                verboseFlag = o.Verbose;
            })
            .Run();

        // Assert
        Assert.IsTrue(verboseFlag);
    }

    [TestMethod("2.2. kebab-case 长选项名")]
    public void Option_KebabCase_LongName()
    {
        // Arrange
        string[] args = ["--output-directory", "bin"];
        string? capturedOutput = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<BuildWithOptionsCommand>(o =>
            {
                capturedOutput = o.OutputDirectory;
            })
            .Run();

        // Assert
        Assert.AreEqual("bin", capturedOutput);
    }

    [TestMethod("2.3. 短选项名（单字符）")]
    public void Option_ShortName_SingleCharacter()
    {
        // Arrange
        string[] args = ["-v"]; // 短选项
        bool verboseFlag = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<BuildWithShortOptionsCommand>(o =>
            {
                verboseFlag = o.Verbose;
            })
            .Run();

        // Assert
        Assert.IsTrue(verboseFlag);
    }

    [TestMethod("2.4. 短选项名和长选项名组合")]
    public void Option_ShortAndLongName_Combined()
    {
        // Arrange - 测试短选项
        string[] shortArgs = ["-o", "bin"];
        string? capturedOutputShort = null;

        // Act
        CommandLine.Parse(shortArgs, Flexible)
            .AddHandler<BuildWithCombinedOptionsCommand>(o =>
            {
                capturedOutputShort = o.Output;
            })
            .Run();

        // Assert
        Assert.AreEqual("bin", capturedOutputShort);

        // Arrange - 测试长选项
        string[] longArgs = ["--output", "lib"];
        string? capturedOutputLong = null;

        // Act
        CommandLine.Parse(longArgs, Flexible)
            .AddHandler<BuildWithCombinedOptionsCommand>(o =>
            {
                capturedOutputLong = o.Output;
            })
            .Run();

        // Assert
        Assert.AreEqual("lib", capturedOutputLong);
    }

    [TestMethod("2.5. 选项别名（Aliases）")]
    public void Option_Aliases()
    {
        // Arrange - 测试第一个别名
        string[] aliasArgs1 = ["--out", "bin"];
        string? capturedOutput1 = null;

        // Act
        CommandLine.Parse(aliasArgs1, Flexible)
            .AddHandler<BuildWithAliasesCommand>(o =>
            {
                capturedOutput1 = o.OutputPath;
            })
            .Run();

        // Assert
        Assert.AreEqual("bin", capturedOutput1);

        // Arrange - 测试第二个别名
        string[] aliasArgs2 = ["--directory", "lib"];
        string? capturedOutput2 = null;

        // Act
        CommandLine.Parse(aliasArgs2, Flexible)
            .AddHandler<BuildWithAliasesCommand>(o =>
            {
                capturedOutput2 = o.OutputPath;
            })
            .Run();

        // Assert
        Assert.AreEqual("lib", capturedOutput2);
    }

    [TestMethod("2.6. ExactSpelling 精确拼写")]
    public void Option_ExactSpelling()
    {
        // Arrange - 使用精确拼写的选项名
        string[] exactArgs = ["--SampleProperty", "test"];
        string? capturedValue = null;

        // Act
        CommandLine.Parse(exactArgs, CommandLineParsingOptions.Gnu)
            .AddHandler<ExactSpellingCommand>(o =>
            {
                capturedValue = o.SampleProperty;
            })
            .Run();

        // Assert
        Assert.AreEqual("test", capturedValue);

        // Arrange - 尝试使用自动转换的名称（应该失败）
        string[] kebabArgs = ["--sample-property", "test"];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(kebabArgs, CommandLineParsingOptions.Gnu)
                .AddHandler<ExactSpellingCommand>(_ => { })
                .Run();
        });
    }

    [TestMethod("2.7. 选项大小写敏感性")]
    public void Option_CaseSensitive()
    {
        // Arrange
        string[] args = ["--VERBOSE"]; // 大写选项

        // Act - 大小写不敏感模式（默认）
        bool verboseFlexible = false;
        CommandLine.Parse(args, Flexible)
            .AddHandler<DefaultCaseSensitiveOptionsCommand>(o =>
            {
                verboseFlexible = o.Verbose;
            })
            .Run();

        // Assert
        Assert.IsTrue(verboseFlexible);

        // Act & Assert - 大小写敏感模式
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, CaseSensitive)
                .AddHandler<DefaultCaseSensitiveOptionsCommand>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 3. ValueAttribute 命名规则测试

    [TestMethod("3.1. 无参数 ValueAttribute - 默认索引 0")]
    public void Value_NoParameter_DefaultIndex()
    {
        // Arrange
        string[] args = ["input.txt"];
        string? capturedInput = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<FileProcessCommand>(o =>
            {
                capturedInput = o.InputFile;
            })
            .Run();

        // Assert
        Assert.AreEqual("input.txt", capturedInput);
    }

    [TestMethod("3.2. 指定索引的 ValueAttribute")]
    public void Value_SpecificIndex()
    {
        // Arrange
        string[] args = ["source.txt", "destination.txt"];
        string? capturedSource = null;
        string? capturedDestination = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<FileCopyCommand>(o =>
            {
                capturedSource = o.Source;
                capturedDestination = o.Destination;
            })
            .Run();

        // Assert
        Assert.AreEqual("source.txt", capturedSource);
        Assert.AreEqual("destination.txt", capturedDestination);
    }

    [TestMethod("3.3. 可变长度 ValueAttribute")]
    public void Value_VariableLength()
    {
        // Arrange
        string[] args = ["file1.txt", "file2.txt", "file3.txt"];
        string[]? capturedFiles = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<MultiFileCommand>(o =>
            {
                capturedFiles = o.Files;
            })
            .Run();

        // Assert
        Assert.IsNotNull(capturedFiles);
        Assert.AreEqual(3, capturedFiles.Length);
        Assert.AreEqual("file1.txt", capturedFiles[0]);
        Assert.AreEqual("file2.txt", capturedFiles[1]);
        Assert.AreEqual("file3.txt", capturedFiles[2]);
    }

    [TestMethod("3.4. 指定索引和长度的 ValueAttribute")]
    public void Value_SpecificIndexAndLength()
    {
        // Arrange
        string[] args = ["cmd", "arg1", "arg2", "remaining"];
        string? capturedCommand = null;
        string[]? capturedArgs = null;
        string? capturedRemaining = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<ComplexValueCommand>(o =>
            {
                capturedCommand = o.Command;
                capturedArgs = o.Arguments;
                capturedRemaining = o.Remaining;
            })
            .Run();

        // Assert
        Assert.AreEqual("cmd", capturedCommand);
        Assert.IsNotNull(capturedArgs);
        Assert.AreEqual(2, capturedArgs.Length);
        Assert.AreEqual("arg1", capturedArgs[0]);
        Assert.AreEqual("arg2", capturedArgs[1]);
        Assert.AreEqual("remaining", capturedRemaining);
    }

    #endregion

    #region 4. 混合命名风格测试

    [TestMethod("4.1. PascalCase 属性名自动转换为 kebab-case")]
    public void Mixed_PascalCaseToKebabCase()
    {
        // Arrange
        string[] args = ["--sample-property", "test", "--another-option", "value"];
        string? capturedSample = null;
        string? capturedAnother = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<MixedNamingCommand>(o =>
            {
                capturedSample = o.SampleProperty;
                capturedAnother = o.AnotherOption;
            })
            .Run();

        // Assert
        Assert.AreEqual("test", capturedSample);
        Assert.AreEqual("value", capturedAnother);
    }

    [TestMethod("4.2. camelCase 属性名自动转换为 kebab-case")]
    public void Mixed_CamelCaseToKebabCase()
    {
        // Arrange
        string[] args = ["--my-option", "test", "--another-value", "value"];
        string? capturedOption = null;
        string? capturedValue = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<CamelCaseNamingCommand>(o =>
            {
                capturedOption = o.myOption;
                capturedValue = o.anotherValue;
            })
            .Run();

        // Assert
        Assert.AreEqual("test", capturedOption);
        Assert.AreEqual("value", capturedValue);
    }

    [TestMethod("4.3. 多种命名风格的兼容性")]
    public void Mixed_MultipleNamingStyles()
    {
        // Arrange - 测试不同的输入风格都能被识别
        string[] kebabArgs = ["--output-directory", "bin"];
        string[] pascalArgs = ["--OutputDirectory", "lib"];
        string? capturedKebab = null;
        string? capturedPascal = null;

        // Act - kebab-case 输入
        CommandLine.Parse(kebabArgs, Flexible)
            .AddHandler<CompatibilityCommand>(o =>
            {
                capturedKebab = o.OutputDirectory;
            })
            .Run();

        // Act - PascalCase 输入（在非精确拼写模式下应该也能工作）
        CommandLine.Parse(pascalArgs, Flexible)
            .AddHandler<CompatibilityCommand>(o =>
            {
                capturedPascal = o.OutputDirectory;
            })
            .Run();

        // Assert
        Assert.AreEqual("bin", capturedKebab);
        Assert.AreEqual("lib", capturedPascal);
    }

    #endregion

    #region 5. 边界情况和错误处理测试

    [TestMethod("5.1. 无效的短选项名（非字母字符）")]
    public void Error_InvalidShortOptionName()
    {
        // 这个测试主要验证 OptionAttribute 构造函数的参数验证
        // 在实际使用中，这会在编译时就报错，所以我们这里测试运行时的行为

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            var _ = new OptionAttribute('1'); // 数字不是有效的短选项名
        });

        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            var _ = new OptionAttribute('-'); // 符号不是有效的短选项名
        });
    }

    [TestMethod("5.2. 空字符串选项名")]
    public void Error_EmptyOptionName()
    {
        // Arrange
        string[] args = ["test"];

        // Act & Assert - 空的选项名应该被忽略或使用属性名
        // 这里我们测试使用空字符串作为选项名时的行为
        bool handlerCalled = false;
        CommandLine.Parse(args, Flexible)
            .AddHandler<EmptyOptionNameCommand>(_ => handlerCalled = true)
            .Run();

        Assert.IsTrue(handlerCalled);
    }

    [TestMethod("5.3. 重复的 ValueAttribute 索引")]
    public void Error_DuplicateValueIndex()
    {
        // Arrange
        string[] args = ["value"];

        // Act & Assert - 这种情况下应该根据实现决定如何处理
        // 通常最后一个定义会生效或者抛出异常
        bool handlerCalled = false;
        CommandLine.Parse(args, Flexible)
            .AddHandler<DuplicateValueIndexCommand>(o =>
            {
                handlerCalled = true;
                // 验证至少有一个值被设置
                Assert.IsTrue(!string.IsNullOrEmpty(o.Value1) || !string.IsNullOrEmpty(o.Value2));
            })
            .Run();

        Assert.IsTrue(handlerCalled);
    }

    #endregion
}

#region 测试用数据模型

// 1. CommandAttribute 测试类

[Command("build-project")]
internal class BuildProjectCommand
{
    [Option("verbose")]
    public bool Verbose { get; init; }
}

[Command("user-management create-account")]
internal class UserManagementCreateAccountCommand
{
    [Option("username")]
    public required string Username { get; init; }
}

[Command(null)] // 无参数，表示默认命令
internal class DefaultCommand
{
    [Option("help")]
    public bool Help { get; init; }

    [Option] // 无参数，使用属性名
    public bool Verbose { get; init; }
}

[Command("build")]
internal class BuildCommand
{
    [Option("output")]
    public required string Output { get; init; }
}

// 2. OptionAttribute 测试类

internal class BuildWithOptionsCommand
{
    [Option("output-directory")]
    public required string OutputDirectory { get; init; }
}

internal class BuildWithShortOptionsCommand
{
    [Option('v')]
    public bool Verbose { get; init; }
}

internal class BuildWithCombinedOptionsCommand
{
    [Option('o', "output")]
    public required string Output { get; init; }
}

internal class BuildWithAliasesCommand
{
    [Option([], ["output-path", "out", "directory"])]
    public required string OutputPath { get; init; }
}

internal class ExactSpellingCommand
{
    [Option("SampleProperty")]
    public required string SampleProperty { get; init; }
}

internal class DefaultCaseSensitiveOptionsCommand
{
    [Option("verbose")]
    public required bool Verbose { get; init; }
}

// 3. ValueAttribute 测试类

internal class FileProcessCommand
{
    [Value] // 默认索引 0
    public required string InputFile { get; init; }
}

internal class FileCopyCommand
{
    [Value(0)]
    public required string Source { get; init; }

    [Value(1)]
    public required string Destination { get; init; }
}

internal class MultiFileCommand
{
    [Value(Length = int.MaxValue)]
    public required string[] Files { get; init; }
}

internal class ComplexValueCommand
{
    [Value(0)]
    public required string Command { get; init; }

    [Value(1, 2)]
    public required string[] Arguments { get; init; }

    [Value(3)]
    public required string Remaining { get; init; }
}

// 4. 混合命名风格测试类

internal class MixedNamingCommand
{
    [Option] // 使用属性名，PascalCase -> kebab-case
    public required string SampleProperty { get; init; }

    [Option] // 使用属性名，PascalCase -> kebab-case
    public required string AnotherOption { get; init; }
}

internal class CamelCaseNamingCommand
{
    [Option] // camelCase -> kebab-case
    public required string myOption { get; init; }

    [Option] // camelCase -> kebab-case
    public required string anotherValue { get; init; }
}

internal class CompatibilityCommand
{
    [Option] // 应该支持多种输入风格
    public required string OutputDirectory { get; init; }
}

// 5. 边界情况测试类

internal class EmptyOptionNameCommand
{
    [Option("")] // 空字符串选项名
    public string EmptyName { get; init; } = "";
}

internal class DuplicateValueIndexCommand
{
    [Value(0)]
    public string Value1 { get; init; } = "";

    [Value(0)] // 重复的索引
    public string Value2 { get; init; } = "";
}

#endregion
