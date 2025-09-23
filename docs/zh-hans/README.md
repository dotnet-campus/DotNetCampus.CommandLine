# 命令行解析

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine 提供了简单而高性能的命令行解析功能，得益于源代码生成器的加持，它现在提供了更高效的解析能力和更友好的开发体验。所有功能都位于 DotNetCampus.Cli 命名空间下。

## 快速使用

```csharp
class Program
{
    static void Main(string[] args)
    {
        // 从命令行参数创建一个 CommandLine 类型的新实例
        var commandLine = CommandLine.Parse(args);

        // 将命令行解析为 Options 类型的实例
        // 源生成器会自动为你处理解析过程，无需手动创建解析器
        var options = commandLine.As<Options>();

        // 接下来，使用你的 options 对象编写其他的功能
    }
}
```

你需要定义一个包含命令行参数映射的类型：

```csharp
public class Options
{
    [Option("debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "count")]
    public required int TestCount { get; init; }

    [Option('n', "test-name")]
    public string? TestName { get; set; }

    [Option("test-category")]
    public string? TestCategory { get; set; }

    [Option('d', "detail-level")]
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}

public enum DetailLevel
{
    Low,
    Medium,
    High,
}
```

然后在命令行中使用不同风格的命令填充这个类型的实例。库支持多种命令行风格：

| 风格           | 示例                                                                                       |
| -------------- | ------------------------------------------------------------------------------------------ |
| DotNet         | `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`         |
| PowerShell     | `demo.exe 1.txt 2.txt 3.txt -c 20 -TestName BenchmarkTest -DetailLevel High -Debug`        |
| CMD            | `demo.exe 1.txt 2.txt 3.txt /c 20 /TestName BenchmarkTest /DetailLevel High /Debug`        |
| Gnu            | `demo.exe 1.txt 2.txt 3.txt -c 20 --test-name BenchmarkTest --detail-level High --debug`   |
| 灵活(Flexible) | `demo.exe 1.txt 2.txt 3.txt --count:20 /TestName BenchmarkTest --detail-level=High -Debug` |

## 命令行风格

DotNetCampus.CommandLine 支持多种命令行风格，你可以在解析时指定使用哪种风格：

