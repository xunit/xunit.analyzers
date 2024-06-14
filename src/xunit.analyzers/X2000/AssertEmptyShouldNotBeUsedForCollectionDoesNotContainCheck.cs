using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Xunit.Analyzers;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.Empty,
	};

	public AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck()
		: base(Descriptors.X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context, 
		XunitContext xunitContext, 
		IInvocationOperation invocationOperation, 
		IMethodSymbol method)
	{
		return;
	}
}
