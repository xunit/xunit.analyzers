using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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

		var factAndTheoryAttributeTypes = xunitContext.Core.FactAndTheoryAttributeTypes;
		if (factAndTheoryAttributeTypes.Count == 0)
			return;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IAttributeOperation { Operation: IObjectCreationOperation { Initializer: { } initializer } attributeCreation })
				return;

			var skipAssignment = default(ISimpleAssignmentOperation);
			foreach (var initializerOperation in initializer.Initializers)
			{
				if (initializerOperation is not ISimpleAssignmentOperation { Target: IPropertyReferenceOperation propertyReference } assignment)
					continue;

				switch (propertyReference.Property.Name)
				{
					case Constants.AttributeProperties.SkipUnless:
					case Constants.AttributeProperties.SkipWhen:
						return;

					case Constants.AttributeProperties.Skip:
						skipAssignment = assignment;
						break;
				}
			}

			if (skipAssignment is null)
				return;

			var attributeType = attributeCreation.Type;
			if (!factAndTheoryAttributeTypes.Any(f => f.IsAssignableFrom(attributeType)))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1004_TestMethodShouldNotBeSkipped,
					skipAssignment.Syntax.GetLocation()
				)
			);
		}, OperationKind.Attribute);
	}
}