```csharp
// 使用 .NET CLI 风格解析命令行参数
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

支持的风格包括：

- `CommandLineStyle.Flexible`（默认）：灵活风格，在各种风格间提供最大的兼容性，默认大小写不敏感
- `CommandLineStyle.DotNet`：.NET CLI 风格，默认大小写敏感
- `CommandLineStyle.Gnu`：符合 GNU 规范的风格，默认大小写敏感
- `CommandLineStyle.Posix`：符合 POSIX 规范的风格，默认大小写敏感
- `CommandLineStyle.PowerShell`：PowerShell 风格，默认大小写不敏感

默认情况下，这些风格的详细区别如下：

| 风格              | Flexible       | DotNet         | Gnu               | Posix      | PowerShell   | URL               |
| ----------------- | -------------- | -------------- | ----------------- | ---------- | ------------ | ----------------- |
| 位置参数          | 支持           | 支持           | 支持              | 支持       | 支持         | 支持              |
| 后置位置参数 `--` | 支持           | 支持           | 支持              | 支持       | 不支持       | 不支持            |
| 大小写            | 不敏感         | 敏感           | 敏感              | 敏感       | 不敏感       | 不敏感            |
| 长选项            | 支持           | 支持           | 支持              | 不支持     | 支持         | 支持              |
| 短选项            | 支持           | 支持           | 支持              | 支持       | 支持         | 不支持            |
| 长选项前缀        | `--` `-` `/`   | `--`           | `--`              | 不支持     | `-` `/`      |                   |
| 短选项前缀        | `-` `/`        | `-`            | `-`               | `-`        | `-` `/`      |                   |
| 长选项 ` `        | --option value | --option value | -o value          | -o value   | -o value     |                   |
| 长选项 `=`        | --option=value | --option=value | --option=value    |            | -o=value     | option=value      |
| 长选项 `:`        | --option:value | --option:value |                   |            | -o:value     |                   |
| 短选项 ` `        | -o value       | -o value       | -o value          | -o value   | -o value     |                   |
| 短选项 `=`        | -o=value       | -o=value       |                   |            | -o=value     | option=value      |
| 短选项 `:`        | -o:value       | -o:value       |                   |            | -o:value     |                   |
| 短选项 `null`     |                |                | -ovalue           |            |              |                   |
| 多字符短选项      | -abc value     | -abc value     |                   |            | -abc value   |                   |
| 长布尔选项        | --option       | --option       | --option          |            | -Option      | option            |
| 长布尔选项 ` `    | --option true  | --option true  |                   |            | -Option true |                   |
| 长布尔选项 `=`    | --option=true  | --option=true  | --option=true[^1] |            | -Option=true |                   |
| 长布尔选项 `:`    | --option:true  | --option:true  |                   |            | -Option:true |                   |
| 短布尔选项        | -o             | -o             | -o                | -o         | -o           |                   |
| 短布尔选项 ` `    | -o true        | -o true        |                   |            | -o true      |                   |
| 短布尔选项 `=`    | -o=true        | -o=true        |                   |            | -o=true      | option=true       |
| 短布尔选项 `:`    | -o:true        | -o:true        |                   |            | -o:true      |                   |
| 短布尔选项 `null` |                |                | -o1               |            |              |                   |
| 布尔/开关值       | true/false     | true/false     | true/false        | true/false | true/false   | true/false        |
| 布尔/开关值       | yes/no         | yes/no         | yes/no            | yes/no     | yes/no       | yes/no            |
| 布尔/开关值       | on/off         | on/off         | on/off            | on/off     | on/off       | on/off            |
| 布尔/开关值       | 1/0            | 1/0            | 1/0               | 1/0        | 1/0          | 1/0               |
| 短布尔选项合并    |                |                | -abc              | -abc       |              |                   |
| 集合选项          | -o A -o B      | -o A -o B      | -o A -o B         | -o A -o B  | -o A -o B    | option=A&option=B |
| 集合选项 ` `      | -o A B C       | -o A B C       |                   |            | -o A B C     |                   |
| 集合选项 `,`      | -o A,B,C       | -o A,B,C       | -o A,B,C          | -o A,B,C   | -o A,B,C     |                   |
| 集合选项 `;`      | -o A;B;C       | -o A;B;C       | -o A;B;C          | -o A;B;C   | -o A;B;C     |                   |
| 字典选项          | -o:A=X;B=Y     | -o:A=X;B=Y     |                   |            | -o:A=X;B=Y   |                   |
| 命名法            | --kebab-case   | --kebab-case   | --kebab-case      |            |              | kebab-case        |
| 命名法            | -PascalCase    |                |                   |            | -PascalCase  |                   |
| 命名法            | -camelCase     |                |                   |            | -camelCase   |                   |
| 命名法            | /PascalCase    |                |                   |            | /PascalCase  |                   |
| 命名法            | /camelCase     |                |                   |            | /camelCase   |                   |

[^1]: GNU 风格并不支持布尔选项显式带值，但因为这种情况并没有歧义，所以我们考虑额外支持它。

说明：

1. 除 PowerShell 风格外，其他风格均支持 `--` 作为后置位置参数的标记，之后的所有参数均视为位置参数；另外，URL 风格写不出来后置位置参数。
1. 在 `--` 之前，选项和位置参数是可以混合使用的，规则如下。

选项会优先取出紧跟着的值，但凡能放入该选项的，均会放入该选项，一旦放不下了，后面如果还有值，就会算作位置参数。

例如，`--option` 是个布尔选项时，`--option true text` 或 `--option 1 text` 后面的 `true` 和 `1` 会被 `--option` 选项取走，再后面的 `text` 则是位置参数。
再例如，`--option` 是个布尔选项时，`--option text` 由于 `text` 不是布尔值，所以 `text` 直接就是位置参数。
再例如，如果风格支持空格分隔集合（见上表），那么当 `--option a b c` 是个集合选项时，`a` `b` `c` 都会被取走，直到遇到下一个选项或 `--`。GNU 不支持空格分隔集合。

## 命名法

1. 在代码中定义一个选项时，你应该使用 kebab-case 命名法
    - [为什么要这么做？](https://github.com/dotnet-campus/DotNetCampus.CommandLine/blob/main/docs/analyzers/DCL101.md)
    - 如果我们猜测你写的不是 kebab-case 命名法，会提供一个警告 DCL101
    - 但你可以忽略这个警告，无论你最终写了什么字符串，我们都视你写的是 kebab-case 命名法（这可以提供无歧义的命名信息，见下例）
2. 当你在代码中定义了被视为 kebab-case 命名法的字符串后
    - 根据你设置的不同命令行解析风格，你可以使用 kebab-case PascalCase 和 camelCase 三种风格的命名法

例如你定义了如下命令行对象：

```csharp
[Command("open command-line")]
public class Options
{
    [Option('o', "option-name")]
    public required string OptionName { get; init; }
}
```

这里存在两个使用了 kebab-case 命名法的地方，一个是 `Command` 特性，另一个是 `Option` 特性。你可以接受以下这些命令行传入：

- DotNet/Gnu 风格: `demo.exe open command-line --option-name value`
- PowerShell 风格: `demo.exe Open CommandLine -OptionName value`
- CMD 风格: `demo.exe Open CommandLine /optionName value`

但加入你把这两处的名字都写成其他风格，你可能会获得不太符合预期的结果（当然，也可能你故意如此）：

```csharp
#pragma warning disable DCL101
[Command("Open CommandLine")]
public class Options
{
    // 此时会有分析器警告，OptionName 不是 kebab-case 风格。如果需要，你可以抑制 DCL101。
    [Option('o', "OptionName")]
    public required string OptionName { get; init; }
}
#pragma warning restore DCL101
```

由于我们视这些都是 kebab-case 风格，所以你将接受以下这些命令行传入（注意 DotNet/Gnu 风格已经发生了变化）：

- DotNet/Gnu 风格: `demo.exe Open CommandLine --OptionName value`
- PowerShell 风格: `demo.exe Open CommandLine -OptionName value`
- CMD 风格: `demo.exe Open CommandLine /optionName value`

## 数据类型

库支持多种数据类型的解析：

1. **基本类型**: 字符串、整数、布尔值、枚举等
2. **集合类型**: 数组、列表、只读集合、不可变集合
3. **字典类型**: `IDictionary`、`IReadOnlyDictionary`、`ImmutableDictionary` 等

关于这些类型如何通过命令行传入，请见上表（最详细的那个）。

## 必需选项与默认值

当你定义一个属性的时候，这些标记会影响到默认值：

1. `required`：标记一个属性是必须的
1. `init`：标记一个属性是不可变的
1. `?`：标记一个属性是可空的
1. 特别的，集合类型也会有特别处理

这些行为具体以如下表格影响着属性的初值：

| required | init | nullable | list | 行为        | 解释                              |
| -------- | ---- | -------- | ---- | ----------- | --------------------------------- |
| 1        | _    | _        | _    | 抛异常      | 要求必须传入，没有传就抛异常      |
| 0        | 1    | 1        | _    | null        | 可空，没有传就赋值 null           |
| 0        | 1    | 0        | 1    | 空集合      | 集合永不为 null，没传就赋值空集合 |
| 0        | 1    | 0        | 0    | 默认值/空值 | 不可空，没有传就赋值默认值[^2]    |
| 0        | 0    | _        | _    | 保留初值    | 不要求必须或立即赋值的，保留初值  |

[^2]: 如果是值类型，则会赋值其默认值；如果是引用类型，目前只有一种情况，就是字符串，会赋值为空字符串 `""`。

- 1 = 标记了
- 0 = 没标记
- _ = 无论有没有标记

1. 可空，无论是引用类型还是值类型，其行为完全一致。要硬说不同，就是那个「默认值」会导致引用类型得到 `null`。
2. 如果未提供必需选项，解析时会抛出`RequiredPropertyNotAssignedException`异常。
3. 上述行为的「保留初值」的意思是，你可以在定义这个属性的时候写一个初值，就像下面这样：

```csharp
// 请注意，这里的初值仅在没有 required 也没有 init 时才生效。
[Option('o', "option-name")]
public string OptionName { get; set; } = "Default Value"
```

## 命令与子命令

你可以使用命令处理器模式处理不同的命令，类似于`git commit`、`git remote add`等。DotNetCampus.CommandLine 提供了多种添加命令处理器的方式：

### 1. 使用委托处理命令

最简单的方式是通过委托处理命令，将命令选项类型和处理逻辑分离：

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* 处理add命令 */ })
    .AddHandler<RemoveOptions>(options => { /* 处理remove命令 */ })
    .Run();
```

