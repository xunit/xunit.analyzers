using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
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

    [Flags]
    enum XunitReferences
    {
        None = 0,

        /// <summary>Adds a reference to xunit.abstractions</summary>
        Abstractions = 0x01,

        /// <summary>Adds a reference to xunit.assert</summary>
        Assert = 0x02,

        /// <summary>Adds a reference to xunit.core</summary>
        Core = 0x04,

        /// <summary>Adds a reference to xunit.execution</summary>
        Execution = 0x08,

        /// <summary>
        /// This adds references akin to the xunit.extensibility.core NuGet package. This is appropriate
        /// for simulating the typing references used when writing core-only extensibility.
        /// </summary>
        PkgCoreExtensibility = Abstractions | Core,

        /// <summary>
        /// This adds references akin to the xunit.extensibility.execution NuGet package. This is appropriate
        /// for simulating the typing references used when writing full extensibility.
        /// </summary>
        PkgExecutionExtensibility = Abstractions | Core | Execution,

        /// <summary>
        /// This adds references akin to the xunit NuGet package. This is appropriate for simulating the
        /// typical references used when writing unit tests.
        /// </summary>
        PkgXunit = Abstractions | Assert | Core,

        /// <summary>
        /// This adds references akin to the xunit.core NuGet package. This is appropriate for simulating the
        /// typical references used when writing unit tests with a third party assertion library.
        /// </summary>
        PkgXunitCore = Abstractions | Core,
    }

    class CodeAnalyzerHelper
    {
        static readonly MetadataReference CorlibReference = GetAssemblyReference(typeof(object));
        static readonly MetadataReference NetStandardReference = GetAssemblyReference("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");
        static readonly MetadataReference SystemCollectionsImmutableReference = GetAssemblyReference(typeof(ImmutableArray));
        static readonly MetadataReference SystemCollectionsReference = GetAssemblyReference("System.Collections");
        static readonly MetadataReference SystemConsoleReference = GetAssemblyReference("System.Console");
        static readonly MetadataReference SystemCoreReference = GetAssemblyReference(typeof(Enumerable));
        static readonly MetadataReference SystemTextReference = GetAssemblyReference(typeof(System.Text.RegularExpressions.Regex));
        static readonly MetadataReference SystemRuntimeExtensionsReference = GetAssemblyReference("System.Runtime.Extensions");
        static readonly MetadataReference SystemRuntimeReference;
        static readonly MetadataReference SystemThreadingTasksReference;
        static readonly MetadataReference XunitAbstractionsReference = GetAssemblyReference(typeof(ITest));
        static readonly MetadataReference XunitAssertReference = GetAssemblyReference(typeof(Assert));
        static readonly MetadataReference XunitCoreReference = GetAssemblyReference(typeof(FactAttribute));
        static readonly MetadataReference XunitExecutionReference = GetAssemblyReference(typeof(XunitTestCase));

        static readonly IEnumerable<MetadataReference> SystemReferences;

        static readonly Dictionary<XunitReferences, MetadataReference[]> ReferenceMap = new Dictionary<XunitReferences, MetadataReference[]> {
            { XunitReferences.Abstractions, new[] { XunitAbstractionsReference } },
            { XunitReferences.Assert, new[] { XunitAssertReference } },
            { XunitReferences.Core, new[] {  XunitCoreReference } },
            { XunitReferences.Execution, new[] { XunitExecutionReference } },
        };

        static CodeAnalyzerHelper()
        {
            // Xunit is a PCL linked against System.Runtime, however on the Desktop framework all types in that assembly have been forwarded to
            // System.Core, so we need to find the assembly by name to compile without errors.
            var referencedAssemblies = typeof(FactAttribute).Assembly.GetReferencedAssemblies();

            SystemRuntimeReference = GetAssemblyReference(referencedAssemblies, "System.Runtime");
            SystemThreadingTasksReference = GetAssemblyReference(referencedAssemblies, "System.Threading.Tasks");
            SystemReferences = new[] {
                CorlibReference,
                NetStandardReference,
                SystemCollectionsImmutableReference,
                SystemCollectionsReference,
                SystemConsoleReference,
                SystemCoreReference,
                SystemRuntimeReference,
                SystemRuntimeExtensionsReference,
                SystemTextReference,
                SystemThreadingTasksReference,
            }.Where(x => x != null).ToArray();
        }

        static async Task<ImmutableArray<Diagnostic>> ApplyAnalyzers(Compilation compilation, params DiagnosticAnalyzer[] analyzers)
        {
            var compilationWithAnalyzers = compilation
                .WithOptions(((CSharpCompilationOptions)compilation.Options)
                .WithWarningLevel(4))
                .WithAnalyzers(ImmutableArray.Create(analyzers));

            var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

            Assert.DoesNotContain(allDiagnostics, d => d.Id == "AD0001");

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        static MetadataReference GetAssemblyReference(Type type)
            => MetadataReference.CreateFromFile(type.GetTypeInfo().Assembly.Location);

        static MetadataReference GetAssemblyReference(string name)
        {
            try
            {
                return MetadataReference.CreateFromFile(Assembly.Load(name).Location);
            }
            catch
            {
                return null;
            }
        }

        static MetadataReference GetAssemblyReference(IEnumerable<AssemblyName> assemblies, string name)
            => MetadataReference.CreateFromFile(Assembly.Load(assemblies.First(n => n.Name == name)).Location);

        static async Task<(Compilation, Document, Workspace)> GetCompilationAsync(CompilationReporting compilationReporting, XunitReferences references, string source, params string[] additionalSources)
        {
            const string fileNamePrefix = "Source";
            const string projectName = "Project";

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            var workspace = new AdhocWorkspace();
            var solution = workspace
                .CurrentSolution
                .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                .AddMetadataReferences(projectId, GetMetadataReferences(references));

            var count = 0;
            var firstDocument = default(Document);

            foreach (var text in new[] { source }.Concat(additionalSources))
            {
                var newFileName = $"{fileNamePrefix}{count++}.cs";
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(text));
                if (firstDocument == default(Document))
                    firstDocument = solution.GetDocument(documentId);
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
                if (compilationDiagnostics.Length > 0)
                {
                    var messages = compilationDiagnostics.Select(d => (diag: d, line: d.Location.GetLineSpan().StartLinePosition))
                                                         .Select(t => $"source.cs({t.line.Line},{t.line.Character}): {t.diag.Severity.ToString().ToLowerInvariant()} {t.diag.Id}: {t.diag.GetMessage()}");
                    throw new InvalidOperationException($"Compilation has issues:{Environment.NewLine}{string.Join(Environment.NewLine, messages)}");
                }
            }

            return (compilation, firstDocument, workspace);
        }

        public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, string source, params string[] additionalSources)
            => GetDiagnosticsAsync(analyzer, CompilationReporting.FailOnErrors, XunitReferences.PkgXunit, source, additionalSources);

        public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, CompilationReporting compilationReporting, string source, params string[] additionalSources)
            => GetDiagnosticsAsync(analyzer, compilationReporting, XunitReferences.PkgXunit, source, additionalSources);

        public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, XunitReferences references, string source, params string[] additionalSources)
            => GetDiagnosticsAsync(analyzer, CompilationReporting.FailOnErrors, references, source, additionalSources);

        public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, CompilationReporting compilationReporting, XunitReferences references, string source, params string[] additionalSources)
        {
            var (compilation, _, workspace) = await GetCompilationAsync(compilationReporting, references, source, additionalSources);

            using (workspace)
                return await ApplyAnalyzers(compilation, analyzer);
        }

        public static async Task<string> GetFixedCodeAsync(DiagnosticAnalyzer analyzer,
                                                           CodeFixProvider fixer,
                                                           string source,
                                                           CompilationReporting compilationReporting = CompilationReporting.FailOnErrors,
                                                           XunitReferences references = XunitReferences.PkgXunit,
                                                           int actionIndex = 0)
        {
            var (compilation, document, workspace) = await GetCompilationAsync(compilationReporting, references, source);

            using (workspace)
            {
                var diagnostics = await ApplyAnalyzers(compilation, analyzer);
                if (diagnostics.Length == 0)
                    throw new InvalidOperationException("The requested source code does not trigger the analyzer");
                if (diagnostics.Length > 1)
                    throw new InvalidOperationException($"The requested source code triggered the analyzer too many times (expected 1, got {diagnostics.Length})");

                var codeActions = new List<CodeAction>();
                var context = new CodeFixContext(document, diagnostics[0], (a, d) => codeActions.Add(a), CancellationToken.None);
                await fixer.RegisterCodeFixesAsync(context);
                if (codeActions.Count <= actionIndex)
                    throw new InvalidOperationException($"Not enough code actions were registered (index {actionIndex} is out of range for length {codeActions.Count})");

                var operations = await codeActions[actionIndex].GetOperationsAsync(CancellationToken.None);
                var changeOperations = operations.OfType<ApplyChangesOperation>().ToList();
                if (changeOperations.Count != 1)
                    throw new InvalidOperationException($"The change action did not yield the right number of ApplyChangesOperation objects (expected 1, got {changeOperations.Count})");

                var changeOperation = changeOperations[0];
                changeOperation.Apply(workspace, CancellationToken.None);

                var solution = changeOperation.ChangedSolution;
                var changedDocument = solution.GetDocument(document.Id);
                var text = await changedDocument.GetTextAsync();
                return text.ToString();
            }
        }

        static IEnumerable<MetadataReference> GetMetadataReferences(XunitReferences references)
        {
            var result = SystemReferences;

            foreach (var kvp in ReferenceMap)
                if (references.HasFlag(kvp.Key))
                    result = result.Concat(kvp.Value);

            return result;
        }
    }
}
