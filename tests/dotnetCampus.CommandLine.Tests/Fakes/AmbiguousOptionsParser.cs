﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetCampus.Cli.Tests.Fakes
{
    public class AmbiguousOptionsParser : CommandLineOptionParser<AmbiguousOptions>
    {
        public AmbiguousOptionsParser()
        {
            bool boolean = false;
            string stringBoolean = null;
            string stringArray = null;
            IReadOnlyList<string> array = null;

            AddMatch("Boolean", value => boolean = value);
            AddMatch("StringBoolean", value => stringBoolean = value);
            AddMatch("stringArray", value => stringArray = value);
            AddMatch("array", value => array = value);

            SetResult(() => new AmbiguousOptions()
            {
                Boolean = boolean,
                StringBoolean = stringBoolean,
                StringArray = stringArray,
                Array = array,
            });
        }
    }
}
