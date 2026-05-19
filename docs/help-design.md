# 内置帮助支持设计

本文档描述 DotNetCampus.CommandLine 内置帮助支持的设计方案。这是一份设计文档，不包含代码实现。

## 背景

DotNetCampus.CommandLine 提供了丰富的命令行解析能力，支持多风格、多命令、子命令、委托处理等多种使用模式。但在实际使用中，用户需要一种标准方式来了解当前程序支持哪些命令和选项。常见的做法是在命令行中传入 `--help` 或 `-h` 等参数来获取帮助信息。

过去，框架曾提供 `AddStandardHandlers()` 方法来自动注册 `--help` 和 `--version` 处理逻辑。但该方法的行为不符合大多数开发者的预期，已被标记为 `[Obsolete]` 并从代码库中移除。当前版本中，帮助功能需要完全由使用方自行实现，缺乏统一的内置支持。

本设计的目标是在保持框架现有架构风格的前提下，为 DotNetCampus.CommandLine 提供可选的内置帮助支持。

## 目标与非目标

### 目标

- 提供风格感知的内置帮助检测，在 `DotNet`、`Gnu`、`Flexible`、`Windows`、`Posix` 等风格下自动识别对应的帮助写法。
- 提供 `AddHelpHandler()` 扩展方法，允许用户按需启用帮助输出。
- 根帮助应展示默认命令的参数信息（当且仅当存在默认命令时）；命令帮助展示该命令的选项信息。
- 通过源生成器扩展现有 `Metadata` 类型，使其携带帮助所需的描述信息。
- 保持无反射、AOT 安全的设计原则。
- 输出文本的描述来源限定为 `CommandLineAttribute.Description` 属性，这是当前唯一的信息源。
- `CommandRunner` 应在正常命令匹配之前检测帮助请求，而不是通过 fallback 机制。

### 非目标

- **不支持 `-help` 写法**。理由详见下文。
- **不包含 help 子命令**。本文档仅讨论内置的 `--help` 风格支持，不涉及 `myapp help` 这种子命令形式。子命令形式的帮助可留作后续独立设计。
- **不支持 XML 文档注释作为帮助信息来源**。`Description` 是当前唯一的信息源，XML 文档注释不在本设计范围内。
- **不支持本地化**。本设计不涉及多语言的描述文本切换。如后续需要本地化支持，应另案处理。
- **不展示选项的默认值**。框架在属性未赋值时的回退行为（如空字符串、空集合、`default(T)` 等）是内部的兜底逻辑，并非用户在业务上有意义的默认值声明。在没有提供显式声明默认值的机制之前，帮助信息不展示默认值，以免对 CLI 使用者造成误导。
- **不修改现有的选项匹配或命令匹配逻辑**。帮助检测是一个独立的先行检查步骤，不干扰常规解析路径。
- **不产生隐式的控制台输出**。所有帮助文本的输出都要求用户显式调用 `AddHelpHandler()` 后才生效。

## 用户侧 API 设计

用户通过链式调用中的 `AddHelpHandler()` 方法来启用内置帮助支持：

```csharp
// 启用帮助支持
var commandLine = CommandLine.Parse(args)
    .AddHandler<AddOptions>(options => { /* 处理 add 命令 */ })
    .AddHandler<RemoveOptions>(options => { /* 处理 remove 命令 */ })
    .AddHelpHandler()       // <-- 启用内置帮助
    .Run();
```

`AddHelpHandler()` 是一个扩展方法，内部调用 `CommandRunner` 的专用方法以注册帮助元数据。它返回 `IAsyncCommandRunnerBuilder` 以支持继续链式调用。

```csharp
// 更复杂的调用链
var commandLine = CommandLine.Parse(args)
    .AddHandler<ConvertCommandHandler>()
    .AddHelpHandler()
    .ForState(myState).AddHandler<CommandHandlerWithState>()
    .RunAsync();
```

`AddHelpHandler()` 在框架层面的作用包括：

1. 在 `CommandRunner` 上注册一个内部帮助标记，告诉运行器在运行前检查帮助请求。
2. 源生成器在生 `Metadata` 时额外实现一个帮助元数据接口，携带描述信息。
3. 当检测到帮助请求时，运行器使用已注册的所有处理器的元数据来构建帮助文本。

