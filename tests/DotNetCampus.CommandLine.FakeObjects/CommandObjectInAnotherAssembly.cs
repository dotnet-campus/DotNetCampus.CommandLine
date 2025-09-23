using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.CommandLine.FakeObjects;

public class CommandObject0InAnotherAssembly
{
    [Option('o', "option")]
    public string? Option { get; set; }
}

[Command("test")]
public class CommandObject1InAnotherAssembly
{
    [Option('o', "option")]
    public string? Option { get; set; }
}

[Command("command in-another-assembly")]
public class CommandObject2InAnotherAssembly
{
    [Option('o', "option")]
    public string? Option { get; set; }
}
