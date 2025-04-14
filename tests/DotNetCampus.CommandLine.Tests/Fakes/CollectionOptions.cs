using System.Collections.Generic;
using System.Collections.ObjectModel;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Tests.Fakes;

public class CollectionOptions
{
    [Option("ReadOnlyList")]
    public IReadOnlyList<string>? ReadOnlyList { get; set; }

    [Option("List")]
    public IList<string>? List { get; set; }

    [Option("Collection")]
    public Collection<string>? Collection { get; set; }

    [Option("Array")]
    public string[]? Array { get; set; }

    [Option("Enumerable")]
    public IEnumerable<string>? Enumerable { get; set; }
}
