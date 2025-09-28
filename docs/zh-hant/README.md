# 命令列解析

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine 提供簡單且高效能的命令列解析功能。得益於原始碼產生器（以及攔截器），它現在提供更高效率的解析能力與更友善的開發體驗。所有功能均位於 `DotNetCampus.Cli` 命名空間下。

## 快速使用

```csharp
class Program
{
    static void Main(string[] args)
    {
        // 從命令列參數建立一個新的 CommandLine 執行個體
        var commandLine = CommandLine.Parse(args);

        // 將命令列解析為 Options 型別的執行個體
        // 原始碼產生器會自動處理解析過程，無需手動建立解析器
        var options = commandLine.As<Options>();

        // 接下來，使用 options 物件撰寫其他功能
    }
}
```

你需要定義一個包含命令列參數對應的型別：

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

然後在命令列中使用不同風格的命令填充這個型別的執行個體。程式庫支援多種命令列風格：

| 風格            | 範例                                                                                       |
| --------------- | ------------------------------------------------------------------------------------------ |
| DotNet          | `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`         |
| Windows 经典    | `demo.exe 1.txt 2.txt 3.txt -c 20 -TestName BenchmarkTest -DetailLevel High -Debug`        |
| CMD             | `demo.exe 1.txt 2.txt 3.txt /c 20 /TestName BenchmarkTest /DetailLevel High /Debug`        |
| Gnu             | `demo.exe 1.txt 2.txt 3.txt -c 20 --test-name BenchmarkTest --detail-level High --debug`   |
| 彈性 (Flexible) | `demo.exe 1.txt 2.txt 3.txt --count:20 /TestName BenchmarkTest --detail-level=High -Debug` |

## 命令列風格

DotNetCampus.CommandLine 支援多種命令列風格，你可以在解析時指定使用哪種風格：

