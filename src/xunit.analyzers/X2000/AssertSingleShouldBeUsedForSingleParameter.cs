using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertSingleShouldBeUsedForSingleParameter : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	[
		Constants.Asserts.Collection,
		Constants.Asserts.CollectionAsync,
	];

	public AssertSingleShouldBeUsedForSingleParameter() :
		base(Descriptors.X2023_AssertSingleShouldBeUsedForSingleParameter, targetMethods)
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

		if (invocationOperation.Arguments.Length != 2)
			return;

		var secondParameter = invocationOperation.Arguments[1];
		if (secondParameter.ArgumentKind != ArgumentKind.ParamArray)
			return;

		if (secondParameter.Value is not IArrayCreationOperation operation)
			return;

		if (operation.DimensionSizes.Length != 1 || (int)(operation.DimensionSizes[0].ConstantValue.Value ?? 0) != 1)
			return;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.AssertMethodName] = method.Name;
		builder[Constants.Properties.Replacement] = Constants.Asserts.Single;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2023_AssertSingleShouldBeUsedForSingleParameter,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				method.Name
			)
		);
	}
}