如果用户没有调用 `AddHelpHandler()`，则 `--help`、`-h` 等参数会被当做普通选项处理，不会触发任何帮助输出。这是设计红线：**框架不会在用户未显式选择的情况下向控制台写入任何内容**。

## 按风格区分的帮助写法支持

不同的命令行风格对选项前缀、大小写敏感性、分隔符有不同的规则。内置帮助检测必须感知这些差异，才能正确识别用户的帮助意图。

下表列出了各风格下支持的帮助写法：

| 风格     | `--help` | `-h`   | `-?`   | `/?`   | `/help` | `/h`   | `-help` |
| -------- | -------- | ------ | ------ | ------ | ------- | ------ | ------- |
| DotNet   | 支持     | 支持   | 不支持 | 不支持 | 不支持  | 不支持 | 不支持  |
| Gnu      | 支持     | 支持   | 不支持 | 不支持 | 不支持  | 不支持 | 不支持  |
| Flexible | 支持     | 支持   | 支持   | 支持   | 支持    | 支持¹  | 不支持  |
| Windows  | 不支持   | 不支持 | 支持   | 支持   | 支持    | 支持¹  | 不支持  |
| Posix    | 不支持   | 支持   | 不支持 | 不支持 | 不支持  | 不支持 | 不支持  |
| URL      | 不支持   | 不支持 | 不支持 | 不支持 | 不支持  | 不支持 | 不支持  |

> ¹ `/h` 作为兼容别名支持，前提是其实现不引入额外的复杂度。它并非本设计的主要推荐写法，只在现有前缀与短选项规则能够自然覆盖时顺带支持。

### 各风格的匹配细节

**DotNet 风格**：
- 使用 `--help` 作为长选项，大小写敏感。这是 .NET CLI 的标准用法。
- 使用 `-h` 作为短选项。DotNet 风格支持多字符短选项（`-tl`），因此 `-h` 是安全的单字符短选项。
- 访问值分隔符支持 `:`、`=` 和空格，但帮助选项是布尔型，不需要携带值。

**Gnu 风格**：
- 使用 `--help` 作为长选项，大小写敏感，符合 GNU 标准惯例。
- 使用 `-h` 作为短选项。注意 GNU 风格支持短选项组合（`-abc` 等价于 `-a -b -c`），因此 `-h` 可能被组合使用（如 `-hv`）。帮助检测应在解析器完成短选项拆分后，对拆分出的单个短字符进行匹配。

**Flexible 风格**：
- 支持最多的写法变体，因为它兼容所有前缀（`--`、`-`、`/`）。
- 大小写不敏感，因此 `--Help`、`-H`、`/HELP` 均可匹配。
- `/?` 和 `-?` 也在支持范围内，因为 Flexible 风格天然支持短选项和 `/` 前缀。

**Windows 风格**：
- 核心支持 `/?`（传统 Windows 帮助写法）和 `-?`。
- 支持 `/help` 作为显式帮助写法。
- 由于 Windows 风格接受 `/` 或 `-` 作为前缀，大小写不敏感，这些写法的各种大小写变体均能匹配。
- `/h` 作为一个单字符短选项，在 Windows 风格下可作为兼容性别名支持，但不作为主要推荐写法。

**Posix 风格**：
- 仅支持短选项，不支持长选项，因此只支持 `-h`。
- 支持短选项组合，因此 `-h` 可能被组合使用。

**URL 风格**：
- 不适用于帮助场景。URL 风格通常用于程序间通信的 deep link，不涉及用户交互的帮助输出。

## 为什么不支持某些写法

### `-help` 不被支持

`-help`（单短横线后跟完整单词 "help"）在多个风格中与现有规则冲突：

1. **与短选项组合规则冲突**。在 Gnu 和 Posix 风格中，`-abc` 被解析为三个短选项 `-a -b -c`。`-help` 在这样的风格下会被拆解为 `-h -e -l -p`，而非当作一个名为 "help" 的长选项。如果特例化处理，将破坏短选项组合语法的可预测性。

