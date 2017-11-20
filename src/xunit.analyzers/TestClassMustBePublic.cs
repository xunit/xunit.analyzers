using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestClassMustBePublic : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.X1000_TestClassMustBePublic);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
            {
                var classDeclaration = syntaxNodeContext.Node as ClassDeclarationSyntax;
                if (classDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
                    return;

                var methods = classDeclaration.Members.Where(n => n.IsKind(SyntaxKind.MethodDeclaration)).Cast<MethodDeclarationSyntax>();
                if (methods.Any(method => method.AttributeLists.ContainsAttributeType(syntaxNodeContext.SemanticModel, xunitContext.Core.FactAttributeType)))
                {
                    syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X1000_TestClassMustBePublic,
                        classDeclaration.Identifier.GetLocation(),
                        classDeclaration.Identifier.ValueText));
                }
            }, SyntaxKind.ClassDeclaration);
        }
    }
}
