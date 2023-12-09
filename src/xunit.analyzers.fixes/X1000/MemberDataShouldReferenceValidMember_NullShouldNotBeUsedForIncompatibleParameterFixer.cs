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

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer : BatchedCodeFixProvider
{
	public const string Key_MakeParameterNullable = "xUnit1034_MakeParameterNullable";

	public MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer() :
		base(Descriptors.X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter.Id)
	{ }

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

		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		var attributeList = node.FirstAncestorOrSelf<AttributeArgumentListSyntax>();
		if (attributeList is null)
			return;

		var propertyAttributeParameters =
			attributeList
				.Arguments
				.Count(a => !string.IsNullOrEmpty(a.NameEquals?.Name.Identifier.ValueText));

		var paramsCount = attributeList.Arguments.Count - 1 - propertyAttributeParameters;

		(_, var declaredMemberTypeSymbol) = MemberDataShouldReferenceValidMember.GetClassTypesForAttribute(
			attributeList, semanticModel, context.CancellationToken);
		if (declaredMemberTypeSymbol is null)
			return;

		var memberName = diagnostic.Properties[Constants.Properties.MemberName];
		if (memberName is null)
			return;

		var memberSymbol = MemberDataShouldReferenceValidMember.FindMethodSymbol(memberName, declaredMemberTypeSymbol, paramsCount);
		if (memberSymbol is not IMethodSymbol methodSymbol)
			return;

		var methodSyntaxes = methodSymbol.DeclaringSyntaxReferences;
		if (methodSyntaxes.Length != 1)
			return;
		if (await methodSyntaxes[0].GetSyntaxAsync().ConfigureAwait(false) is not MethodDeclarationSyntax method)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Make parameter nullable",
				ct => MakeParameterNullable(context.Document, method, parameterIndex, ct),
				Key_MakeParameterNullable
			),
			context.Diagnostics
		);
	}

	static async Task<Document> MakeParameterNullable(
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
				var paramTypeSymbol = semanticModel.GetTypeInfo(param.Type, cancellationToken).Type;
				if (paramTypeSymbol is not null)
				{
					var nullableT = paramTypeSymbol.IsReferenceType
						? paramTypeSymbol.WithNullableAnnotation(NullableAnnotation.Annotated)
						: TypeSymbolFactory.NullableOfT(semanticModel.Compilation).Construct(paramTypeSymbol);
					editor.SetType(param, editor.Generator.TypeExpression(nullableT));
				}
			}
		}

		return editor.GetChangedDocument();
	}
}
