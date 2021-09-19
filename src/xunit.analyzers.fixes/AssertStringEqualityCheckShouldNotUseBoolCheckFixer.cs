using System.Collections.Immutable;
using System.Composition;
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

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertStringEqualityCheckShouldNotUseBoolCheckFixer : CodeFixProvider
	{
		const string equivalenceKeyTemplate = "Use Assert.{0} for string equality checks";
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
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

			var ignoreCase = ignoreCaseText switch
			{
				"True" => true,
				"False" => false,
				_ => default(bool?)
			};

			context.RegisterCodeFix(
				CodeAction.Create(
					string.Format(titleTemplate, replacement),
					createChangedDocument: ct => UseEqualCheck(context.Document, invocation, replacement, isStaticMethodCall == bool.TrueString, ignoreCase, ct),
					equivalenceKey: string.Format(equivalenceKeyTemplate, replacement)
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
}
