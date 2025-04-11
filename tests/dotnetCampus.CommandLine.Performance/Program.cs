using System.Reflection;
using BenchmarkDotNet.Running;

namespace dotnetCampus.Cli.Performance;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
    }
}
