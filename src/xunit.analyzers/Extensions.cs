using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers
{
    static class Extensions
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

        internal static bool ContainsAttributeType(this ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType, bool exactMatch = false)
        {
            return attributes.Any(a => attributeType.IsAssignableFrom(a.AttributeClass, exactMatch));
        }

        internal static ImmutableArray<ISymbol> GetInheritedAndOwnMembers(this ITypeSymbol symbol, string name = null)
        {
            var builder = ImmutableArray.CreateBuilder<ISymbol>();
            while (symbol != null)
            {
                builder.AddRange(name == null ? symbol.GetMembers() : symbol.GetMembers(name));
                symbol = symbol.BaseType;
            }
            return builder.ToImmutable();
        }

        internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType, bool exactMatch = false)
        {
            if (targetType != null)
            {
                while (sourceType != null)
                {
                    if (sourceType.Equals(targetType))
                        return true;

                    if (exactMatch)
                        return false;

                    if (targetType.TypeKind == TypeKind.Interface)
                        return sourceType.AllInterfaces.Any(i => i.Equals(targetType));

                    sourceType = sourceType.BaseType;
                }
            }

            return false;
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

        internal static bool IsEnumValueExpression(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return false;

            var symbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;
            return symbol?.Kind == SymbolKind.Field && symbol.ContainingType.TypeKind == TypeKind.Enum;
        }
    }
}
