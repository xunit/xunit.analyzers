using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SetsMustBeComparedWithEqualityComparer : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
	};

	const string set = "ISet";
	const string readOnlySet = "IReadOnlySet";

	public SetsMustBeComparedWithEqualityComparer()
		: base(Descriptors.X2026_SetsMustBeComparedWithEqualityComparer, targetMethods)
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

		var arguments = invocationOperation.Arguments;
		if (arguments.Length != 3)
			return;

		var semanticModel = context.Operation.SemanticModel;
		if (semanticModel == null)
			return;

		var collection0Type = semanticModel.GetTypeInfo(arguments[0].Value.Syntax).Type as INamedTypeSymbol;
		if (collection0Type == null)
			return;

		if (!collection0Type.AllInterfaces.Select(i => i.Name).Any(n => n is set or readOnlySet))
			return;

		var collection1Type = semanticModel.GetTypeInfo(arguments[1].Value.Syntax).Type as INamedTypeSymbol;
		if (collection1Type == null)
			return;

		if (!collection1Type.AllInterfaces.Select(i => i.Name).Any(n => n is set or readOnlySet))
			return;

#pragma warning disable CA1508
		if (collection0Type.TypeArguments.Length != 1 || collection1Type.TypeArguments.Length != 1)
#pragma warning restore CA1508

			return;

		if (arguments[2].Value is not IDelegateCreationOperation && arguments[2].Value is not ILocalReferenceOperation)
			return;

		if (arguments[2].Value.Type is not INamedTypeSymbol funcTypeSymbol || funcTypeSymbol.DelegateInvokeMethod == null)
			return;

		var funcDelegate = funcTypeSymbol.DelegateInvokeMethod;
		var isFuncOverload = funcDelegate.ReturnType.SpecialType == SpecialType.System_Boolean &&
		                     funcDelegate.Parameters.Length == 2 &&
							 funcDelegate.Parameters[0].Type.Equals(collection0Type.TypeArguments[0], SymbolEqualityComparer.Default) &&
		                     funcDelegate.Parameters[1].Type.Equals(collection1Type.TypeArguments[0], SymbolEqualityComparer.Default);
		
		// Wrong method overload
		if (!isFuncOverload)
			return;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2026_SetsMustBeComparedWithEqualityComparer,
				invocationOperation.Syntax.GetLocation(),
				method.Name
			)
		);
	}
}
