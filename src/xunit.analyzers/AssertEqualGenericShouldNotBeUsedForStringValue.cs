using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEqualGenericShouldNotBeUsedForStringValue : AssertUsageAnalyzerBase
	{
		static readonly string[] targetMethods =
		{
			Constants.Asserts.Equal,
			Constants.Asserts.StrictEqual
		};

		public AssertEqualGenericShouldNotBeUsedForStringValue()
			: base(Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue, targetMethods)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			if (invocationOperation.Arguments.Length != 2)
				return;

			if (!method.IsGenericMethod && method.Name == Constants.Asserts.Equal)
				return;

			if (method.IsGenericMethod &&
			   (!method.TypeArguments[0].SpecialType.Equals(SpecialType.System_String) ||
				!method.Parameters[0].Type.SpecialType.Equals(SpecialType.System_String) ||
				!method.Parameters[1].Type.SpecialType.Equals(SpecialType.System_String)))
				return;

			var invalidUsageDescription = method.Name == Constants.Asserts.Equal ? "generic Assert.Equal overload" : "Assert.StrictEqual";

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue,
					invocationOperation.Syntax.GetLocation(),
					invalidUsageDescription
				)
			);
		}
	}
}
