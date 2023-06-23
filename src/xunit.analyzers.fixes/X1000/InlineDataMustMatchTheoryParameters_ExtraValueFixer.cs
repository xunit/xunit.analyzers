using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class InlineDataMustMatchTheoryParameters_ExtraValueFixer : BatchedCodeFixProvider
{
	public const string AddTheoryParameterTitle = "Add Theory Parameter";
	public const string RemoveValueTitle = "Remove Value";

	public InlineDataMustMatchTheoryParameters_ExtraValueFixer() :
		base(Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var node = root.FindNode(context.Span);
		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		// Fix #1: remove the extra data from the inline data attribute
		context.RegisterCodeFix(
			CodeAction.Create(
				RemoveValueTitle,
				ct => context.Document.RemoveNode(node, ct),
				RemoveValueTitle
			),
			context.Diagnostics
		);

		// Fix #2: add a parameter to the theory for the extra data
		// (only valid for the first item after the supported parameters are exhausted)
		var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (method is null)
			return;

		var parameterIndexText = diagnostic.Properties[Constants.Properties.ParameterIndex];

		if (parameterIndexText is not null)
		{
			var parameterIndex = int.Parse(parameterIndexText);
			Enum.TryParse<SpecialType>(diagnostic.Properties[Constants.Properties.ParameterSpecialType], out var parameterSpecialType);

			var existingParameters = method.ParameterList.Parameters.Select(p => p.Identifier.Text).ToImmutableHashSet();
			var parameterName = "p";
			var nextIndex = 2;
			while (existingParameters.Contains(parameterName))
				parameterName = $"p_{nextIndex++}";

			if (method.ParameterList.Parameters.Count == parameterIndex)
				context.RegisterCodeFix(
					CodeAction.Create(
						AddTheoryParameterTitle,
						ct => AddTheoryParameter(context.Document, method, parameterSpecialType, parameterName, ct),
						AddTheoryParameterTitle
					),
					context.Diagnostics
				);
		}
	}

	async Task<Document> AddTheoryParameter(
		Document document,
		MethodDeclarationSyntax method,
		SpecialType parameterSpecialType,
		string parameterName,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var parameterTypeExpression =
			parameterSpecialType != SpecialType.None
				? editor.Generator.TypeExpression(parameterSpecialType)
				: editor.Generator.TypeExpression(SpecialType.System_Object);

		editor.AddParameter(
			method,
			editor.Generator.ParameterDeclaration(parameterName, parameterTypeExpression)
		);

		return editor.GetChangedDocument();
	}
}
