using System.Runtime.CompilerServices;
using ConsoleAppFramework;

namespace DotNetCampus.Cli.Performance.Fakes;

public class BenchmarkOptionsConsoleAppFramework
{
    /// <summary>
    /// 性能测试的命令行参数
    /// </summary>
    /// <param name="debug">表示是否开启调试模式</param>
    /// <param name="testCount">-c, 表示测试的次数</param>
    /// <param name="testName">-n, 表示测试的名称</param>
    /// <param name="testCategories">表示测试的类别</param>
    /// <param name="detailLevel">-d, 表示测试的详细等级</param>
    /// <param name="testItems">要测试的项目，可以是多个</param>
    [Command("")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Root(
        [Argument] string[] testItems,
        bool debug, int testCount, string? testName,
        DetailLevel detailLevel, string[]? testCategories = null)
    {
    }
}
