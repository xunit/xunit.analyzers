using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseConfigureAwait : XunitDiagnosticAnalyzer
{
	public DoNotUseConfigureAwait() :
		base(Descriptors.X1030_DoNotUseConfigureAwait)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var taskType = TypeSymbolFactory.Task(context.Compilation);
		var taskOfTType = TypeSymbolFactory.TaskOfT(context.Compilation)?.ConstructUnboundGenericType();
		var valueTaskType = TypeSymbolFactory.ValueTask(context.Compilation);
		var valueTaskOfTType = TypeSymbolFactory.ValueTaskOfT(context.Compilation)?.ConstructUnboundGenericType();

		if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null)
			return;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IInvocationOperation invocation)
				return;

			var methodSymbol = invocation.TargetMethod;
			if (methodSymbol.MethodKind != MethodKind.Ordinary || methodSymbol.Name != nameof(Task.ConfigureAwait))
				return;

			bool match;

			if (methodSymbol.ContainingType.IsGenericType)
			{
				var unboundGeneric = methodSymbol.ContainingType.ConstructUnboundGenericType();

				match =
					SymbolEqualityComparer.Default.Equals(unboundGeneric, taskOfTType) ||
					SymbolEqualityComparer.Default.Equals(unboundGeneric, valueTaskOfTType);
			}
			else
				match =
					SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, taskType) ||
					SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, valueTaskType);

			if (!match)
				return;

			if (!invocation.IsInTestMethod(xunitContext))
				return;

			// invocation should be two nodes: "(some other code).ConfigureAwait" and the arguments (like "(false)")
			var invocationChildren = invocation.Syntax.ChildNodes().ToList();
			if (invocationChildren.Count != 2)
				return;

			// We only care about invocations with a single parameter
			var arguments = invocationChildren[1];
			var argumentChildren = arguments.ChildNodes().ToList();
			if (argumentChildren.Count != 1 || argumentChildren[0] is not ArgumentSyntax argumentSyntax)
				return;

			// Determine the invocation type and resolution
			var parameterType = invocation.TargetMethod.Parameters[0].Type;
			string resolution;

			// We want to exempt calls with "(true)" because of CA2007
			if (SymbolEqualityComparer.Default.Equals(parameterType, context.Compilation.GetSpecialType(SpecialType.System_Boolean)))
			{
				if (argumentSyntax.Expression is LiteralExpressionSyntax literalExpression && literalExpression.IsKind(SyntaxKind.TrueLiteralExpression))
					return;

				resolution = "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.";
			}
			else
				return;

			// First child node should be split into three pieces: "(some other code)", ".", and "ConfigureAwait"
			var methodCallChildren = invocationChildren[0].ChildNodesAndTokens().ToList();
			if (methodCallChildren.Count != 3)
				return;

			// Construct a location that covers "ConfigureAwait(arguments)"
			var length = methodCallChildren[2].Span.Length + invocationChildren[1].Span.Length;
			var textSpan = new TextSpan(methodCallChildren[2].SpanStart, length);
			var location = Location.Create(invocation.Syntax.SyntaxTree, textSpan);

			context.ReportDiagnostic(Diagnostic.Create(Descriptors.X1030_DoNotUseConfigureAwait, location, argumentSyntax.ToFullString(), resolution));
		}, OperationKind.Invocation);
	}
}
