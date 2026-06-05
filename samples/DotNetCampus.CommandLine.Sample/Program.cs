using DotNetCampus.Cli.Legacy;
using DotNetCampus.Cli.Properties;

namespace DotNetCampus.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        var appState = new AppState
        {
            AppName = "DotNetCampus.CommandLine.Sample",
        };

        await CommandLine.Parse(args, CommandLineParsingOptions.Flexible)
            .AddHandler<DefaultOptions>(o => o.Run())
            .AddHandler<ConvertHandler>()
            .AddHandler<EditHandler>()
            .AddHandler<BenchmarkHandler>()
            .AddHelpHandler(new HelpConfigurations
            {
                HelpTextLocalizer = key => LocalizableStrings.ResourceManager.GetString(key) ?? key,
            })
            .ForState(appState).AddHandler<UrlOpenHandler>()
            .ForState()
            .RunAsync();
    }
}
