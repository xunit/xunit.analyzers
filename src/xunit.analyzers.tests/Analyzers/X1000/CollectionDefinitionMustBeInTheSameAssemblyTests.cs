using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionMustBeInTheSameAssembly>;

public class CollectionDefinitionMustBeInTheSameAssemblyTests
{
	public static TheoryData<string, string, string> BasicNoDiagnosticsCases = new()
	{
		{
			"[Collection(\"Test collection definition\")]",
			"[CollectionDefinition(\"Test collection definition\")]",
			"TestFixture fixture"
		},
		{
			string.Empty,
			"[CollectionDefinition(\"Test collection definition\")]",
			"TestFixture fixture"
		},
		{
			string.Empty,
			string.Empty,
			"TestFixture fixture"
		},
		{
			"[Collection(\"No fixture required collection definition\")]",
			"[CollectionDefinition(\"Test collection definition\")]",
			string.Empty
		},
		{
			string.Empty,
			string.Empty,
			string.Empty
		},
		{
			string.Empty,
			"[CollectionDefinition(\"Test collection definition\")]",
			string.Empty
		},
		{
			"[Collection(\"No fixture required collection definition\")]",
			string.Empty,
			string.Empty
		},
	};

	static readonly string BasicCasesTemplate = @"
using Xunit;

{0}
public class TestClass
{{
    public TestClass({2}) {{ }}
}}

public class TestFixture {{ }}

namespace TestNamespace {{
    {1}
    public class TestDefinition : ICollectionFixture<TestFixture> {{ }}
}}";

	[Theory]
	[MemberData(nameof(BasicNoDiagnosticsCases))]
	public async void CollectionDefinitionIsPresentInTheAssembly_NoDiagnostics(
		string classAttribute,
		string definitionAttribute,
		string classConstructorParams)
	{
		var source = string.Format(BasicCasesTemplate, classAttribute, definitionAttribute, classConstructorParams);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void CollectionDefinitionIsMissingInTheAssembly_ReturnsError()
	{
		var source = string.Format(BasicCasesTemplate, "[Collection(\"Test collection definition\")]", string.Empty, "TestFixture fixture");

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 14, 5, 23)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("Test collection definition", "TestProject");

		await Verify.VerifyAnalyzer(source, expected);
	}
}
