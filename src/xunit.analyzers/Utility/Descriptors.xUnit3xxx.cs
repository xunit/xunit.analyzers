using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static Xunit.Analyzers.Category;

namespace Xunit.Analyzers;

public static partial class Descriptors
{
	public static DiagnosticDescriptor X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObject { get; } =
		Diagnostic(
			"xUnit3000",
			"Classes which cross AppDomain boundaries must derive directly or indirectly from LongLivedMarshalByRefObject",
			Extensibility,
			Error,
			"Class {0} must derive directly or indirectly from LongLivedMarshalByRefObject."
		);

	public static DiagnosticDescriptor X3001_SerializableClassMustHaveParameterlessConstructor { get; } =
		Diagnostic(
			"xUnit3001",
			"Classes that implement Xunit.Abstractions.IXunitSerializable must have a public parameterless constructor",
			Extensibility,
			Error,
			"Class {0} must have a public parameterless constructor to support Xunit.Abstractions.IXunitSerializable."
		);

	// Placeholder for rule X3002

	// Placeholder for rule X3003

	// Placeholder for rule X3004

	// Placeholder for rule X3005

	// Placeholder for rule X3006

	// Placeholder for rule X3007

	// Placeholder for rule X3008

	// Placeholder for rule X3009
}