2. **与多字符短选项规则冲突**。在 DotNet 和 Windows 风格中，`-abc` 被视为一个名为 "abc" 的多字符短选项。`-help` 在这些风格下确实会被当作一个整体，但问题是 "help" 不是一个普通选项名，而是一个内置语义关键词。多字符短选项本身已经用于用户定义的选项名（如 `-tl` 作为 `--terminal-logger` 的短选项），增加内置关键词的特殊匹配会引入解析器的额外复杂性。

3. **风格间行为不统一**。`-help` 在 Gnu 风格下会被拆成 `-h -e -l -p`，在 DotNet 风格下会被当作一个整体。这种不一致对用户和实现都是困惑的来源。维护风格间行为一致性的代价太高。

4. **破坏用户的选项命名空间**。用户可能恰好定义了一个名为 `-help` 的多字符短选项，或者恰好定义了 `-h`、`-e`、`-l`、`-p` 等短选项。内置的 `-help` 检测会与这些用户选项产生意外的交互。

基于以上原因，`-help` 在本设计中不被支持。用户应使用 `--help`（双短横线，所有支持长选项的风格均认可）或对应风格认可的其他写法（`-h`、`/?` 等）。

### `help` 子命令不被包含在本设计中

`myapp help` 形式的子命令具有独立的语义和实现路径。它涉及命令名的特殊注册、与现有 `AddHandler` 机制的交互、以及对用户自定义的 `help` 命令的冲突处理。这些复杂度需要单独的设计文档来处理，不在本文档的范围内。

## 帮助请求的检测与命令定位

### 检测时机

帮助请求的检测发生在 `CommandRunner.RunAsync()` 的开头，在所有正常的命令匹配和参数解析之前。这是一个独立的先行检查步骤。

伪流程如下：

```
RunAsync()
├── 步骤 1: 检查帮助请求
│   ├── 如果调用了 AddHelpHandler()，扫描原始参数
│   ├── 匹配到帮助写法 → 进入帮助输出流程
│   └── 未匹配到帮助写法 → 继续正常流程
├── 步骤 2: 正常的命令匹配 (现有的 MatchCommandObject)
└── 步骤 3: 命令构建和执行
```

这个顺序保证了：

- 帮助请求不会被用户的命令处理器意外拦截。
- 即使解析器后续可能遇到错误，帮助请求也能优先响应。
- 帮助输出的行为不依赖 fallback 机制，路径清晰可控。

### 检测算法

检测算法需要考虑风格的差异：

1. 提取原始命令行参数 `args`。
2. 根据 `CommandLineStyle` 确定当前风格支持的帮助写法集合。
3. 对每个参数检查是否匹配帮助写法集合中的任意一项。
4. 匹配时需要考虑大小写敏感性（`CaseSensitive` 属性）和前缀规则。

对于短选项组合（Gnu、Posix），检测需要在组合拆分之后进行。例如 `-hv` 在 GNU 风格下应被拆分为 `-h` 和 `-v`，其中 `-h` 触发帮助。

```
检测算法：
if (!_hasHelpHandler)
    return; // 没有注册帮助处理器，不做任何事

var helpSpellings = GetHelpSpellingsForStyle(currentStyle);
foreach (var arg in commandLineArgs)
{
    if (MatchHelpSpelling(arg, helpSpellings, currentStyle))
    {
        _isHelpRequested = true;
        break;
    }
}
```

### 命令定位

当帮助请求被检测到后，系统需要确定用户想知道哪个命令的帮助。判断逻辑如下：

1. 如果命令行中除了帮助写法之外没有其他参数，或者没有任何命令名，则输出**根帮助**。
2. 如果命令行中除了帮助写法之外还包含一个命令名称（如 `myapp add --help`），则输出该命令的命令帮助。
3. 如果命令行中包含多级子命令（如 `myapp remote add --help`），则输出最深层的子命令帮助。

命令位置的解析复用在 `CommandRunner` 中已实现的命令前缀匹配逻辑来获取候选命令名。检测到帮助请求后，从参数列表中移除帮助写法对应的参数，用剩余参数执行命令匹配，然后根据匹配结果决定输出哪种帮助。

