using System;
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
        static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference SystemRuntimeReference;
        static readonly MetadataReference XunitCoreReference = MetadataReference.CreateFromFile(typeof(FactAttribute).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference XunitAbstractionsReference = MetadataReference.CreateFromFile(typeof(ITest).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference XunitAssertReference = MetadataReference.CreateFromFile(typeof(Assert).GetTypeInfo().Assembly.Location);

        static CodeAnalyzerHelper()
        {
            // Xunit is a PCL linked against System.Runtime, however on the Desktop framework all types in that assembly have been forwarded to
            // System.Core, so we need to find the assembly by name to compile without errors.
            var fullName = typeof(FactAttribute).Assembly.GetReferencedAssemblies().FirstOrDefault(n => n.Name == "System.Runtime");
            var a = Assembly.Load(fullName);
            SystemRuntimeReference = MetadataReference.CreateFromFile(a.Location);
        }

        public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, string source, params string[] additionalSources)
        {
            const string fileNamePrefix = "Source";
            const string projectName = "Project";

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                .AddMetadataReferences(projectId, new[] {
                    CorlibReference,
                    SystemCoreReference,
                    SystemRuntimeReference,
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
            project = project.WithCompilationOptions(project.CompilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary));
            var compilation = await project.GetCompilationAsync();
            var compilationDiagnostics = compilation.GetDiagnostics();
            if (compilationDiagnostics.Any())
                throw new InvalidOperationException("Compilation has errors. First error: " + compilationDiagnostics.First().GetMessage());
            var results = await compilation.WithAnalyzers(ImmutableArray.Create(analyzer)).GetAnalyzerDiagnosticsAsync();
            return results;
        }
    }
}
