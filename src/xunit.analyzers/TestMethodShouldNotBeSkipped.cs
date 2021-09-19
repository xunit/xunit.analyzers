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
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1004_TestMethodShouldNotBeSkipped);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSyntaxNodeAction(context =>
			{
				var attribute = (AttributeSyntax)context.Node;
				var skipArgument = attribute.ArgumentList?.Arguments.FirstOrDefault(arg => arg.NameEquals?.Name?.Identifier.ValueText == "Skip");
				if (skipArgument is null)
					return;

				var attributeType = context.SemanticModel.GetTypeInfo(attribute).Type;
				if (!xunitContext.V2Core.FactAttributeType.IsAssignableFrom(attributeType))
					return;

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1004_TestMethodShouldNotBeSkipped,
						skipArgument.GetLocation()
					)
				);
			}, SyntaxKind.Attribute);
		}
	}
}

