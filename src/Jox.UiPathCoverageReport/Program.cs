using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Jox.UiPathCoverageReport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.Error.WriteLine("Syntax error.\r\nUsage: {0} [uipath-test.xaml]", Path.GetFileName(Assembly.GetExecutingAssembly().Location));
                    return;
                }
                (var files, var reports) = ParseCommandLine(args);
                var tree = Tree.Build(files);
                foreach (var report in reports)
                {
                    report.Print(tree, Console.Out);
                }
            }
            finally
            {
                if (args.Length == 1)
                {
                    Console.Error.WriteLine("Press any key to exit");
                    Console.Read();
                }
            }
        }

        private static (IReadOnlyList<string> files, IReadOnlyList<IReport> reports) ParseCommandLine(string[] args)
        {
            var files = new List<string>();
            var reports = new List<IReport>();
            var csvReport = new CsvReport();
            var treeReport = new LineGraphReport();
            foreach (var arg in args)
            {
                // flags
                switch (arg.ToLowerInvariant())
                {
                    case "/c":
                    case "/csv":
                    case "-c":
                    case "-csv":
                        if (!reports.Contains(csvReport)) reports.Add(csvReport);
                        continue;
                    case "/t":
                    case "/tree":
                    case "-t":
                    case "-tree":
                        if (!reports.Contains(treeReport)) reports.Add(treeReport);
                        continue;
                }
                // files/directories including wildcards
                if (arg.Contains('?') || arg.Contains('*'))
                {
                    var dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(arg)));
                    foreach (var expanded in dir.EnumerateFiles(Path.GetFileName(arg)))
                    {
                        files.Add(expanded.FullName);
                    }
                }
                else
                {
                    files.Add(arg);
                }
            }
            if (reports.Count == 0)
            {
                // by default, print both types
                reports.Add(treeReport);
                reports.Add(csvReport);
            }
            return (files, reports);
        }
    }
}