```csharp
// 使用 .NET CLI 風格解析命令列參數
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

支援的風格包含：

- `CommandLineStyle.Flexible`（預設）：彈性風格，於各種風格間提供最大相容性，大小寫不敏感
- `CommandLineStyle.DotNet`：.NET CLI 風格，大小寫敏感
- `CommandLineStyle.Gnu`：符合 GNU 規範，大小寫敏感
- `CommandLineStyle.Posix`：符合 POSIX 規範，大小寫敏感
- `CommandLineStyle.Windows`：Windows 風格，大小寫不敏感，混用 `-` 和 `/` 作为选项前缀

預設情況下，這些風格的詳細差異如下：

| 風格              | Flexible       | DotNet         | Gnu               | Posix      | Windows      | URL               |
| ----------------- | -------------- | -------------- | ----------------- | ---------- | ------------ | ----------------- |
| 位置參數          | 支援           | 支援           | 支援              | 支援       | 支援         | 支援              |
| 後置位置參數 `--` | 支援           | 支援           | 支援              | 支援       | 不支援       | 不支援            |
| 大小寫            | 不敏感         | 敏感           | 敏感              | 敏感       | 不敏感       | 不敏感            |
| 長選項            | 支援           | 支援           | 支援              | 不支援     | 支援         | 支援              |
| 短選項            | 支援           | 支援           | 支援              | 支援       | 支援         | 不支援            |
| 長選項前綴        | `--` `-` `/`   | `--`           | `--`              | (無)       | `-` `/`      |                   |
| 短選項前綴        | `-` `/`        | `-`            | `-`               | `-`        | `-` `/`      |                   |
| 長選項 ` `        | --option value | --option value | --option value    | -o value   | -o value     |                   |
| 長選項 `=`        | --option=value | --option=value | --option=value    |            | -o=value     | option=value      |
| 長選項 `:`        | --option:value | --option:value |                   |            | -o:value     |                   |
| 短選項 ` `        | -o value       | -o value       | -o value          | -o value   | -o value     |                   |
| 短選項 `=`        | -o=value       | -o=value       |                   |            | -o=value     | option=value      |
| 短選項 `:`        | -o:value       | -o:value       |                   |            | -o:value     |                   |
| 短選項 `null`     |                |                | -ovalue           |            |              |                   |
| 多字元短選項      | -abc value     | -abc value     |                   |            | -abc value   |                   |
| 長布林選項        | --option       | --option       | --option          |            | -Option      | option            |
| 長布林選項 ` `    | --option true  | --option true  |                   |            | -Option true |                   |
| 長布林選項 `=`    | --option=true  | --option=true  | --option=true[^1] |            | -Option=true |                   |
| 長布林選項 `:`    | --option:true  | --option:true  |                   |            | -Option:true |                   |
| 短布林選項        | -o             | -o             | -o                | -o         | -o           |                   |
| 短布林選項 ` `    | -o true        | -o true        |                   |            | -o true      |                   |
| 短布林選項 `=`    | -o=true        | -o=true        |                   |            | -o=true      | option=true       |
| 短布林選項 `:`    | -o:true        | -o:true        |                   |            | -o:true      |                   |
| 短布林選項 `null` |                |                | -o1               |            |              |                   |
| 布林/開關值       | true/false     | true/false     | true/false        | true/false | true/false   | true/false        |
| 布林/開關值       | yes/no         | yes/no         | yes/no            | yes/no     | yes/no       | yes/no            |
| 布林/開關值       | on/off         | on/off         | on/off            | on/off     | on/off       | on/off            |
| 布林/開關值       | 1/0            | 1/0            | 1/0               | 1/0        | 1/0          | 1/0               |
| 多短布林合併      |                |                | -abc              | -abc       |              |                   |
| 集合選項          | -o A -o B      | -o A -o B      | -o A -o B         | -o A -o B  | -o A -o B    | option=A&option=B |
| 集合選項 ` `[^2]  |                |                |                   |            |              |                   |
| 集合選項 `,`      | -o A,B,C       | -o A,B,C       | -o A,B,C          | -o A,B,C   | -o A,B,C     |                   |
| 集合選項 `;`      | -o A;B;C       | -o A;B;C       | -o A;B;C          | -o A;B;C   | -o A;B;C     |                   |
| 字典選項          | -o:A=X;B=Y     | -o:A=X;B=Y     |                   |            | -o:A=X;B=Y   |                   |
| 命名法            | --kebab-case   | --kebab-case   | --kebab-case      |            |              | kebab-case        |
| 命名法            | -PascalCase    |                |                   |            | -PascalCase  |                   |
| 命名法            | -camelCase     |                |                   |            | -camelCase   |                   |
| 命名法            | /PascalCase    |                |                   |            | /PascalCase  |                   |
| 命名法            | /camelCase     |                |                   |            | /camelCase   |                   |

[^1]: GNU 風格並不支援布林選項顯式帶值，但因為這種情況沒有歧義，所以我們額外支援它。
[^2]: 所有風格預設都不支援空格分隔集合，以儘可能避免與位置參數的歧義。但如果你需要，可以透過 `CommandLineParsingOptions.Style.SupportsSpaceSeparatedCollectionValues` 啟用它。

說明：

1. 除 Windows 風格外，其他風格都支援使用 `--` 作為後置位置參數標記，其後所有參數皆視為位置參數；另外，URL 風格無法表達後置位置參數。
2. 在 `--` 之前，選項與位置參數可以交錯出現，規則如下。

選項會優先取得緊跟的值；凡是能放進該選項的值都會被取走。一旦放不下，後面若還有值，就視為位置參數。

例如，`--option` 是布林選項時，`--option true text` 或 `--option 1 text` 中的 `true` 與 `1` 會被 `--option` 取走，之後的 `text` 為位置參數。
再例如，`--option` 是布林選項時，`--option text` 因為 `text` 不是布林值，所以 `text` 直接視為位置參數。
再例如，若風格支援空白分隔集合（見上表），則當 `--option a b c` 是集合選項時，`a` `b` `c` 都會被取走，直到遇到下一個選項或 `--`。GNU 不支援空白分隔集合。

## 命名法

1. 在程式碼中定義選項時，應使用 kebab-case 命名法
   - [為什麼要這麼做？](https://github.com/dotnet-campus/DotNetCampus.CommandLine/blob/main/docs/analyzers/DCL101.md)
   - 若推測你寫的不是 kebab-case，會提供警告 DCL101
   - 你可以忽略該警告；無論實際字串為何，都當作 kebab-case（提供無歧義的單詞邊界資訊，見下例）
2. 當你定義了被視為 kebab-case 的字串後
   - 依據設定的解析風格，可使用 kebab-case / PascalCase / camelCase 三種風格

範例：

```csharp
[Command("open command-line")]
public class Options
{
    [Option('o', "option-name")]
    public required string OptionName { get; init; }
}
```

此處有兩個 kebab-case：`Command` 特性與 `Option` 特性。可接受：

- DotNet/Gnu：`demo.exe open command-line --option-name value`
- Windows：`demo.exe Open CommandLine -OptionName value`
- CMD：`demo.exe Open CommandLine /optionName value`

若改寫為其他風格，可能出現與預期不同（或是刻意的）結果：

```csharp
#pragma warning disable DCL101
[Command("Open CommandLine")]
public class Options
{
    // 分析器警告：OptionName 不是 kebab-case，可視需要抑制 DCL101。
    [Option('o', "OptionName")]
    public required string OptionName { get; init; }
}
#pragma warning restore DCL101
```

因為仍視為 kebab-case，於是可接受：

- DotNet/Gnu：`demo.exe Open CommandLine --OptionName value`
- Windows：`demo.exe Open CommandLine -OptionName value`
- CMD：`demo.exe Open CommandLine /optionName value`

## 資料型別

程式庫支援多種資料型別：

1. **基本型別**：字串、整數、布林、列舉等
2. **集合型別**：陣列、List、唯讀集合、不可變集合
3. **字典型別**：`IDictionary`、`IReadOnlyDictionary`、`ImmutableDictionary` 等

如何透過命令列傳入，詳見前面的大型表格。

## 必需選項與預設值

定義屬性時，可用下列標記：

1. 使用 `required` 標記選項為必需
2. 使用 `init` 標記選項為唯讀（初始化後不可改）
3. 使用 `?` 標記選項可為 null

實際指派的值依下表行為：

| required | init | nullable | 集合屬性 | 行為         | 說明                             |
| -------- | ---- | -------- | -------- | ------------ | -------------------------------- |
| 1        | _    | _        | _        | 擲出例外     | 必須傳入，缺少則擲出例外         |
| 0        | 1    | 1        | _        | null         | 可為 null，缺少則給 null         |
| 0        | 1    | 0        | 1        | 空集合       | 集合永不為 null，缺少則給空集合  |
| 0        | 1    | 0        | 0        | 預設值／空值 | 不可為 null，缺少則給預設值[^2]  |
| 0        | 0    | _        | _        | 保留初始值   | 非必需或非立即，保留定義時初始值 |

[^2]: 如果是值型別，則會賦值其預設值；如果是參考型別，目前只有一種情況，就是字串，會賦值為空字串 `""`。

- 1 = 標記過
- 0 = 未標記
- _ = 不論是否標記

1. 可空行為對參考與值型別一致（差別只是預設值對參考型別為 null）
2. 缺少必需選項會擲出 `RequiredPropertyNotAssignedException`
3. 「保留初始值」表示可直接在屬性定義時給初值：

```csharp
// 注意：只有未使用 required 與 init 時，初值才會生效。
[Option('o', "option-name")]
public string OptionName { get; set; } = "Default Value";
```

## 異常

命令列程式庫的異常分為以下幾種：

1. 命令列解析異常 `CommandLineParseException`
    - 選項或位置參數未匹配異常
    - 命令列參數格式異常
    - 命令列值轉換異常
2. 命令列物件建立異常
    - 僅此一個 `RequiredPropertyNotAssignedException`，當屬性標記了 `required` 而未在命令列中傳入時發生異常
3. 命令與子命令匹配異常
    - 多次匹配異常 `CommandNameAmbiguityException`
    - 未匹配異常 `CommandNameNotFoundException`

一個很常見的情況是多個協同工作的應用程式未同步升級時，可能某程式使用了新的命令列選項呼叫了本程式，本程式當前版本不可能認識這種「下個版本」才會出現的選項。此時有可能需要忽略這種相容性錯誤（選項或位置參數未匹配異常）。如果你預感到這種情況會經常發生，你可以忽略這種錯誤：

```csharp
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet with
{
    // 可以只忽略選項，也可以只忽略位置參數；也可以像這樣都忽略。
    UnknownArgumentsHandling = UnknownCommandArgumentHandling.IgnoreAllUnknownArguments,
});
```

## 命令與子命令

可使用命令處理器模式處理不同命令，類似 `git commit`、`git remote add`。提供多種方式：

### 1. 使用委派處理

```csharp
var commandLine = CommandLine.Parse(args)
    .AddHandler<AddOptions>(options => { /* 處理 add */ })
    .AddHandler<RemoveOptions>(options => { /* 處理 remove */ })
    .Run();
