using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DotNetCampus.Cli.Performance;

class Program
{
    static void Main(string[] args)
    {
#if DEBUG
        if (args.Contains("--debug"))
        {
            DebugAll();
            return;
        }
#endif

        BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
    }

    [Conditional("DEBUG")]
    private static void DebugAll()
    {
        var methods = typeof(Program).Assembly.GetTypes()
            .Where(x => x.IsDefined(typeof(BenchmarkCategoryAttribute)))
            .Select(x => (Instance: Activator.CreateInstance(x), Methods: x.GetMethods().Where(m => m.IsDefined(typeof(BenchmarkAttribute)))))
            .SelectMany(x => x.Methods.Select(m => (x.Instance, m)));
        foreach (var (instance, method) in methods)
        {
            method.Invoke(instance, []);
        }
    }
}
