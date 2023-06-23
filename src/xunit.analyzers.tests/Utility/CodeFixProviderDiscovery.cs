using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.Composition;
using Xunit.Analyzers.Fixes;

static class CodeFixProviderDiscovery
{
	static readonly Lazy<IExportProviderFactory> ExportProviderFactory;

	static CodeFixProviderDiscovery()
	{
		ExportProviderFactory = new Lazy<IExportProviderFactory>(
			() =>
			{
				var discovery = new AttributedPartDiscovery(Resolver.DefaultInstance, isNonPublicSupported: true);
				var parts = Task.Run(() => discovery.CreatePartsAsync(typeof(CodeAnalysisExtensions).Assembly)).GetAwaiter().GetResult();
				var catalog = ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);

				var configuration = CompositionConfiguration.Create(catalog);
				var runtimeComposition = RuntimeComposition.CreateRuntimeComposition(configuration);
				return runtimeComposition.CreateExportProviderFactory();
			},
			LazyThreadSafetyMode.ExecutionAndPublication
		);
	}

	public static IEnumerable<CodeFixProvider> GetCodeFixProviders(string language)
	{
		var exportProvider = ExportProviderFactory.Value.CreateExportProvider();
		var exports = exportProvider.GetExports<CodeFixProvider, LanguageMetadata>();

		return exports.Where(export => export.Metadata.Languages.Contains(language)).Select(export => export.Value);
	}

	class LanguageMetadata
	{
		public LanguageMetadata(IDictionary<string, object> data)
		{
			if (!data.TryGetValue(nameof(ExportCodeFixProviderAttribute.Languages), out var languages))
				languages = new string[0];

			Languages = ((string[])languages).ToImmutableArray();
		}

		public ImmutableArray<string> Languages { get; }
	}
}
