using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertThrowsShouldUseGenericOverloadCheck : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.Throws,
		Constants.Asserts.ThrowsAsync,
	};

	public AssertThrowsShouldUseGenericOverloadCheck()
		: base(Descriptors.X2015_AssertThrowsShouldUseGenericOverload, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		var parameters = invocationOperation.TargetMethod.Parameters;
		if (parameters.Length != 2)
			return;

		var typeArgument = invocationOperation.Arguments.FirstOrDefault(arg => SymbolEqualityComparer.Default.Equals(arg.Parameter, parameters[0]))?.Value;
		if (typeArgument is not ITypeOfOperation typeOfOperation)
			return;

		var type = typeOfOperation.TypeOperand;
		var typeName = SymbolDisplay.ToDisplayString(type);

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		builder[Constants.Properties.TypeName] = typeName;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2015_AssertThrowsShouldUseGenericOverload,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				method.Name,
				typeName
			)
		);
	}
}
