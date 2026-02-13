using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class MemberDataShouldReferenceValidMember_StaticFixer : XunitMemberFixProvider
{
	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public const string Key_MakeMemberStatic = "xUnit1017_MakeMemberStatic";

	public MemberDataShouldReferenceValidMember_StaticFixer() :
		base(Descriptors.X1017_MemberDataMustReferenceStaticMember.Id)
	{ }

	public override Task RegisterCodeFixesAsync(
		CodeFixContext context,
		ISymbol member)
	{
		context.RegisterCodeFix(
			CodeAction.Create(
				"Make member static",
				ct => context.Document.Project.Solution.ChangeMemberStaticModifier(member, true, ct),
				Key_MakeMemberStatic
			),
			context.Diagnostics
		);

		return Task.CompletedTask;
	}
}
