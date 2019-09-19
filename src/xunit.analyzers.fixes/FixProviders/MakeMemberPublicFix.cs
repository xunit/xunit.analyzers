using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers.FixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class MakeMemberPublicFix : MemberFixBase
    {
        const string title = "Make Member Public";

        public MakeMemberPublicFix() : base(new[] {
            Descriptors.X1016_MemberDataMustReferencePublicMember.Id
        })
        { }

        public override Task RegisterCodeFixesAsync(CodeFixContext context, ISymbol member)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: ct => Actions.ChangeMemberAccessibility(context.Document.Project.Solution, member, Accessibility.Public, ct),
                    equivalenceKey: title),
                context.Diagnostics);
            return Task.FromResult(0);
        }
    }
}
