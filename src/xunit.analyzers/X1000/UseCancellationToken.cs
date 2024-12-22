using System.Collections.Immutable;
using System.Globalization;
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

		var xunitContainerTypes = new[]
		{
			TypeSymbolFactory.Assert(context.Compilation),
			TypeSymbolFactory.Record(context.Compilation),
		}.WhereNotNull().ToImmutableHashSet(SymbolEqualityComparer.Default);

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IInvocationOperation invocationOperation)
				return;

			var (foundSymbol, lambdaOwner) = invocationOperation.IsInTestMethod(xunitContext);
			if (!foundSymbol || lambdaOwner is ILocalFunctionOperation or IAnonymousFunctionOperation)
				return;

			// We want to try to catch anything that's a lambda from Assert or Record, but
			// ignore all other lambdas, because we don't want to catch false positives from
			// things like mocking libraries.
			if (lambdaOwner is IInvocationOperation lambdaOwnerInvocation)
				if (!xunitContainerTypes.Contains(lambdaOwnerInvocation.TargetMethod.ContainingType))
					return;

			var invokedMethod = invocationOperation.TargetMethod;
			var parameters = invokedMethod.Parameters;

			IArgumentOperation? argument = null;
			foreach (var parameter in parameters)
				if (SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationTokenType))
				{
					argument = invocationOperation.Arguments.FirstOrDefault(arg => SymbolEqualityComparer.Default.Equals(arg.Parameter, parameter));
					break;
				}

			// The invoked method has the parameter we're looking for
			if (argument is not null)
			{
				// Default parameter value
				if (argument.ArgumentKind == ArgumentKind.DefaultValue)
					Report(context, invocationOperation.Syntax.GetLocation(), argument.Parameter!);

				// Explicit parameter value
				else if (argument.Syntax is ArgumentSyntax argumentSyntax)
				{
					var kind = argumentSyntax.Expression.Kind();
					if (kind is SyntaxKind.DefaultExpression or SyntaxKind.DefaultLiteralExpression)
						Report(context, invocationOperation.Syntax.GetLocation(), argument.Parameter!);
				}
			}
			// Look for an overload with the exact same parameter types + a CancellationToken
			else
			{
				var targetParameterTypes = parameters.Select(p => p.Type).Concat([cancellationTokenType]).ToArray();
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
							Report(context, invocationOperation.Syntax.GetLocation(), method.Parameters.Last());
							return;
						}
					}
			}
		}, OperationKind.Invocation);

		static void Report(
			OperationAnalysisContext context,
			Location location,
			IParameterSymbol parameter)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.ParameterName] = parameter.Name;
			builder[Constants.Properties.ParameterIndex] = parameter.Ordinal.ToString(CultureInfo.InvariantCulture);

			context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1051_UseCancellationToken,
						location,
						builder.ToImmutable()
					)
				);
		}
	}

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		Guard.ArgumentNotNull(xunitContext).HasV3References;
}
