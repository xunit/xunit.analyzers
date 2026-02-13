using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class UseCancellationTokenFixer : XunitCodeFixProvider
{
	public const string Key_UseCancellationTokenArgument = "xUnit1051_UseCancellationTokenArgument";

	public UseCancellationTokenFixer() :
		base(Descriptors.X1051_UseCancellationToken.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		var testContextType = TypeSymbolFactory.TestContext_V3(semanticModel.Compilation);
		if (testContextType is null)
			return;

		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		if (!diagnostic.Properties.TryGetValue(Constants.Properties.ParameterName, out var parameterName))
			return;
		if (parameterName is null)
			return;

		if (!diagnostic.Properties.TryGetValue(Constants.Properties.ParameterIndex, out var parameterIndexText))
			return;
		if (!int.TryParse(parameterIndexText, out var parameterIndex))
			return;

		if (root.FindNode(diagnostic.Location.SourceSpan) is not InvocationExpressionSyntax invocation)
			return;

		if (semanticModel.GetOperation(invocation, context.CancellationToken) is not IInvocationOperation invocationOperation)
			return;

		var arguments = invocation.ArgumentList.Arguments;

		for (var argumentIndex = 0; argumentIndex < arguments.Count; argumentIndex++)
			if (arguments[argumentIndex].NameColon?.Name.Identifier.Text == parameterName)
			{
				parameterIndex = argumentIndex;
				break;
			}

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				async ct =>
				{
					var editor = await DocumentEditor.CreateAsync(context.Document, ct).ConfigureAwait(false);

					var testContextCancellationTokenExpression = (ExpressionSyntax)editor.Generator.MemberAccessExpression(
						editor.Generator.MemberAccessExpression(
							editor.Generator.TypeExpression(testContextType),
							"Current"
						),
						"CancellationToken"
					);

					var args = new List<ArgumentSyntax>(arguments);

					if (invocationOperation.Arguments.FirstOrDefault(arg => arg.ArgumentKind == ArgumentKind.ParamArray) is { } paramsArgument)
						TransformParamsArgument(args, paramsArgument, editor.Generator);

					if (parameterIndex < args.Count)
						args[parameterIndex] = args[parameterIndex].WithExpression(testContextCancellationTokenExpression);
					else
					{
						var argument = Argument(testContextCancellationTokenExpression);
						if (parameterIndex > args.Count || args.Any(arg => arg.NameColon is not null))
							argument = argument.WithNameColon(NameColon(parameterName));

						args.Add(argument);
					}

					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList(args)))
					);

					return editor.GetChangedDocument();
				},
				Key_UseCancellationTokenArgument,
				"{0} TestContext.Current.CancellationToken", parameterIndex < arguments.Count ? "Use" : "Add"
			),
			context.Diagnostics
		);
	}

	static void TransformParamsArgument(
		List<ArgumentSyntax> arguments,
		IArgumentOperation paramsOperation,
		SyntaxGenerator generator)
	{
		if (paramsOperation is not { Value: IArrayCreationOperation { Type: IArrayTypeSymbol arrayTypeSymbol, Initializer: { } initializer } })
			return;

		// We know that the params arguments occupy the end of the list because the language
		// does not allow regular arguments after params.
		int paramsCount = initializer.ElementValues.Length;
		int paramsStart = arguments.Count - paramsCount;

		arguments.RemoveRange(paramsStart, paramsCount);

		ExpressionSyntax arrayCreation = (ExpressionSyntax)generator.ArrayCreationExpression(
			generator.TypeExpression(arrayTypeSymbol.ElementType),
			initializer.ElementValues.Select(x => x.Syntax)
		);

		arguments.Add(Argument(arrayCreation));
	}
}