```
命令定位算法：
// 从原始参数中过滤掉帮助写法
var remainingArgs = FilterOutHelpSpellings(args, helpSpellings);

// 使用剩余参数匹配命令
var matchedCommand = MatchCommand(remainingArgs);

if (matchedCommand == null)
{
    // 没有任何命令匹配 → 输出根帮助
    ShowRootHelp();
}
else
{
    // 匹配到命令 → 输出该命令的帮助
    ShowCommandHelp(matchedCommand);
}
```

## 根帮助与命令帮助的内容范围

### 根帮助的内容

**根的适用范围**：当用户在命令行中只输入了帮助写法（如 `myapp --help`），或者输入了帮助写法但没有匹配到任何命令时，输出根帮助。

根帮助展示的内容：

| 内容项             | 说明                                                                 | 条件         |
| ------------------ | -------------------------------------------------------------------- | ------------ |
| 程序描述           | 顶层类型的 `Description`（如果 `Command` 特性标记了描述）            | 有默认命令时 |
| 用法行             | `usage: <程序名> [options]` 或 `usage: <程序名> <command> [options]` | 总是显示     |
| 默认命令的选项列表 | 如果存在默认命令，展示其所有选项及其 `Description`                   | 有默认命令时 |
| 已注册的命令列表   | 列出所有已注册的非默认命令及其 `Description`                         | 有命令时     |

如果程序没有默认命令，也没有注册任何命令，那么根帮助仅显示程序名和一条提示信息，指示当前没有注册任何命令。

### 命令帮助的内容

**命令的适用范围**：当用户在命令名称后输入了帮助写法（如 `myapp add --help` 或 `myapp remote add --help`），输出该命令的帮助。

命令帮助展示的内容：

| 内容项       | 说明                                                               |
| ------------ | ------------------------------------------------------------------ |
| 命令名称     | 完整的命令名称（包括所有级别的子命令，如 `remote add`）            |
| 命令描述     | `[Command]` 特性上的 `Description` 属性                            |
| 用法行       | `usage: <程序名> <command> [options]`                              |
| 选项列表     | 该命令类型中所有标记了 `[Option]` 的属性，包含短名称、长名称和描述 |
| 位置参数列表 | 该命令类型中所有标记了 `[Value]` 的属性，包含索引和描述            |
| 子命令提示   | 如果有子命令也注册到了相同的命名空间，列出子命令                   |

### 选项列表的格式化

选项列表的每条记录包含：

- 短名称（如果有多个则全部列出）：如 `-h`、`-n`
- 长名称（如果有多个则全部列出）：如 `--help`、`--name`
- 是否必需：`required` 标记的属性
- 类型提示：布尔型、数值型、字符串型等
- 描述：`[Option]` 特性上的 `Description` 属性

注意：由于 `OptionAttribute` 支持为一个选项声明多个短名称和多个长名称（如 `[Option(new[] { "n", "N" }, new[] { "name", "file-name" })]`），帮助信息必须完整呈现所有别名，而不是仅取第一个。

示例输出：

```
选项：
  -c, --count           (必需) 测试循环的次数
  -n, --test-name       测试名称
  -d, --detail-level    详细级别
      --debug           是否启用调试模式
```

## 源生成器与元数据设计

### 现有元数据机制的回顾

当前的源生成器（`ModelBuilderGenerator`）为每个命令类型生成一个 `*Builder` 类，其中包含：

- `CommandNameGroup` 静态字段：携带命令的名称信息（Ordinal 和 PascalCase 两种命名法）。
- `Metadata` 内部嵌套类：实现了 `ICommandObjectMetadata` 接口，提供 `Build(CommandRunningContext)` 方法。
- 解析相关的成员方法（`MatchLongOption`、`MatchShortOption` 等）。

当前的 `ICommandObjectMetadata` 接口：

```csharp
public interface ICommandObjectMetadata
{
    object Build(CommandRunningContext context);
}
```

### 扩展 Metadata 以携带帮助信息

为了支持帮助输出，需要在生成的 `Metadata` 上增加一个帮助元数据接口。新增的接口不同于 `ICommandObjectMetadata`，它将被专门的帮助提供者使用，而不是被命令执行流程使用。

新的接口定义（位于 `DotNetCampus.Cli.Compiler` 命名空间）：

