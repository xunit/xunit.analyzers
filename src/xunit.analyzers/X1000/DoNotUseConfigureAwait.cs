using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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
		var taskType = TypeSymbolFactory.Task(context.Compilation);

		if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null)
			return;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IInvocationOperation invocation)
				return;

			var methodSymbol = invocation.TargetMethod;
			if (methodSymbol.MethodKind != MethodKind.Ordinary || !SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, taskType) || methodSymbol.Name != nameof(Task.ConfigureAwait))
				return;

			var semanticModel = invocation.SemanticModel;
			if (semanticModel is null)
				return;

			// Only trigger when you're inside a test method
			for (var parent = invocation.Parent; parent != null; parent = parent.Parent)
			{
				if (parent is not IMethodBodyOperation methodBodyOperation)
					continue;
				if (methodBodyOperation.Syntax is not MethodDeclarationSyntax methodSyntax)
					continue;

				var inTestMethod = methodSyntax.AttributeLists.SelectMany(list => list.Attributes).Any(attr =>
				{
					var typeInfo = semanticModel.GetTypeInfo(attr);
					if (typeInfo.Type is null)
						return false;

					return
						SymbolEqualityComparer.Default.Equals(typeInfo.Type, xunitContext.Core.FactAttributeType) ||
						SymbolEqualityComparer.Default.Equals(typeInfo.Type, xunitContext.Core.TheoryAttributeType);
				});

				if (!inTestMethod)
					return;

				// invocation should be two nodes: "(some other code).ConfigureAwait" and the arguments (like "(false)")
				var invocationChildren = invocation.Syntax.ChildNodes().ToList();
				if (invocationChildren.Count != 2)
					return;

				// First child node should be split into three pieces: "(some other code)", ".", and "ConfigureAwait"
				var methodCallChildren = invocationChildren[0].ChildNodesAndTokens().ToList();
				if (methodCallChildren.Count != 3)
					return;

				// Construct a location that covers "ConfigureAwait(arguments)"
				var length = methodCallChildren[2].Span.Length + invocationChildren[1].Span.Length;
				var textSpan = new TextSpan(methodCallChildren[2].SpanStart, length);
				var location = Location.Create(invocation.Syntax.SyntaxTree, textSpan);

				context.ReportDiagnostic(Diagnostic.Create(Descriptors.X1030_DoNotUseConfigureAwait, location));
			}
		}, OperationKind.Invocation);
	}
}
