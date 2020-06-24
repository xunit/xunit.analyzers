using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TestCaseMustBeLongLivedMarshalByRefObject : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				var namedType = (INamedTypeSymbol)context.Symbol;
				if (namedType.TypeKind != TypeKind.Class)
					return;

				var isTestCase = xunitContext.Abstractions.ITestCaseType?.IsAssignableFrom(namedType) ?? false;
				if (!isTestCase)
					return;

				var hasMBRO = xunitContext.Execution.LongLivedMarshalByRefObjectType?.IsAssignableFrom(namedType) ?? false;
				if (hasMBRO)
					return;

				context.ReportDiagnostic(Diagnostic.Create(
					Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject,
					namedType.Locations.First(),
					namedType.Name));
			}, SymbolKind.NamedType);
		}

		protected override bool ShouldAnalyze(XunitContext xunitContext)
			=> xunitContext.HasAbstractionsReference && xunitContext.HasExecutionReference;
	}
}
