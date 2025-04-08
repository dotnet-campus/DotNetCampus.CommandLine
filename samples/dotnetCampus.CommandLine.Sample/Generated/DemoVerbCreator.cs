using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Generated;

internal class DemoVerbCreator : IVerbCreator<DemoCommandHandler>
{
    public DemoCommandHandler CreateInstance(CommandLine commandLine)
    {
        var option = commandLine.GetOption<string>("Option") ?? ThrowRequiredOptionException("Option");
        var argument = commandLine.GetPositionalArgument() ?? ThrowRequiredValueException("Argument");
        return new DemoCommandHandler
        {
            Option = option,
            Argument = argument,
        };
    }

    private string ThrowRequiredOptionException(string propertyName)
    {
        throw new System.NotImplementedException();
    }

    private string ThrowRequiredValueException(string propertyName)
    {
        throw new System.NotImplementedException();
    }
}
