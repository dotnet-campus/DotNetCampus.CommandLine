using System;

namespace dotnetCampus.Cli.Standard
{
    /// <summary>
    /// 命令行参数的来源。指定命令行参数的来源助于更准确地分析命令行的选项、参数和帮助输出。
    /// </summary>
    internal enum CommandLineArgsSource
    {
        /// <summary>
        /// 无法得知此命令行参数的来源。
        /// 目前版本会按照 <see cref="MainArgs"/> 来处理，后续版本可能可以自动检测。
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 命令行参数来源于 Main 函数的 args 参数。
        /// </summary>
        MainArgs = 0,

        /// <summary>
        /// 命令行参数来源于 <see cref="Environment.GetCommandLineArgs"/> 方法。等同于 <see cref="WithEntryPointName"/>。
        /// <para>例如：</para>
        /// <list type="bullet">
        /// <item>命令行 <code>foo --option</code> <see cref="MainArgs"/> 版本是 <code>--option</code> <see cref="EnvironmentGetCommandLineArgs"/> 版本是 <code>D:\foo.exe --option</code></item>
        /// <item>命令行 <code>dotnet foo --option</code> <see cref="MainArgs"/> 版本是 <code>--option</code> <see cref="EnvironmentGetCommandLineArgs"/> 版本是 <code>D:\foo.dll --option</code></item>
        /// </list>
        /// </summary>
        EnvironmentGetCommandLineArgs = 1,

        /// <summary>
        /// 命令行参数来源于本机方法（绕过了 .NET 运行时），这种参数列表会忽略 .NET 对命令行参数做的任何修改。
        /// <para>例如：</para>
        /// <list type="bullet">
        /// <item>命令行 <code>dotnet foo --option</code> <see cref="EnvironmentGetCommandLineArgs"/> 版本是 <code>D:\foo.dll --option</code> <see cref="Native"/> 版本是 <code>D:\dotnet.exe foo --option</code></item>
        /// </list>
        /// </summary>
        Native = 2,

        /// <summary>
        /// 命令行参数不包含入口点。同 <see cref="MainArgs"/>。
        /// </summary>
        WithoutEntryPointName = 0,

        /// <summary>
        /// 命令行参数包含入口点。同 <see cref="EnvironmentGetCommandLineArgs"/>。
        /// </summary>
        WithEntryPointName = 1,

        /// <summary>
        /// 命令行参数包含入口点和启动器，即进程的命令行参数。
        /// </summary>
        WithLauncherAndExecutableName = 2,

        /// <summary>
        /// 命令行参数来源于其他进程，但特别去掉了进程名部分。
        /// </summary>
        FromOtherProcessWithoutProcessName = 17,

        /// <summary>
        /// 命令行参数来源于其他进程。
        /// </summary>
        FromOtherProcess = 18,
    }
}
