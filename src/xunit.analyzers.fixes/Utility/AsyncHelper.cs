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
	static TypeSyntax? ConvertActionTypeToAsyncFunctionType(
		INamedTypeSymbol declarationTypeSymbol,
		Compilation compilation,
		DocumentEditor editor)
	{
		var taskTypeSymbol = TypeSymbolFactory.Task(compilation);
		if (taskTypeSymbol is null)
			return null;

		var unboundFunctionTypeSymbol = TypeSymbolFactory.Func(compilation, declarationTypeSymbol.Arity + 1);
		if (unboundFunctionTypeSymbol is null)
			return null;

		var typeArgumentsLength = declarationTypeSymbol.TypeArguments.Length + 1;
		var typeArguments = new ITypeSymbol[typeArgumentsLength];
		var returnTypeIndex = typeArgumentsLength - 1;
		declarationTypeSymbol.TypeArguments.CopyTo(typeArguments);
		typeArguments[returnTypeIndex] = taskTypeSymbol;

		var constructedFunctionTypeSymbol =
			unboundFunctionTypeSymbol
				.Construct([.. typeArguments])
				.WithNullableAnnotation(declarationTypeSymbol.NullableAnnotation);

		return editor.Generator.TypeExpression(constructedFunctionTypeSymbol) as TypeSyntax;
	}

	static TypeSyntax? ConvertFunctionTypeToAsyncFunctionType(
		INamedTypeSymbol declarationTypeSymbol,
		Compilation compilation,
		DocumentEditor editor)
	{
		var taskTypeSymbol = TypeSymbolFactory.Task(compilation);
		if (taskTypeSymbol is null)
			return null;

		var unboundTaskTypeSymbol = TypeSymbolFactory.TaskOfT(compilation);
		if (unboundTaskTypeSymbol is null)
			return null;

		var unboundFunctionTypeSymbol = TypeSymbolFactory.Func(compilation, declarationTypeSymbol.Arity);
		if (unboundFunctionTypeSymbol is null)
			return null;

		var returnTypeIndex = declarationTypeSymbol.TypeArguments.Length - 1;
		var returnTypeSymbol = declarationTypeSymbol.TypeArguments[returnTypeIndex];

		// Function return type is already a task.
		if (taskTypeSymbol.IsAssignableFrom(returnTypeSymbol))
			return null;

		var typeArguments = declarationTypeSymbol.TypeArguments.ToArray();
		var constructedTaskTypeSymbol = unboundTaskTypeSymbol.Construct(returnTypeSymbol);
		typeArguments[returnTypeIndex] = constructedTaskTypeSymbol;

		var constructedFunctionTypeSymbol =
			unboundFunctionTypeSymbol
				.Construct(typeArguments)
				.WithNullableAnnotation(declarationTypeSymbol.NullableAnnotation);

		return editor.Generator.TypeExpression(constructedFunctionTypeSymbol) as TypeSyntax;
	}

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

	/// <summary>
	/// Convert an anonymous function's declaration type to the corresponding async system delegate type, if possible.
	/// <para>
	/// If the anonymous function's declaration type is not <see langword="var"/>, and if it is a delegate type,
	/// and if it is equal to the anonymous function's inferred converted type (a system delegate type such as
	/// <see cref="Action"/>, <see cref="Action{T}"/>, <see cref="Func{TResult}"/>, etc.), then the converted
	/// async system delegate type is returned. Otherwise, or if symbols cannot be accessed or created through
	/// the semantic model, then <see langword="null"/> is returned.
	/// </para>
	/// <para>
	/// <see cref="Action"/> is converted to a <see cref="Func{TResult}"/> returning <see cref="Task"/>,
	/// and <see cref="Action{T}"/> is converted to a <see cref="Func{T, TResult}"/> returning <see cref="Task"/>, etc.
	/// </para>
	/// <para>
	/// <see cref="Func{TResult}"/> is converted to a <see cref="Func{TResult}"/> returning <see cref="Task{TResult}"/>,
	/// if it is not already, and <see cref="Func{T, TResult}"/> is converted to a <see cref="Func{T, TResult}"/>
	/// returning <see cref="Task{TResult}"/>, if it is not already, etc.
	/// </para>
	/// </summary>
	public static async Task<TypeSyntax?> GetAsyncSystemDelegateType(
		VariableDeclarationSyntax declaration,
		AnonymousFunctionExpressionSyntax anonymousFunction,
		DocumentEditor editor,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(declaration);
		Guard.ArgumentNotNull(anonymousFunction);
		Guard.ArgumentNotNull(editor);

		var semanticModel = await editor.OriginalDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

		if (semanticModel is not null
			&& semanticModel.GetTypeInfo(declaration.Type, cancellationToken).Type is INamedTypeSymbol declarationTypeSymbol
			&& !declaration.Type.IsVar
			&& declarationTypeSymbol.TypeKind == TypeKind.Delegate
			&& semanticModel.GetTypeInfo(anonymousFunction, cancellationToken).ConvertedType is ITypeSymbol functionTypeSymbol
			&& editor.Generator.TypeExpression(declarationTypeSymbol) is TypeSyntax declaredType
			&& editor.Generator.TypeExpression(functionTypeSymbol) is TypeSyntax functionType
			&& declaredType.IsEquivalentTo(functionType))
		{
			var compilation = semanticModel.Compilation;

			if (IsSystemActionType(declarationTypeSymbol, compilation))
				return ConvertActionTypeToAsyncFunctionType(declarationTypeSymbol, compilation, editor);

			if (IsSystemFunctionType(declarationTypeSymbol, compilation))
				return ConvertFunctionTypeToAsyncFunctionType(declarationTypeSymbol, compilation, editor);
		}

		return null;
	}

	static bool IsSystemActionType(
		INamedTypeSymbol typeSymbol,
		Compilation compilation)
	{
		var arity = typeSymbol.Arity;

		if (typeSymbol.Name == "Action")
		{
			if (arity == 0)
				return SymbolEqualityComparer.Default.Equals(typeSymbol.ConstructedFrom, TypeSymbolFactory.Action(compilation));

			if (arity is >= 1 and <= 16)
				return SymbolEqualityComparer.Default.Equals(typeSymbol.ConstructedFrom, TypeSymbolFactory.Action(compilation, arity));
		}

		return false;
	}

	static bool IsSystemFunctionType(
		INamedTypeSymbol typeSymbol,
		Compilation compilation)
	{
		var arity = typeSymbol.Arity;

		if (typeSymbol.Name == "Func" && arity >= 1 && arity <= 17)
			return SymbolEqualityComparer.Default.Equals(typeSymbol.ConstructedFrom, TypeSymbolFactory.Func(compilation, arity));

		return false;
	}
}