定义命令选项类时使用`Command`特性标记命令：

```csharp
[Command("add")]
public class AddOptions
{
    [Value(0)]
    public string ItemToAdd { get; init; }
}

[Command("remove")]
public class RemoveOptions
{
    [Value(0)]
    public string ItemToRemove { get; init; }
}
```

### 2. 使用 ICommandHandler 接口

对于更复杂的命令处理逻辑，你可以创建实现 `ICommandHandler` 接口的类，将命令选项和处理逻辑封装在一起：

```csharp
[Command("convert")]
internal class ConvertCommandHandler : ICommandHandler
{
    [Option('i', "input")]
    public required string InputFile { get; init; }

    [Option('o', "output")]
    public string? OutputFile { get; init; }

    [Option('f', "format")]
    public string Format { get; set; } = "json";

    public Task<int> RunAsync()
    {
        // 实现命令处理逻辑
        Console.WriteLine($"Converting {InputFile} to {Format} format");
        // ...
        return Task.FromResult(0); // 返回退出代码
    }
}
```

然后直接添加到命令行解析器中：

```csharp
var commandLine = CommandLine.Parse(args);
commandLine
    .AddHandler<ConvertCommandHandler>()
    .AddHandler<FooHandler>()
    .AddHandler<BarHandler>(options => { /* 处理remove命令 */ })
    .Run();
```

