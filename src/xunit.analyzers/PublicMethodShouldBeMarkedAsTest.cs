using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PublicMethodShouldBeMarkedAsTest : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Descriptors.X1013_PublicMethodShouldBeMarkedAsTest);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext)
		{
			var taskType = context.Compilation.GetTypeByMetadataName(Constants.Types.SystemThreadingTasksTask);
			var configuredTaskAwaitableType = context.Compilation.GetTypeByMetadataName(Constants.Types.SystemRuntimeCompilerServicesConfiguredTaskAwaitable);
			var interfacesToIgnore = new List<INamedTypeSymbol>
			{
				context.Compilation.GetSpecialType(SpecialType.System_IDisposable),
				context.Compilation.GetTypeByMetadataName(Constants.Types.XunitIAsyncLifetime),
			};

			context.RegisterSymbolAction(context =>
			{
				var type = (INamedTypeSymbol)context.Symbol;

				if (type.TypeKind != TypeKind.Class ||
					type.DeclaredAccessibility != Accessibility.Public ||
					type.IsAbstract)
					return;

				var methodsToIgnore = interfacesToIgnore.Where(i => i != null && type.AllInterfaces.Contains(i))
					.SelectMany(i => i.GetMembers())
					.Select(m => type.FindImplementationForInterfaceMember(m))
					.Where(s => s != null)
					.ToList();

				var hasTestMethods = false;
				var violations = new List<IMethodSymbol>();
				foreach (var member in type.GetMembers().Where(m => m.Kind == SymbolKind.Method))
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					var method = (IMethodSymbol)member;
					// Check for method.IsAbstract and earlier for type.IsAbstract is done
					// twice to enable better diagnostics during code editing. It is useful with
					// incomplete code for abstract types - missing abstract keyword  on type
					// or on abstract method
					if (method.MethodKind != MethodKind.Ordinary || method.IsAbstract)
						continue;

					var attributes = method.GetAttributes();
					var isTestMethod = attributes.ContainsAttributeType(xunitContext.Core.FactAttributeType);
					hasTestMethods = hasTestMethods || isTestMethod;

					if (isTestMethod ||
						attributes.Any(attribute => attribute.AttributeClass.GetAttributes()
							.Any(att => att.AttributeClass.Name.EndsWith("IgnoreXunitAnalyzersRule1013Attribute"))))
						continue;

					if (method.DeclaredAccessibility == Accessibility.Public &&
						(method.ReturnsVoid ||
						 (taskType != null && Equals(method.ReturnType, taskType)) ||
						 (configuredTaskAwaitableType != null && Equals(method.ReturnType, configuredTaskAwaitableType))))
					{
						var shouldIgnore = false;
						while (!shouldIgnore || method.IsOverride)
						{
							if (methodsToIgnore.Any(m => method.Equals(m)))
								shouldIgnore = true;

							if (!method.IsOverride)
								break;

							method = method.OverriddenMethod;
							if (method == null)
							{
								shouldIgnore = true;
								break;
							}
						}

						if (!shouldIgnore)
							violations.Add(method);
					}
				}

				if (hasTestMethods)
				{
					foreach (var method in violations)
					{
						var testType = method.Parameters.Any() ? "Theory" : "Fact";
						context.ReportDiagnostic(
							Diagnostic.Create(
								Descriptors.X1013_PublicMethodShouldBeMarkedAsTest,
								method.Locations.First(),
								method.Name,
								method.ContainingType.Name,
								testType));
					}
				}
			}, SymbolKind.NamedType);
		}
	}
}
