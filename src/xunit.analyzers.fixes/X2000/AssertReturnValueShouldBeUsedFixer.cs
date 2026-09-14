using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertReturnValueShouldBeUsedFixer : XunitCodeFixProvider
{
	const string singleVariableName = "item";
	const string typedVariableName = "typed";

	public const string Key_UseReturnValue = "xUnit2033_UseReturnValue";

	public AssertReturnValueShouldBeUsedFixer() :
		base(Descriptors.X2033_AssertReturnValueShouldBeUsed.Id)
	{ }

	// The fix introduces new local variables, so the batch fixer cannot be used: it would compute each
	// variable name independently and produce duplicate declarations. Instead, all the fixes in a document
	// are applied in a single pass which knows about the names it has already introduced.
	public override FixAllProvider? GetFixAllProvider() =>
		FixAllProvider.Create(FixAllInDocument);

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		if (context.Diagnostics.FirstOrDefault() is not Diagnostic diagnostic)
			return;
		if (!TryGetFixTarget(root, context.Span, diagnostic, out var target))
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => UseReturnValue(context.Document, target, ct),
				Key_UseReturnValue,
				"Use the return value of Assert.{0}", target.AssertMethodName
			),
			context.Diagnostics
		);
	}

	static void ApplyFix(
		DocumentEditor editor,
		FixTarget target,
		string variableName)
	{
		// Use the callback overloads, so that edits nested inside one another (for example, a re-derivation
		// which is the argument of a later assertion) compose when fixing all diagnostics at once.
		editor.ReplaceNode(target.AssertStatement, (current, _) =>
			current is ExpressionStatementSyntax { Expression: var invocation }
				? LocalDeclarationStatement(
					VariableDeclaration(
						ParseTypeName("var"),
						SingletonSeparatedList(
							VariableDeclarator(Identifier(variableName))
								.WithInitializer(EqualsValueClause(invocation.WithoutTrivia()))
						)
					).NormalizeWhitespace()
				).WithTriviaFrom(current)
				: current
		);
		editor.ReplaceNode(target.Rederivation, (current, _) => IdentifierName(variableName).WithTriviaFrom(current));
	}

	static async Task<Document?> FixAllInDocument(
		FixAllContext fixAllContext,
		Document document,
		ImmutableArray<Diagnostic> diagnostics)
	{
		var cancellationToken = fixAllContext.CancellationToken;
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (root is null || semanticModel is null)
			return document;

		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var claimedRederivations = new List<TextSpan>();
		var introducedVariables = new List<(SyntaxNode Scope, string Name)>();

		foreach (var diagnostic in diagnostics.OrderBy(d => d.Location.SourceSpan.Start))
		{
			if (!TryGetFixTarget(root, diagnostic.Location.SourceSpan, diagnostic, out var target) || target.AssertStatement.Parent is not SyntaxNode scope)
				continue;

			// Fixing an earlier diagnostic replaces its re-derivation with the new variable. When that code is part
			// of this assertion or of its re-derivation, this diagnostic would no longer be reported once the earlier
			// fix is applied, so it is skipped (the same result as applying the fixes one at a time).
			if (claimedRederivations.Any(span => span.OverlapsWith(target.Invocation.Span) || span.OverlapsWith(target.Rederivation.Span)))
				continue;

			claimedRederivations.Add(target.Rederivation.Span);

			// A variable introduced by an earlier fix conflicts when it is declared in the same scope or in a scope
			// which contains (or is contained by) this one; variables in sibling scopes can safely share a name.
			var localNames = GetLocalNames(semanticModel, target.Invocation, cancellationToken);
			var variableName = GetSafeVariableName(
				target.BaseVariableName,
				name => localNames.Contains(name) || introducedVariables.Any(v => v.Name == name && (v.Scope.Span.Contains(scope.Span) || scope.Span.Contains(v.Scope.Span)))
			);

			introducedVariables.Add((scope, variableName));
			ApplyFix(editor, target, variableName);
		}

		return editor.GetChangedDocument();
	}

	static ImmutableHashSet<string> GetLocalNames(
		SemanticModel semanticModel,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return
			semanticModel
				.LookupSymbols(invocation.SpanStart)
				.OfType<ILocalSymbol>()
				.Select(s => s.Name)
				.ToImmutableHashSet();
	}

	static ExpressionSyntax? GetRederivation(
		SyntaxNode root,
		Diagnostic diagnostic)
	{
		if (!TryGetIntProperty(diagnostic, Constants.Properties.RederivationSpanStart, out var start))
			return null;
		if (!TryGetIntProperty(diagnostic, Constants.Properties.RederivationSpanLength, out var length))
			return null;

		return root.FindNode(new TextSpan(start, length), getInnermostNodeForTie: true) as ExpressionSyntax;
	}

	static string GetSafeVariableName(
		string baseName,
		Func<string, bool> isNameTaken)
	{
		var idx = 2;
		var result = baseName;

		while (isNameTaken(result))
			result = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", baseName, idx++);

		return result;
	}

	static bool TryGetFixTarget(
		SyntaxNode root,
		TextSpan span,
		Diagnostic diagnostic,
		[NotNullWhen(true)] out FixTarget? target)
	{
		target = null;

		if (root.FindNode(span).FirstAncestorOrSelf<InvocationExpressionSyntax>() is not { Parent: ExpressionStatementSyntax assertStatement } invocation)
			return false;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.AssertMethodName, out var assertMethodName) || assertMethodName is null)
			return false;

		var rederivation = GetRederivation(root, diagnostic);
		if (rederivation is null)
			return false;

		target = new FixTarget(assertMethodName, invocation, assertStatement, rederivation);
		return true;
	}

	static bool TryGetIntProperty(
		Diagnostic diagnostic,
		string key,
		out int value)
	{
		value = 0;

		return
			diagnostic.Properties.TryGetValue(key, out var text)
			&& int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
	}

	static async Task<Document> UseReturnValue(
		Document document,
		FixTarget target,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return document;

		var localNames = GetLocalNames(semanticModel, target.Invocation, cancellationToken);
		ApplyFix(editor, target, GetSafeVariableName(target.BaseVariableName, localNames.Contains));

		return editor.GetChangedDocument();
	}

	sealed class FixTarget(
		string assertMethodName,
		InvocationExpressionSyntax invocation,
		ExpressionStatementSyntax assertStatement,
		ExpressionSyntax rederivation)
	{
		public string AssertMethodName { get; } = assertMethodName;

		public ExpressionStatementSyntax AssertStatement { get; } = assertStatement;

		public string BaseVariableName =>
			AssertMethodName == Constants.Asserts.Single ? singleVariableName : typedVariableName;

		public InvocationExpressionSyntax Invocation { get; } = invocation;

		public ExpressionSyntax Rederivation { get; } = rederivation;
	}
}
