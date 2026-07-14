using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEqualGenericShouldNotBeUsedForStringValue : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	[
		Constants.Asserts.Equal,
		Constants.Asserts.StrictEqual,
	];

	public AssertEqualGenericShouldNotBeUsedForStringValue()
		: base(Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue, targetMethods)
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

		// Assert.Equal is available in generic for both reflection and AOT
		// Assert.StrictEqual is available in generic only for reflection
		if (method.IsGenericMethod)
		{
			// Only report if the type argument is string
			if (!method.TypeArguments[0].SpecialType.Equals(SpecialType.System_String)
					|| !method.Parameters[0].Type.SpecialType.Equals(SpecialType.System_String)
					|| !method.Parameters[1].Type.SpecialType.Equals(SpecialType.System_String))
				return;
		}
		// Assert.Equal is available in non-generic for both reflection and AOT
		// Assert.StrictEqual is available in non-generic only for AOT
		else
		{
			// Non-generic Assert.Equal is what we want them to use
			if (method.Name == Constants.Asserts.Equal)
				return;

			// Non-generic StrictEqual requires we check the argument types, which will always
			// be a conversion operation (unless they're already object, which isn't a string)
			if ((invocationOperation.Arguments[0].Value as IConversionOperation)?.Operand.Type?.SpecialType.Equals(SpecialType.System_String) != true
					&& (invocationOperation.Arguments[1].Value as IConversionOperation)?.Operand.Type?.SpecialType.Equals(SpecialType.System_String) != true)
				return;
		}

		var invalidUsageDescription =
			method.Name == Constants.Asserts.Equal
				? "Assert.Equal<string>"
				: "Assert.StrictEqual";
		var replacement =
			method.Name == Constants.Asserts.Equal
				? "non-generic Assert.Equal"
				: "Assert.Equal";

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue,
				invocationOperation.Syntax.GetLocation(),
				invalidUsageDescription,
				replacement
			)
		);
	}
}
