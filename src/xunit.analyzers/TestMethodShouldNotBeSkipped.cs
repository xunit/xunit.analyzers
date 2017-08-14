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
           ImmutableArray.Create(Descriptors.X1004_TestMethodShouldNotBeSkipped);

        public override void Initialize(AnalysisContext context)
        {
            context.RequireTypes(Constants.Types.XunitFactAttribute).RegisterSyntaxNodeAction(syntaxContext =>
            {
                var attribute = syntaxContext.Node as AttributeSyntax;
                if (!(attribute.ArgumentList?.Arguments.Any() ?? false))
                    return;

                var factType = syntaxContext.Compilation().GetFactAttributeType();
                var attributeType = syntaxContext.SemanticModel.GetTypeInfo(attribute).Type;
                if (!factType.IsAssignableFrom(attributeType))
                    return;

                var skipArgument = attribute.ArgumentList.Arguments
                    .FirstOrDefault(arg => arg.NameEquals?.Name?.Identifier.ValueText == "Skip");

                if (skipArgument != null)
                    syntaxContext.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X1004_TestMethodShouldNotBeSkipped,
                        skipArgument.GetLocation()));
            }, SyntaxKind.Attribute);
        }
    }
}

