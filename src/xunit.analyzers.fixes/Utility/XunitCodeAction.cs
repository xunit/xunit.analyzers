using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.CodeAnalysis.CodeActions;

internal static class XunitCodeAction
{
	public static CodeAction Create(
		Func<CancellationToken, Task<Document>> createChangedDocument,
		string equivalenceKey,
		string title) =>
			CodeAction.Create(
				title,
				createChangedDocument,
				equivalenceKey
			);

#pragma warning disable RS1035  // The prohibiton on CultureInfo.CurrentCulture does not apply to fixers

	public static CodeAction Create(
		Func<CancellationToken, Task<Document>> createChangedDocument,
		string equivalenceKey,
		string titleFormat,
		object? arg0) =>
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, titleFormat, arg0),
				createChangedDocument,
				equivalenceKey
			);

	public static CodeAction Create(
		Func<CancellationToken, Task<Document>> createChangedDocument,
		string equivalenceKey,
		string titleFormat,
		object? arg0,
		object? arg1) =>
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, titleFormat, arg0, arg1),
				createChangedDocument,
				equivalenceKey
			);

	public static CodeAction Create(
		Func<CancellationToken, Task<Document>> createChangedDocument,
		string equivalenceKey,
		string titleFormat,
		object? arg0,
		object? arg1,
		object? arg2) =>
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, titleFormat, arg0, arg1, arg2),
				createChangedDocument,
				equivalenceKey
			);

	public static CodeAction Create(
		Func<CancellationToken, Task<Document>> createChangedDocument,
		string equivalenceKey,
		string titleFormat,
		params object?[] args) =>
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, titleFormat, args),
				createChangedDocument,
				equivalenceKey
			);

#pragma warning restore RS1035

	public static CodeAction UseDifferentAssertMethod(
		string equivalenceKey,
		Document document,
		InvocationExpressionSyntax invocation,
		string replacementMethod) =>
			CodeAction.Create(
				"Use Assert." + replacementMethod,
				async ct =>
				{
					var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

					if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
						if (editor.Generator.IdentifierName(replacementMethod) is SimpleNameSyntax replacementNameSyntax)
							editor.ReplaceNode(memberAccess, memberAccess.WithName(replacementNameSyntax));

					return editor.GetChangedDocument();
				},
				equivalenceKey
			);
}
