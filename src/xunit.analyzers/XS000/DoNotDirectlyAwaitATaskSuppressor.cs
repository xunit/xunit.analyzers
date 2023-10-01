using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers.Utility;

namespace Xunit.Analyzers.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotDirectlyAwaitATaskSuppressor : XunitDiagnosticSuppressor
{
	public DoNotDirectlyAwaitATaskSuppressor()
		: base(Descriptors.XS001_DoNotDirectlyAwaitATaskSuppressor)
	{
	}

	public override void ReportSuppressions(SuppressionAnalysisContext context, XunitContext xunitContext)
	{
		var cancellationToken = context.CancellationToken;

		foreach (var diagnostic in context.ReportedDiagnostics)
		{
			var location = diagnostic.Location;
			var sourceTree = location.SourceTree;
			if (sourceTree == null)
				continue;

			var root = sourceTree.GetRoot(cancellationToken);

			var sourceSpan = location.SourceSpan;
			var elementNode = root.FindNode(sourceSpan);
			var semanticModel = context.GetSemanticModel(elementNode.SyntaxTree);
			var operation = semanticModel.GetOperation(elementNode, cancellationToken);

			if (operation?.IsInTestMethod(xunitContext) != true)
				return;

			context.ReportSuppression(Suppression.Create(SupportedSuppressions[0], diagnostic));
		}
	}
}