### 一些说明

1. `[Command]` 特性支持多个单词，表示子命令，如 `[Command("remote add")]`。
1. 没有标 `[Command]` 特性，或标了但传 `null` 或空字符串时，表示默认命令，如 `[Command("")]`。
1. 如果多个命令处理器匹配同一个命令，会抛出 `CommandNameAmbiguityException`。
1. 命令处理器中，有任何一个是异步时，你将必须使用 `RunAsync` 替代 `Run`，否则会编译不通过。

## URL协议支持

DotNetCampus.CommandLine 支持解析 URL 协议字符串，格式如下：

```ini
// scheme://command/subcommand/positional-argument1/positional-argument2?option1=value1&option2=value2
```

本文开头示例中的那个命令行，使用 URL 传入的话将是下面这样：

```ini
# `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`
dotnet-campus://1.txt/2.txt?count=20&test-name=BenchmarkTest&detail-level=High&debug
```

特别的：

1. 集合类型选项可通过重复参数名传入多个值，如：`tags=csharp&tags=dotnet`
2. URL中的特殊字符和非 ASCII 字符会自动进行 URL 解码

## 源生成器、拦截器与性能优化

DotNetCampus.CommandLine 使用源代码生成器技术大幅提升了命令行解析的性能。其中的拦截器（[Interceptor](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md)）让性能提升发挥得更淋漓尽致。

### 源生成器生成的代码示例

下面是一个简单的命令行选项类型及其对应生成的源代码示例：

