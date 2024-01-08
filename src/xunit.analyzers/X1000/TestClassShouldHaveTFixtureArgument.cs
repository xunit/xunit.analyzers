using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestClassShouldHaveTFixtureArgument : XunitDiagnosticAnalyzer
{
	public TestClassShouldHaveTFixtureArgument() :
		base(Descriptors.X1033_TestClassShouldHaveTFixtureArgument)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.IClassFixtureType is null || xunitContext.Core.ICollectionFixtureType is null)
				return;

			if (context.Symbol.DeclaredAccessibility != Accessibility.Public)
				return;
			if (context.Symbol is not INamedTypeSymbol classSymbol)
				return;

			var doesClassContainTests =
				classSymbol
					.GetMembers()
					.OfType<IMethodSymbol>()
					.Any(m => m.GetAttributes().Any(a => xunitContext.Core.FactAttributeType.IsAssignableFrom(a.AttributeClass)));

			if (!doesClassContainTests)
				return;

			foreach (var interfaceOnTestClass in classSymbol.AllInterfaces)
			{
				var isFixtureInterface =
					interfaceOnTestClass.OriginalDefinition.IsAssignableFrom(xunitContext.Core.IClassFixtureType)
					|| interfaceOnTestClass.OriginalDefinition.IsAssignableFrom(xunitContext.Core.ICollectionFixtureType);

				if (isFixtureInterface && interfaceOnTestClass.TypeArguments[0] is INamedTypeSymbol tFixtureDataType)
				{
					var hasConstructorWithTFixtureArg = classSymbol
						.Constructors
						.Any(x => x.Parameters.Length > 0 && x.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, tFixtureDataType)));

					if (hasConstructorWithTFixtureArg)
						continue;

					var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
					propertiesBuilder.Add(Constants.Properties.TFixtureDisplayName, tFixtureDataType.ToDisplayString());
					propertiesBuilder.Add(Constants.Properties.TFixtureName, tFixtureDataType.Name);
					propertiesBuilder.Add(Constants.Properties.TestClassName, classSymbol.Name);

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1033_TestClassShouldHaveTFixtureArgument,
							location: classSymbol.Locations.First(),
							properties: propertiesBuilder.ToImmutable(),
							classSymbol.ToDisplayString(),
							tFixtureDataType.ToDisplayString()
						)
					);
				}
			}
		}, SymbolKind.NamedType);
	}
}
