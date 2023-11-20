using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CollectionDefinitionMustBeInTheSameAssembly : XunitV2DiagnosticAnalyzer
{
	public CollectionDefinitionMustBeInTheSameAssembly() :
		base(Descriptors.X3002_CollectionDefinitionMustBeInTheSameAssembly)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol namedType)
				return;

			var collectionAttributeType = xunitContext.Core.CollectionAttributeType;
			var collectionAttribute = namedType
				.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass.IsAssignableFrom(collectionAttributeType));

			var collectionDefinitionName = collectionAttribute?.ConstructorArguments[0].Value?.ToString();
			if (collectionDefinitionName == null)
				return;

			var collectionDefinitionAttributeType = xunitContext.Core.CollectionDefinitionAttributeType;
			var visitor = new SymbolAssemblyVisitor(symbol => symbol
				.GetAttributes()
				.Any(a => a.AttributeClass.IsAssignableFrom(collectionDefinitionAttributeType) &&
				          !a.ConstructorArguments.IsDefaultOrEmpty &&
						  a.ConstructorArguments[0].Value?.ToString() == collectionDefinitionName)
			);

			var currentAssembly = context.Compilation.Assembly;
			visitor.Visit(currentAssembly);
			if (visitor.ShortCircuitTriggered)
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X3002_CollectionDefinitionMustBeInTheSameAssembly,
					namedType.Locations.First(),
					collectionDefinitionName,
					currentAssembly.Name
				)
			);
		}, SymbolKind.NamedType);
	}
}
