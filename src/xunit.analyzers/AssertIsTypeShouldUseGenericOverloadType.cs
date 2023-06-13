using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertIsTypeShouldUseGenericOverloadType : AssertUsageAnalyzerBase
	{
		static readonly string[] targetMethods =
		{
			Constants.Asserts.IsType,
			Constants.Asserts.IsNotType,
			Constants.Asserts.IsAssignableFrom
		};

		public AssertIsTypeShouldUseGenericOverloadType()
			: base(Descriptors.X2007_AssertIsTypeShouldUseGenericOverload, targetMethods)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			var parameters = invocationOperation.TargetMethod.Parameters;
			if (parameters.Length != 2)
				return;

			var typeArgument = invocationOperation.Arguments.FirstOrDefault(arg => SymbolEqualityComparer.Default.Equals(arg.Parameter, parameters[0]));
			if (typeArgument?.Value is not ITypeOfOperation typeOfOperation)
				return;

			var type = typeOfOperation.TypeOperand;
			var typeName = SymbolDisplay.ToDisplayString(type);

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
}
