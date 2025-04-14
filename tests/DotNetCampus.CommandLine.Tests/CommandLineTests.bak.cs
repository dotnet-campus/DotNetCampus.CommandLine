using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DotNetCampus.Cli.Tests.Fakes;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MSTest.Extensions.Contracts;

using static DotNetCampus.Cli.Tests.Fakes.CommandLineArgs;

namespace DotNetCampus.Cli.Tests
{
    [TestClass]
    public partial class CommandLineTests
    {
        [ContractTestCase]
        public void ParseAs()
        {
            "命令行中没有参数，正确完成解析。".Test(() =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(NoArgs);
                var options = commandLine.As(new OptionsParser());

                // Assert
                Assert.AreEqual(null, options.FilePath);
                Assert.AreEqual(false, options.IsFromCloud);
                Assert.AreEqual(false, options.IsIwb);
                Assert.AreEqual(null, options.StartupMode);
                Assert.AreEqual(false, options.IsSilence);
                Assert.AreEqual(null, options.Placement);
                Assert.AreEqual(null, options.StartupSession);
            });

            "使用 {0} 风格的命令行，正确完成解析。".Test((string name, string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args, protocolName: UrlProtocol);
                var options = commandLine.As(new OptionsParser());

                // Assert
                Assert.AreEqual(FileValue, options.FilePath);
                Assert.AreEqual(CloudValue, options.IsFromCloud);
                Assert.AreEqual(IwbValue, options.IsIwb);
                Assert.AreEqual(ModeValue, options.StartupMode);
                Assert.AreEqual(SilenceValue, options.IsSilence);
                Assert.AreEqual(PlacementValue, options.Placement);
                Assert.AreEqual(StartupSessionValue, options.StartupSession);
            }).WithArguments(
                ("Windows", WindowsStyleArgs),
                ("Cmd", CmdStyleArgs),
                ("Cmd2", Cmd2StyleArgs),
                ("Linux", LinuxStyleArgs),
                ("Url", UrlArgs));

