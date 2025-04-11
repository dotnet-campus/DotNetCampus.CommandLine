using System.Collections.Generic;
using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Tests.Fakes
{
    public class DictionaryOptions
    {
        [Option('a', "Aaa")]
        public IReadOnlyDictionary<string, string>? Aaa { get; set; }

        [Option('b', "Bbb")]
        public IDictionary<string, string>? Bbb { get; set; }

        [Option('c', "Ccc")]
        public Dictionary<string, string>? Ccc { get; set; }

        [Option('d', "Ddd")]
        public KeyValuePair<string, string> Ddd { get; set; }
    }
}