```csharp
// 用户代码中的类型
public class BenchmarkOptions41
{
    [Option("debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "count")]
    public required int TestCount { get; init; }

    [Option('n', "test-name")]
    public string? TestName { get; set; }

    [Option("test-category")]
    public string? TestCategory { get; set; }

    [Option('d', "detail-level")]
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}
```

<details>
  <summary>对应生成的源</summary>

```csharp
#nullable enable
using global::System;
using global::DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Performance.Fakes;

/// <summary>
/// 辅助 <see cref="global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41"/> 生成命令行选项、子命令或处理函数的创建。
/// </summary>
public sealed class BenchmarkOptions41Builder(global::DotNetCampus.Cli.CommandLine commandLine)
{
    public static readonly global::DotNetCampus.Cli.Compiler.NamingPolicyNameGroup CommandNameGroup = default;

    public static global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 CreateInstance(global::DotNetCampus.Cli.CommandLine commandLine)
    {
        return new DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41Builder(commandLine).Build();
    }

    private global::DotNetCampus.Cli.Compiler.BooleanArgument IsDebugMode = new();

    private global::DotNetCampus.Cli.Compiler.NumberArgument TestCount = new();

    private global::DotNetCampus.Cli.Compiler.StringArgument TestName = new();

    private global::DotNetCampus.Cli.Compiler.StringArgument TestCategory = new();

    private __GeneratedEnumArgument__DotNetCampus_Cli_Performance_Fakes_DetailLevel__ DetailLevel = new();

    private global::DotNetCampus.Cli.Compiler.StringListArgument TestItems = new();

    public global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 Build()
    {
        if (commandLine.RawArguments.Count is 0)
        {
            return BuildDefault();
        }

        var parser = new global::DotNetCampus.Cli.Utils.Parsers.CommandLineParser(commandLine, "BenchmarkOptions41", 0)
        {
            MatchLongOption = MatchLongOption,
            MatchShortOption = MatchShortOption,
            MatchPositionalArguments = MatchPositionalArguments,
            AssignPropertyValue = AssignPropertyValue,
        };
        parser.Parse().WithFallback(commandLine);
        return BuildCore(commandLine);
    }

    private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchLongOption(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, global::DotNetCampus.Cli.CommandNamingPolicy namingPolicy)
    {
        // 1. 先匹配 kebab-case 命名法（原样字符串）
        if (namingPolicy.SupportsOrdinal())
        {
            // 1.1 先快速原字符匹配一遍（能应对规范命令行大小写，并优化 DotNet / GNU 风格的性能）。
            switch (longOption)
            {
                case "debug":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(IsDebugMode), 0, global::DotNetCampus.Cli.Compiler.OptionValueType.Boolean);
                case "count":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
                case "test-name":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
                case "test-category":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCategory), 3, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
                case "detail-level":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }

            // 1.2 再按指定大小写匹配一遍（能应对不规范命令行大小写）。
            var defaultComparison = defaultCaseSensitive
                ? global::System.StringComparison.Ordinal
                : global::System.StringComparison.OrdinalIgnoreCase;
            if (longOption.Equals("debug".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(IsDebugMode), 0, global::DotNetCampus.Cli.Compiler.OptionValueType.Boolean);
            }
            if (longOption.Equals("count".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("test-name".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("test-category".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCategory), 3, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("detail-level".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
        }

        // 2. 再匹配其他命名法（能应对所有不规范命令行大小写，并支持所有风格）。
        if (namingPolicy.SupportsPascalCase())
        {
            var defaultComparison = defaultCaseSensitive
                ? global::System.StringComparison.Ordinal
                : global::System.StringComparison.OrdinalIgnoreCase;
            if (longOption.Equals("Debug".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(IsDebugMode), 0, global::DotNetCampus.Cli.Compiler.OptionValueType.Boolean);
            }
            if (longOption.Equals("Count".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("TestName".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("TestCategory".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCategory), 3, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("DetailLevel".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
        }

        return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;
    }

    private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchShortOption(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive)
    {
        // 1. 先快速原字符匹配一遍（能应对规范命令行大小写，并优化 DotNet / GNU 风格的性能）。
        switch (shortOption)
        {
            // 属性 IsDebugMode 没有短名称，无需匹配。
            case "c":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            case "n":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            // 属性 TestCategory 没有短名称，无需匹配。
            case "d":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }

        var defaultComparison = defaultCaseSensitive
            ? global::System.StringComparison.Ordinal
            : global::System.StringComparison.OrdinalIgnoreCase;

        // 2. 再按指定大小写指定命名法匹配一遍（能应对不规范命令行大小写）。
        // 属性 IsDebugMode 没有短名称，无需匹配。
        if (shortOption.Equals("c".AsSpan(), defaultComparison))
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }
        if (shortOption.Equals("n".AsSpan(), defaultComparison))
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }
        // 属性 TestCategory 没有短名称，无需匹配。
        if (shortOption.Equals("d".AsSpan(), defaultComparison))
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }

        return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;
    }

    private global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch MatchPositionalArguments(ReadOnlySpan<char> value, int argumentIndex)
    {
        // 属性 TestItems 覆盖了所有位置参数，直接匹配。
        return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("TestItems", 5, global::DotNetCampus.Cli.Compiler.PositionalArgumentValueType.Normal);
    }

    private void AssignPropertyValue(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        switch (propertyIndex)
        {
            case 0:
                IsDebugMode = IsDebugMode.Assign(value);
                break;
            case 1:
                TestCount = TestCount.Assign(value);
                break;
            case 2:
                TestName = TestName.Assign(value);
                break;
            case 3:
                TestCategory = TestCategory.Assign(value);
                break;
            case 4:
                DetailLevel = DetailLevel.Assign(value);
                break;
            case 5:
                TestItems = TestItems.Append(value);
                break;
        }
    }

    private global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 BuildCore(global::DotNetCampus.Cli.CommandLine commandLine)
    {
        var result = new global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41
        {
            // 1. There is no [RawArguments] property to be initialized.

            // 2. [Option]
            IsDebugMode = IsDebugMode.ToBoolean() ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain a required option 'debug'. Command line: {commandLine}", "IsDebugMode"),
            TestCount = TestCount.ToInt32() ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain a required option 'count'. Command line: {commandLine}", "TestCount"),

            // 3. [Value]
            TestItems = TestItems.ToList() ?? [],
        };

        // 1. There is no [RawArguments] property to be assigned.

        // 2. [Option]
        if (TestName.ToString() is { } o0)
        {
            result.TestName = o0;
        }
        if (TestCategory.ToString() is { } o1)
        {
            result.TestCategory = o1;
        }
        if (DetailLevel.ToEnum() is { } o2)
        {
            result.DetailLevel = o2;
        }

        // 3. There is no [Value] property to be assigned.

        return result;
    }

    private global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 BuildDefault()
    {
        throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain any required option or positional argument. Command line: {commandLine}", null!);
    }

    /// <summary>
    /// Provides parsing and assignment for the enum type <see cref="global::DotNetCampus.Cli.Performance.Fakes.DetailLevel"/>.
    /// </summary>
    private readonly record struct __GeneratedEnumArgument__DotNetCampus_Cli_Performance_Fakes_DetailLevel__
    {
        /// <summary>
        /// Indicates whether to ignore exceptions when parsing fails.
        /// </summary>
        public bool IgnoreExceptions { get; init; }

        /// <summary>
        /// Stores the parsed enum value.
        /// </summary>
        private global::DotNetCampus.Cli.Performance.Fakes.DetailLevel? Value { get; init; }

        /// <summary>
        /// Assigns a value when a command line input is parsed.
        /// </summary>
        /// <param name="value">The parsed string value.</param>
        public __GeneratedEnumArgument__DotNetCampus_Cli_Performance_Fakes_DetailLevel__ Assign(ReadOnlySpan<char> value)
        {
            Span<char> lowerValue = stackalloc char[value.Length];
            for (var i = 0; i < value.Length; i++)
            {
                lowerValue[i] = char.ToLowerInvariant(value[i]);
            }
            global::DotNetCampus.Cli.Performance.Fakes.DetailLevel? newValue = lowerValue switch
            {
                "low" => global::DotNetCampus.Cli.Performance.Fakes.DetailLevel.Low,
                "medium" => global::DotNetCampus.Cli.Performance.Fakes.DetailLevel.Medium,
                "high" => global::DotNetCampus.Cli.Performance.Fakes.DetailLevel.High,
                _ when IgnoreExceptions => null,
                _ => throw new global::DotNetCampus.Cli.Exceptions.CommandLineParseValueException($"Cannot convert '{value.ToString()}' to enum type 'DotNetCampus.Cli.Performance.Fakes.DetailLevel'."),
            };
            return this with { Value = newValue };
        }

        /// <summary>
        /// Converts the parsed value to the enum type.
        /// </summary>
        public global::DotNetCampus.Cli.Performance.Fakes.DetailLevel? ToEnum() => Value;
    }
}
```

