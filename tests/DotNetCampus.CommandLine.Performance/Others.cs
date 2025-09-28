// using System.CommandLine;
// using System.IO;
// using BenchmarkDotNet.Attributes;
// using dotnetCampus.Cli;
// using Microsoft.Extensions.Options;
// using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;
// using static DotNetCampus.Cli.CommandLineParsingOptions;
//
// namespace DotNetCampus.Cli.Performance;
//
// // [MemoryDiagnoser]
// // [BenchmarkCategory("Parse No Args")]
// public class Others
// {
//     [Benchmark(Description = "handle [Edit,Print] --flexible")]
//     public void Handle_Verbs_Flexible()
//     {
//         CommandLine.Parse(EditVerbArgs)
//             .AddHandler<EditOptions>(options => 0)
//             .AddHandler<PrintOptions>(options => 0)
//             .Run();
//     }
//
//     [Benchmark(Description = "handle [Edit,Print] --dotnet")]
//     public void Handle_Verbs_DotNet()
//     {
//         CommandLine.Parse(EditVerbArgs)
//             .AddHandler<EditOptions>(options => 0)
//             .AddHandler<PrintOptions>(options => 0)
//             .Run();
//     }
//
//     [Benchmark(Description = "handle [Edit,Print] -v=3.x -p=parser")]
//     public void Handle_Verbs_Parser()
//     {
//         var commandLine = dotnetCampus.Cli.CommandLine.Parse(EditVerbArgs);
//         commandLine
//             .AddHandler(options => 0, new SelfWrittenEditOptionsParser())
//             .AddHandler(options => 0, new SelfWrittenPrintOptionsParser())
//             .Run();
//     }
//
//     [Benchmark(Description = "handle [Edit,Print] -v=3.x -p=runtime")]
//     public void Handle_Verbs_Runtime()
//     {
//         var commandLine = dotnetCampus.Cli.CommandLine.Parse(EditVerbArgs);
//         commandLine
//             .AddHandler<EditOptions>(options => 0)
//             .AddHandler<PrintOptions>(options => 0)
//             .Run();
//     }
//
//     [Benchmark(Description = "parse  [URL]")]
//     public void Parse_Url()
//     {
//         var commandLine = CommandLine.Parse(UrlArgs, new CommandLineParsingOptions { SchemeNames = ["walterlv"] });
//         commandLine.As<Options>();
//     }
//
//     [Benchmark(Description = "parse  [URL] -v=3.x -p=parser")]
//     public void Parse_Url_3x_Parser()
//     {
//         var commandLine = dotnetCampus.Cli.CommandLine.Parse(UrlArgs);
//         commandLine.As(new OptionsParser());
//     }
//
//     [Benchmark(Description = "parse  [URL] -v=3.x -p=runtime")]
//     public void Parse_Url_3x_Runtime()
//     {
//         var commandLine = dotnetCampus.Cli.CommandLine.Parse(UrlArgs);
//         commandLine.As<Options>();
//     }
//
//     [Benchmark(Description = "NuGet: CommandLineParser")]
//     public void CommandLineParser()
//     {
//         Parser.Default.ParseArguments<ComparedOptions>(GnuStyleArgs).WithParsed(options => { });
//     }
//
//     [Benchmark(Description = "NuGet: System.CommandLine")]
//     public void SystemCommandLine()
//     {
//         var fileOption = new System.CommandLine.Option<FileInfo?>(
//             name: "--file",
//             description: "The file to read and display on the console.");
//
//         var rootCommand = new RootCommand("Benchmark for System.CommandLine");
//         rootCommand.AddOption(fileOption);
//         rootCommand.SetHandler(file => { }, fileOption);
//
//         rootCommand.Invoke(GnuStyleArgs);
//     }
// }
