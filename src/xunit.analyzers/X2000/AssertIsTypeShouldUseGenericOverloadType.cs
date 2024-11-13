using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertIsTypeShouldUseGenericOverloadType : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	[
		Constants.Asserts.IsAssignableFrom,
		Constants.Asserts.IsNotType,
		Constants.Asserts.IsType,
	];

	public AssertIsTypeShouldUseGenericOverloadType()
		: base(Descriptors.X2007_AssertIsTypeShouldUseGenericOverload, targetMethods)
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

		if (method.IsGenericMethod)
			return;

		var parameters = invocationOperation.TargetMethod.Parameters;
		if (parameters.Length < 2)
			return;

		var typeArgument = invocationOperation.Arguments.FirstOrDefault(arg => SymbolEqualityComparer.Default.Equals(arg.Parameter, parameters[0]));
		if (typeArgument?.Value is not ITypeOfOperation typeOfOperation)
			return;

		var type = typeOfOperation.TypeOperand;
		var typeName = SymbolDisplay.ToDisplayString(type);

		// Static abstract interface members can't be used as types in generics
		if (type.TypeKind == TypeKind.Interface)
		{
			var allInterfaces = (type as INamedTypeSymbol)?.AllInterfaces;
			if (allInterfaces is not null)
			{
				var allMembers =
					allInterfaces
						.Value
						.SelectMany(i => i.GetMembers())
						.Concat(type.GetMembers());

				if (allMembers.Any(m => m is { IsAbstract: true, IsStatic: true }))
					return;
			}
		}

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		builder[Constants.Properties.TypeName] = typeName;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2007_AssertIsTypeShouldUseGenericOverload,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				typeName
			)
		);
	}
}
