using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers;

namespace Xunit.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonNullableFieldInitializationSuppressor : XunitDiagnosticSuppressor
{
	public NonNullableFieldInitializationSuppressor() :
		base(Descriptors.CS8618_Suppression)
	{ }

	protected override bool ShouldSuppress(
		Diagnostic diagnostic,
		SuppressionAnalysisContext context,
		XunitContext xunitContext)
	{
		if (diagnostic.Location.SourceTree is null)
			return false;

		var asyncLifetimeType = TypeSymbolFactory.IAsyncLifetime(context.Compilation);
		if (asyncLifetimeType is null)
			return false;

		var root = diagnostic.Location.SourceTree.GetRoot(context.CancellationToken);
		var node = root?.FindNode(diagnostic.Location.SourceSpan);
		if (node is null)
			return false;

		var semanticModel = context.GetSemanticModel(diagnostic.Location.SourceTree);

		// CS8618 can target field variable declarators or property declarations
		ISymbol? memberSymbol = node switch
		{
			VariableDeclaratorSyntax variableDeclarator => semanticModel.GetDeclaredSymbol(variableDeclarator),
			PropertyDeclarationSyntax propertyDeclaration => semanticModel.GetDeclaredSymbol(propertyDeclaration),
			_ => null,
		};

		if (memberSymbol is null)
			return false;

		var containingType = memberSymbol.ContainingType;
		if (containingType is null)
			return false;

		if (!containingType.AllInterfaces.Contains(asyncLifetimeType, SymbolEqualityComparer.Default))
			return false;

		// Find the InitializeAsync method implementation
		var initializeAsyncInterfaceMethod = asyncLifetimeType.GetMembers("InitializeAsync").FirstOrDefault();
		if (initializeAsyncInterfaceMethod is null)
			return false;

		var initializeAsyncImpl = containingType.FindImplementationForInterfaceMember(initializeAsyncInterfaceMethod);
		if (initializeAsyncImpl is null)
			return false;

		// Check if the member is assigned in InitializeAsync
		foreach (var syntaxRef in initializeAsyncImpl.DeclaringSyntaxReferences)
		{
			var methodSyntax = syntaxRef.GetSyntax(context.CancellationToken);
			if (methodSyntax is not MethodDeclarationSyntax methodDecl)
				continue;

			var methodSemanticModel = context.GetSemanticModel(methodSyntax.SyntaxTree);

			foreach (var assignment in methodDecl.DescendantNodes().OfType<AssignmentExpressionSyntax>())
			{
				var assignedSymbol = methodSemanticModel.GetSymbolInfo(assignment.Left).Symbol;
				if (assignedSymbol is not null && SymbolEqualityComparer.Default.Equals(assignedSymbol, memberSymbol))
					return true;
			}
		}

		return false;
	}
}
