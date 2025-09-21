using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.Issues;

/// <summary>
/// 命令行参数定义的短/长名称出现冲突时，没有任何提示，编译错误和运行时异常都没有
/// https://github.com/dotnet-campus/DotNetCampus.CommandLine/issues/36
/// </summary>
[TestClass]
public class OptionNameConflictionTests
{
    public record Options
    {
        [Option('d', "data-folder")]
        public string? LogFolder { get; set; }

        // 注释以下代码，是因为解除注释能复现问题；但因为我们在 #61 修复了问题，所以会导致编译不通过。
        // [Option('c', "data-folder")]
        // public string? DataFolder { get; set; }
    }
}
