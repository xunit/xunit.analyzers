using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestCaseMustBeLongLivedMarshalByRefObject : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
            {
                var classDeclaration = syntaxNodeContext.Node as ClassDeclarationSyntax;
                if (classDeclaration.BaseList == null)
                    return;

                var semanticModel = syntaxNodeContext.SemanticModel;
                var isTestCase = false;
                var hasMBRO = false;

                foreach (var baseType in classDeclaration.BaseList.Types)
                {
                    var type = semanticModel.GetTypeInfo(baseType.Type, compilationStartContext.CancellationToken).Type;
                    if (xunitContext.ITestCaseType?.IsAssignableFrom(type) == true)
                        isTestCase = true;
                    if (xunitContext.LongLivedMarshalByRefObjectType?.IsAssignableFrom(type) == true)
                        hasMBRO = true;
                }

                if (isTestCase && !hasMBRO)
                {
                    syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject,
                        classDeclaration.Identifier.GetLocation(),
                        classDeclaration.Identifier.ValueText));
                }
            }, SyntaxKind.ClassDeclaration);
        }
    }
}
