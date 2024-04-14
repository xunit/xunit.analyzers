using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseAsyncVoidForTestMethods : XunitDiagnosticAnalyzer
{
	public DoNotUseAsyncVoidForTestMethods() :
		base([
			Descriptors.X1048_DoNotUseAsyncVoidForTestMethods_V2,
			Descriptors.X1049_DoNotUseAsyncVoidForTestMethods_V3,
		])
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var attributeUsageType = TypeSymbolFactory.AttributeUsageAttribute(context.Compilation);
		if (attributeUsageType is null)
			return;

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not IMethodSymbol method)
				return;

			if (!method.IsTestMethod(xunitContext, attributeUsageType, strict: true))
				return;

			if (!method.ReturnsVoid)
				return;

			var location = context.Symbol.Locations.FirstOrDefault();
			if (location is null)
				return;

			var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
			propertiesBuilder.Add(Constants.Properties.DeclaringType, method.ContainingType.ToDisplayString());
			propertiesBuilder.Add(Constants.Properties.MemberName, method.Name);
			var properties = propertiesBuilder.ToImmutableDictionary();

			if (xunitContext.HasV3References)
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.X1049_DoNotUseAsyncVoidForTestMethods_V3, location, properties));
			else
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.X1048_DoNotUseAsyncVoidForTestMethods_V2, location, properties));
		}, SymbolKind.Method);
	}
}
