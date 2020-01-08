using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TestMethodShouldNotBeSkipped : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Descriptors.X1004_TestMethodShouldNotBeSkipped);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
			{
				var attribute = (AttributeSyntax)syntaxNodeContext.Node;
				if (!(attribute.ArgumentList?.Arguments.Any() ?? false))
					return;

				var attributeType = syntaxNodeContext.SemanticModel.GetTypeInfo(attribute).Type;
				if (!xunitContext.Core.FactAttributeType.IsAssignableFrom(attributeType))
					return;

				var skipArgument = attribute.ArgumentList.Arguments.FirstOrDefault(arg => arg.NameEquals?.Name?.Identifier.ValueText == "Skip");
				if (skipArgument != null)
					syntaxNodeContext.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1004_TestMethodShouldNotBeSkipped,
							skipArgument.GetLocation()));
			}, SyntaxKind.Attribute);
		}
	}
}

