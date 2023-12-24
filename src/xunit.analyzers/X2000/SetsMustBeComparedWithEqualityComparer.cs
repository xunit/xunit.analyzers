#if ROSLYN_3_11
#pragma warning disable RS1024 // Incorrectly triggered by Roslyn 3.11
#endif

using System.Collections.Generic;
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

		var setType = TypeSymbolFactory.ISetOfT(context.Compilation)?.ConstructUnboundGenericType();
		var readOnlySetType = TypeSymbolFactory.IReadOnlySetOfT(context.Compilation)?.ConstructUnboundGenericType();
		var setInterfaces = new HashSet<INamedTypeSymbol>(new[] { setType, readOnlySetType }.WhereNotNull(), SymbolEqualityComparer.Default);

		if (semanticModel.GetTypeInfo(arguments[0].Value.Syntax).Type is not INamedTypeSymbol collection0Type)
			return;
		var interface0Type =
			collection0Type
				.AllInterfaces
				.Where(i => i.IsGenericType)
				.FirstOrDefault(i => setInterfaces.Contains(i.ConstructUnboundGenericType()));
		if (interface0Type is null)
			return;

		if (semanticModel.GetTypeInfo(arguments[1].Value.Syntax).Type is not INamedTypeSymbol collection1Type)
			return;
		var interface1Type =
			collection1Type
				.AllInterfaces
				.Where(i => i.IsGenericType)
				.FirstOrDefault(i => setInterfaces.Contains(i.ConstructUnboundGenericType()));
		if (interface1Type is null)
			return;

		if (arguments[2].Value is not IDelegateCreationOperation && arguments[2].Value is not ILocalReferenceOperation)
			return;

		if (arguments[2].Value.Type is not INamedTypeSymbol funcTypeSymbol || funcTypeSymbol.DelegateInvokeMethod == null)
			return;

		var funcDelegate = funcTypeSymbol.DelegateInvokeMethod;
		var isFuncOverload =
			funcDelegate.ReturnType.SpecialType == SpecialType.System_Boolean &&
			funcDelegate.Parameters.Length == 2 &&
			funcDelegate.Parameters[0].Type.Equals(interface0Type.TypeArguments[0], SymbolEqualityComparer.Default) &&
			funcDelegate.Parameters[1].Type.Equals(interface1Type.TypeArguments[0], SymbolEqualityComparer.Default);

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
