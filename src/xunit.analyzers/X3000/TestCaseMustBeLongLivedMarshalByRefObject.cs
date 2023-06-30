using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestCaseMustBeLongLivedMarshalByRefObject : XunitV2DiagnosticAnalyzer
{
	public TestCaseMustBeLongLivedMarshalByRefObject() :
		base(Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol namedType)
				return;
			if (namedType.TypeKind != TypeKind.Class)
				return;

			var isTestCase = xunitContext.V2Abstractions?.ITestCaseType?.IsAssignableFrom(namedType) ?? false;
			if (!isTestCase)
				return;

			var hasMBRO = xunitContext.V2Execution?.LongLivedMarshalByRefObjectType?.IsAssignableFrom(namedType) ?? false;
			if (hasMBRO)
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.CanFix] = (xunitContext.V2Execution != null).ToString();

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject,
					namedType.Locations.First(),
					builder.ToImmutable(),
					namedType.Name
				)
			);
		}, SymbolKind.NamedType);
	}

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		xunitContext.V2Abstractions is not null;
}
