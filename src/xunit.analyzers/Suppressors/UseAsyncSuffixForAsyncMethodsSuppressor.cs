using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers;

namespace Xunit.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseAsyncSuffixForAsyncMethodsSuppressor : XunitDiagnosticSuppressor
{
	public UseAsyncSuffixForAsyncMethodsSuppressor() :
		base(Descriptors.VSTHRD200_Suppression)
	{ }

	protected override bool ShouldSuppress(
		Diagnostic diagnostic,
		SuppressionAnalysisContext context,
		XunitContext xunitContext)
	{
		var attributeUsageType = TypeSymbolFactory.AttributeUsageAttribute(context.Compilation);
		if (attributeUsageType is null)
			return false;

		if (diagnostic?.Location.SourceTree is null)
			return false;

		if (diagnostic.Location.SourceTree.GetRoot().FindNode(diagnostic.Location.SourceSpan) is not MethodDeclarationSyntax methodDeclaration)
			return false;

		var semanticModel = context.GetSemanticModel(diagnostic.Location.SourceTree);
		var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
		return methodSymbol.IsTestMethod(xunitContext, attributeUsageType, strict: false);
	}
}
