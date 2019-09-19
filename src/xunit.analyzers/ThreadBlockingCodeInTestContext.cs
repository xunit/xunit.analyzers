using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThreadBlockingCodeInTestContext : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptors.X1027_ThreadBlockingCodeInTest);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext,
            XunitContext xunitContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(
                new SyncTaskInvocationAnalysis(compilationStartContext.Compilation).Analyze,
                SyntaxKind.SimpleMemberAccessExpression);
        }

        private class SyncTaskInvocationAnalysis
        {
            private static readonly ImmutableHashSet<string> SyncTaskMemberNames = ImmutableHashSet.Create(
                "Wait", "Result", "GetAwaiter", "WaitAll", "WaitAny");

            private readonly ISet<ISymbol> syncTaskMemberSymbols;

            public SyncTaskInvocationAnalysis(Compilation compilation)
            {
                var taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var taskWithResultSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

                if (taskSymbol == null || taskWithResultSymbol == null)
                    return;

                syncTaskMemberSymbols = CollectMembers(taskSymbol).Union(CollectMembers(taskWithResultSymbol))
                    .ToImmutableHashSet();

                IEnumerable<ISymbol> CollectMembers(INamedTypeSymbol symbol)
                    => symbol.GetMembers().Where(m => SyncTaskMemberNames.Contains(m.Name));
            }

            public void Analyze(SyntaxNodeAnalysisContext context)
            {
                var memberAccess = (MemberAccessExpressionSyntax) context.Node;
                var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;

                if (symbol == null)
                    return;

                if (!syncTaskMemberSymbols.Contains(symbol)
                    && symbol.OriginalDefinition != null
                    && !syncTaskMemberSymbols.Contains(symbol.OriginalDefinition))
                    return;

                var invocationDescriptiveString = memberAccess.ToString();
                var testMethodName = memberAccess.AncestorsAndSelf().OfType<MethodDeclarationSyntax>()
                    .Single().Identifier;

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.X1027_ThreadBlockingCodeInTest,
                    memberAccess.GetLocation(),
                    invocationDescriptiveString,
                    testMethodName));
            }
        }
    }
}
