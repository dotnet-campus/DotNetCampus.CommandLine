//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//using DotNetCampus.CommandLine;
//using DotNetCampus.CommandLine.Analyzers;

//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//using MSTest.Extensions.Contracts;

//using RoslynTestKit;

//namespace DotNetCampus.Cli.Tests.Analyzers
//{
//    [TestClass]
//    public class OptionLongNameMustBePascalCaseAnalyzerTest : AnalyzerTestFixture
//    {
//        protected override string LanguageName => LanguageNames.CSharp;
//        protected override DiagnosticAnalyzer CreateAnalyzer() => new OptionLongNameMustBePascalCaseAnalyzer();

//        [ContractTestCase]
//        public void TestWithoutNumbers()
//        {
//            "使用 Pascal 风格的长名称，不报告 Pascal 诊断。".Test<string>(TestNoDiagnostic).WithArguments(
//                "WalterlvIsAdobe",
//                "Walterlv");

//            "使用非 Pascal 风格的长名称，报告 Pascal 诊断。".Test<string>(TestHasDiagnostic).WithArguments(
//                "--walterlv-is-adobe",
//                "-WalterlvIsAdobe",
//                "/WalterlvIsAdobe",
//                "walterlv-is-adobe",
//                "walterlvIsAdobe",
//                "walterlv_is_adobe",
//                "waltelv",
//                "--walterlv",
//                "-Walterlv",
//                "/Walterlv");

//            "多位全大写字母，报告 Pascal 诊断。".Test<string>(TestHasDiagnostic).WithArguments(
//                "HTML",
//                "AddedHTMLFile");

//            "两位全大写字母，不报告 Pascal 诊断。".Test<string>(TestNoDiagnostic).WithArguments(
//                "IO",
//                "IOSetting",
//                "TestIO",
//                "TestIOSetting");
//        }

//        [ContractTestCase]
//        public void TestWithNumbers()
//        {
//            "使用 Pascal 风格的长名称，不报告 Pascal 诊断。".Test<string>(TestNoDiagnostic).WithArguments(
//                "Files2Build",
//                "Html5");

//            "使用非 Pascal 风格的长名称，报告 Pascal 诊断。".Test<string>(TestHasDiagnostic).WithArguments(
//                "--walterlv-is-adobe",
//                "-WalterlvIsAdobe",
//                "/WalterlvIsAdobe",
//                "walterlv-is-adobe",
//                "walterlvIsAdobe",
//                "walterlv_is_adobe",
//                "waltelv",
//                "--walterlv",
//                "-Walterlv",
//                "/Walterlv");
//        }

//        private void TestHasDiagnostic(string longName)
//        {
//            string code = $@"
//class Options
//{{
//    [Option('d', ""{longName}"")]
//    public string? DemoOption {{ get; set; }}
//}}";
//            HasDiagnostic(code, DiagnosticIds.OptionLongNameMustBePascalCase);
//        }

//        private void TestNoDiagnostic(string longName)
//        {
//            string code = $@"
//class Options
//{{
//    [Option('d', ""{longName}"")]
//    public string? DemoOption {{ get; set; }}
//}}";
//            NoDiagnostic(code, DiagnosticIds.OptionLongNameMustBePascalCase);
//        }
//    }
//}
