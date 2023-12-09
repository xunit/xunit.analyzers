using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers;

public static class SyntaxExtensions
{
	public static bool ContainsAttributeType(
		this SyntaxList<AttributeListSyntax> attributeLists,
		SemanticModel semanticModel,
		INamedTypeSymbol attributeType,
		bool exactMatch = false)
	{
		Guard.ArgumentNotNull(semanticModel);
		Guard.ArgumentNotNull(attributeType);

		foreach (var attributeList in attributeLists)
		{
			foreach (var attribute in attributeList.Attributes)
			{
				var type = semanticModel.GetTypeInfo(attribute).Type;
				if (attributeType.IsAssignableFrom(type, exactMatch))
					return true;
			}
		}

		return false;
	}

	public static SimpleNameSyntax? GetSimpleName(this InvocationExpressionSyntax invocation) =>
		Guard.ArgumentNotNull(invocation).Expression switch
		{
			MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
			SimpleNameSyntax simpleName => simpleName,
			_ => null,
		};

	public static bool IsEnumValueExpression(
		this ExpressionSyntax expression,
		SemanticModel semanticModel,
		CancellationToken cancellationToken = default)
	{
		Guard.ArgumentNotNull(expression);
		Guard.ArgumentNotNull(semanticModel);

		if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			return false;

		var symbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;
		return symbol?.Kind == SymbolKind.Field && symbol.ContainingType.TypeKind == TypeKind.Enum;
	}

	public static bool IsNameofExpression(
		this ExpressionSyntax expression,
		SemanticModel semanticModel,
		CancellationToken cancellationToken = default)
	{
		Guard.ArgumentNotNull(expression);
		Guard.ArgumentNotNull(semanticModel);

		if (!expression.IsKind(SyntaxKind.InvocationExpression))
			return false;
		if (expression is not InvocationExpressionSyntax invocation)
			return false;
		if (invocation.ArgumentList.Arguments.Count != 1)
			return false;
		if ((invocation.Expression as IdentifierNameSyntax)?.Identifier.ValueText != "nameof")
			return false;

		// A real nameof expression doesn't have a matching symbol, but it does have the string type
		return
			semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol is null
			&& semanticModel.GetTypeInfo(expression, cancellationToken).Type?.SpecialType == SpecialType.System_String;
	}
}
