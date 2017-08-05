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

namespace Xunit.Analyzers
{
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

        static CodeAnalyzerHelper()
        {
            // Xunit is a PCL linked against System.Runtime, however on the Desktop framework all types in that assembly have been forwarded to
            // System.Core, so we need to find the assembly by name to compile without errors.
            AssemblyName[] referencedAssemblies = typeof(FactAttribute).Assembly.GetReferencedAssemblies();
            SystemRuntimeReference = GetAssemblyReference(referencedAssemblies, "System.Runtime");
            SystemThreadingTasksReference = GetAssemblyReference(referencedAssemblies, "System.Threading.Tasks");
        }

        static MetadataReference GetAssemblyReference(IEnumerable<AssemblyName> assemblies, string name)
        {
            return MetadataReference.CreateFromFile(Assembly.Load(assemblies.First(n => n.Name == name)).Location);
        }

        public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, string source, params string[] additionalSources)
        {
            return GetDiagnosticsAsync(analyzer, false, source, additionalSources);
        }

        public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, bool ignoreCompilationErrors, string source, params string[] additionalSources)
        {
            const string fileNamePrefix = "Source";
            const string projectName = "Project";

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            var solution = new AdhocWorkspace()
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
                });

            int count = 0;
            foreach (var text in new[] { source }.Concat(additionalSources))
            {
                var newFileName = $"{fileNamePrefix}{count++}.cs";
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(text));
            }

            var project = solution.GetProject(projectId);
            var compilationOptions = ((CSharpCompilationOptions)project.CompilationOptions)
                .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                .WithWarningLevel(2);
            project = project.WithCompilationOptions(compilationOptions);

            var compilation = await project.GetCompilationAsync();
            var compilationDiagnostics = compilation.GetDiagnostics();
            if (!ignoreCompilationErrors && compilationDiagnostics.Any())
            {
                Diagnostic error = compilationDiagnostics.First();
                throw new InvalidOperationException($"Compilation has errors. First error: {error.Id} {error.WarningLevel} {error.GetMessage()}");
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
