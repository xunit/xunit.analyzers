using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

static class CodeAnalysisExtensions
{
	public static bool IsInTestMethod(
		this IOperation operation,
		XunitContext xunitContext)
	{
		if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null)
			return false;

		var semanticModel = operation.SemanticModel;
		if (semanticModel is null)
			return false;

		for (var parent = operation.Parent; parent != null; parent = parent.Parent)
		{
			if (parent is not IMethodBodyOperation methodBodyOperation)
				continue;
			if (methodBodyOperation.Syntax is not MethodDeclarationSyntax methodSyntax)
				continue;

			return methodSyntax.AttributeLists.SelectMany(list => list.Attributes).Any(attr =>
			{
				var typeInfo = semanticModel.GetTypeInfo(attr);
				if (typeInfo.Type is null)
					return false;

				return
					SymbolEqualityComparer.Default.Equals(typeInfo.Type, xunitContext.Core.FactAttributeType) ||
					SymbolEqualityComparer.Default.Equals(typeInfo.Type, xunitContext.Core.TheoryAttributeType);
			});
		}

		return false;
	}

	public static IOperation WalkDownImplicitConversions(this IOperation operation)
	{
		var current = operation;
		while (current is IConversionOperation conversion && conversion.Conversion.IsImplicit)
			current = conversion.Operand;

		return current;
	}
}
