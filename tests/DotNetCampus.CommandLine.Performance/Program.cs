using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DotNetCampus.Cli.Performance;

class Program
{
    static void Main(string[] args)
    {
        if (args.Contains("--debug"))
        {
            DebugAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
    }

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
