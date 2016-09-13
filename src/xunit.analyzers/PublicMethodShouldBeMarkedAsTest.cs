using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicMethodShouldBeMarkedAsTest : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X1013_PublicMethodShouldBeMarkedAsTest);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var factType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
                if (factType == null)
                    return;

                var iDisposableType = compilationStartContext.Compilation.GetSpecialType(SpecialType.System_IDisposable);
                var taskType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.SystemThreadingTasksTask);

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var type = (INamedTypeSymbol)symbolContext.Symbol;

                    if (type.TypeKind != TypeKind.Class ||
                        type.DeclaredAccessibility != Accessibility.Public)
                        return;

                    ISymbol disposeMethod = null;
                    if (iDisposableType != null && type.AllInterfaces.Contains(iDisposableType))
                    {
                        disposeMethod = type.FindImplementationForInterfaceMember(iDisposableType.GetMembers().First());
                    }

                    bool hasTestMethods = false;
                    var violations = new List<IMethodSymbol>();
                    foreach (var member in type.GetMembers().Where(m => m.Kind == SymbolKind.Method))
                    {
                        symbolContext.CancellationToken.ThrowIfCancellationRequested();

                        var method = (IMethodSymbol)member;
                        if (method.MethodKind != MethodKind.Ordinary)
                            continue;

                        var isTestMethod = method.GetAttributes().ContainsAttributeType(factType);
                        hasTestMethods = hasTestMethods || isTestMethod;

                        if (!isTestMethod &&
                            method.DeclaredAccessibility == Accessibility.Public &&
                            (method.ReturnsVoid || (taskType != null && method.ReturnType == taskType)) &&
                            !method.Equals(disposeMethod))
                        {
                            violations.Add(method);
                        }
                    }

                    if (hasTestMethods)
                    {
                        foreach (var method in violations)
                        {
                            var testType = method.Parameters.Any() ? "Theory" : "Fact";
                            symbolContext.ReportDiagnostic(Diagnostic.Create(Constants.Descriptors.X1013_PublicMethodShouldBeMarkedAsTest,
                                method.Locations.First(),
                                method.Name, method.ContainingType.Name, testType));
                        }
                    }
                }, SymbolKind.NamedType);
            });
        }
    }
}
