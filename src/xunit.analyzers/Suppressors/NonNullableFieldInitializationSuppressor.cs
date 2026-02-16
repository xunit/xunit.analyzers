using System.Globalization;
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

		var memberSymbol = ResolveMemberSymbol(diagnostic, node, semanticModel, context);
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

		return IsMemberAssignedInMethod(initializeAsyncImpl, memberSymbol, context);
	}

	static ISymbol? ResolveMemberSymbol(
		Diagnostic diagnostic,
		SyntaxNode node,
		SemanticModel semanticModel,
		SuppressionAnalysisContext context)
	{
		// CS8618 can target field variable declarators or property declarations directly
		ISymbol? memberSymbol = node switch
		{
			VariableDeclaratorSyntax variableDeclarator => semanticModel.GetDeclaredSymbol(variableDeclarator),
			PropertyDeclarationSyntax propertyDeclaration => semanticModel.GetDeclaredSymbol(propertyDeclaration),
			_ => null,
		};

		if (memberSymbol is not null)
			return memberSymbol;

		// Check AdditionalLocations (some compiler versions include member location here)
		foreach (var additionalLocation in diagnostic.AdditionalLocations)
		{
			if (additionalLocation.SourceTree is null)
				continue;

			var addRoot = additionalLocation.SourceTree.GetRoot(context.CancellationToken);
			var addNode = addRoot.FindNode(additionalLocation.SourceSpan);
			var addModel = context.GetSemanticModel(additionalLocation.SourceTree);
			var symbol = addModel.GetDeclaredSymbol(addNode, context.CancellationToken);
			if (symbol is IFieldSymbol or IPropertySymbol)
				return symbol;
		}

		// Fallback: CS8618 on a constructor — extract member name from diagnostic message
		var declaredSymbol = semanticModel.GetDeclaredSymbol(node, context.CancellationToken);
		if (declaredSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructorSymbol)
		{
			var message = diagnostic.GetMessage(CultureInfo.InvariantCulture);
			var startQuote = message.IndexOf('\'');
			if (startQuote >= 0)
			{
				var endQuote = message.IndexOf('\'', startQuote + 1);
				if (endQuote > startQuote)
				{
					var memberName = message.Substring(startQuote + 1, endQuote - startQuote - 1);
					return constructorSymbol.ContainingType
						.GetMembers(memberName)
						.FirstOrDefault(m => m is IFieldSymbol or IPropertySymbol);
				}
			}
		}

		return null;
	}

	static bool IsMemberAssignedInMethod(
		ISymbol methodSymbol,
		ISymbol targetMember,
		SuppressionAnalysisContext context)
	{
		foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
		{
			var methodSyntax = syntaxRef.GetSyntax(context.CancellationToken);
			if (methodSyntax is not MethodDeclarationSyntax methodDecl)
				continue;

			var methodSemanticModel = context.GetSemanticModel(methodSyntax.SyntaxTree);

			foreach (var assignment in methodDecl.DescendantNodes().OfType<AssignmentExpressionSyntax>())
			{
				var assignedSymbol = methodSemanticModel.GetSymbolInfo(assignment.Left).Symbol;
				if (assignedSymbol is not null && SymbolEqualityComparer.Default.Equals(assignedSymbol, targetMember))
					return true;
			}
		}

		return false;
	}
}
