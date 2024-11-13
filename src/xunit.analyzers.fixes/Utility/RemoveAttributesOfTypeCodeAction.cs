using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

public class RemoveAttributesOfTypeCodeAction(
	string title,
	string equivalenceKey,
	Document document,
	SyntaxList<AttributeListSyntax> attributeLists,
	string attributeType,
	bool exactMatch = false) :
		CodeAction
{
	readonly string attributeType = Guard.ArgumentNotNull(attributeType);
	readonly Document document = Guard.ArgumentNotNull(document);

	public override string EquivalenceKey { get; } = Guard.ArgumentNotNull(equivalenceKey);

	public override string Title { get; } = Guard.ArgumentNotNull(title);

	protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

		if (semanticModel is not null)
		{
			var dataAttributeType = semanticModel.Compilation.GetTypeByMetadataName(attributeType);

			if (dataAttributeType is not null)
				foreach (var attributeList in attributeLists)
					foreach (var attribute in attributeList.Attributes)
						if (dataAttributeType.IsAssignableFrom(semanticModel.GetTypeInfo(attribute, cancellationToken).Type, exactMatch))
							editor.RemoveNode(attribute);
		}

		return editor.GetChangedDocument();
	}
}
