using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestMethodShouldNotBeSkipped : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X1004_TestMethodShouldNotBeSkipped);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var factType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
                if (factType == null)
                    return;

                compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
                {
                    var attribute = syntaxNodeContext.Node as AttributeSyntax;
                    if (!(attribute.ArgumentList?.Arguments.Any() ?? false))
                        return;

                    var attributeType = syntaxNodeContext.SemanticModel.GetTypeInfo(attribute).Type;
                    if (!factType.IsAssignableFrom(attributeType))
                        return;

                    var skipArgument = attribute.ArgumentList.Arguments
                        .FirstOrDefault(arg => arg.NameEquals?.Name?.Identifier.ValueText == "Skip");

                    if (skipArgument != null)
                        syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                            Constants.Descriptors.X1004_TestMethodShouldNotBeSkipped,
                            skipArgument.GetLocation()));
                }, SyntaxKind.Attribute);
            });
        }
    }
}

