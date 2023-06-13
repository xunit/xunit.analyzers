using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixer : CodeFixProvider
	{
		const string title = "Make Parameter Nullable";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root is null)
				return;

			var node = root.FindNode(context.Span);
			var diagnostic = context.Diagnostics.FirstOrDefault();
			if (diagnostic is null)
				return;
			if (!diagnostic.Properties.TryGetValue(Constants.Properties.ParameterIndex, out var parameterIndexText))
				return;
			if (!int.TryParse(parameterIndexText, out var parameterIndex))
				return;

			var diagnosticId = diagnostic.Id;
			var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			if (method is null)
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					ct => MakeParameterNullable(context.Document, method, parameterIndex, ct),
					title
				),
				context.Diagnostics
			);
		}

		async Task<Document> MakeParameterNullable(
			Document document,
			MethodDeclarationSyntax method,
			int parameterIndex,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			if (method.ParameterList.Parameters.Count > parameterIndex)
			{
				var param = method.ParameterList.Parameters[parameterIndex];
				var semanticModel = editor.SemanticModel;

				if (semanticModel is not null && param.Type is not null)
				{
					var nullableT = semanticModel.Compilation.GetSpecialType(SpecialType.System_Nullable_T);
					var paramTypeSymbol = semanticModel.GetTypeInfo(param.Type, cancellationToken).Type;

					if (paramTypeSymbol is not null)
						editor.SetType(param, editor.Generator.TypeExpression(nullableT.Construct(paramTypeSymbol)));
				}
			}

			return editor.GetChangedDocument();
		}
	}
}
