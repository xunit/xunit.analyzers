using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConstructorsOnFactAttributeSubclassShouldBePublic : XunitDiagnosticAnalyzer
{
	public ConstructorsOnFactAttributeSubclassShouldBePublic() :
		base(Descriptors.X1043_ConstructorOnFactAttributeSubclassShouldBePublic)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		if (xunitContext.Core.FactAttributeType is null)
			return;

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not IMethodSymbol method)
				return;

			var attributes = method.GetAttributes();
			foreach (var attribute in attributes)
			{
				var attributeClass = attribute.AttributeClass;
				if (attributeClass is null)
					continue;

				if (!xunitContext.Core.FactAttributeType.IsAssignableFrom(attributeClass))
					continue;

				var constructor = attribute.AttributeConstructor;
				if (constructor is null)
					continue;

				if (constructor.DeclaredAccessibility == Accessibility.ProtectedOrInternal
					|| constructor.DeclaredAccessibility == Accessibility.Internal)
				{
					if (attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) is not AttributeSyntax attributeSyntax)
						return;

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1043_ConstructorOnFactAttributeSubclassShouldBePublic,
							attributeSyntax.GetLocation(),
							attributeClass.Name
						)
					);
				}
			}
		}, SymbolKind.Method);
	}
}
