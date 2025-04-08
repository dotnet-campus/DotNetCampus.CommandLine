using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Generated;

internal class SampleVerbCreator : IVerbCreator<SampleCommandHandler>
{
    public SampleCommandHandler CreateInstance(CommandLine commandLine) => new()
    {
        Option = commandLine.GetOption<string>("SampleProperty") ?? ThrowRequiredOptionException("SampleProperty"),
        Argument = commandLine.GetPositionalArgument(),
    };

    private string ThrowRequiredOptionException(string propertyName)
    {
        throw new System.NotImplementedException();
    }

    private string ThrowRequiredValueException(string propertyName)
    {
        throw new System.NotImplementedException();
    }
}
