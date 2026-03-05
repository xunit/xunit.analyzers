using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public static partial class Descriptors
{
	public static SuppressionDescriptor CA1515_Suppression { get; } =
		Suppression("CA1515", "xUnit.net's test classes must be public.");

	public static SuppressionDescriptor CA2007_Suppression { get; } =
		Suppression("CA2007", "xUnit.net test methods should not call ConfigureAwait");

	public static SuppressionDescriptor CS8618_Suppression { get; } =
		Suppression("CS8618", "Non-nullable member is initialized in IAsyncLifetime.InitializeAsync");

	public static SuppressionDescriptor VSTHRD200_Suppression { get; } =
		Suppression("VSTHRD200", "xUnit.net test methods are not directly callable and do not benefit from this naming rule");
}
