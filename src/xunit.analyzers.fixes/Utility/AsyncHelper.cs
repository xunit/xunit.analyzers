using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

public static class AsyncHelper
{
	/// <summary>
	/// Get a method's modifiers that include the async keyword.
	/// </summary>
	public static SyntaxTokenList GetModifiersWithAsyncKeywordAdded(MethodDeclarationSyntax method) =>
		method.Modifiers.Any(SyntaxKind.AsyncKeyword)
			? method.Modifiers
			: method.Modifiers.Add(Token(SyntaxKind.AsyncKeyword));

	/// <summary>
	/// Get the syntax type for an updated return type to support using async.
	/// </summary>
	public static async Task<TypeSyntax?> GetReturnType(
		MethodDeclarationSyntax method,
		InvocationExpressionSyntax invocation,
		Document document,
		DocumentEditor editor,
		CancellationToken cancellationToken)
	{
		// Consider the case where a custom awaiter type is awaited
		if (invocation.Parent.IsKind(SyntaxKind.AwaitExpression))
			return method.ReturnType;

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return null;

		var methodSymbol = semanticModel.GetSymbolInfo(method.ReturnType, cancellationToken).Symbol as ITypeSymbol;
		var taskType = TypeSymbolFactory.Task(semanticModel.Compilation);
		if (taskType is null)
			return null;

		if (taskType.IsAssignableFrom(methodSymbol))
			return method.ReturnType;

		return editor.Generator.TypeExpression(taskType) as TypeSyntax;
	}
}
