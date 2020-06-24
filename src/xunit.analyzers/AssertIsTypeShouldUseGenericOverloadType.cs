using System.Collections.Generic;
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
		internal static string MethodName = "MethodName";
		internal static string TypeName = "TypeName";
		internal static HashSet<string> IsTypeMethods = new HashSet<string>(new[] { "IsType", "IsNotType", "IsAssignableFrom" });

		public AssertIsTypeShouldUseGenericOverloadType()
			: base(Descriptors.X2007_AssertIsTypeShouldUseGenericOverload, IsTypeMethods)
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var parameters = invocationOperation.TargetMethod.Parameters;
			if (parameters.Length != 2)
				return;

			var typeArgument = invocationOperation.Arguments.FirstOrDefault(arg => arg.Parameter.Equals(parameters[0]));
			if (!(typeArgument?.Value is ITypeOfOperation typeOfOperation))
				return;

			var type = typeOfOperation.TypeOperand;
			var typeName = SymbolDisplay.ToDisplayString(type);

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			builder[TypeName] = typeName;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2007_AssertIsTypeShouldUseGenericOverload,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					typeName));
		}
	}
}
