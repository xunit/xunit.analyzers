using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEmptyCollectionCheckShouldNotBeUsed : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	[
		Constants.Asserts.Collection,
		Constants.Asserts.CollectionAsync,
	];

	public AssertEmptyCollectionCheckShouldNotBeUsed()
		: base(Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		Guard.ArgumentNotNull(xunitContext);
		Guard.ArgumentNotNull(invocationOperation);
		Guard.ArgumentNotNull(method);

		if (invocationOperation.Syntax is not InvocationExpressionSyntax invocation)
			return;

		var arguments = invocation.ArgumentList.Arguments;
		if (arguments.Count != 1)
			return;

		var matchedType = false;
		var asyncEnumerable = TypeSymbolFactory.IAsyncEnumerableOfT(context.Compilation);
		if (asyncEnumerable != null)
			matchedType = SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type.OriginalDefinition, asyncEnumerable);

		matchedType = matchedType || method.Parameters[0].Type.OriginalDefinition.SpecialType.Equals(SpecialType.System_Collections_Generic_IEnumerable_T);

		if (!matchedType)
			return;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed,
				invocationOperation.Syntax.GetLocation(),
				SymbolDisplay.ToDisplayString(
					method,
					SymbolDisplayFormat
						.CSharpShortErrorMessageFormat
						.WithParameterOptions(SymbolDisplayParameterOptions.None)
						.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
				)
			)
		);
	}
}
