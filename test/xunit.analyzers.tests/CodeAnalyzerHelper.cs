using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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
        private static readonly ImmutableArray<MetadataReference> MetadataReferences = GetMetadataReferences().ToImmutableArray();

        private static IEnumerable<MetadataReference> GetMetadataReferences()
            => GetFrameworkMetadataReferences().Concat(GetXunitMetadataReferences());

        private static IEnumerable<MetadataReference> GetFrameworkMetadataReferences()
        {
            var systemPrivateCorelibNi = typeof(object).GetTypeInfo().Assembly;
            var frameworkAssembliesDir = Path.GetDirectoryName(systemPrivateCorelibNi.Location);

            var assemblyNames = new[]
            {
                "mscorlib",
                "System.Collections",
                "System.Collections.Immutable",
                "System.IO",
                "System.Linq",
                "System.Private.CoreLib",
                "System.Runtime",
                "System.Text.RegularExpressions",
                "System.Threading.Tasks"
            };

            foreach (var assemblyName in assemblyNames)
            {
                var assemblyPath = Path.Combine(frameworkAssembliesDir, assemblyName + ".dll");
                yield return MetadataReference.CreateFromFile(assemblyPath);
            }

            // System.Collections.NonGeneric does not appear in NETStandard.Library for .NET Core apps.
            // https://stackoverflow.com/q/39339427/4077294
            yield return MetadataReference.CreateFromFile(typeof(ArrayList).GetTypeInfo().Assembly.Location);
        }

        private static IEnumerable<MetadataReference> GetXunitMetadataReferences()
        {
            yield return MetadataReference.CreateFromFile(typeof(FactAttribute).GetTypeInfo().Assembly.Location); // xunit.core
            yield return MetadataReference.CreateFromFile(typeof(ITest).GetTypeInfo().Assembly.Location); // xunit.abstractions
            yield return MetadataReference.CreateFromFile(typeof(Assert).GetTypeInfo().Assembly.Location); // xunit.assert
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
                .AddMetadataReferences(projectId, MetadataReferences);

            int count = 0;
            foreach (var text in new[] { source }.Concat(additionalSources))
            {
                var newFileName = $"{fileNamePrefix}{count++}.cs";
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(text));
            }

            var project = solution.GetProject(projectId);
            var compilationOptions = ((CSharpCompilationOptions)project.CompilationOptions)
                .WithAllowUnsafe(true)
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
