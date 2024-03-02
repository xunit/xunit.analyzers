using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public static partial class Descriptors
{
	public static SuppressionDescriptor CA1515_Suppression { get; } =
		Suppression("CA1515", "xUnit.net's test classes must be public.");

	public static SuppressionDescriptor CA2007_Suppression { get; } =
		Suppression("CA2007", "xUnit.net test methods should not call ConfigureAwait");
}
