using System.Collections.Generic;
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

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class TestMethodMustNotHaveMultipleFactAttributesFixer : BatchedCodeFixProvider
{
	public TestMethodMustNotHaveMultipleFactAttributesFixer() :
		base(Descriptors.X1002_TestMethodMustNotHaveMultipleFactAttributes.Id)
	{ }

	public static string Key_KeepAttribute(string simpleTypeName) =>
		string.Format(CultureInfo.CurrentCulture, "xUnit1002_KeepAttribute_{0}", simpleTypeName);

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (methodDeclaration is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		var attributeTypes = diagnostic.Properties.Keys.ToList();

		foreach (var attributeType in attributeTypes)
		{
			var simpleName = GetAttributeSimpleName(attributeType);

			context.RegisterCodeFix(
				CodeAction.Create(
					string.Format(CultureInfo.CurrentCulture, "Keep '{0}' attribute", simpleName),
					ct => RemoveAttributes(context.Document, methodDeclaration, attributeTypes, attributeType, ct),
					Key_KeepAttribute(simpleName)
				),
				context.Diagnostics
			);
		}
	}

	static string GetAttributeSimpleName(string attributeType)
	{
		var simpleName = attributeType;
		if (simpleName.Contains("."))
			simpleName = simpleName.Substring(attributeType.LastIndexOf('.') + 1);

		const string nameSuffix = "Attribute";
		if (simpleName.EndsWith(nameSuffix, System.StringComparison.Ordinal))
			simpleName = simpleName.Substring(0, simpleName.Length - nameSuffix.Length);

		return simpleName;
	}

	static async Task<Document> RemoveAttributes(
		Document document,
		MethodDeclarationSyntax methodDeclaration,
		IReadOnlyList<string> attributeTypesToConsider,
		string attributeTypeToKeep,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

		if (semanticModel is not null)
		{
			var oneKept = false;

			foreach (var attributeList in methodDeclaration.AttributeLists)
				foreach (var attribute in attributeList.Attributes)
				{
					var attributeType = semanticModel.GetTypeInfo(attribute, cancellationToken).Type;
					if (attributeType is null)
						continue;

					var attributeTypeDisplay = attributeType.ToDisplayString();
					if (attributeTypesToConsider.Contains(attributeTypeDisplay))
					{
						if (attributeTypeDisplay == attributeTypeToKeep && !oneKept)
						{
							oneKept = true;
							continue;
						}

						editor.RemoveNode(attribute);
					}
				}
		}

		return editor.GetChangedDocument();
	}
}
