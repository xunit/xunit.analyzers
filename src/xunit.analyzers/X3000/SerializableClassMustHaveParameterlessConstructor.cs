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

			var serializableTargetDisplay = GetSerializableTargetDisplay(context, xunitContext, namedType);
			if (serializableTargetDisplay is null)
				return;

			var parameterlessCtor = namedType.InstanceConstructors.FirstOrDefault(c => c.Parameters.IsEmpty);
			if (parameterlessCtor is not null && parameterlessCtor.DeclaredAccessibility == Accessibility.Public)
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor,
					namedType.Locations.First(),
					namedType.Name,
					serializableTargetDisplay
				)
			);
		}, SymbolKind.NamedType);
	}

	static string? GetSerializableTargetDisplay(
		SymbolAnalysisContext context,
		XunitContext xunitContext,
		INamedTypeSymbol namedType)
	{
		// Types that implement IXunitSerializable
		if (xunitContext.Abstractions.IXunitSerializableType?.IsAssignableFrom(namedType) == true)
			return xunitContext.Abstractions.IXunitSerializableType.ToDisplayString();

		// Types that decorate with [JsonTypeID]
		if (xunitContext.V3Core?.JsonTypeIDAttributeType is INamedTypeSymbol jsonTypeIDAttributeType)
			if (namedType.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, jsonTypeIDAttributeType)))
				return jsonTypeIDAttributeType.ToDisplayString();

		return null;
	}
}