</details>

## 性能数据

源代码生成器实现提供了极高的命令行解析性能。

解析空白命令行参数：

| Method                        |         Mean |      Error |     StdDev |   Gen0 | Allocated |
| ----------------------------- | -----------: | ---------: | ---------: | -----: | --------: |
| 'parse [] -v=4.1 -p=flexible' |     27.25 ns |   0.485 ns |   0.454 ns | 0.0143 |     240 B |
| 'parse [] -v=4.1 -p=dotnet'   |     27.35 ns |   0.471 ns |   0.440 ns | 0.0143 |     240 B |
| 'parse [] -v=4.0 -p=flexible' |     97.16 ns |   0.708 ns |   0.628 ns | 0.0134 |     224 B |
| 'parse [] -v=4.0 -p=dotnet'   |     95.90 ns |   0.889 ns |   0.742 ns | 0.0134 |     224 B |
| 'parse [] -v=3.x -p=parser'   |     49.73 ns |   0.931 ns |   0.870 ns | 0.0239 |     400 B |
| 'parse [] -v=3.x -p=runtime'  | 19,304.17 ns | 194.337 ns | 162.280 ns | 0.4272 |    7265 B |

解析 GNU 风格命令行参数：

```bash
test DotNetCampus.CommandLine.Performance.dll DotNetCampus.CommandLine.Sample.dll DotNetCampus.CommandLine.Test.dll -c 20 --test-name BenchmarkTest --detail-level High --debug
```

