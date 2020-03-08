using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers
{
	internal static class SyntaxExtensions
	{
		internal static bool ContainsAttributeType(this SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel, INamedTypeSymbol attributeType, bool exactMatch = false)
		{
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

		internal static SimpleNameSyntax GetSimpleName(this InvocationExpressionSyntax invocation)
		{
			return invocation.Expression switch
			{
				MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
				SimpleNameSyntax simpleName => simpleName,
				_ => null,
			};
		}

		internal static bool IsEnumValueExpression(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
				return false;

			var symbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;
			return symbol?.Kind == SymbolKind.Field && symbol.ContainingType.TypeKind == TypeKind.Enum;
		}

		internal static bool IsNameofExpression(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!expression.IsKind(SyntaxKind.InvocationExpression))
				return false;

			var invocation = (InvocationExpressionSyntax)expression;
			if (invocation.ArgumentList.Arguments.Count != 1)
				return false;

			if ((invocation.Expression as IdentifierNameSyntax)?.Identifier.ValueText != "nameof")
				return false;

			// A real nameof expression doesn't have a matching symbol, but it does have the string type
			return semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol == null &&
				semanticModel.GetTypeInfo(expression, cancellationToken).Type?.SpecialType == SpecialType.System_String;
		}
	}
}
