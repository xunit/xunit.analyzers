using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class MemberDataShouldReferenceValidMember_VisibilityFixer : BatchedMemberFixProvider
{
	public const string Key_MakeMemberPublic = "xUnit1016_MakeMemberPublic";

	public MemberDataShouldReferenceValidMember_VisibilityFixer() :
		base(Descriptors.X1016_MemberDataMustReferencePublicMember.Id)
	{ }

	public override Task RegisterCodeFixesAsync(
		CodeFixContext context,
		ISymbol member)
	{
		context.RegisterCodeFix(
			CodeAction.Create(
				"Make member public",
				ct => context.Document.Project.Solution.ChangeMemberAccessibility(member, Accessibility.Public, ct),
				Key_MakeMemberPublic
			),
			context.Diagnostics
		);

		return Task.CompletedTask;
	}
}