```csharp
/// <summary>
/// 提供命令的帮助信息，由源生成器生成。
/// </summary>
public interface IHelpProvider
{
    /// <summary>
    /// 命令的名称（如 "add" 或 "remote add"）。
    /// </summary>
    string? CommandName { get; }

    /// <summary>
    /// 命令的描述。
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 获取此命令的选项帮助信息列表。
    /// </summary>
    IReadOnlyList<OptionHelpInfo> Options { get; }

    /// <summary>
    /// 获取此命令的位置参数帮助信息列表。
    /// </summary>
    IReadOnlyList<ValueHelpInfo> PositionalArguments { get; }
}

/// <summary>
/// 单个选项的帮助信息。
/// </summary>
public readonly record struct OptionHelpInfo
{
    public IReadOnlyList<string> ShortNames { get; init; }
    public IReadOnlyList<string> LongNames { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
    public OptionValueType ValueType { get; init; }
}

/// <summary>
/// 单个位置参数的帮助信息。
/// </summary>
public readonly record struct ValueHelpInfo
{
    public int Index { get; init; }
    public int? Count { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
}
```

生成的 `Metadata` 类型将同时实现 `ICommandObjectMetadata` 和 `IHelpProvider`：

```csharp
// 生成后的 Metadata 类型
public sealed class Metadata : global::DotNetCampus.Cli.Compiler.ICommandObjectMetadata,
    global::DotNetCampus.Cli.Compiler.IHelpProvider
{
    // 原有的 Build 方法
    public object Build(global::DotNetCampus.Cli.Compiler.CommandRunningContext context)
    {
        return new global::MyNamespace.MyCommandBuilder().Build(context);
    }

    // 新增的帮助接口实现
    public string? CommandName => "add";
    public string? Description => "添加一个新项目";

    public global::System.Collections.Generic.IReadOnlyList<global::DotNetCampus.Cli.Compiler.OptionHelpInfo> Options
        => [new() { ShortNames = ["n"], LongNames = ["name"], Description = "项目名称", IsRequired = true, ValueType = global::DotNetCampus.Cli.Compiler.OptionValueType.Normal }];

    public global::System.Collections.Generic.IReadOnlyList<global::DotNetCampus.Cli.Compiler.ValueHelpInfo> PositionalArguments
        => [new() { Index = 0, Description = "要添加的文件路径", IsRequired = true }];
}
```

### 源生成器的修改

在 `ModelBuilderGenerator` 中，生成 `Metadata` 类型时需要额外执行以下操作：

1. 读取 `[Command]` 特性上的 `Description` 值：从 `CommandAttribute.Description` 获取命令描述。
2. 读取每个 `[Option]` 特性上的 `Description` 值：从 `OptionAttribute.Description` 获取选项描述。
3. 读取每个 `[Value]` 特性上的 `Description` 值：从 `ValueAttribute.Description` 获取位置参数描述。
4. 根据命令名称的 kebab-case 形式和 PascalCase 形式，确定 `CommandName` 的返回值（取 Ordinal 形式）。
5. 判断选项是否为 `required`，以及值的类型。

对于命令名称、属性类型、`required` 等信息，现有的 `CommandObjectGeneratingModel` 和 `PropertyGeneratingModel` 已经提供了基础元数据；但 `Description` 目前尚未进入生成模型，因此需要先扩展这些模型，再在生成代码时将其烘焙到 `IHelpProvider` 的实现中。

关键修改点：

```
ModelBuilderGenerator.Execute 中
  └── GenerateCommandObjectCreatorCode 中
       └── 在生成 Metadata 类型时
            ├── 添加 IHelpProvider 接口声明
            ├── 生成 CommandName 属性（从 CommandNames 获取）
            ├── 生成 Description 属性（从模型中的 Description 获取）
            ├── 生成 Options 属性（遍历 OptionProperties）
            └── 生成 PositionalArguments 属性（遍历 PositionalArgumentProperties）
```

### 运行时注册

`AddHelpHandler()` 不需要通过拦截器实现——它不涉及泛型类型参数的编译期解析，可以作为普通扩展方法直接在运行时工作。调用 `AddHelpHandler()` 只是在 `CommandRunner` 上设置一个标记，表示帮助功能已启用。

`CommandRunner` 内部新增一个字段来持有帮助状态：

