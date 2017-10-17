using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers.DocBuilder
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Expected the path to the docs to be passed in as the single argument");
                return 1;
            }

            var docsPath = Path.GetFullPath(args[0]);
            var rulesPath = Path.Combine(docsPath, "_rules");
            var template = File.ReadAllText(Path.Combine(docsPath, "RULE_TEMPLATE.md"));
            var result = 0;
            foreach (var descriptor in EnumerateDescriptors())
            {
                var ruleDocFilePath = Path.Combine(rulesPath, descriptor.Id + ".md");
                if (!File.Exists(ruleDocFilePath) || new FileInfo(ruleDocFilePath).Length == 0)
                {
                    File.WriteAllText(ruleDocFilePath, template);
                    Console.Error.WriteLine($"Documentation file for rule {descriptor.Id} could not be found at ${ruleDocFilePath} or it is empty");
                    result = 1;
                }

                var changed = false;
                var fileLines = File.ReadAllLines(ruleDocFilePath).ToList();
                if (fileLines.First() != "---")
                {
                    fileLines.Insert(0, "---");
                    fileLines.Insert(0, "---");
                    changed = true;
                    Console.Error.WriteLine($"Documentation file for rule {descriptor.Id} needs to start with a front matter declaration");
                }

                var endFrontMatter = fileLines.IndexOf("---", 1);
                foreach (var expected in GetFrontMatterValues(descriptor))
                {
                    bool found = false;
                    for (int i = 1; i < endFrontMatter; i++)
                    {
                        var line = fileLines[i];
                        if (line.StartsWith(expected.Name + ":"))
                        {
                            found = true;
                            if (!line.Equals(expected.ToString()))
                            {
                                fileLines[i] = expected.ToString();
                                changed = true;
                                Console.Error.WriteLine($"Documentation file for rule {descriptor.Id} has wrong value for front matter field {expected.Name}");
                            }
                        }
                    }
                    if (!found)
                    {
                        fileLines.Insert(1, expected.ToString());
                        result = 1;
                        Console.Error.WriteLine($"Documentation file for rule {descriptor.Id} is missing front matter field {expected.Name}");
                    }
                }

                if (changed)
                {
                    result = 1;
                    File.WriteAllLines(ruleDocFilePath, fileLines);
                }
            }
            return result;
        }

        static IEnumerable<FrontMatter> GetFrontMatterValues(DiagnosticDescriptor descriptor)
        {
            yield return new FrontMatter("severity", descriptor.DefaultSeverity.ToString());
            yield return new FrontMatter("category", descriptor.Category);
            yield return new FrontMatter("description", descriptor.Title.ToString());
            yield return new FrontMatter("title", descriptor.Id);
        }

        static IEnumerable<DiagnosticDescriptor> EnumerateDescriptors()
        {
            foreach (var prop in typeof(Descriptors).GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (prop.GetValue(null) is DiagnosticDescriptor descriptor)
                {
                    yield return descriptor;
                }
            }
        }

        private class FrontMatter
        {
            public string Name { get; }
            public string Value { get; }

            public FrontMatter(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public override string ToString()
            {
                return $"{Name}: {Value}";
            }
        }
    }
}
