﻿using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class TestClassShouldHaveTFixtureArgumentFixer : CodeFixProvider
	{
		const string title = "Generate constructor {0}({1})";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
			= ImmutableArray.Create(Descriptors.X1033_TestClassShouldHaveTFixtureArgument.Id);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			var first = context.Diagnostics.First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: string.Format(
						title,
						first.Properties[TestClassShouldHaveTFixtureArgument.TestClassNamePropertyKey],
						first.Properties[TestClassShouldHaveTFixtureArgument.TFixtureNamePropertyKey]),
					createChangedDocument: ct => Actions.AddConstructor(
						context.Document,
						classDeclaration,
						typeDisplayName: first.Properties[TestClassShouldHaveTFixtureArgument.TFixtureDisplayNamePropertyKey],
						typeName: first.Properties[TestClassShouldHaveTFixtureArgument.TFixtureNamePropertyKey],
						ct),
					equivalenceKey: title),
				context.Diagnostics);
		}
	}
}
