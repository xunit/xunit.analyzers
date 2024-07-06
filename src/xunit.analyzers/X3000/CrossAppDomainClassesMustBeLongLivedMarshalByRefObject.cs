using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CrossAppDomainClassesMustBeLongLivedMarshalByRefObject : XunitV2DiagnosticAnalyzer
{
	public CrossAppDomainClassesMustBeLongLivedMarshalByRefObject() :
		base(Descriptors.X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObject)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol namedType)
				return;
			if (namedType.TypeKind != TypeKind.Class)
				return;
			if (xunitContext.V2Abstractions is null)
				return;

			var mbroInterfaces = new INamedTypeSymbol?[]
			{
				xunitContext.V2Abstractions.IAssemblyInfoType,
				xunitContext.V2Abstractions.IAttributeInfoType,
				xunitContext.V2Abstractions.IMessageSinkMessageType,
				xunitContext.V2Abstractions.IMessageSinkType,
				xunitContext.V2Abstractions.IMethodInfoType,
				xunitContext.V2Abstractions.IParameterInfoType,
				xunitContext.V2Abstractions.ISourceInformationProviderType,
				xunitContext.V2Abstractions.ISourceInformationType,
				xunitContext.V2Abstractions.ITestAssemblyType,
				xunitContext.V2Abstractions.ITestCaseType,
				xunitContext.V2Abstractions.ITestClassType,
				xunitContext.V2Abstractions.ITestCollectionType,
				xunitContext.V2Abstractions.ITestFrameworkDiscovererType,
				xunitContext.V2Abstractions.ITestFrameworkExecutorType,
				xunitContext.V2Abstractions.ITestFrameworkType,
				xunitContext.V2Abstractions.ITestMethodType,
				xunitContext.V2Abstractions.ITestType,
				xunitContext.V2Abstractions.ITypeInfoType,
			};

			if (!mbroInterfaces.Any(t => t.IsAssignableFrom(namedType)))
				return;

			var hasMBRO =
				(xunitContext.V2Execution?.LongLivedMarshalByRefObjectType?.IsAssignableFrom(namedType) ?? false) ||
				(xunitContext.V2RunnerUtility?.LongLivedMarshalByRefObjectType?.IsAssignableFrom(namedType) ?? false);

			if (hasMBRO)
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.NewBaseType] =
				xunitContext.V2RunnerUtility is not null ? Constants.Types.Xunit.LongLivedMarshalByRefObject_RunnerUtility :
				xunitContext.V2Execution is not null ? Constants.Types.Xunit.LongLivedMarshalByRefObject_Execution_V2 :
				null;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObject,
					namedType.Locations.First(),
					builder.ToImmutable(),
					namedType.Name
				)
			);
		}, SymbolKind.NamedType);
	}

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		Guard.ArgumentNotNull(xunitContext).V2Abstractions is not null;
}
