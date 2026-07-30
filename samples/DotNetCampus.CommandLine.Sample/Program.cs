using DotNetCampus.Cli;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Legacy;
using DotNetCampus.Cli.Properties;

var appState = new AppState
{
    AppName = "DotNetCampus.CommandLine.Sample",
};

await CommandLine.Parse(args, CommandLineParsingOptions.Flexible)
    .AddHandler<DefaultOptions>(o => o.Run())
    .AddHandler<ConvertHandler>()
    .AddHandler<EditHandler>()
    .AddHandler<TestHandler>()
    .AddHandler<BenchmarkHandler>()
    .AddHelpHandler(new HelpConfigurations
    {
        HelpTextLocalizer = key => LocalizableStrings.ResourceManager.GetString(key) ?? key,
    })
    .ForState(appState).AddHandler<UrlOpenHandler>()
    .ForState()
    .RunAsync();


[Command("test")]
internal record TestHandler : ICommandHandler
{
    [Option("test")]
    public int? Count { get; init; }

    public Task<int> RunAsync()
    {
        return Task.FromResult(0);
    }
}
