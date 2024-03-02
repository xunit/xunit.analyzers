using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers;

namespace Xunit.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MakeTypesInternalSuppressor : XunitDiagnosticSuppressor
{
	public MakeTypesInternalSuppressor() :
		base(Descriptors.CA1515_Suppression)
	{ }

	protected override bool ShouldSuppress(
		Diagnostic diagnostic,
		SuppressionAnalysisContext context,
		XunitContext xunitContext)
	{
		if (diagnostic.Location.SourceTree is null)
			return false;

		var root = diagnostic.Location.SourceTree.GetRoot(context.CancellationToken);
		if (root?.FindNode(diagnostic.Location.SourceSpan) is not ClassDeclarationSyntax classDeclaration)
			return false;

		var semanticModel = context.GetSemanticModel(diagnostic.Location.SourceTree);
		var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as ITypeSymbol;
		return classSymbol.IsTestClass(xunitContext, strict: false);
	}
}
