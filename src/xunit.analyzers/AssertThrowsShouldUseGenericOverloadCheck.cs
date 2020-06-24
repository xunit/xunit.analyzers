using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var arguments = invocation.ArgumentList.Arguments;
			if (arguments.Count != 2)
				return;

			if (!(arguments[0].Expression is TypeOfExpressionSyntax typeOfExpression))
				return;

			var typeInfo = context.GetSemanticModel().GetTypeInfo(typeOfExpression.Type);
			var typeName = SymbolDisplay.ToDisplayString(typeInfo.Type);

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			builder[TypeName] = typeName;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2015_AssertThrowsShouldUseGenericOverload,
					invocation.GetLocation(),
					builder.ToImmutable(),
					typeName));
		}
	}
}
