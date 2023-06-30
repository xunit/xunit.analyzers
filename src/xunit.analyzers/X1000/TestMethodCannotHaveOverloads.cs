using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodCannotHaveOverloads : XunitDiagnosticAnalyzer
{
	public TestMethodCannotHaveOverloads() :
		base(Descriptors.X1024_TestMethodCannotHaveOverloads)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null)
				return;
			if (context.Symbol is not INamedTypeSymbol typeSymbol)
				return;
			if (typeSymbol.TypeKind != TypeKind.Class)
				return;

#pragma warning disable RS1024 // Compare symbols correctly
			var methodsByName =
				typeSymbol
					.GetInheritedAndOwnMembers()
					.Where(s => s.Kind == SymbolKind.Method)
					.Cast<IMethodSymbol>()
					.Where(m => m.MethodKind == MethodKind.Ordinary)
					.GroupBy(m => m.Name);
#pragma warning restore RS1024 // Compare symbols correctly

			foreach (var grouping in methodsByName)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var methods = grouping.ToList();
				var methodName = grouping.Key;
				if (methods.Count == 1 || !methods.Any(m => m.GetAttributes().ContainsAttributeType(xunitContext.Core.FactAttributeType)))
					continue;

				var methodsWithoutOverloads = new List<IMethodSymbol>(methods.Count);
				foreach (var method in methods)
					if (!methods.Any(m => m.IsOverride && SymbolEqualityComparer.Default.Equals(m.OverriddenMethod, method)))
						methodsWithoutOverloads.Add(method);

				if (methodsWithoutOverloads.Count == 1)
					continue;

				foreach (var method in methodsWithoutOverloads.Where(m => SymbolEqualityComparer.Default.Equals(m.ContainingType, typeSymbol)))
				{
					var otherType =
						methodsWithoutOverloads
							.Where(m => !SymbolEqualityComparer.Default.Equals(m, method))
							.OrderBy(m => m.ContainingType, TypeHierarchyComparer.Instance)
							.First()
							.ContainingType;

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1024_TestMethodCannotHaveOverloads,
							method.Locations.First(),
							methodName,
							method.ContainingType.ToDisplayString(),
							otherType.ToDisplayString()
						)
					);
				}
			}
		}, SymbolKind.NamedType);
	}
}
