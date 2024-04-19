using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Resources;

using dotnetCampus.Cli.Core;

namespace dotnetCampus.Cli.Standard
{
    [Filter]
    internal partial class GnuFilter : CommandLineOptionParser<GnuFilter>, ICommandLineFilter
    {
        private readonly ResourceManager? _resourceManager;

        private LocalizableStrings? _localizableStrings;

        [Option(nameof(Version), LocalizableDescription = nameof(LocalizableStrings.VersionOptionDescription))]
        public bool Version { get; private set; }

        [Option('h', nameof(Help), LocalizableDescription = nameof(LocalizableStrings.HelpOptionDescription))]
        public bool Help { get; private set; }

        internal GnuFilter(CommandLine? commandLine)
        {
            _resourceManager = commandLine?.ResourceManager;

            AddMatch(nameof(Version), v => Version = v);
            AddMatch('h', nameof(Help), v => Help = v);
            SetResult(() => this);
        }

        public void Filter(ICommandLineFilterContext context)
        {
            // 启用标准命令行处理（额外：空谓词时不处理，留后续处理）。
            Run(context, false);
        }

        public void PostFilter(ICommandLineFilterContext context)
        {
            var verb = context.Verb;
            if (string.IsNullOrWhiteSpace(verb))
            {
                // 启用标准命令行处理（额外：这是最后机会，空谓词时也提示帮助）。
                Run(context, true);
            }
            else
            {
                // 无论是否启用了 Help 均提示谓词找不到。
                context.SuppressFurtherHandlers(0);
                PrintUnknownVerbHelpText(verb);
            }
        }

        private void Run(ICommandLineFilterContext contextInterface, bool helpEmptyVerb)
        {
            var context = (CommandLineFilterContext)contextInterface;
            var types = context.EnumerateRelatedTypes().ToList();
            _localizableStrings ??= new LocalizableStrings();

            if (Help)
            {
                context.SuppressFurtherHandlers(0);
                if (context.Verb is string verb && !string.IsNullOrWhiteSpace(verb))
                {
                    PrintVerbHelpText(context.GetVerbType()!, verb);
                }
                else
                {
                    PrintDetailHelpText(types);
                }
            }
            else if (Version)
            {
                context.SuppressFurtherHandlers(0);
                PrintVersionText();
            }
            else if (helpEmptyVerb)
            {
                context.SuppressFurtherHandlers(0);
                PrintHelpText(types);
            }
        }
    }
}
