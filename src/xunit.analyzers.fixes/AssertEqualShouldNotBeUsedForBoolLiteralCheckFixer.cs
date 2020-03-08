﻿using System;
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
	public class AssertEqualShouldNotBeUsedForBoolLiteralCheckFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
			= ImmutableArray.Create(Descriptors.X2004_AssertEqualShouldNotUsedForBoolLiteralCheck.Id);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var methodName = context.Diagnostics.First().Properties[AssertEqualShouldNotBeUsedForBoolLiteralCheck.MethodName];
			var literalValue = context.Diagnostics.First().Properties[AssertEqualShouldNotBeUsedForBoolLiteralCheck.LiteralValue];
			var replacement = GetReplacementMethodName(methodName, literalValue);

			if (replacement != null && invocation.Expression is MemberAccessExpressionSyntax)
			{
				var title = string.Format(titleTemplate, replacement);
				context.RegisterCodeFix(
					CodeAction.Create(
						title,
						createChangedDocument: ct => UseBoolCheckAsync(context.Document, invocation, replacement, ct),
						equivalenceKey: title),
					context.Diagnostics);
			}
		}

		static string GetReplacementMethodName(string methodName, string literalValue)
		{
			if (AssertEqualShouldNotBeUsedForBoolLiteralCheck.EqualMethods.Contains(methodName))
				return literalValue == bool.TrueString ? "True" : "False";
			if (AssertEqualShouldNotBeUsedForBoolLiteralCheck.NotEqualMethods.Contains(methodName))
				return literalValue == bool.TrueString ? "False" : "True";

			return null;
		}

		static async Task<Document> UseBoolCheckAsync(Document document, InvocationExpressionSyntax invocation, string replacementMethod, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			editor.ReplaceNode(
				invocation,
				invocation.WithArgumentList(
					invocation.ArgumentList.WithArguments(
						SyntaxFactory.SingletonSeparatedList(invocation.ArgumentList.Arguments[1])))
						  .WithExpression(memberAccess.WithName(SyntaxFactory.IdentifierName(replacementMethod))));

			return editor.GetChangedDocument();
		}
	}
}
