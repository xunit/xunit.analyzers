using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestClassMustBePublic : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Constants.Descriptors.X1000_TestClassMustBePublic);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var factType = compilationStartContext.Compilation.GetTypeByMetadataName("Xunit.FactAttribute");
                if (factType == null)
                    return;

                compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
                {
                    var classDeclaration = syntaxNodeContext.Node as ClassDeclarationSyntax;
                    if (classDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
                        return;

                    var methods = classDeclaration.Members.Where(n => n.IsKind(SyntaxKind.MethodDeclaration)).Cast<MethodDeclarationSyntax>();
                    if (methods.Any(method => MarkedWithAttributeAssignableToType(method, factType, syntaxNodeContext.SemanticModel)))
                    {
                        syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                            Constants.Descriptors.X1000_TestClassMustBePublic,
                            classDeclaration.Identifier.GetLocation(), 
                            classDeclaration.Identifier.ValueText));
                    }
                }, SyntaxKind.ClassDeclaration);
            });
        }

        bool MarkedWithAttributeAssignableToType(MethodDeclarationSyntax method, INamedTypeSymbol factType, SemanticModel semanticModel)
        {
            foreach (var attributeList in method.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeType = semanticModel.GetTypeInfo(attribute).Type;
                    while (attributeType != null)
                    {
                        if (attributeType == factType)
                            return true;
                        attributeType = attributeType.BaseType;
                    }
                }
            }
            return false;
        }
    }
}