```

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

### 2. `ICommandHandler` 介面

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
        // 命令處理邏輯
        Console.WriteLine($"Converting {InputFile} to {Format} format");
        // ...
        return Task.FromResult(0); // 結束代碼
    }
}
```

```csharp
var commandLine = CommandLine.Parse(args)
    .AddHandler<ConvertCommandHandler>()
    .AddHandler<FooHandler>()
    .AddHandler<BarHandler>(options => { /* 處理 remove */ })
    .RunAsync();
```

### 3. 使用 ICommandHandler<TState> 介面

有時候，程式的狀態不完全由命令列決定，程式內部也會有一些狀態會影響到命令列處理器的執行。由於我們前面使用 `AddHandler<T>` 沒有辦法傳入任何參數，所以我們還有其他方法傳入狀態進去：

```csharp
using var scope = serviceProvider.BeginScope();
var state = scope.ServiceProvider.GetRequiredService<MyState>();
var commandLine = CommandLine.Parse(args)
    .ForState(state).AddHandler<CommandHandlerWithState>()
    .RunAsync();
```

```csharp
internal class CommandHandlerWithState : ICommandHandler
{
    [Option('o', "option")]
    public required string Option { get; init; }

    public Task<int> RunAsync(MyState state)
    {
        // 這時，你可以額外使用這個傳入的 state。
    }
}
```

