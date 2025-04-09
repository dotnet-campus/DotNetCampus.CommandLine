using System;
using System.IO;
using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Properties;

namespace dotnetCampus.Cli
{
    internal class DefaultOptions
    {
        [Option(LocalizableDescription = nameof(LocalizableStrings.SamplePropertyDescription))]
        public string? DefaultText { get; set; }

        [Option(LocalizableDescription = nameof(LocalizableStrings.SampleDirectoryPropertyDescription))]
        public DirectoryInfo? DefaultDirectory { get; set; }

        internal void Run()
        {
            Console.WriteLine("默认行为执行……");
        }
    }
}
