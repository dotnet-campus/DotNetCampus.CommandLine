namespace dotnetCampus.Cli;

/// <summary>
/// 命令行参数的风格规范。
/// 不同的命令行工具可能使用不同的参数风格，本枚举定义了常见的几种命令行参数风格。
/// </summary>
public enum CommandLineStyle
{
    /// <summary>
    /// 灵活风格。<br/>
    /// 根据实际传入的参数，自动识别并支持多种主流风格，包括 <see cref="GNU"/>、<see cref="DotNet"/>、<see cref="PowerShell"/> 等风格。
    /// 适用于希望为用户提供更灵活的参数传递体验的工具。
    /// </summary>
    Flexible,

    /// <summary>
    /// GNU风格，支持长选项和短选项：<br/>
    /// 1. 双破折线(--) + 长选项名称，通过等号(=)或空格赋值<br/>
    /// 2. 单破折线(-) + 短选项字符，可以空格赋值，也可以紧跟参数值<br/>
    /// 3. 同时支持多个单字符选项合并（如-abc 表示 -a -b -c）
    /// </summary>
    /// <remarks>
    /// GNU风格是现代命令行工具中最广泛采用的标准之一，包括大多数Linux工具和跨平台应用程序。<br/>
    /// <br/>
    /// 详细规则：<br/>
    /// 1. 长选项以双破折线(--)开头，后跟由字母、数字、连字符组成的选项名<br/>
    /// 2. 长选项参数可以用等号(=)连接或用空格分隔<br/>
    /// 3. 短选项以单破折线(-)开头，后跟单个字符<br/>
    /// 4. 短选项参数可以直接跟在选项字符后，无需空格<br/>
    /// 5. 短选项也可以用空格分隔参数，或用等号连接参数<br/>
    /// 6. 多个不需要参数的短选项可以合并（如 -abc 等同于 -a -b -c）<br/>
    /// 7. 合并的短选项中，最后一个短选项可以带参数（如 -abc value 中 -c 接收 value 参数）<br/>
    /// 8. 双破折号(--) 作为单独参数时表示选项结束标记，之后的所有内容都被视为位置参数<br/>
    ///
    /// <code>
    /// # 长选项示例
    /// app --option=value     # 长选项用等号赋值
    /// app --option value     # 长选项用空格赋值
    /// app --enable-feature   # 布尔类型长选项（不需要值）
    /// app --no-color         # 否定形式的布尔长选项
    ///
    /// # 短选项示例
    /// app -o=value           # 短选项用等号赋值
    /// app -o value           # 短选项用空格赋值
    /// app -ovalue            # 短选项直接跟参数值（无空格）
    /// app -abc               # 多个布尔短选项合并（等同于 -a -b -c）
    /// app -abc value         # 合并短选项，其中 c 接收参数值
    ///
    /// # 混合使用
    /// app value1 value2 --option value -f  # 位置参数 + 长选项 + 短选项
    /// app --option value -- -value1 --value2  # -- 后的 -value1 和 --value2 被视为位置参数
    /// </code>
    /// </remarks>
    GNU,

    /// <summary>
    /// POSIX/UNIX风格，类似GNU但更严格：<br/>
    /// 1. 支持 - 开头的短选项，单个字符<br/>
    /// 2. 短选项可以组合使用（-abc 表示 -a -b -c）<br/>
    /// 3. 需要参数的选项必须与参数分开或使用特定格式
    /// </summary>
    /// <remarks>
    /// POSIX风格是UNIX系统中规范的命令行参数格式，相比GNU风格更加严格和精简，许多传统UNIX工具遵循此规范。<br/>
    /// <br/>
    /// 详细规则：<br/>
    /// 1. 只支持短选项，以单破折线(-)开头，后跟单个字母<br/>
    /// 2. 短选项参数必须用空格与选项分隔（标准做法）<br/>
    /// 3. 不需要参数的短选项（布尔选项）可以组合在一起（如 -abc 等同于 -a -b -c）<br/>
    /// 4. 在组合的短选项中，通常不支持为最后一个选项提供参数（这点与GNU不同）<br/>
    /// 5. 标准POSIX不支持长选项（以--开头）<br/>
    /// 6. 有些遵循POSIX的工具允许用破折线后跟操作数而不是选项（如 -42 表示数字42）<br/>
    /// 7. 双破折号(--)作为选项终止符，之后的参数被当作操作数而非选项<br/>
    ///
    /// <code>
    /// # 标准短选项
    /// app -o value           # 短选项用空格赋值
    /// app -a                 # 布尔短选项
    /// app -abc               # 多个布尔短选项合并（等同于 -a -b -c）
    ///
    /// # 选项结束标记
    /// app -a -- -b file.txt  # -- 后的 -b 被视为文件名而非选项
    /// app -a -b -- -c        # -a 和 -b 是选项，-c 是参数
    ///
    /// # 位置参数
    /// app file1.txt -a file2.txt  # file1.txt 和 file2.txt 是位置参数
    /// </code>
    /// </remarks>
    POSIX,

