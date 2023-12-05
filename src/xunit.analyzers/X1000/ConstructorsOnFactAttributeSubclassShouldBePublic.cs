using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConstructorsOnFactAttributeSubclassShouldBePublic : XunitDiagnosticAnalyzer
{
	public ConstructorsOnFactAttributeSubclassShouldBePublic() :
		base(Descriptors.X1043_ConstructorsOnFactAttributeSubclassShouldBePublic)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null)
				return;
			if (context.Symbol is not INamedTypeSymbol type)
				return;
			if (type.TypeKind != TypeKind.Class || type.IsAbstract)
				return;
			if (!xunitContext.Core.FactAttributeType.IsAssignableFrom(type))
				return;

			var violations = type.Constructors.WhereNotNull().Where(
				c => c.DeclaredAccessibility == Accessibility.ProtectedOrInternal || c.DeclaredAccessibility == Accessibility.Internal);

			foreach (var method in violations)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1043_ConstructorsOnFactAttributeSubclassShouldBePublic,
						method.Locations.First(),
						type.Name
					)
				);
			}
		}, SymbolKind.NamedType);
	}
}
