using System.Collections.Generic;
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

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class TestMethodMustNotHaveMultipleFactAttributesFixer : CodeFixProvider
	{
		const string titleTemplate = "Keep {0} Attribute";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X1002_TestMethodMustNotHaveMultipleFactAttributes.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

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
				var title = string.Format(titleTemplate, simpleName);

				context.RegisterCodeFix(
					CodeAction.Create(
						title: title,
						createChangedDocument: ct => RemoveAttributes(context.Document, methodDeclaration, attributeTypes, attributeType, ct),
						equivalenceKey: title
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
			if (simpleName.EndsWith(nameSuffix))
				simpleName = simpleName.Substring(0, simpleName.Length - nameSuffix.Length);

			return simpleName;
		}

		async Task<Document> RemoveAttributes(
			Document document,
			MethodDeclarationSyntax methodDeclaration,
			IReadOnlyList<string> attributeTypesToConsider,
			string attributeTypeToKeep,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

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
}
