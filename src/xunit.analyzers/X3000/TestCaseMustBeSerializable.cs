using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestCaseMustBeSerializable() :
	XunitV3NonAotDiagnosticAnalyzer(
		Descriptors.X3006_TestCaseImplementationMustBeSerializable,
		Descriptors.X3007_TestCaseImplementationMightNotBeSerializable)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		if (SerializableTypeSymbols.Create(context.Compilation, xunitContext) is not SerializableTypeSymbols typeSymbols)
			return;

		var iTestCaseType = xunitContext.Common.ITestCaseType;
		if (iTestCaseType is null)
			return;

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol namedType)
				return;
			if (namedType.TypeKind != TypeKind.Class)
				return;
			if (namedType.IsAbstract)
				return;
			if (!iTestCaseType.IsAssignableFrom(namedType))
				return;
			if (typeSymbols.IXunitSerializable.IsAssignableFrom(namedType) || typeSymbols.TypesWithCustomSerializers.Any(t => t.IsAssignableFrom(namedType)))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					namedType.IsSealed
						? Descriptors.X3006_TestCaseImplementationMustBeSerializable
						: Descriptors.X3007_TestCaseImplementationMightNotBeSerializable,
					namedType.Locations.First(),
					namedType.Name,
					iTestCaseType.ToDisplayString(),
					xunitContext.Common.IXunitSerializableType?.ToDisplayString() ?? "IXunitSerializable"
				)
			);
		}, SymbolKind.NamedType);
	}
}
