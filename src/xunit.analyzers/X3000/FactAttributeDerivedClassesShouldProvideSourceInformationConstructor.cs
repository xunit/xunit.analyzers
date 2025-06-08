using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FactAttributeDerivedClassesShouldProvideSourceInformationConstructor() :
	XunitV3DiagnosticAnalyzer(Descriptors.X3003_ProvideConstructorForFactAttributeOverride)
{
	static readonly Version Version_3_0_0 = new(3, 0, 0);

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var factAttributeType = xunitContext.Core.FactAttributeType;
		if (factAttributeType is null)
			return;

		var callerFilePathAttribute = TypeSymbolFactory.CallerFilePathAttribute(context.Compilation);
		if (callerFilePathAttribute is null)
			return;

		var callerLineNumberAttribute = TypeSymbolFactory.CallerLineNumberAttribute(context.Compilation);
		if (callerLineNumberAttribute is null)
			return;

		context.RegisterSymbolAction(context =>
		{
			var type = context.Symbol as INamedTypeSymbol;
			if (type is null)
				return;

			var baseType = type.BaseType;
			while (true)
			{
				if (baseType is null)
					return;

				if (SymbolEqualityComparer.Default.Equals(factAttributeType, baseType))
					break;

				baseType = baseType.BaseType;
			}

			if (type.Constructors.Any(hasSourceInformationParameters))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X3003_ProvideConstructorForFactAttributeOverride,
					type.Locations.First()
				)
			);
		}, SymbolKind.NamedType);

		bool hasSourceInformationParameters(IMethodSymbol symbol)
		{
			var hasCallerFilePath = false;
			var hasCallerLineNumber = false;

			foreach (var parameter in symbol.Parameters)
				foreach (var attribute in parameter.GetAttributes().Select(a => a.AttributeClass))
				{
					if (SymbolEqualityComparer.Default.Equals(callerFilePathAttribute, attribute))
						hasCallerFilePath = true;
					if (SymbolEqualityComparer.Default.Equals(callerLineNumberAttribute, attribute))
						hasCallerLineNumber = true;
				}

			return hasCallerFilePath && hasCallerLineNumber;
		}
	}

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		xunitContext?.V3Core is not null && xunitContext.V3Core.Version >= Version_3_0_0;
}
