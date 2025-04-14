using System.Collections;

using DotNetCampus.Cli.Tests.Fakes;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MSTest.Extensions.Contracts;

namespace DotNetCampus.Cli.Tests
{
    public partial class CommandLineTests
    {
        [ContractTestCase]
        public void ParseValues()
        {
            "命令行中包含 --，那么 -- 后的字符串完全属于值。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<ValueOptions>();

                // Assert
                Assert.AreEqual("foo", options.Foo);
                Assert.AreEqual(8, options.LongValue);
                CollectionAssert.AreEqual(new[] { "x", "y" }, (ICollection?)options.Values);
                Assert.AreEqual(2, options.Int32Value);
            }).WithArguments(
                new[] { "8", "x", "y", "2", "-f", "foo" },
                new[] { "-f", "foo", "--", "8", "x", "y", "2" }
            );

            "命令行中包含 --，那么 -- 后的字符串完全属于值，即使后面包含 -。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<ValueOptions>();

                // Assert
                Assert.AreEqual("foo", options.Foo);
                Assert.AreEqual(-8, options.LongValue);
                CollectionAssert.AreEqual(new[] { "-x", "-y" }, (ICollection?)options.Values);
                Assert.AreEqual(-2, options.Int32Value);
            }).WithArguments(
                new[] { "-f", "foo", "--", "-8", "-x", "-y", "-2" }
            );

            "命令行中包含 --，那么 -- 后的字符串完全属于值，且完全赋值。".Test((string[] args) =>
            {
                // Arrange & Action
                var commandLine = CommandLine.Parse(args);
                var options = commandLine.As<UnlimitedValueOptions>();

                // Assert
                Assert.AreEqual("foo", options.Section);
                Assert.AreEqual(8, options.Count);
                CollectionAssert.AreEqual(new[] { "dcl.exe", "--foo", "xyz", "-s", "some", "2" }, (ICollection?)options.Args);
            }).WithArguments(
                new[] { "-s", "foo", "--", "8", "dcl.exe", "--foo", "xyz", "-s", "some", "2" }
            );
        }
    }
}