如果對同一個狀態可以執行多個處理器，可以一直鏈式呼叫 `AddHandler`；而如果不同的命令處理器要處理不同的狀態，可以再次使用 `ForState`；如果後面不再需要狀態，則 `ForState` 中不要傳入參數。一個更複雜的例子如下：

```csharp
commandLine
    .AddHandler<Handler0>()
    .ForState(state1).AddHandler<Handler1>().AddHandler<Handler2>()
    .ForState(state2).AddHandler<Handler3>()
    .ForState().AddHandler<Handler4>()
    .RunAsync();
```

### 說明

1. `[Command]` 支援多個單字，表示子命令（例：`[Command("remote add")]`）。
2. 未標記 `[Command]`，或標記為 null / 空字串，表示預設命令（`[Command("")]`）。
3. 多個處理器匹配同一命令會擲出 `CommandNameAmbiguityException`。
4. 若有任何處理器為非同步，必須使用 `RunAsync`（否則編譯失敗）。

## URL 協議支援

可解析 URL 協議字串：

```ini
// scheme://command/subcommand/positional-argument1/positional-argument2?option1=value1&option2=value2
```

開頭示例命令列可寫成：

```ini
# `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`
dotnet-campus://1.txt/2.txt?count=20&test-name=BenchmarkTest&detail-level=High&debug
```

特別說明：

1. 集合型別可重複參數名：`tags=csharp&tags=dotnet`
2. URL 中特殊與非 ASCII 字元會自動進行解碼

## 原始碼產生器、攔截器與效能

使用原始碼產生器與攔截器大幅提升效能。

### 使用者程式碼範例

