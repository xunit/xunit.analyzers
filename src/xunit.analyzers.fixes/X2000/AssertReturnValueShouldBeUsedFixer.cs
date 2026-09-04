using System.Collections.Immutable;
using System.Composition;
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
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;
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

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		if (root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>() is not { Parent: ExpressionStatementSyntax assertStatement } invocation)
			return;

		if (context.Diagnostics.FirstOrDefault() is not Diagnostic diagnostic)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.AssertMethodName, out var assertMethodName) || assertMethodName is null)
			return;

		var rederivation = GetRederivation(root, diagnostic);
		if (rederivation is null)
			return;

		var variableName = assertMethodName == Constants.Asserts.Single ? singleVariableName : typedVariableName;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => UseReturnValue(context.Document, invocation, assertStatement, rederivation, variableName, ct),
				Key_UseReturnValue,
				"Use the return value of Assert.{0}", assertMethodName
			),
			context.Diagnostics
		);
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
		ImmutableHashSet<string> localSymbols)
	{
		var idx = 2;
		var result = baseName;

		while (localSymbols.Contains(result))
			result = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", baseName, idx++);

		return result;
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
		InvocationExpressionSyntax invocation,
		ExpressionStatementSyntax assertStatement,
		ExpressionSyntax rederivation,
		string variableName,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return document;

		var localSymbols =
			semanticModel
				.LookupSymbols(invocation.GetLocation().SourceSpan.Start)
				.OfType<ILocalSymbol>()
				.Select(s => s.Name)
				.ToImmutableHashSet();
		var safeName = GetSafeVariableName(variableName, localSymbols);

		var declaration =
			LocalDeclarationStatement(
				VariableDeclaration(
					ParseTypeName("var"),
					SingletonSeparatedList(
						VariableDeclarator(Identifier(safeName))
							.WithInitializer(EqualsValueClause(invocation.WithoutTrivia()))
					)
				).NormalizeWhitespace()
			).WithTriviaFrom(assertStatement);

		editor.ReplaceNode(assertStatement, declaration);
		editor.ReplaceNode(rederivation, IdentifierName(safeName).WithTriviaFrom(rederivation));

		return editor.GetChangedDocument();
	}
}
