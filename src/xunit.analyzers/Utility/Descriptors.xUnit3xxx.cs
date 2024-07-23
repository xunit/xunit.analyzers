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
			"Classes that are marked as serializable must have a public parameterless constructor",
			Extensibility,
			Error,
			"Class {0} must have a public parameterless constructor to support {1}."
		);

	public static DiagnosticDescriptor X3002_DoNotTestForConcreteTypeOfJsonSerializableTypes { get; } =
		Diagnostic(
			"xUnit3002",
			"Classes which are JSON serializable should not be tested for their concrete type",
			Extensibility,
			Hidden,
			"Class {0} is JSON serializable and should not be tested for its concrete type. Test for its primary interface instead."
		);

	// Placeholder for rule X3003

	// Placeholder for rule X3004

	// Placeholder for rule X3005

	// Placeholder for rule X3006

	// Placeholder for rule X3007

	// Placeholder for rule X3008

	// Placeholder for rule X3009
}
