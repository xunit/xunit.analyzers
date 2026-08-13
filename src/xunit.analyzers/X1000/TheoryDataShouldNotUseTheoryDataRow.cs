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

		var theoryDataOfTSymbol = TypeSymbolFactory.TheoryData(context.Compilation, 1);
		if (theoryDataOfTSymbol is null)
			return;

		context.RegisterSyntaxNodeAction(context =>
		{
			var genericName = (GenericNameSyntax)context.Node;

			if (context.SemanticModel.GetSymbolInfo(genericName).Symbol is not INamedTypeSymbol typeSymbol)
				return;

			// Only care about TheoryData<ITheoryDataRow>
			if (!SymbolEqualityComparer.Default.Equals(theoryDataOfTSymbol, typeSymbol.OriginalDefinition))
				return;

			if (IsOrImplementsITheoryDataRow(typeSymbol.TypeArguments[0], iTheoryDataRowSymbol))
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
