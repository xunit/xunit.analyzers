using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers.Utilities;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestMethodCannotHaveOverloads : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.X1024_TestMethodCannotHaveOverloads);

        public override void Initialize(AnalysisContext context)
        {
            context.RequireTypes(Constants.Types.XunitFactAttribute).RegisterSymbolAction(symbolContext =>
            {
                var typeSymbol = (INamedTypeSymbol)symbolContext.Symbol;
                if (typeSymbol.TypeKind != TypeKind.Class)
                    return;

                var factType = symbolContext.Compilation.GetFactAttributeType();
                var methodsByName = typeSymbol.GetInheritedAndOwnMembers()
                    .Where(s => s.Kind == SymbolKind.Method)
                    .Cast<IMethodSymbol>()
                    .Where(m => m.MethodKind == MethodKind.Ordinary)
                    .GroupBy(m => m.Name);

                foreach (var grouping in methodsByName)
                {
                    symbolContext.CancellationToken.ThrowIfCancellationRequested();

                    var methods = grouping.ToList();
                    var methodName = grouping.Key;
                    if (methods.Count == 1 ||
                        !methods.Any(m => m.GetAttributes().ContainsAttributeType(factType)))
                        continue;

                    var methodsWithoutOverloads = new List<IMethodSymbol>(methods.Count);
                    foreach (var method in methods)
                    {
                        if (!methods.Any(m => m.IsOverride && m.OverriddenMethod.Equals(method)))
                        {
                            methodsWithoutOverloads.Add(method);
                        }
                    }

                    if (methodsWithoutOverloads.Count == 1)
                        continue;

                    foreach (var method in methodsWithoutOverloads.Where(m => m.ContainingType.Equals(typeSymbol)))
                    {
                        var otherType = methodsWithoutOverloads.Where(m => !m.Equals(method))
                            .OrderBy(m => m.ContainingType, TypeHierarchyComparer.Instance)
                            .First().ContainingType;
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.X1024_TestMethodCannotHaveOverloads,
                            method.Locations.First(),
                            methodName,
                            method.ContainingType.ToDisplayString(),
                            otherType.ToDisplayString()));
                    }
                }
            }, SymbolKind.NamedType);
        }
    }
}