| Method                           | Runtime       |         Mean |       Error |      StdDev |    Gen0 | Allocated |
| -------------------------------- | ------------- | -----------: | ----------: | ----------: | ------: | --------: |
| 'parse [GNU] -v=4.1 -p=flexible' | .NET 10.0     |     355.9 ns |     4.89 ns |     4.58 ns |  0.0548 |     920 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | .NET 10.0     |     339.7 ns |     6.81 ns |     7.57 ns |  0.0548 |     920 B |
| 'parse [GNU] -v=4.0 -p=flexible' | .NET 10.0     |     945.9 ns |    14.87 ns |    13.19 ns |  0.1583 |    2656 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | .NET 10.0     |     882.1 ns |    11.30 ns |    10.57 ns |  0.1631 |    2736 B |
| 'parse [GNU] -v=3.x -p=parser'   | .NET 10.0     |     495.7 ns |     9.26 ns |     9.09 ns |  0.1040 |    1752 B |
| 'parse [GNU] -v=3.x -p=runtime'  | .NET 10.0     |  18,025.5 ns |   194.73 ns |   162.61 ns |  0.4883 |    8730 B |
| 'NuGet: ConsoleAppFramework'     | .NET 10.0     |     134.1 ns |     2.70 ns |     2.65 ns |  0.0215 |     360 B |
| 'NuGet: CommandLineParser'       | .NET 10.0     | 177,520.8 ns | 2,225.66 ns | 1,737.65 ns |  3.9063 |   68895 B |
| 'NuGet: System.CommandLine'      | .NET 10.0     |  66,581.6 ns | 1,323.17 ns | 3,245.76 ns |  1.0986 |   18505 B |
| 'parse [GNU] -v=4.1 -p=flexible' | NativeAOT 9.0 |     624.3 ns |     7.06 ns |     6.60 ns |  0.0505 |     856 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | NativeAOT 9.0 |     600.3 ns |     6.72 ns |     6.28 ns |  0.0505 |     856 B |
| 'parse [GNU] -v=4.0 -p=flexible' | NativeAOT 9.0 |   1,395.6 ns |    20.43 ns |    19.11 ns |  0.1507 |    2529 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | NativeAOT 9.0 |   1,438.1 ns |    19.84 ns |    18.55 ns |  0.1545 |    2609 B |
| 'parse [GNU] -v=3.x -p=parser'   | NativeAOT 9.0 |     720.8 ns |     7.47 ns |     6.99 ns |  0.1030 |    1737 B |
| 'parse [GNU] -v=3.x -p=runtime'  | NativeAOT 9.0 |           NA |          NA |          NA |      NA |        NA |
| 'NuGet: ConsoleAppFramework'     | NativeAOT 9.0 |     195.3 ns |     3.76 ns |     3.69 ns |  0.0234 |     392 B |
| 'NuGet: CommandLineParser'       | NativeAOT 9.0 |           NA |          NA |          NA |      NA |        NA |
| 'NuGet: System.CommandLine'      | NativeAOT 9.0 |           NA |          NA |          NA |      NA |        NA |