```csharp
// CommandRunner 新增的字段
private bool _helpEnabled;
```

帮助元数据的收集发生在 `RunAsync()` 内部：当帮助功能已启用且检测到帮助请求后，运行器才遍历所有已注册的 `ICommandObjectMetadata`，将其中实现了 `IHelpProvider` 的实例收集起来用于输出。这意味着无论 `AddHelpHandler()` 出现在调用链的什么位置——在所有 `AddHandler` 之前、之间或之后——行为都是一致的。

## 运行时执行流程

完整的帮助响应流程如下：

```
用户输入: myapp add --help

1. CommandLine.Parse(args) → 创建 CommandLine 实例
2. .AddHandler<AddOptions>(...) → 注册 AddOptions 的命令元数据
3. .AddHelpHandler() → 设置帮助启用标记
4. .Run() / .RunAsync()
   │
   ├── 4.1 CommandRunner.RunAsync()
   │   │
   │   ├── 4.2 帮助检测阶段
   │   │   ├── 检查帮助是否已启用 → 是
   │   │   ├── 遍历原始参数，匹配帮助写法
   │   │   ├── 匹配到 "--help"
   │   │   ├── 收集所有已注册 metadata 中的 IHelpProvider
   │   │   ├── 过滤掉 "--help"，剩余参数为 ["add"]
   │   │   └── 调用命令匹配，匹配到 "add" 命令
   │   │
   │   ├── 4.3 帮助输出阶段
   │   │   ├── 找到 "add" 命令对应的 IHelpProvider
   │   │   ├── 查询该命令的 Options、PositionalArguments 等
   │   │   ├── 格式化帮助文本
   │   │   ├── 输出到 Console.Out
   │   │   └── 返回退出码 0
   │   │
   │   └── 4.4 (未匹配帮助时的正常路径)
   │       └── 执行现有的 MatchCommandObject → Build → Run 流程
   │
   └── 4.5 返回 CommandRunningResult
```

对于没有命令名的情况（根帮助）：

```
用户输入: myapp --help

4.2 帮助检测 → 匹配 "--help"
4.3 过滤后剩余参数为空
    → 命令匹配返回 null（或返回默认命令的 metadata）
    → 如果存在默认命令，输出含有默认命令选项的根帮助
    → 如果不存在默认命令，输出仅含命令列表的根帮助
```

## 输出格式建议

帮助文本的格式设计遵循以下原则：

1. 保持简洁，不产生过多的控制台行数。
2. 信息的组织结构清晰可扫描。
3. 优先采用常见的 CLI 帮助格式（类 `git --help` 风格），降低用户的学习成本。

### 根帮助输出格式

```
<程序名> <版本信息（如有）>

<Description（如果有默认命令）>

用法: <程序名> [选项] [命令]
用法: <程序名> <命令> [选项]

选项：
  -h, --help             显示帮助信息

命令：
  add                    添加一个新项目
  remove                 删除一个项目
  remote add             添加一个远程仓库
```

如果没有默认命令，选项部分只显示 `-h, --help`。如果也没有注册任何命令，则显示一条提示信息。

### 命令帮助输出格式

```
<命令名> - <命令描述>

用法: <程序名> <命令> [选项] [参数]

选项：
  -n, --name <string>     (必需) 项目名称
  -p, --path <string>     项目路径
  -f, --format <string>   输出格式
  -v, --verbose           显示详细信息

位置参数：
  0                       输入文件路径  (必需)
  1..                    输出文件路径 (可选)
```

### 格式化策略

- 短名称和长名称在同一行展示，用逗号分隔。
- 选项名右对齐到固定的列宽，描述从固定的列宽开始。
- 类型提示放在尖括号中，如 `<string>`、`<int>`、`<bool>`。
- 如果选项没有短名称，对应的位置留空或对齐调整。
- 当描述文本超长时，应换行并在下一行对齐到描述起始列。
- 输出使用 `Console.Out`，而不是 `Console.Error`，因为帮助信息是正常的程序输出。

这些格式仅作为参考，具体实现允许调整间距、对齐方式和装饰字符。

## 兼容性、性能与红线

### 兼容性

