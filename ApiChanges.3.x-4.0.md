# 重大更新

## 3.x -> 4.0

DotNetCampus.CommandLine 4.0版本带来了全面的架构升级和功能增强，使命令行参数解析更加高效、灵活且易于使用。

### 主要更新内容

#### 1. 全面采用源代码生成器

- 完全使用源生成器技术，替代以前的手写解析器（`XxxParser`）和反射解析器（`RuntimeParser`）
- 相比上个版本，性能出现了轻微下降（换来了各种命令行风格的完全支持），好在保持在同一个数量级内，仍然比同类库快得多
- 充分支持 AOT 编译，更适合现代 .NET 应用场景

#### 2. 全面增强的命令行风格支持

- 从原来支持 Linux/GNU、CMD、PowerShell 三种风格的有限子集，升级为全面支持五种完整命令行风格：
  - `CommandLineStyle.Flexible`（默认）：智能识别多种风格
  - `CommandLineStyle.GNU`：符合 GNU 规范的风格
  - `CommandLineStyle.POSIX`：符合 POSIX 规范的风格
  - `CommandLineStyle.DotNet`：.NET CLI 风格
  - `CommandLineStyle.PowerShell`：PowerShell 风格
- 全面支持各种命令行选项格式：
  - 长选项：`--option value`、`--option=value`、`--option:value`
  - 短选项：`-o value`、`-o=value`、`-o:value` 
  - Windows风格：`/option value`、`/option:value`
  - 布尔选项：`--flag`（无值时自动设为true）、`--flag:true/false`
  - URL协议风格：`protocol://command/subcommand?param1=value&param2=value`

#### 3. 增强的类型系统支持

- 支持 C# 8.0+ 的 `init` 和 C# 11.0 的 `required` 关键字，支持不可变对象
- 全面支持更多复杂数据类型：
  - 集合类型：数组、列表、只读集合、不可变集合
  - 字典类型：支持多种传入方式和分隔符
  - 枚举类型：支持枚举名称和数值
- 增强类型转换能力，提供更友好的类型错误提示

#### 4. 改进的命令处理器模式

- `AddHandler<T>` 现在支持实现 `ICommandHandler` 接口的类型，可以在类型内部编写处理逻辑
- 保留了通过委托方式传入处理逻辑的经典用法

### 破坏性变更和升级指南

1. **命名空间变更**：`OptionAttribute`、`ValueAttribute`、`VerbAttribute` 的命名空间发生了变化。升级库后，您可能需要借助IDE来修正相关引用。

2. **参数解析选项变更**：`CommandLine.Parse(args, xxx)` 的第二个参数已从简单的URL scheme字符串升级为完整的 `CommandLineParsingOptions` 对象：
```csharp
// 旧版本
var commandLine = CommandLine.Parse(args, "myapp");

// 新版本
var commandLine = CommandLine.Parse(args, new CommandLineParsingOptions { SchemeNames = ["myapp"] });
// 或者使用预定义样式
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

3. **命名约定变更**：选项命名规则从 `PascalCase` 改为推荐使用 `kebab-case`：
```csharp
// 旧版本（依然支持，但不再推荐）
[Option("OutputFile")]

// 新版本（推荐）
[Option("output-file")]
```
原因请见：[DCL101.md](/docs/analyzers/DCL101.md)
   
4. **标准处理器移除**：删除了 `AddStandardHandlers` 方法（用于自动处理 `--help` 和 `--version`）。这个功能在未来版本可能会以新形式回归，如有需要请暂时自行实现。

5. **过滤器机制移除**：删除了很少使用的 `CommandLineFilter` 机制，该机制主要为 `AddStandardHandlers` 提供支持。

### 保留的功能

本次升级范围很大，不过仍然尽量在更规范更强大的设计中保持了跟上个版本 API 的一致性。你原来的项目通常只进行命名空间的修改即可正常工作；调整选项的命名规则后可消除新引入的警告（我们提供了代码修改器辅助你自动采用新的命名规则）。

1. **多种命令行风格解析**：保留并增强了对多种命令行参数风格的支持
2. **URL协议支持**：完整保留并改进了对URL协议格式命令行的解析
3. **位置参数支持**：保留了对位置参数的支持，并增强了位置参数的处理能力
4. **谓词（命令）支持**：保留并增强了对命令谓词的支持（如 `git commit`、`git push` 等形式）
