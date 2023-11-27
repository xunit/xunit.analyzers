using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionMustBeInTheSameAssembly>;

public class CollectionDefinitionMustBeInTheSameAssemblyTests
{
	public static TheoryData<string, string> NoDiagnosticsCases = new()
	{
		{
			"[Collection(\"Test collection definition\")]",
			"[CollectionDefinition(\"Test collection definition\")]"
		},
		{
			string.Empty,
			"[CollectionDefinition(\"Test collection definition\")]"
		},
		{
			string.Empty,
			string.Empty
		}
	};

	static readonly string Template = @"
using Xunit;

{0}
public class TestClass {{ }}

namespace TestNamespace {{
    {1}
    public class TestDefinition {{ }}
}}";

	[Theory]
	[MemberData(nameof(NoDiagnosticsCases))]
	public async void CollectionDefinitionIsPresentInTheAssembly_NoDiagnostics(string classAttribute, string definitionAttribute)
	{
		var source = string.Format(Template, classAttribute, definitionAttribute);

		await Verify.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async void CollectionDefinitionIsMissingInTheAssembly_ReturnsError()
	{
		var source = string.Format(Template, "[Collection(\"Test collection definition\")]", string.Empty);

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 14, 5, 23)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("Test collection definition", "TestProject");

		await Verify.VerifyAnalyzerV2(source, expected);
	}
}
