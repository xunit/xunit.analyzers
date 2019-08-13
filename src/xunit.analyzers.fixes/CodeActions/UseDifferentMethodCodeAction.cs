using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.CodeActions
{
    public class UseDifferentMethodCodeAction : CodeAction
    {
        readonly Document document;
        readonly InvocationExpressionSyntax invocation;
        readonly string replacementMethod;
        readonly string title;

        public UseDifferentMethodCodeAction(string title, Document document, InvocationExpressionSyntax invocation, string replacementMethod)
        {
            this.document = document;
            this.invocation = invocation;
            this.replacementMethod = replacementMethod;
            this.title = title;
        }

        public override string Title => title;

        public override string EquivalenceKey => Title;

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            editor.ReplaceNode(memberAccess, memberAccess.WithName((SimpleNameSyntax)editor.Generator.IdentifierName(replacementMethod)));
            return editor.GetChangedDocument();
        }
    }
}
