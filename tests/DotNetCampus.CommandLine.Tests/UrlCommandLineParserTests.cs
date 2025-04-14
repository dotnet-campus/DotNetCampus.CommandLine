using System.Collections.Generic;
using System.Linq;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 测试 URL 风格命令行参数是否正确被解析。
/// 注意：URL风格参数通常由Web浏览器或其他应用程序传入，而不是用户直接在终端输入。
/// 因此URL风格参数通常只有一个完整的URL参数，而不是像其他风格那样有多个参数。
/// </summary>
[TestClass]
public class UrlCommandLineParserTests
{
    private CommandLineParsingOptions Scheme { get; } = new CommandLineParsingOptions { SchemeNames = ["myapp"] };

    #region 1. 基本URL解析测试

    [TestMethod("1.1. 完整URL格式解析（含scheme、path、query参数）")]
    public void CompleteUrl_WithSchemePathAndQuery_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://documents/open?readOnly=true&highlight=yes"];
        string? path = null;
        bool? readOnly = false;
        string? highlight = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url01_BasicUrlOptions>(o =>
            {
                path = o.Path;
                readOnly = o.ReadOnly;
                highlight = o.Highlight;
            })
            .Run();

        // Assert
        Assert.AreEqual("documents/open", path);
        Assert.IsTrue(readOnly);
        Assert.AreEqual("yes", highlight);
    }

    [Ignore("虽然正常解析时，这种Scheme不匹配应该抛异常；但我们是主命令行程序，兼容被 Web 调用；所以这种情况代码都进不来。")]
    [TestMethod("1.2. 指定SchemeNames时正确匹配scheme")]
    public void SchemeNames_SpecifiedAndMatched_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["sample://action?param=value"];
        string? action = null;
        string? param = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url02_SchemeOptions>(o =>
            {
                action = o.Action;
                param = o.Param;
            })
            .Run();

        // Assert
        Assert.AreEqual("action", action);
        Assert.AreEqual("value", param);
    }

    [TestMethod("1.3. 不在SchemeNames列表中的scheme不被识别为URL")]
    public void SchemeNames_NotMatched_NotParsedAsUrl()
    {
        // Arrange
        string[] args = ["unknown://path?param=value"];
        string? value = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url03_PositionalValueOptions>(o => value = o.Value)
            .Run();

        // Assert
        Assert.AreEqual("unknown://path?param=value", value); // 作为普通位置参数处理
    }

    [TestMethod("1.4. 大小写不敏感的scheme匹配")]
    public void SchemeNames_CaseInsensitive_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["MYAPP://path?param=value"];
        string? path = null;
        string? param = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url04_CaseInsensitiveSchemeOptions>(o =>
            {
                path = o.Path;
                param = o.Param;
            })
            .Run();

        // Assert
        Assert.AreEqual("path", path);
        Assert.AreEqual("value", param);
    }

    #endregion

    #region 2. 查询参数(QueryString)解析测试

    [TestMethod("2.1. 基本键值对参数解析")]
    public void BasicQueryParam_KeyValuePair_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?name=value"];
        string? name = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url05_BasicQueryParamOptions>(o => name = o.Name)
            .Run();

        // Assert
        Assert.AreEqual("value", name);
    }

    [TestMethod("2.2. 多参数解析")]
    public void MultipleQueryParams_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?name=John&age=25&location=Beijing"];
        string? name = null;
        int? age = null;
        string? location = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url06_MultipleQueryParamOptions>(o =>
            {
                name = o.Name;
                age = o.Age;
                location = o.Location;
            })
            .Run();

        // Assert
        Assert.AreEqual("John", name);
        Assert.AreEqual(25, age);
        Assert.AreEqual("Beijing", location);
    }

    [TestMethod("2.3. 无值参数解析为布尔true")]
    public void QueryParamWithoutValue_ParsedAsTrue()
    {
        // Arrange
        string[] args = ["myapp://path?debug&verbose"];
        bool? debug = false;
        bool? verbose = false;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url07_BooleanQueryParamOptions>(o =>
            {
                debug = o.Debug;
                verbose = o.Verbose;
            })
            .Run();

        // Assert
        Assert.IsTrue(debug);
        Assert.IsTrue(verbose);
    }

    [TestMethod("2.4. 空值参数解析为空字符串")]
    public void QueryParamWithEmptyValue_ParsedAsEmptyString()
    {
        // Arrange
        string[] args = ["myapp://path?name=&comment="];
        string? name = "default";
        string? comment = "default";

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url08_EmptyValueQueryParamOptions>(o =>
            {
                name = o.Name;
                comment = o.Comment;
            })
            .Run();

        // Assert
        Assert.AreEqual("", name);
        Assert.AreEqual("", comment);
    }

    [TestMethod("2.5. 同名参数多次出现（数组）解析")]
    public void DuplicateQueryParams_ParsedAsArray()
    {
        // Arrange
        string[] args = ["myapp://path?tags=csharp&tags=dotnet&tags=cli"];
        string[]? tags = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url09_ArrayQueryParamOptions>(o => tags = o.Tags)
            .Run();

        // Assert
        Assert.IsNotNull(tags);
        Assert.AreEqual(3, tags.Length);
        CollectionAssert.AreEqual(new[] { "csharp", "dotnet", "cli" }, tags);
    }

    #endregion

    #region 3. 类型转换测试

    [TestMethod("3.1. 整数类型转换")]
    public void QueryParamIntegerType_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?id=42&count=100"];
        int? id = null;
        int? count = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url10_IntegerTypeOptions>(o =>
            {
                id = o.Id;
                count = o.Count;
            })
            .Run();

        // Assert
        Assert.AreEqual(42, id);
        Assert.AreEqual(100, count);
    }

    [TestMethod("3.2. 布尔类型转换")]
    public void QueryParamBooleanType_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?enabled=true&disabled=false&flag"];
        bool? enabled = null;
        bool? disabled = null;
        bool? flag = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url11_BooleanTypeOptions>(o =>
            {
                enabled = o.Enabled;
                disabled = o.Disabled;
                flag = o.Flag;
            })
            .Run();

        // Assert
        Assert.IsTrue(enabled);
        Assert.IsFalse(disabled);
        Assert.IsTrue(flag);
    }

    [TestMethod("3.3. 枚举类型转换")]
    public void QueryParamEnumType_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?logLevel=Warning&style=gnu"];
        LogLevel? logLevel = null;
        CommandLineStyle? style = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url12_EnumTypeOptions>(o =>
            {
                logLevel = o.LogLevel;
                style = o.Style;
            })
            .Run();

        // Assert
        Assert.AreEqual(LogLevel.Warning, logLevel);
        Assert.AreEqual(CommandLineStyle.GNU, style);
    }

    [TestMethod("3.4. 数组/列表类型转换")]
    public void QueryParamCollectionType_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?ids=1&ids=2&ids=3&names=Alice&names=Bob"];
        string[]? ids = null;
        List<string>? names = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url13_CollectionTypeOptions>(o =>
            {
                ids = o.Ids;
                names = o.Names?.ToList();
            })
            .Run();

        // Assert
        Assert.IsNotNull(ids);
        Assert.AreEqual(3, ids.Length);
        CollectionAssert.AreEqual(new[] { "1", "2", "3" }, ids);

        Assert.IsNotNull(names);
        Assert.AreEqual(2, names.Count);
        CollectionAssert.AreEqual(new[] { "Alice", "Bob" }, names);
    }

    #endregion

    #region 4. URL编码解析测试

    [TestMethod("4.1. 基本URL编码解析（空格等）")]
    public void UrlEncodedSpaces_DecodedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?query=hello%20world&path=my%20documents"];
        string? query = null;
        string? path = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url14_UrlEncodedOptions>(o =>
            {
                query = o.Query;
                path = o.Path;
            })
            .Run();

        // Assert
        Assert.AreEqual("hello world", query);
        Assert.AreEqual("my documents", path);
    }

    [TestMethod("4.2. 特殊字符编码解析（#、&、%等）")]
    public void UrlEncodedSpecialChars_DecodedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?special=hash%23ampersand%26percent%25"];
        string? special = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url15_SpecialCharsOptions>(o => special = o.Special)
            .Run();

        // Assert
        Assert.AreEqual("hash#ampersand&percent%", special);
    }

    [TestMethod("4.3. 中文和非ASCII字符编码解析")]
    public void UrlEncodedNonAsciiChars_DecodedCorrectly()
    {
        // Arrange
        string[] args = ["myapp://path?chinese=%E4%BD%A0%E5%A5%BD&emoji=%F0%9F%98%80"];
        string? chinese = null;
        string? emoji = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url16_NonAsciiOptions>(o =>
            {
                chinese = o.Chinese;
                emoji = o.Emoji;
            })
            .Run();

        // Assert
        Assert.AreEqual("你好", chinese);
        Assert.AreEqual("😀", emoji);
    }

    #endregion

    #region 5. 路径解析测试

    [TestMethod("5.1. 路径部分作为位置参数")]
    public void PathPart_ParsedAsPositionalValue()
    {
        // Arrange
        string[] args = ["myapp://documents/reports/annual"];
        string[]? paths = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url17_PathAsPositionalOptions>(o => paths = o.Paths)
            .Run();

        // Assert
        CollectionAssert.AreEqual(new[] { "documents", "reports", "annual" }, paths);
    }

    [TestMethod("5.2. 路径首部分作为谓词，其余作为位置参数")]
    public void FirstPathSegmentAsVerb_RemainingAsPositional()
    {
        // Arrange
        string[] args = ["myapp://open/document.txt?readOnly=true"];
        string? filePath = null;
        bool? readOnly = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url18_VerbPathOptions>(o =>
            {
                filePath = o.FilePath;
                readOnly = o.ReadOnly;
            })
            .Run();

        // Assert
        Assert.AreEqual("document.txt", filePath);
        Assert.IsTrue(readOnly);
    }

    #endregion

    #region 6. 边界情况测试

    [TestMethod("6.1. 空参数列表")]
    public void EmptyArgs_ProcessedGracefully()
    {
        // Arrange
        string[] args = [];
        string? path = "default";
        bool handlerCalled = false;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url19_DefaultValueOptions>(o =>
            {
                handlerCalled = true;
                path = o.Path;
            })
            .Run();

        // Assert
        Assert.IsTrue(handlerCalled);
        Assert.AreEqual("default-path", path); // 使用默认值
    }

    [Ignore("虽然正常解析时，这种格式应该抛异常；但我们是主命令行程序，兼容被 Web 调用；所以这种情况代码都进不来。")]
    [TestMethod("6.2. 畸形URL格式")]
    public void MalformedUrl_ThrowsException()
    {
        // Arrange
        string[] args = ["myapp:/path?invalid-format"];

        // Act & Assert
        Assert.ThrowsException<CommandLineParseException>(() =>
        {
            CommandLine.Parse(args, Scheme)
                .AddHandler<Url20_MalformedUrlOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("6.3. 重复的查询参数名")]
    public void DuplicateQueryParamName_LastOneWins()
    {
        // Arrange
        string[] args = ["myapp://path?name=first&name=second&name=last"];
        string? name = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url21_DuplicateParamOptions>(o => name = o.Name)
            .Run();

        // Assert
        Assert.AreEqual("last", name); // 最后一个值被使用
    }

    [TestMethod("6.4. 特殊URL格式（片段标识符等）")]
    public void SpecialUrlFormats_ParsedAppropriately()
    {
        // Arrange
        string[] args = ["myapp://path?param=value#section"];
        string? param = null;
        string? fragment = null;

        // Act
        CommandLine.Parse(args, Scheme)
            .AddHandler<Url22_FragmentOptions>(o =>
            {
                param = o.Param;
                fragment = o.Fragment;
            })
            .Run();

        // Assert
        Assert.AreEqual("value", param);
        Assert.AreEqual("section", fragment); // 片段标识符被正确处理
    }

    #endregion
}

#region 测试用数据模型

internal record Url01_BasicUrlOptions
{
    [Value(Length = int.MaxValue)]
    public string Path { get; init; } = string.Empty;

    [Option("readOnly")]
    public bool ReadOnly { get; init; }

    [Option]
    public string Highlight { get; init; } = string.Empty;
}

internal record Url02_SchemeOptions
{
    [Value]
    public string Action { get; init; } = string.Empty;

    [Option]
    public string Param { get; init; } = string.Empty;
}

internal record Url03_PositionalValueOptions
{
    [Value]
    public string Value { get; init; } = string.Empty;
}

internal record Url04_CaseInsensitiveSchemeOptions
{
    [Value]
    public string Path { get; init; } = string.Empty;

    [Option]
    public string Param { get; init; } = string.Empty;
}

internal record Url05_BasicQueryParamOptions
{
    [Option]
    public string Name { get; init; } = string.Empty;
}

internal record Url06_MultipleQueryParamOptions
{
    [Option]
    public string Name { get; init; } = string.Empty;

    [Option]
    public int Age { get; init; }

    [Option]
    public string Location { get; init; } = string.Empty;
}

internal record Url07_BooleanQueryParamOptions
{
    [Option]
    public bool Debug { get; init; }

    [Option]
    public bool Verbose { get; init; }
}

internal record Url08_EmptyValueQueryParamOptions
{
    [Option]
    public string Name { get; init; } = string.Empty;

    [Option]
    public string Comment { get; init; } = string.Empty;
}

internal record Url09_ArrayQueryParamOptions
{
    [Option]
    public string[] Tags { get; init; } = [];
}

internal record Url10_IntegerTypeOptions
{
    [Option]
    public int Id { get; init; }

    [Option]
    public int Count { get; init; }
}

internal record Url11_BooleanTypeOptions
{
    [Option]
    public bool Enabled { get; init; }

    [Option]
    public bool Disabled { get; init; }

    [Option]
    public bool Flag { get; init; }
}

internal record Url12_EnumTypeOptions
{
    /// <summary>
    /// 当前项目中的枚举。（源生成器应该要能正确识别。）
    /// </summary>
    [Option]
    public LogLevel LogLevel { get; init; }

    /// <summary>
    /// 引用项目中的枚举。（源生成器应该要能正确识别。）
    /// </summary>
    [Option]
    public CommandLineStyle Style { get; init; }
}

internal record Url13_CollectionTypeOptions
{
    [Option]
    public string[] Ids { get; init; } = [];

    [Option]
    public IReadOnlyList<string> Names { get; init; } = [];
}

internal record Url14_UrlEncodedOptions
{
    [Option]
    public string Query { get; init; } = string.Empty;

    [Option]
    public string Path { get; init; } = string.Empty;
}

internal record Url15_SpecialCharsOptions
{
    [Option]
    public string Special { get; init; } = string.Empty;
}

internal record Url16_NonAsciiOptions
{
    [Option]
    public string Chinese { get; init; } = string.Empty;

    [Option]
    public string Emoji { get; init; } = string.Empty;
}

internal record Url17_PathAsPositionalOptions
{
    [Value(Length = int.MaxValue)]
    public required string[] Paths { get; init; }
}

[Verb("open")]
internal record Url18_VerbPathOptions
{
    [Value(0)]
    public string FilePath { get; init; } = string.Empty;

    [Option]
    public bool ReadOnly { get; init; }
}

internal record Url19_DefaultValueOptions
{
    [Value]
    public string Path { get; set; } = "default-path";
}

internal record Url20_MalformedUrlOptions
{
    [Option]
    public string Value { get; init; } = string.Empty;
}

internal record Url21_DuplicateParamOptions
{
    [Option]
    public string Name { get; init; } = string.Empty;
}

internal record Url22_FragmentOptions
{
    [Option]
    public string Param { get; init; } = string.Empty;

    [Option("fragment")]
    public string Fragment { get; init; } = string.Empty;
}

#endregion
