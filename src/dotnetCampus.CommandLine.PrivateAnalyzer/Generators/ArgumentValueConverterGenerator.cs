using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators;

[Generator(LanguageNames.CSharp)]
public class VerbCreatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(Execute);
    }

    private void Execute(IncrementalGeneratorPostInitializationContext context)
    {
    }
}
