using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TheoryDataShouldNotUseTheoryDataRow() :
	XunitV3DiagnosticAnalyzer(Descriptors.X1052_TheoryDataShouldNotUseITheoryDataRow)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var iTheoryDataRowSymbol = TypeSymbolFactory.ITheoryDataRow_V3(context.Compilation);
		if (iTheoryDataRowSymbol is null)
			return;

		var theoryDataTypes = TypeSymbolFactory.TheoryData_ByGenericArgumentCount(context.Compilation);

		context.RegisterSyntaxNodeAction(context =>
		{
			var genericName = (GenericNameSyntax)context.Node;

			if (context.SemanticModel.GetSymbolInfo(genericName).Symbol is not INamedTypeSymbol typeSymbol)
				return;

			if (!theoryDataTypes.TryGetValue(typeSymbol.TypeArguments.Length, out var expectedSymbol))
				return;

			if (!SymbolEqualityComparer.Default.Equals(expectedSymbol, typeSymbol.OriginalDefinition))
				return;

			foreach (var typeArg in typeSymbol.TypeArguments)
				if (IsOrImplementsITheoryDataRow(typeArg, iTheoryDataRowSymbol))
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1052_TheoryDataShouldNotUseITheoryDataRow,
							genericName.GetLocation()
						)
					);
		}, SyntaxKind.GenericName);
	}

	static bool IsOrImplementsITheoryDataRow(
		ITypeSymbol typeArg,
		INamedTypeSymbol iTheoryDataSymbol)
	{
		if (SymbolEqualityComparer.Default.Equals(typeArg, iTheoryDataSymbol) ||
				typeArg.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iTheoryDataSymbol)))
			return true;

		if (typeArg is ITypeParameterSymbol typeParameter)
			foreach (var constraint in typeParameter.ConstraintTypes)
				if (SymbolEqualityComparer.Default.Equals(constraint, iTheoryDataSymbol) ||
						constraint.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iTheoryDataSymbol)))
					return true;

		return false;
	}
}
