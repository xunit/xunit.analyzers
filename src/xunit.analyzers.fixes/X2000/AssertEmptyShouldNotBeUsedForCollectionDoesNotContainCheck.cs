using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Xunit.Analyzers.Fixes;
using System.Threading.Tasks;
using Xunit.Analyzers;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
internal class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2017_UseAlternateAssert";

	public AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck() :
		base(Descriptors.X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck.Id)
	{ }

	public override Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		throw new NotImplementedException();
	}
}
