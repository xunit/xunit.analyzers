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

			var isXunitSerializable = xunitContext.Abstractions.IXunitSerializableType?.IsAssignableFrom(namedType) ?? false;
			if (!isXunitSerializable)
				return;

			var parameterlessCtor = namedType.InstanceConstructors.FirstOrDefault(c => c.Parameters.IsEmpty);
			if (parameterlessCtor is not null && parameterlessCtor.DeclaredAccessibility == Accessibility.Public)
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor,
					namedType.Locations.First(),
					namedType.Name
				)
			);
		}, SymbolKind.NamedType);
	}
}
