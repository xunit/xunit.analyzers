using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class MemberDataShouldReferenceValidMember_ExtraValueFixer : XunitCodeFixProvider
{
	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public const string Key_AddMethodParameter = "xUnit1036_AddMethodParameter";
	public const string Key_RemoveExtraDataValue = "xUnit1036_RemoveExtraDataValue";

	public MemberDataShouldReferenceValidMember_ExtraValueFixer() :
		base(Descriptors.X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue.Id)
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

		// Fix #1: remove the extra data from the member data attribute
		context.RegisterCodeFix(
			CodeAction.Create(
				"Remove extra data value",
				ct => context.Document.RemoveNode(node, ct),
				Key_RemoveExtraDataValue
			),
			context.Diagnostics
		);

		// Fix #2: add a parameter to the theory for the extra data
		// (only valid for the first item after the supported parameters are exhausted)
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

		IMethodSymbol? methodSymbol = null;
		while (paramsCount >= 0)
		{
			var memberSymbol = MemberDataShouldReferenceValidMember.FindMethodSymbol(memberName, declaredMemberTypeSymbol, paramsCount);
			methodSymbol = memberSymbol as IMethodSymbol;
			if (methodSymbol is not null)
				break;
			paramsCount--;
		}
		if (methodSymbol is null)
			return;

		var methodSyntaxes = methodSymbol.DeclaringSyntaxReferences;
		if (methodSyntaxes.Length != 1)
			return;
		if (await methodSyntaxes[0].GetSyntaxAsync().ConfigureAwait(false) is not MethodDeclarationSyntax method)
			return;

		var parameterIndexText = diagnostic.Properties[Constants.Properties.ParameterIndex];

		if (parameterIndexText is not null)
		{
			var parameterIndex = int.Parse(parameterIndexText, CultureInfo.InvariantCulture);
			if (!Enum.TryParse<SpecialType>(diagnostic.Properties[Constants.Properties.ParameterSpecialType], out var parameterSpecialType))
				return;

			var existingParameters = method.ParameterList.Parameters.Select(p => p.Identifier.Text).ToImmutableHashSet();
			var parameterName = "p";
			var nextIndex = 2;
			while (existingParameters.Contains(parameterName))
				parameterName = $"p_{nextIndex++}";

			if (method.ParameterList.Parameters.Count == parameterIndex)
				context.RegisterCodeFix(
					CodeAction.Create(
						"Add method parameter",
						ct => AddMethodParameter(context.Document, method, parameterSpecialType, parameterName, ct),
						Key_AddMethodParameter
					),
					context.Diagnostics
				);
		}
	}

	static async Task<Document> AddMethodParameter(
		Document document,
		MethodDeclarationSyntax method,
		SpecialType parameterSpecialType,
		string parameterName,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var parameterTypeExpression =
			parameterSpecialType != SpecialType.None
				? editor.Generator.TypeExpression(parameterSpecialType)
				: editor.Generator.TypeExpression(SpecialType.System_Object);

		editor.AddParameter(
			method,
			editor.Generator.ParameterDeclaration(parameterName, parameterTypeExpression)
		);

		return editor.GetChangedDocument();
	}
}
