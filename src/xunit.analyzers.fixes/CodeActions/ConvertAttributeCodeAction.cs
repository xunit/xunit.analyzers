using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.CodeActions
{
	public class ConvertAttributeCodeAction : CodeAction
	{
		readonly SyntaxList<AttributeListSyntax> attributeLists;
		readonly Document document;
		readonly string fromTypeName;
		readonly string toTypeName;

		public ConvertAttributeCodeAction(
			string title,
			Document document,
			SyntaxList<AttributeListSyntax> attributeLists,
			string fromTypeName,
			string toTypeName)
		{
			Title = title;

			this.toTypeName = toTypeName;
			this.fromTypeName = fromTypeName;
			this.attributeLists = attributeLists;
			this.document = document;
		}

		public override string Title { get; }

		public override string EquivalenceKey => Title;

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var fromTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(fromTypeName);

			if (fromTypeSymbol is not null)
				foreach (var attributeList in attributeLists)
					foreach (var attribute in attributeList.Attributes)
					{
						cancellationToken.ThrowIfCancellationRequested();

						var currentType = semanticModel.GetTypeInfo(attribute).Type;
						if (Equals(currentType, fromTypeSymbol))
							editor.SetName(attribute, toTypeName);
					}

			return editor.GetChangedDocument();
		}
	}
}
