﻿using System.Collections.Generic;

namespace dotnetCampus.Cli.Tests.Fakes
{
    public class UnlimitedValueOptions
    {
        [Option('s', nameof(Section))]
        public string? Section { get; set; }

        [Value(0)]
        public int Count { get; set; }

        [Value(1, int.MaxValue)]
        public IEnumerable<string>? Args { get; set; }
    }
}
