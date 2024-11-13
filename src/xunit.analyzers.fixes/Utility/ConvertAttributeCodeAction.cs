using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

public class ConvertAttributeCodeAction(
	string title,
	string equivalenceKey,
	Document document,
	SyntaxList<AttributeListSyntax> attributeLists,
	string fromTypeName,
	string toTypeName) :
		CodeAction
{
	readonly Document document = Guard.ArgumentNotNull(document);
	readonly string fromTypeName = Guard.ArgumentNotNull(fromTypeName);
	readonly string toTypeName = Guard.ArgumentNotNull(toTypeName);

	public override string EquivalenceKey { get; } = Guard.ArgumentNotNull(equivalenceKey);

	public override string Title { get; } = Guard.ArgumentNotNull(title);

	protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

		if (semanticModel is not null)
		{
			var fromTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(fromTypeName);

			if (fromTypeSymbol is not null)
				foreach (var attributeList in attributeLists)
					foreach (var attribute in attributeList.Attributes)
					{
						cancellationToken.ThrowIfCancellationRequested();

						var currentType = semanticModel.GetTypeInfo(attribute, cancellationToken).Type;
						if (SymbolEqualityComparer.Default.Equals(currentType, fromTypeSymbol))
							editor.SetName(attribute, toTypeName);
					}
		}

		return editor.GetChangedDocument();
	}
}
