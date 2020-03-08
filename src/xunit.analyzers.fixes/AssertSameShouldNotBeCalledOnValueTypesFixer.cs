﻿using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertSameShouldNotBeCalledOnValueTypesFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
			= ImmutableArray.Create(Descriptors.X2005_AssertSameShouldNotBeCalledOnValueTypes.Id);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var methodName = context.Diagnostics.First().Properties[AssertSameShouldNotBeCalledOnValueTypes.MethodName];
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			string replacement = null;
			switch (methodName)
			{
				case AssertSameShouldNotBeCalledOnValueTypes.SameMethod:
					replacement = "Equal";
					break;

				case AssertSameShouldNotBeCalledOnValueTypes.NotSameMethod:
					replacement = "NotEqual";
					break;
			}

			if (replacement != null && invocation.Expression is MemberAccessExpressionSyntax)
			{
				var title = string.Format(titleTemplate, replacement);
				context.RegisterCodeFix(
					new UseDifferentMethodCodeAction(title, context.Document, invocation, replacement),
					context.Diagnostics);
			}
		}
	}
}
