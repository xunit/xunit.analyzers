using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TestClassShouldHaveTFixtureArgument : XunitDiagnosticAnalyzer
	{
		public const string TFixtureDisplayNamePropertyKey = nameof(TFixtureDisplayNamePropertyKey);
		public const string TFixtureNamePropertyKey = nameof(TFixtureNamePropertyKey);
		public const string TestClassNamePropertyKey = nameof(TestClassNamePropertyKey);
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Descriptors.X1033_TestClassShouldHaveTFixtureArgument);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (context.Symbol.DeclaredAccessibility != Accessibility.Public)
					return;

				var classSymbol = (INamedTypeSymbol)context.Symbol; // RegisterSymbolAction guarantees by 2nd arg

				var doesClassContainTests = classSymbol
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
							.Any(x => x.Parameters.Length == 1 && x.Parameters.Any(p => Equals(p.Type, tFixtureDataType)));

						if (hasConstructorWithTFixtureArg)
							continue;

						var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>();
						propertiesBuilder.Add(TFixtureDisplayNamePropertyKey, tFixtureDataType.ToDisplayString());
						propertiesBuilder.Add(TFixtureNamePropertyKey, tFixtureDataType.Name);
						propertiesBuilder.Add(TestClassNamePropertyKey, classSymbol.Name);

						context.ReportDiagnostic(
							Diagnostic.Create(
								Descriptors.X1033_TestClassShouldHaveTFixtureArgument,
								location: classSymbol.Locations.First(),
								properties: propertiesBuilder.ToImmutable(),
								classSymbol.ToDisplayString(),
								tFixtureDataType.ToDisplayString()));
					}
				}
			}, SymbolKind.NamedType);
		}
	}
}
