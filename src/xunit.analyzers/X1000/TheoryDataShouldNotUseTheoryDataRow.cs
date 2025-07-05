using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TheoryDataShouldNotUseTheoryDataRow : XunitV3DiagnosticAnalyzer
{
	public TheoryDataShouldNotUseTheoryDataRow() : base(Descriptors.X1052_TheoryDataShouldNotUseITheoryDataRow) { }

	public override void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		Dictionary<int, INamedTypeSymbol> theoryDataDict = TypeSymbolFactory.TheoryData_ByGenericArgumentCount(context.Compilation);
		INamedTypeSymbol? itheoryDataRowSymbol = TypeSymbolFactory.ITheoryDataRow_V3(context.Compilation);

		if (itheoryDataRowSymbol is null)
		{
			return;
		}

		context.RegisterSyntaxNodeAction(context =>
		{
			var genericName = (GenericNameSyntax)context.Node;
			ISymbol? symbol = context.SemanticModel.GetSymbolInfo(genericName).Symbol;

			if (symbol is null)
			{
				return;
			}

			if (symbol is not INamedTypeSymbol typeSymbol)
			{
				return;
			}

			if (!theoryDataDict.TryGetValue(typeSymbol.TypeArguments.Length, out var expectedSymbol))
			{
				return;
			}

			if (!SymbolEqualityComparer.Default.Equals(expectedSymbol, typeSymbol.OriginalDefinition))
			{
				return;
			}

			foreach (var typeArg in typeSymbol.TypeArguments)
			{
				if (IsOrImplementsITheoryDataRow(typeArg, itheoryDataRowSymbol))
				{
					context.ReportDiagnostic(
							Diagnostic.Create(Descriptors.X1052_TheoryDataShouldNotUseITheoryDataRow, genericName.GetLocation()));
				}
			}

		}, SyntaxKind.GenericName);
	}


	private static bool IsOrImplementsITheoryDataRow(ITypeSymbol typeArg, INamedTypeSymbol itheoryDataSymbol)
	{
		if (SymbolEqualityComparer.Default.Equals(typeArg, itheoryDataSymbol) ||
		   typeArg.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, itheoryDataSymbol)))
		{
			return true;
		}

		if (typeArg is ITypeParameterSymbol typeParameter)
		{
			foreach (var constraint in typeParameter.ConstraintTypes)
			{
				if (SymbolEqualityComparer.Default.Equals(constraint, itheoryDataSymbol) ||
					constraint.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, itheoryDataSymbol)))
				{
					return true;
				}
			}
		}

		return false;
	}
}
