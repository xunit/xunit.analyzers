using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodShouldNotBeSkipped : XunitDiagnosticAnalyzer
{
	public TestMethodShouldNotBeSkipped() :
		base(Descriptors.X1004_TestMethodShouldNotBeSkipped)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSyntaxNodeAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null)
				return;
			if (context.Node is not AttributeSyntax attribute)
				return;
			if (attribute.ArgumentList is null)
				return;

			var skipArgument = default(AttributeArgumentSyntax);
			foreach (var argument in attribute.ArgumentList.Arguments)
			{
				var valueText = argument.NameEquals?.Name?.Identifier.ValueText;

				if (valueText == "SkipWhen" || valueText == "SkipUnless")
					return;

				if (valueText == "Skip")
					skipArgument = argument;
			}

			if (skipArgument is null)
				return;

			var attributeType = context.SemanticModel.GetTypeInfo(attribute).Type;
			if (!xunitContext.Core.FactAttributeType.IsAssignableFrom(attributeType))
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
