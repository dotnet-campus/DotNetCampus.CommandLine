using System.Reflection;
using BenchmarkDotNet.Running;

namespace DotNetCampus.Cli.Performance;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
    }
}
