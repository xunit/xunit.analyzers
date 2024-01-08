using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEqualsShouldNotBeUsed : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		nameof(object.Equals),
		nameof(object.ReferenceEquals),
	};

	public AssertEqualsShouldNotBeUsed()
		: base(Descriptors.X2001_AssertEqualsShouldNotBeUsed, targetMethods)
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

		var replacement = method.Name switch
		{
			nameof(object.Equals) => Constants.Asserts.Equal,
			nameof(object.ReferenceEquals) => Constants.Asserts.Same,
			_ => null
		};

		if (replacement is null)
			return;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		builder[Constants.Properties.Replacement] = replacement;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2001_AssertEqualsShouldNotBeUsed,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				SymbolDisplay.ToDisplayString(
					method,
					SymbolDisplayFormat
						.CSharpShortErrorMessageFormat
						.WithParameterOptions(SymbolDisplayParameterOptions.None)
				),
				replacement
			)
		);
	}
}
