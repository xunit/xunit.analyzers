using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Analyzers
{
    enum CompilationReporting
    {
        /// <summary>Ignores all errors and warnings</summary>
        IgnoreErrors = -1,

        /// <summary>Fails on all errors, ignores all warnings</summary>
        FailOnErrors = 0,

        /// <summary>Fails on all errors and level 1 warnings, ignores all other warnings</summary>
        FailOnErrorsAndLevel1Warnings = 1,

        /// <summary>Fails on all errors and level 1 and 2 warnings, ignores all other warnings</summary>
        FailOnErrorsAndLevel2Warnings = 2,

        /// <summary>Fails on all errors and level 1 through 3 warnings, ignores all other warnings</summary>
        FailOnErrorsAndLevel3Warnings = 3,

        /// <summary>Fails on all errors and warnings</summary>
        FailOnErrorsAndLevel4Warnings = 4,
    }

    class CodeAnalyzerHelper
    {
        static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference SystemCollectionsImmutable = MetadataReference.CreateFromFile(typeof(ImmutableArray).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference SystemTextReference = MetadataReference.CreateFromFile(typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference SystemRuntimeReference;
        static readonly MetadataReference SystemThreadingTasksReference;
        static readonly MetadataReference XunitCoreReference = MetadataReference.CreateFromFile(typeof(FactAttribute).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference XunitAbstractionsReference = MetadataReference.CreateFromFile(typeof(ITest).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference XunitAssertReference = MetadataReference.CreateFromFile(typeof(Assert).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference XunitExecutionReference = MetadataReference.CreateFromFile(typeof(XunitTestCase).GetTypeInfo().Assembly.Location);

        static CodeAnalyzerHelper()
        {
            // Xunit is a PCL linked against System.Runtime, however on the Desktop framework all types in that assembly have been forwarded to
            // System.Core, so we need to find the assembly by name to compile without errors.
            var referencedAssemblies = typeof(FactAttribute).Assembly.GetReferencedAssemblies();
            SystemRuntimeReference = GetAssemblyReference(referencedAssemblies, "System.Runtime");
            SystemThreadingTasksReference = GetAssemblyReference(referencedAssemblies, "System.Threading.Tasks");
        }

        static MetadataReference GetAssemblyReference(IEnumerable<AssemblyName> assemblies, string name)
        {
            return MetadataReference.CreateFromFile(Assembly.Load(assemblies.First(n => n.Name == name)).Location);
        }

        public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, string source, params string[] additionalSources)
            => GetDiagnosticsAsync(analyzer, CompilationReporting.FailOnErrors, source, additionalSources);

        public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, CompilationReporting compilationReporting, string source, params string[] additionalSources)
        {
            const string fileNamePrefix = "Source";
            const string projectName = "Project";

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            using (var workspace = new AdhocWorkspace())
            {
                var solution = workspace
                    .CurrentSolution
                    .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                    .AddMetadataReferences(projectId, new[] {
                        CorlibReference,
                        SystemCollectionsImmutable,
                        SystemCoreReference,
                        SystemTextReference,
                        SystemRuntimeReference,
                        SystemThreadingTasksReference,
                        XunitCoreReference,
                        XunitAbstractionsReference,
                        XunitAssertReference,
                        XunitExecutionReference,
                    });

                var count = 0;
                foreach (var text in new[] { source }.Concat(additionalSources))
                {
                    var newFileName = $"{fileNamePrefix}{count++}.cs";
                    var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                    solution = solution.AddDocument(documentId, newFileName, SourceText.From(text));
                }

                var compileWarningLevel = Math.Max(0, (int)compilationReporting);
                var project = solution.GetProject(projectId);
                var compilationOptions = ((CSharpCompilationOptions)project.CompilationOptions)
                    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                    .WithWarningLevel(compileWarningLevel);
                project = project.WithCompilationOptions(compilationOptions);

                var compilation = await project.GetCompilationAsync();
                if (compilationReporting != CompilationReporting.IgnoreErrors)
                {
                    var compilationDiagnostics = compilation.GetDiagnostics();
                    if (compilationReporting == CompilationReporting.FailOnErrors)
                        compilationDiagnostics = compilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToImmutableArray();

                    if (compilationDiagnostics.Length > 0)
                    {
                        var messages = compilationDiagnostics.Select(d => (diag: d, line: d.Location.GetLineSpan().StartLinePosition))
                                                             .Select(t => $"source.cs({t.line.Line},{t.line.Character}): {t.diag.Severity.ToString().ToLowerInvariant()} {t.diag.Id}: {t.diag.GetMessage()}");
                        throw new InvalidOperationException($"Compilation has issues:{Environment.NewLine}{string.Join(Environment.NewLine, messages)}");
                    }
                }

                var compilationWithAnalyzers = compilation
                    .WithOptions(((CSharpCompilationOptions)compilation.Options)
                    .WithWarningLevel(4))
                    .WithAnalyzers(ImmutableArray.Create(analyzer));

                var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

                Assert.DoesNotContain(allDiagnostics, d => d.Id == "AD0001");

                return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
            }
        }
    }
}
