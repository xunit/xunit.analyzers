using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class MemberDataShouldReferenceValidMember_ReturnTypeFixer : BatchedMemberFixProvider
{
	public const string Key_ChangeMemberReturnType = "xUnit1019_ChangeMemberReturnType";

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
				"Change return type to IEnumerable<object[]>",
				ct => context.Document.Project.Solution.ChangeMemberType(member, type, ct),
				Key_ChangeMemberReturnType
			),
			context.Diagnostics
		);
	}
}
