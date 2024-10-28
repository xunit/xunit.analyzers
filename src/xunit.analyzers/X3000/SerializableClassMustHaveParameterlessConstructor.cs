using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SerializableClassMustHaveParameterlessConstructor : XunitDiagnosticAnalyzer
{
	public SerializableClassMustHaveParameterlessConstructor() :
		base(Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor)
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

			var serializableTargetDisplay = GetSerializableTargetDisplay(xunitContext, namedType);
			if (serializableTargetDisplay is null)
				return;

			var parameterlessCtor = namedType.InstanceConstructors.FirstOrDefault(c => c.Parameters.IsEmpty);
			if (parameterlessCtor is not null && parameterlessCtor.DeclaredAccessibility == Accessibility.Public)
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.IsCtorObsolete] = serializableTargetDisplay.Value.IsCtorObsolete ? bool.TrueString : bool.FalseString;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor,
					namedType.Locations.First(),
					builder.ToImmutable(),
					namedType.Name,
					serializableTargetDisplay.Value.DisplayName
				)
			);
		}, SymbolKind.NamedType);
	}

	static (string DisplayName, bool IsCtorObsolete)? GetSerializableTargetDisplay(
		XunitContext xunitContext,
		INamedTypeSymbol namedType)
	{
		// Types that implement IRunnerReporter (v3 only)
		if (xunitContext.V3RunnerCommon?.IRunnerReporterType?.IsAssignableFrom(namedType) == true)
			return (xunitContext.V3RunnerCommon.IRunnerReporterType.ToDisplayString(), false);

		// Types that implement IXunitSerializable
		if (xunitContext.Common.IXunitSerializableType?.IsAssignableFrom(namedType) == true)
			return (xunitContext.Common.IXunitSerializableType.ToDisplayString(), true);

		// Types that implement IXunitSerializer
		if (xunitContext.V3Common?.IXunitSerializerType?.IsAssignableFrom(namedType) == true)
			return (xunitContext.V3Common.IXunitSerializerType.ToDisplayString(), false);

		// Types that decorate with [JsonTypeID]
		if (xunitContext.V3Core?.JsonTypeIDAttributeType is INamedTypeSymbol jsonTypeIDAttributeType)
			if (namedType.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, jsonTypeIDAttributeType)))
				return (jsonTypeIDAttributeType.ToDisplayString(), true);

		return null;
	}
}
