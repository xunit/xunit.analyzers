using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseCancellationToken : XunitDiagnosticAnalyzer
{
	public UseCancellationToken() :
		base(Descriptors.X1051_UseCancellationToken)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var cancellationTokenType = TypeSymbolFactory.CancellationToken(context.Compilation);
		if (cancellationTokenType is null)
			return;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IInvocationOperation invocationOperation)
				return;

			if (!invocationOperation.IsInTestMethod(xunitContext))
				return;

			var invokedMethod = invocationOperation.TargetMethod;
			var parameters = invokedMethod.Parameters;

			var parameterIdx = 0;
			for (; parameterIdx < parameters.Length; ++parameterIdx)
				if (SymbolEqualityComparer.Default.Equals(parameters[parameterIdx].Type, cancellationTokenType))
					break;

			// The invoked method has the parameter we're looking for
			if (parameterIdx != parameters.Length)
			{
				var argument = invocationOperation.Arguments[parameterIdx];

				// Default parameter value
				if (argument.ArgumentKind == ArgumentKind.DefaultValue)
					Report(context, invocationOperation.Syntax.GetLocation());

				// Explicit parameter value
				else if (argument.Syntax is ArgumentSyntax argumentSyntax)
				{
					var kind = argumentSyntax.Expression.Kind();
					if (kind == SyntaxKind.DefaultExpression || kind == SyntaxKind.DefaultLiteralExpression)
						Report(context, invocationOperation.Syntax.GetLocation());
				}
			}
			// Look for an overload with the exact same parameter types + a CancellationToken
			else
			{
				var targetParameterTypes = invokedMethod.Parameters.Select(p => p.Type).Concat([cancellationTokenType]).ToArray();
				foreach (var member in invokedMethod.ContainingType.GetMembers(invokedMethod.Name))
					if (member is IMethodSymbol method)
					{
						var methodParameterTypes = method.Parameters.Select(p => p.Type).ToArray();
						if (methodParameterTypes.Length != targetParameterTypes.Length)
							continue;

						var match = true;
						for (var idx = 0; idx < targetParameterTypes.Length; ++idx)
							if (!SymbolEqualityComparer.Default.Equals(targetParameterTypes[idx], methodParameterTypes[idx]))
							{
								match = false;
								break;
							}

						if (match)
						{
							Report(context, invocationOperation.Syntax.GetLocation());
							return;
						}
					}
			}
		}, OperationKind.Invocation);

		static void Report(
			OperationAnalysisContext context,
			Location location) =>
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1051_UseCancellationToken,
						location
					)
				);
	}

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		Guard.ArgumentNotNull(xunitContext).HasV3References;
}