            "使用运行时解析器解析至可变类型，正确完成解析。".Test(() =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(WindowsStyleArgs);
                var options = commandLine.As<RuntimeOptions>();

                // Assert
                Assert.AreEqual(FileValue, options.FilePath);
                Assert.AreEqual(CloudValue, options.IsFromCloud);
                Assert.AreEqual(IwbValue, options.IsIwb);
                Assert.AreEqual(ModeValue, options.StartupMode);
                Assert.AreEqual(SilenceValue, options.IsSilence);
                Assert.AreEqual(PlacementValue, options.Placement);
                Assert.AreEqual(StartupSessionValue, options.StartupSession);
            });

            "使用运行时解析器解析至不可变类型，正确完成解析。".Test(() =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(WindowsStyleArgs);
                var options = commandLine.As<RuntimeImmutableOptions>();

                // Assert
                Assert.AreEqual(FileValue, options.FilePath);
                Assert.AreEqual(CloudValue, options.IsFromCloud);
                Assert.AreEqual(IwbValue, options.IsIwb);
                Assert.AreEqual(ModeValue, options.StartupMode);
                Assert.AreEqual(SilenceValue, options.IsSilence);
                Assert.AreEqual(PlacementValue, options.Placement);
                Assert.AreEqual(StartupSessionValue, options.StartupSession);
            });
        }

        [ContractTestCase]
        public void ParseToPrimary()
        {
            "命令行传入数值，可以解析为数值类型。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<PrimaryOptions>();

                // Assert
                Assert.AreEqual((byte)1, options.Aaa);
                Assert.AreEqual((short)2, options.Bbb);
                Assert.AreEqual((ushort)3, options.Ccc);
                Assert.AreEqual((int)4, options.Ddd);
                Assert.AreEqual((uint)5, options.Eee);
                Assert.AreEqual((long)6, options.Fff);
                Assert.AreEqual((ulong)7, options.Ggg);
                Assert.AreEqual((float)8, options.Hhh);
                Assert.AreEqual((double)9, options.Iii);
                Assert.AreEqual((decimal)10, options.Jjj);
            }).WithArguments(
                new[] { "-a", "1", "-b", "2", "-c", "3", "-d", "4", "-e", "5", "-f", "6", "-g", "7", "-h", "8", "-i", "9", "-j", "10" },
                new[] { "-a", "1", "-b", "2", "-c", "3", "-d", "4", "-e", "5", "-f", "6", "-g", "7", "-h", "8.0", "-i", "9.0", "-j", "10.0" }
                );
        }

        [ContractTestCase]
        public void ParseToIO()
        {
            "命令行传入文件路径，可以解析为文件路径类型。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<IOOptions>();

                // Assert
                Assert.AreEqual(Path.Combine(Directory.GetCurrentDirectory(), "a.txt"), options.File!.FullName);
                Assert.AreEqual(Path.Combine(Directory.GetCurrentDirectory(), "b"), options.Directory!.FullName);
            }).WithArguments(
                new[] { "-f", "a.txt", "-d", "b" },
                new[] { "-f", "   a.txt   ", "-d", "   b  " }
                );
        }

        [ContractTestCase]
        public void ParseToDictionary()
        {
            "命令行传入字典（一项），能接收到字典的所有值。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<DictionaryOptions>();

                // Assert
                Assert.AreEqual("1", options.Aaa!["a"]);
                Assert.AreEqual("1", options.Bbb!["a"]);
                Assert.AreEqual("1", options.Ccc!["a"]);
                Assert.AreEqual("a", options.Ddd.Key);
                Assert.AreEqual("1", options.Ddd.Value);
            }).WithArguments(
                new[] { "-a", "a=1", "-b", "a=1", "-c", "a=1", "-d", "a=1" },
                new[] { "-a:a=1", "-b:a=1", "-c:a=1", "-d:a=1" }
                );

            "命令行传入字典（三项），能接收到字典的所有值。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<DictionaryOptions>();

                // Assert
                Assert.AreEqual("1", options.Aaa!["a"]);
                Assert.AreEqual("2", options.Aaa["b"]);
                Assert.AreEqual("3", options.Aaa["c"]);
                Assert.AreEqual("1", options.Bbb!["a"]);
                Assert.AreEqual("2", options.Bbb["b"]);
                Assert.AreEqual("3", options.Bbb["c"]);
                Assert.AreEqual("1", options.Ccc!["a"]);
                Assert.AreEqual("2", options.Ccc["b"]);
                Assert.AreEqual("3", options.Ccc["c"]);
            }).WithArguments(
                new[] { "-a", "a=1;b=2;c=3", "-b", "a=1;b=2;c=3", "-c", "a=1;b=2;c=3" },
                new[] { "-a:a=1;b=2;c=3", "-b:a=1;b=2;c=3", "-c:a=1;b=2;c=3" }
                );

            "命令行传入字典，能正确处理参数中的空格。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<DictionaryOptions>();

                // Assert
                Assert.AreEqual("1", options.Aaa!["a"]);
                Assert.AreEqual("1  1", options.Bbb!["a"]);
                Assert.AreEqual("1", options.Ccc!["a"]);
                Assert.AreEqual("a", options.Ddd.Key);
                Assert.AreEqual("1", options.Ddd.Value);
            }).WithArguments(
                new[] { "-a", "a = 1", "-b", "a=1  1", "-c", "  a=1  ", "-d", "a  =1" },
                new[] { "-a:a = 1", "-b:a=1  1", "-c:  a=1  ", "-d:a  =1" }
                );
        }

        [ContractTestCase]
        public void ParseAsAmbiguously()
        {
            "命令行传入开关参数，或者传入带有 true/false 值的参数，可以赋值给 bool 类型。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As(new AmbiguousOptionsParser());

                // Assert
                Assert.AreEqual(true, options.Boolean);
            }).WithArguments(
                new[] { "--boolean" },
                new[] { "--boolean", "true" }
                );

            "命令行传入带有 true/false 值的参数，可以赋值给 string 类型。".Test(() =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(new[] { "--string-boolean", "true" });
                var options = commandLine.As(new AmbiguousOptionsParser());

                // Assert
                Assert.AreEqual("true", options.StringBoolean);
            });

            "命令行传入带有多个值的参数，可以赋值给 string 类型。".Test(() =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(new[] { "--string-array", "a", "b" });
                var options = commandLine.As(new AmbiguousOptionsParser());

                // Assert
                Assert.AreEqual("a b", options.StringArray);
            });

            "命令行传入带有多个值的参数，可以赋值给 string 集合类型。".Test(() =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(new[]
                {
                    "--array", "a", "b",
                    "--list", "a", "b",
                    "--read-only-list", "a", "b",
                    "--enumerable", "a", "b",
                });
                var options = commandLine.As<CollectionOptions>();

                // Assert
                CollectionAssert.AreEqual(new[] { "a", "b" }, options.Array);
                CollectionAssert.AreEqual(new[] { "a", "b" }, options.List.ToArray());
                CollectionAssert.AreEqual(new[] { "a", "b" }, options.ReadOnlyList.ToArray());
                CollectionAssert.AreEqual(new[] { "a", "b" }, options.Enumerable.ToArray());
            });

            "命令行传入带有多个值的参数，可以赋值给未内置的 string 集合类型。".Test(() =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(new[]
                {
                    "--collection", "a", "b",
                });
                var options = commandLine.As<CollectionOptions>();

                // Assert
                CollectionAssert.AreEqual(new[] { "a", "b" }, options.Collection.ToArray());
            });
        }

        [ContractTestCase]
        public void Handle()
        {
            const string expectedFilePath = @"C:\Users\lvyi\Test.txt";

            "处理带有谓词的命令行参数，可以正确根据谓词选择处理函数。".Test((string[] args, int expectedExitCode) =>
            {
                // Arrange
                var commandLine = CommandLine.Parse(args);

                // Action
                var exitCode = commandLine
                    .AddHandler<EditOptions>(options => 0)
                    .AddHandler<PrintOptions>(options => 1)
                    .Run();

                // Assert
                Assert.AreEqual(expectedExitCode, exitCode);
            }).WithArguments(
                // 不区分大小写。
                (new[] { "Edit", expectedFilePath }, 0),
                (new[] { "edit", expectedFilePath }, 0),
                (new[] { "Print", expectedFilePath }, 1));

            "处理带有谓词的命令行参数，可以正确解析出含谓词的命令行参数。".Test((string[] args) =>
            {
                // Arrange
                var commandLine = CommandLine.Parse(args);

                // Action
                string? filePath = null;
                commandLine
                    .AddHandler<EditOptions>(options => filePath = options.FilePath)
                    .AddHandler<PrintOptions>(options => filePath = options.FilePath)
                    .AddHandler<ShareOptions>(options => { })
                    .Run();

                // Assert
                Assert.AreEqual(expectedFilePath, filePath);
            }).WithArguments(
                // 不区分大小写。
                new[] { "Edit", expectedFilePath },
                new[] { "Print", expectedFilePath });

            "处理带有默认谓词的命令行参数，可以在没有谓词的情况下解析。".Test((string[] args) =>
            {
                // Arrange
                var commandLine = CommandLine.Parse(args);

                // Action
                string? filePath = null;
                commandLine
                    .AddHandler<Options>(options => filePath = options.FilePath)
                    .AddHandler<PrintOptions>(options => filePath = options.FilePath)
                    .Run();

                // Assert
                Assert.AreEqual(expectedFilePath, filePath);
            }).WithArguments(
                // 不区分大小写。
                new[] { expectedFilePath },
                new[] { "Print", expectedFilePath });
        }

        [ContractTestCase]
        public void HandleAsync()
        {
            const string expectedFilePath = @"C:\Users\lvyi\Test.txt";

            "处理带有谓词的命令行参数，可以正确根据谓词选择处理函数。".Test(async (string[] args, int expectedExitCode) =>
            {
                // Arrange
                var commandLine = CommandLine.Parse(args);

                // Action
                var exitCode = await commandLine
                    .AddHandler<EditOptions>(async options =>
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        return 1;
                    })
                    .AddHandler<PrintOptions>(async options =>
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        return 2;
                    })
                    .RunAsync().ConfigureAwait(false);

                // Assert
                Assert.AreEqual(expectedExitCode, exitCode);
            }).WithArguments(
                // 不区分大小写。
                (new[] { "Edit", expectedFilePath }, 1),
                (new[] { "edit", expectedFilePath }, 1),
                (new[] { "Print", expectedFilePath }, 2));

            "处理带有谓词的命令行参数，可以正确解析出含谓词的命令行参数。".Test(async (string[] args) =>
            {
                // Arrange
                var commandLine = CommandLine.Parse(args);

                // Action
                string? filePath = null;
                await commandLine
                    .AddHandler<EditOptions>(async options =>
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        filePath = options.FilePath;
                    })
                    .AddHandler<PrintOptions>(async options =>
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        filePath = options.FilePath;
                    })
                    .AddHandler<ShareOptions>(
#pragma warning disable 1998
                    async options => { }
#pragma warning restore 1998
                    )
                    .RunAsync().ConfigureAwait(false);

                // Assert
                Assert.AreEqual(expectedFilePath, filePath);
            }).WithArguments(
                // 不区分大小写。
                new[] { "Edit", expectedFilePath },
                new[] { "Print", expectedFilePath });

            "处理带有默认谓词的命令行参数，可以在没有谓词的情况下解析。".Test(async (string[] args) =>
            {
                // Arrange
                var commandLine = CommandLine.Parse(args);

                // Action
                string? filePath = null;
                await commandLine
                    .AddHandler<Options>(async options =>
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        filePath = options.FilePath;
                    })
                    .AddHandler<PrintOptions>(async options =>
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        filePath = options.FilePath;
                    })
                    .RunAsync().ConfigureAwait(false);

                // Assert
                Assert.AreEqual(expectedFilePath, filePath);
            }).WithArguments(
                // 不区分大小写。
                new[] { expectedFilePath },
                new[] { "Print", expectedFilePath });
        }
    }
}
