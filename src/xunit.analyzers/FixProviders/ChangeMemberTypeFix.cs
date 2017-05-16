using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers.FixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class ChangeMemberTypeFix : MemberFixBase
    {
        const string title = "Change Member Return Type";

        public ChangeMemberTypeFix() : base(new[] {
            Constants.Descriptors.X1019_MemberDataMustReferenceMemberOfValidType.Id
        })
        { }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context, ISymbol member)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var type = TypeSymbolFactory.IEnumerableOfObjectArray(semanticModel.Compilation);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: ct => Actions.ChangeMemberType(context.Document.Project.Solution, member, type, ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }
    }
}