    /// <summary>
    /// .NET CLI风格，使用冒号分隔参数：<br/>
    /// 1. 短选项形式为 -参数:值<br/>
    /// 2. 长选项可以是 --参数:值<br/>
    /// 3. 也支持斜杠前缀 /参数:值
    /// </summary>
    /// <remarks>
    /// 这种风格在现代.NET工具链（dotnet CLI、NuGet、MSBuild等）和其他Microsoft工具中广泛使用。<br/>
    /// <br/>
    /// 详细规则：<br/>
    /// 1. 支持使用冒号(:)作为选项和参数值的分隔符<br/>
    /// 2. 短选项以单破折线(-)开头，后跟选项名，然后是冒号和参数值<br/>
    /// 3. 长选项以双破折线(--)开头，后跟选项名，然后是冒号和参数值<br/>
    /// 4. 也支持使用斜杠(/)作为选项前缀，特别是在Windows环境中<br/>
    /// 5. 参数名通常是单个字母、缩写或完整的驼峰式单词<br/>
    /// 6. 布尔选项通常不需要值，或使用true/false、on/off等值<br/>
    /// 7. 多个短选项一般不支持合并（与GNU/POSIX不同）<br/>
    /// 8. 某些.NET工具也接受等号(=)作为选项和值的分隔符<br/>
    ///
    /// <code>
    /// # 短选项示例
    /// dotnet build -c:Release           # 短选项冒号语法
    /// dotnet test -t:UnitTest           # 短选项指定测试类别
    /// dotnet publish -o:./publish       # 指定输出目录
    ///
    /// # 长选项示例
    /// dotnet build --verbosity:minimal  # 长选项冒号语法
    /// dotnet run --project:App1         # 指定项目
    /// msbuild --target:Rebuild          # MSBuild长选项
    ///
    /// # 斜杠前缀(Windows风格)
    /// msbuild /p:Configuration=Release  # MSBuild属性
    /// dotnet test /blame                # 启用故障分析
    /// dotnet nuget push /source:feed    # 指定源
    ///
    /// # 布尔选项
    /// dotnet build -m:1                 # 最大并行度
    /// dotnet test --blame               # 不带值的布尔选项
    /// dotnet build --no-restore         # 否定形式的布尔选项
    ///
    /// # 混合用法
    /// dotnet publish -c:Release --no-build -o:./bin
    /// </code>
    /// </remarks>
    DotNet,

    /// <summary>
    /// PowerShell风格，使用 - 开头，但参数名称通常是完整单词或驼峰形式：<br/>
    /// 1. 长参数形式为 -参数名 值<br/>
    /// 2. 支持不带值的开关参数（开关参数）<br/>
    /// 3. 支持参数名称缩写
    /// </summary>
    /// <remarks>
    /// PowerShell命令行风格在微软的PowerShell脚本语言和相关工具中使用，具有独特的参数处理方式。<br/>
    /// <br/>
    /// 详细规则：<br/>
    /// 1. 参数名称前使用单个破折线(-)，后跟完整的参数名（通常是Pascal或Camel大小写）<br/>
    /// 2. 参数名称与值之间用空格分隔<br/>
    /// 3. 支持参数名称的部分匹配和自动补全（只要能唯一标识参数）<br/>
    /// 4. 支持位置参数（根据位置而非参数名赋值）<br/>
    /// 5. 布尔开关参数不需要显式值（存在即为true）<br/>
    /// 6. 可以使用冒号语法传递数组或哈希表值<br/>
    /// 7. 不支持GNU/POSIX风格的短选项合并<br/>
    /// 8. 支持使用双引号或单引号包围包含空格的参数值<br/>
    /// 9. 支持参数别名（一个参数可以有多个名称）<br/>
    ///
    /// <code>
    /// # 基本参数用法
    /// Get-Process -Name chrome           # 带值的标准参数
    /// New-Item -Path "C:\temp" -ItemType Directory  # 多个参数
    ///
    /// # 开关参数（布尔参数）
    /// Remove-Item -Recurse -Force        # 两个开关参数（无需值）
    /// Copy-Item file.txt backup/ -Verbose  # 启用详细输出
    ///
    /// # 参数名称缩写
    /// Get-Process -n chrome              # -n 是 -Name 的缩写
    /// Get-ChildItem -Recurse -Fo *.txt   # -Fo 是 -Force 的缩写（只要能唯一识别）
    ///
    /// # 位置参数（无需指定参数名）
    /// Get-Process chrome                 # 位置参数，等同于 -Name chrome
    ///
    /// # 数组参数
    /// Get-Process -Name chrome,firefox,edge  # 逗号分隔的数组
    /// Get-Process -ComputerName "srv1","srv2"  # 引号包围的数组元素
    ///
    /// # 复杂值和高级用法
    /// New-Object -TypeName PSObject -Property @{Name="Value"; Count=1}  # 哈希表参数
    /// Invoke-Command -ScriptBlock { Get-Process } -ComputerName Server01  # 脚本块参数
    /// </code>
    /// </remarks>
    PowerShell,
}
