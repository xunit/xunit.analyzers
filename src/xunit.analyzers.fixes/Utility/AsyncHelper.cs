using System;
using System.Linq;
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
	/// Get a list of modifiers with the <see langword="async"/> keyword added if not already present.
	/// </summary>
	public static SyntaxTokenList GetModifiersWithAsyncKeywordAdded(SyntaxTokenList modifiers)
	{
		return modifiers.Any(SyntaxKind.AsyncKeyword)
			? modifiers
			: modifiers.Add(Token(SyntaxKind.AsyncKeyword));
	}

	/// <summary>
	/// Convert a return type to the corresponding async return type, if possible.
	/// <para>
	/// If the return type is already a <see cref="Task"/> or <see cref="Task{TResult}"/>, then <see langword="null"/> is returned.
	/// If the return type is <see langword="void"/>, then a <see cref="Task"/> return type is returned.
	/// If the return type is another type, then a <see cref="Task{TResult}"/> of that type is returned.
	/// However, if symbols cannot be accessed or created through the semantic model, then <see langword="null"/> is returned.
	/// </para>
	/// </summary>
	public static async Task<TypeSyntax?> GetAsyncReturnType(
		TypeSyntax returnType,
		DocumentEditor editor,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(returnType);
		Guard.ArgumentNotNull(editor);

		var semanticModel = await editor.OriginalDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

		if (semanticModel is not null
			&& semanticModel.GetSymbolInfo(returnType, cancellationToken).Symbol is ITypeSymbol returnTypeSymbol
			&& TypeSymbolFactory.Task(semanticModel.Compilation) is INamedTypeSymbol taskTypeSymbol)
		{
			if (returnType is PredefinedTypeSyntax predefinedReturnType && predefinedReturnType.Keyword.IsKind(SyntaxKind.VoidKeyword))
				return editor.Generator.TypeExpression(taskTypeSymbol) as TypeSyntax;

			// Return type is already a task.
			if (taskTypeSymbol.IsAssignableFrom(returnTypeSymbol))
				return null;

			if (TypeSymbolFactory.TaskOfT(semanticModel.Compilation) is INamedTypeSymbol unboundTaskTypeSymbol)
			{
				var constructedTaskTypeSymbol = unboundTaskTypeSymbol.Construct(returnTypeSymbol);
				return editor.Generator.TypeExpression(constructedTaskTypeSymbol) as TypeSyntax;
			}
		}

		return null;
	}
}
