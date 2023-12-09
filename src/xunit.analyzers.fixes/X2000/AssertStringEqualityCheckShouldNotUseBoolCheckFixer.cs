using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertStringEqualityCheckShouldNotUseBoolCheckFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2010_UseAlternateAssert";

	public AssertStringEqualityCheckShouldNotUseBoolCheckFixer() :
		base(Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.AssertMethodName, out var assertMethodName))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.IsStaticMethodCall, out var isStaticMethodCall))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.IgnoreCase, out var ignoreCaseText))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		var ignoreCase = ignoreCaseText switch
		{
			"True" => true,
			"False" => false,
			_ => default(bool?)
		};

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Use Assert.{0}", replacement),
				ct => UseEqualCheck(context.Document, invocation, replacement, isStaticMethodCall == bool.TrueString, ignoreCase, ct),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseEqualCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		bool isStaticMethodCall,
		bool? ignoreCase,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			if (invocation.ArgumentList.Arguments.Count > 0 && invocation.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax equalsInvocation)
				if (equalsInvocation.Expression is MemberAccessExpressionSyntax equalsMethodInvocation)
				{
					var equalsTarget = equalsMethodInvocation.Expression;
					var arguments =
						isStaticMethodCall
							? equalsInvocation.ArgumentList.Arguments
							: equalsInvocation.ArgumentList.Arguments.Insert(0, Argument(equalsTarget));

					if (ignoreCase == true)
						arguments = arguments.Replace(
							arguments[arguments.Count - 1],
							Argument(
								NameColon(IdentifierName(Constants.AssertArguments.IgnoreCase)),
								arguments[arguments.Count - 1].RefOrOutKeyword,
								LiteralExpression(SyntaxKind.TrueLiteralExpression)
							)
						);
					else if (ignoreCase == false)
						arguments = arguments.RemoveAt(arguments.Count - 1);

					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList(arguments)))
							.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
					);
				}

		return editor.GetChangedDocument();
	}
}
