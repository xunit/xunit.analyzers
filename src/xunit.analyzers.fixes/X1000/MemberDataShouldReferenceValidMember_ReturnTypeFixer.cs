using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class MemberDataShouldReferenceValidMember_ReturnTypeFixer : XunitMemberFixProvider
{
	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public const string Key_ChangeMemberReturnType_ObjectArray = "xUnit1019_ChangeMemberReturnType_ObjectArray";
	public const string Key_ChangeMemberReturnType_ITheoryDataRow = "xUnit1019_ChangeMemberReturnType_ITheoryDataRow";

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

		var objectArrayType = TypeSymbolFactory.IEnumerableOfObjectArray(semanticModel.Compilation);

		context.RegisterCodeFix(
			CodeAction.Create(
				"Change return type to IEnumerable<object[]>",
				ct => context.Document.Project.Solution.ChangeMemberType(member, objectArrayType, ct),
				Key_ChangeMemberReturnType_ObjectArray
			),
			context.Diagnostics
		);

		var theoryDataRowType = TypeSymbolFactory.IEnumerableOfITheoryDataRow(semanticModel.Compilation);

		if (theoryDataRowType is not null)
			context.RegisterCodeFix(
				CodeAction.Create(
					"Change return type to IEnumerable<ITheoryDataRow>",
					ct => context.Document.Project.Solution.ChangeMemberType(member, theoryDataRowType, ct),
					Key_ChangeMemberReturnType_ITheoryDataRow
				),
				context.Diagnostics
			);
	}
}
