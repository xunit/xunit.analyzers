using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class MemberDataShouldReferenceValidMember_ReturnTypeFixer : BatchedMemberFixProvider
{
	const string title = "Change Member Return Type";

	public MemberDataShouldReferenceValidMember_ReturnTypeFixer() :
		base(Descriptors.X1019_MemberDataMustReferenceMemberOfValidType.Id)
	{ }

	public override async Task RegisterCodeFixesAsync(
		CodeFixContext context,
		ISymbol member)
	{
		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		var type = TypeSymbolFactory.IEnumerableOfObjectArray(semanticModel.Compilation);

		context.RegisterCodeFix(
			CodeAction.Create(
				title: title,
				createChangedSolution: ct => context.Document.Project.Solution.ChangeMemberType(member, type, ct),
				equivalenceKey: title
			),
			context.Diagnostics
		);
	}
}
