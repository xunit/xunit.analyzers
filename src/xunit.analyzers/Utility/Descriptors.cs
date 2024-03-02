using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public static partial class Descriptors
{
	static readonly ConcurrentDictionary<Category, string> categoryMapping = new();

	static DiagnosticDescriptor Diagnostic(
		string id,
		string title,
		Category category,
		DiagnosticSeverity defaultSeverity,
		string messageFormat)
	{
		var helpLink = $"https://xunit.net/xunit.analyzers/rules/{id}";
		var categoryString = categoryMapping.GetOrAdd(category, c => c.ToString());

		return new DiagnosticDescriptor(id, title, messageFormat, categoryString, defaultSeverity, isEnabledByDefault: true, helpLinkUri: helpLink);
	}

	static SuppressionDescriptor Suppression(
		string suppressedDiagnosticId,
		string justification) =>
			new("xUnitSuppress-" + suppressedDiagnosticId, suppressedDiagnosticId, justification);
}