- `AddHelpHandler()` 是完全新增的 API，不与现有的任何 API 冲突。
- 所有新增的接口（`IHelpProvider`、`OptionHelpInfo`、`ValueHelpInfo`）位于 `DotNetCampus.Cli.Compiler` 命名空间，与 `ICommandObjectMetadata` 同级。
- 现有的 `Metadata` 类型可以直接扩展为同时实现 `IHelpProvider`。即使用户没有调用 `AddHelpHandler()`，这些帮助元数据也只是保持未使用状态，不会改变正常的命令执行路径。
- `AddStandardHandlers()` 已被移除，不存在新旧 API 冲突问题。

### 性能

- 帮助检测仅在注册了 `AddHelpHandler()` 时才会执行额外的扫描。未注册时路径与现有代码一致，无性能损失。
- 帮助检测的扫描是 O(n) 的，其中 n 是参数个数，通常很小（个位数到十位数）。
- `IHelpProvider` 的实现由源生成器生成，不涉及反射。`Options` 和 `PositionalArguments` 属性返回的集合是编译期确定的 `new[]` 数组，分配很小。
- 帮助文本的格式化仅在检测到帮助请求时发生，不影响正常执行路径的性能。

### 红线

- **无反射**：所有帮助元数据通过源生成器在编译期确定，运行时不使用 `Type.GetProperties()`、`Attribute.GetCustomAttributes()` 等反射 API。
- **AOT 安全**：源生成器生成所有需要的代码，不存在动态代码生成或 JIT 依赖。`IHelpProvider` 的实现是具体的已编译类型，适合 NativeAOT 部署。
- **无隐式输出**：`CommandRunner` 不会自动输出帮助信息。只有在用户显式调用 `AddHelpHandler()` 后，框架才有权限向 `Console.Out` 写入内容。
- **无 fallback 依赖**：帮助检测是独立的先行步骤，不依赖 `_fallback` 机制或异常处理的回退。
- **无 `-help` 支持**：无论何种风格，`-help` 都不是内置帮助的合法写法，不提供例外处理。

### 生成策略

为了确保设计简单、稳定，并与现有的按类型生成 `Metadata` 的架构保持一致，推荐采用“总是生成，按需使用”的策略：

- 源生成器始终为所有 `Metadata` 生成 `IHelpProvider` 实现。
- 运行时仅当用户调用了 `AddHelpHandler()` 时，`CommandRunner` 才会读取这些帮助元数据。
- 如果用户没有调用 `AddHelpHandler()`，这些元数据保持未使用状态，但不会改变任何已有行为。

这样做的好处是：

1. 不需要让源生成器跨调用点分析链式调用中是否最终出现了 `AddHelpHandler()`。
2. 不需要为了减少少量未使用代码而显著提升生成器复杂度。
3. 与现有的“每个命令类型生成一个 `Metadata` 类型”的模式最一致。

## 结论

本文档提出了 DotNetCampus.CommandLine 内置帮助支持的设计方案，核心要点如下：

1. **用户选择加入**：通过 `AddHelpHandler()` 显式启用，不产生隐式输出。
2. **风格感知**：根据当前 `CommandLineStyle` 自动识别对应的帮助写法（`--help`、`-h`、`/?` 等），不同风格有不同的匹配规则。
3. **拒绝 `-help`**：该写法与短选项组合及多字符短选项规则冲突，不被支持。
4. **无 help 子命令**：子命令形式的帮助留待独立设计。
5. **先行检测**：帮助检测在 `CommandRunner.RunAsync()` 的开头执行，不影响正常命令匹配流程。
6. **源生成器驱动**：利用现有 `ModelBuilderGenerator` 的管道，扩展 `Metadata` 类型以同时实现 `IHelpProvider` 接口，携带描述信息。
7. **Description 作为唯一源**：使用 `CommandLineAttribute.Description` 属性，不涉及 XML 文档注释。
8. **无反射、AOT 安全**：所有帮助元数据在编译期确定。
9. **本地化与 XML 文档注释**：明确排除在本文档范围之外，留待后续设计。

该方案遵循 DotNetCampus.CommandLine 现有的架构风格：拦截器拦截调用、源生成器生成代码、零反射零 AOT 开销、风格矩阵驱动的解析行为。帮助支持以一种可选、可预测的方式融入到现有框架中。
