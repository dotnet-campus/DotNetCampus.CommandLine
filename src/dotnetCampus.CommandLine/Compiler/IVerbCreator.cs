namespace dotnetCampus.Cli.Compiler;

public interface IVerbCreator<out T>
    where T : class
{
    T CreateInstance(CommandLine commandLine);
}
