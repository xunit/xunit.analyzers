using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertThrowsShouldUseGenericOverloadCheck : AssertUsageAnalyzerBase
	{
		internal static string MethodName = "MethodName";
		internal static string TypeName = "TypeName";

		public AssertThrowsShouldUseGenericOverloadCheck()
			: base(Descriptors.X2015_AssertThrowsShouldUseGenericOverload, new[] { "Throws", "ThrowsAsync" })
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var parameters = invocationOperation.TargetMethod.Parameters;
			if (parameters.Length != 2)
				return;

			var typeArgument = invocationOperation.Arguments.FirstOrDefault(arg => arg.Parameter.Equals(parameters[0]))?.Value;
			if (!(typeArgument is ITypeOfOperation typeOfOperation))
				return;

			var type = typeOfOperation.TypeOperand;
			var typeName = SymbolDisplay.ToDisplayString(type);

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			builder[TypeName] = typeName;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2015_AssertThrowsShouldUseGenericOverload,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					typeName));
		}
	}
}
