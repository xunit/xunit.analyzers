using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PublicMethodShouldBeMarkedAsTest : XunitDiagnosticAnalyzer
{
	public PublicMethodShouldBeMarkedAsTest() :
		base(Descriptors.X1013_PublicMethodShouldBeMarkedAsTest)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var taskType = TypeSymbolFactory.Task(context.Compilation);
		var configuredTaskAwaitableType = TypeSymbolFactory.ConfiguredTaskAwaitable(context.Compilation);
		var interfacesToIgnore = new List<INamedTypeSymbol?>
		{
			TypeSymbolFactory.IDisposable(context.Compilation),
			TypeSymbolFactory.IAsyncLifetime(context.Compilation),
		};

		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null)
				return;
			if (context.Symbol is not INamedTypeSymbol type)
				return;

			var attributeUsageType = TypeSymbolFactory.AttributeUsageAttribute(context.Compilation);

			if (type.TypeKind != TypeKind.Class ||
					type.DeclaredAccessibility != Accessibility.Public ||
					type.IsAbstract)
				return;

			var methodsToIgnore =
				interfacesToIgnore
					.WhereNotNull()
					.Where(i => type.AllInterfaces.Contains(i))
					.SelectMany(i => i.GetMembers())
					.Select(m => type.FindImplementationForInterfaceMember(m))
					.Where(s => s is not null)
					.ToList();

			var hasTestMethods = false;
			var violations = new List<IMethodSymbol>();
			foreach (var member in type.GetMembers().Where(m => m.Kind == SymbolKind.Method))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				// Check for method.IsAbstract and earlier for type.IsAbstract is done
				// twice to enable better diagnostics during code editing. It is useful with
				// incomplete code for abstract types - missing abstract keyword on type
				// or on abstract method
				if (member is not IMethodSymbol method)
					continue;
				if (method.MethodKind != MethodKind.Ordinary || method.IsAbstract)
					continue;

				var attributes = method.GetAttributesWithInheritance(attributeUsageType);
				var isTestMethod = attributes.ContainsAttributeType(xunitContext.Core.FactAttributeType);
				hasTestMethods = hasTestMethods || isTestMethod;

				if (isTestMethod ||
					attributes.Any(attribute => attribute.AttributeClass is not null && attribute.AttributeClass.GetAttributes().Any(att => att.AttributeClass?.Name.EndsWith("IgnoreXunitAnalyzersRule1013Attribute", StringComparison.InvariantCulture) == true)))
				{
					continue;
				}

				if (method.DeclaredAccessibility == Accessibility.Public &&
					(method.ReturnsVoid ||
						(taskType is not null && SymbolEqualityComparer.Default.Equals(method.ReturnType, taskType)) ||
						(configuredTaskAwaitableType is not null && SymbolEqualityComparer.Default.Equals(method.ReturnType, configuredTaskAwaitableType))))
				{
					var shouldIgnore = false;
					while (!shouldIgnore || method.IsOverride)
					{
						if (methodsToIgnore.Any(m => SymbolEqualityComparer.Default.Equals(method, m)) || !method.ReceiverType.IsTestClass(xunitContext, strict: true))
							shouldIgnore = true;

						if (!method.IsOverride)
							break;

						if (method.OverriddenMethod is null)
						{
							shouldIgnore = true;
							break;
						}

						method = method.OverriddenMethod;
					}

					if (method is not null && !shouldIgnore)
						violations.Add(method);
				}
			}

			if (hasTestMethods)
				foreach (var method in violations)
				{
					var testType = method.Parameters.Any() ? Constants.Attributes.Theory : Constants.Attributes.Fact;

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1013_PublicMethodShouldBeMarkedAsTest,
							method.Locations.First(),
							method.Name,
							method.ContainingType.Name,
							testType
						)
					);
				}
		}, SymbolKind.NamedType);
	}
}