```csharp
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
  <summary>對應產生的原始碼</summary>

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

## 效能數據

解析空白命令列參數：

| Method                        |         Mean |      Error |     StdDev |   Gen0 | Allocated |
| ----------------------------- | -----------: | ---------: | ---------: | -----: | --------: |
| 'parse [] -v=4.1 -p=flexible' |     27.25 ns |   0.485 ns |   0.454 ns | 0.0143 |     240 B |
| 'parse [] -v=4.1 -p=dotnet'   |     27.35 ns |   0.471 ns |   0.440 ns | 0.0143 |     240 B |
| 'parse [] -v=4.0 -p=flexible' |     97.16 ns |   0.708 ns |   0.628 ns | 0.0134 |     224 B |
| 'parse [] -v=4.0 -p=dotnet'   |     95.90 ns |   0.889 ns |   0.742 ns | 0.0134 |     224 B |
| 'parse [] -v=3.x -p=parser'   |     49.73 ns |   0.931 ns |   0.870 ns | 0.0239 |     400 B |
| 'parse [] -v=3.x -p=runtime'  | 19,304.17 ns | 194.337 ns | 162.280 ns | 0.4272 |    7265 B |

解析 GNU 風格命令列參數：

```bash
test DotNetCampus.CommandLine.Performance.dll DotNetCampus.CommandLine.Sample.dll DotNetCampus.CommandLine.Test.dll -c 20 --test-name BenchmarkTest --detail-level High --debug
```

| Method                           | Runtime       |         Mean |       Error |      StdDev |   Gen0 | Allocated |
| -------------------------------- | ------------- | -----------: | ----------: | ----------: | -----: | --------: |
| 'parse [GNU] -v=4.1 -p=flexible' | .NET 10.0     |     355.9 ns |     4.89 ns |     4.58 ns | 0.0548 |     920 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | .NET 10.0     |     339.7 ns |     6.81 ns |     7.57 ns | 0.0548 |     920 B |
| 'parse [GNU] -v=4.0 -p=flexible' | .NET 10.0     |     945.9 ns |    14.87 ns |    13.19 ns | 0.1583 |    2656 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | .NET 10.0     |     882.1 ns |    11.30 ns |    10.57 ns | 0.1631 |    2736 B |
| 'parse [GNU] -v=3.x -p=parser'   | .NET 10.0     |     495.7 ns |     9.26 ns |     9.09 ns | 0.1040 |    1752 B |
| 'parse [GNU] -v=3.x -p=runtime'  | .NET 10.0     |  18,025.5 ns |   194.73 ns |   162.61 ns | 0.4883 |    8730 B |
| 'NuGet: ConsoleAppFramework'     | .NET 10.0     |     134.1 ns |     2.70 ns |     2.65 ns | 0.0215 |     360 B |
| 'NuGet: CommandLineParser'       | .NET 10.0     | 177,520.8 ns | 2,225.66 ns | 1,737.65 ns | 3.9063 |   68895 B |
| 'NuGet: System.CommandLine'      | .NET 10.0     |  66,581.6 ns | 1,323.17 ns | 3,245.76 ns | 1.0986 |   18505 B |
| 'parse [GNU] -v=4.1 -p=flexible' | NativeAOT 9.0 |     624.3 ns |     7.06 ns |     6.60 ns | 0.0505 |     856 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | NativeAOT 9.0 |     600.3 ns |     6.72 ns |     6.28 ns | 0.0505 |     856 B |
| 'parse [GNU] -v=4.0 -p=flexible' | NativeAOT 9.0 |   1,395.6 ns |    20.43 ns |    19.11 ns | 0.1507 |    2529 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | NativeAOT 9.0 |   1,438.1 ns |    19.84 ns |    18.55 ns | 0.1545 |    2609 B |
| 'parse [GNU] -v=3.x -p=parser'   | NativeAOT 9.0 |     720.8 ns |     7.47 ns |     6.99 ns | 0.1030 |    1737 B |
| 'parse [GNU] -v=3.x -p=runtime'  | NativeAOT 9.0 |           NA |          NA |          NA |     NA |        NA |
| 'NuGet: ConsoleAppFramework'     | NativeAOT 9.0 |     195.3 ns |     3.76 ns |     3.69 ns | 0.0234 |     392 B |
| 'NuGet: CommandLineParser'       | NativeAOT 9.0 |           NA |          NA |          NA |     NA |        NA |
| 'NuGet: System.CommandLine'      | NativeAOT 9.0 |           NA |          NA |          NA |     NA |        NA |

其中：

1. `parse` 表示呼叫 `CommandLine.Parse`
2. `handle` 表示呼叫 `CommandLine.AddHandler`
3. 中括號 `[Xxx]` 表示傳入參數風格
4. `--flexible`、`--gnu` 等表示解析使用的風格（匹配效率最高）
5. `-v=3.x -p=parser` 為舊版手寫解析器效能（最佳但語法支援少）
6. `-v=3.x -p=runtime` 為舊版反射解析器效能
7. `-v=4.0` 與 `-v=4.1` 顯示版本效能演進
8. `NuGet: ...` 為其他程式庫效能
9. `parse [URL]`（本文省略部分）為解析 URL 協議效能

作者觀察（@walterlv）：

1. 最快的是 [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework)；本庫性能非常接近，同量級。
2. 感謝其對零依賴、零配置、零反射、零分配的極致追求，激勵我們完成目前版本（`-v4.1`）。
3. 它主打極致性能，犧牲部分語法支援；我們主打「全功能 + 高性能」，因此位於同級別，很難超越它。依你的受眾與需求選擇適用方案。
