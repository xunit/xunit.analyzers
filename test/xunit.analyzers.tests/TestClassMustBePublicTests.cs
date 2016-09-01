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
    public class TestClassMustBePublicTests
    {
        public class Analyzer
        {
            readonly TestClassMustBePublic analyzer = new TestClassMustBePublic();

            [Fact]
            public async void DoesNotFindErrorForPublicClass()
            {
                var diagnostics = await GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("Xunit.Fact")]
            [InlineData("Xunit.Theory")]
            public async void FindErrorForPrivateClass(string attribute)
            {
                var diagnostics = await GetDiagnosticsAsync(analyzer, "class TestClass { [" + attribute + "] public void TestMethod() { } }");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("Make the type TestClass public so that test methods on it can be discovered and executed", d.GetMessage());
                        Assert.Equal("xUnit1000", d.Descriptor.Id);
                    });
            }
        }

        static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference XunitCoreReference = MetadataReference.CreateFromFile(typeof(FactAttribute).GetTypeInfo().Assembly.Location);
        static readonly MetadataReference XunitAbstractionsReference = MetadataReference.CreateFromFile(typeof(ITest).GetTypeInfo().Assembly.Location);

        static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, string source, params string[] additionalSources)
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
                    CodeAnalysisReference,
                    CSharpSymbolsReference,
                    XunitCoreReference,
                    XunitAbstractionsReference,
                });

            int count = 0;
            foreach (var text in new[] { source }.Concat(additionalSources))
            {
                var newFileName = $"{fileNamePrefix}{count++}.cs";
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(text));
            }

            var project = solution.GetProject(projectId);
            var compilation = await project.GetCompilationAsync();
            var results = await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new TestClassMustBePublic())).GetAnalyzerDiagnosticsAsync();
            return results;
        }
    }
}