其中：

1. `parse` 表示调用的是 `CommandLine.Parse` 方法
1. `handle` 表示调用的是 `CommandLine.AddHandler` 方法
1. 中括号 `[Xxx]` 表示传入的命令行参数的风格
1. `--flexible` `--gnu` 等表示解析传入命令行时所使用的解析器风格（相匹配时效率最高）
1. `-v=3.x -p=parser` 表示旧版本手工编写解析器并传入时的性能（性能最好，不过旧版本支持的命令行规范较少，很多合法的命令写法并不支持）
1. `-v=3.x -p=runtime` 表示旧版本使用默认的反射解析器时的性能
1. `-v=4.0 -p=dotnet` 表示数月前的 4.0 预览版的性能
1. `-v=4.1 -p=dotnet` 表示当前版本的性能
1. `NuGet: ConsoleAppFramework`、`NuGet: CommandLineParser` 和 `NuGet: System.CommandLine` 表示使用对应名称的 NuGet 包解析命令行参数时的性能
1. `parse [URL]` 表示解析 URL 协议字符串时的性能

库作者 @walterlv 的感受：

1. 性能最好的是 [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) 库，我们的 [DotNetCampus.CommandLine](https://github.com/dotnet-campus/DotNetCampus.CommandLine) 比它差一点，不过仍然在同一个数量级。对比其他库，我们俩比他们好了几个数量级。
1. 非常感谢 [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) 的极致追求（零依赖、零开销、零反射、零分配，由 C# 源码生成器提供的 AOT 安全 CLI 框架）。虽然在发现它之前我们就已经在使用源生成器和拦截器了（`-v4.0`），但确实是它让我们看到了更高的目标和动力，写了现在的版本（`-v4.1`）。
1. 当然，[ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) 的目标是极致的性能追求，为此确实也牺牲了一部分命令行语法支持；而我们的目标是在「全功能」的基础上实现极致的性能追求，所以性能最多只能打在同一级别，确实也无法超越它。如果你的程序极致追求性能，并且使用人群倾向于专业人士或应用程序，则非常推荐使用它；不过如果你希望你的程序极致追求性能的同时，也面向大众群体（非专业人士）或各种不同喜好的人群体，则非常推荐使用我们 [DotNetCampus.CommandLine](https://github.com/dotnet-campus/DotNetCampus.CommandLine)。
