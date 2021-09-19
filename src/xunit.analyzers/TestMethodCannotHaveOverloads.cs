using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TestMethodCannotHaveOverloads : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1024_TestMethodCannotHaveOverloads);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				var typeSymbol = (INamedTypeSymbol)context.Symbol;
				if (typeSymbol.TypeKind != TypeKind.Class)
					return;

				var methodsByName =
					typeSymbol
						.GetInheritedAndOwnMembers()
						.Where(s => s.Kind == SymbolKind.Method)
						.Cast<IMethodSymbol>()
						.Where(m => m.MethodKind == MethodKind.Ordinary)
						.GroupBy(m => m.Name);

				foreach (var grouping in methodsByName)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					var methods = grouping.ToList();
					var methodName = grouping.Key;
					if (methods.Count == 1 || !methods.Any(m => m.GetAttributes().ContainsAttributeType(xunitContext.V2Core.FactAttributeType)))
						continue;

					var methodsWithoutOverloads = new List<IMethodSymbol>(methods.Count);
					foreach (var method in methods)
						if (!methods.Any(m => m.IsOverride && m.OverriddenMethod.Equals(method)))
							methodsWithoutOverloads.Add(method);

					if (methodsWithoutOverloads.Count == 1)
						continue;

					foreach (var method in methodsWithoutOverloads.Where(m => m.ContainingType.Equals(typeSymbol)))
					{
						var otherType =
							methodsWithoutOverloads
								.Where(m => !m.Equals(method))
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
}
