using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

public partial class CSharpVerifier<TAnalyzer>
{
	/// <summary>
	/// Runs analysis against xUnit.net v2 and v3 in reflection mode, with a reference compiled from source.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="referenceSource">The code to compile into a metadata reference</param>
	/// <param name="diagnostics">The expected diagnostics</param>
	public static async Task VerifyAnalyzerNonAotWithMetadataReference(
		string source,
		string referenceSource,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerWithMetadataReference(new TestV2(LanguageVersion.CSharp6), source, referenceSource, diagnostics);
		await VerifyAnalyzerWithMetadataReference(new TestV3(LanguageVersion.CSharp6), source, referenceSource, diagnostics);
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Runs analysis against xUnit.net v3 in Native AOT mode, with a reference compiled from source.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="referenceSource">The code to compile into a metadata reference</param>
	/// <param name="diagnostics">The expected diagnostics</param>
	public static Task VerifyAnalyzerV3AotWithMetadataReference(
		string source,
		string referenceSource,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerWithMetadataReference(new TestV3Aot(LanguageVersion.CSharp13), source, referenceSource, diagnostics);

#endif

	static async Task VerifyAnalyzerWithMetadataReference<TVerifier>(
		TestBase<TVerifier> test,
		string source,
		string referenceSource,
		DiagnosticResult[] diagnostics)
		where TVerifier : XunitVerifier, new()
	{
		var references = await test.ReferenceAssemblies.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
		var compilation = CSharpCompilation.Create(
			"ReferencedTests",
			[CSharpSyntaxTree.ParseText(referenceSource, new CSharpParseOptions(test.LanguageVersion))],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
		);

		using var assembly = new MemoryStream();
		var emitResult = compilation.Emit(assembly);
		Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

		test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromImage(assembly.ToArray()));
		test.TestState.Sources.Add(source);
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		await test.RunAsync();
	}
}
