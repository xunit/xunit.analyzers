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

	public static TheoryData<string, string, string> InheritedDefinitionNoDiagnosticsCases = new()
	{
		{
			string.Empty,
			", ICollectionFixture<TestFixture>",
			": ICollectionFixture<AnotherTestFixture>"
		},
		{
			"TestFixture fixture",
			", ICollectionFixture<TestFixture>",
			": ICollectionFixture<AnotherTestFixture>"
		},
		{
			"AnotherTestFixture anotherFixture",
			", ICollectionFixture<TestFixture>",
			": ICollectionFixture<AnotherTestFixture>"
		},
		{
			"TestFixture fixture, AnotherTestFixture anotherFixture",
			", ICollectionFixture<TestFixture>",
			": ICollectionFixture<AnotherTestFixture>"
		},
		{
			"TestFixture fixture, AnotherTestFixture anotherFixture",
			", ICollectionFixture<TestFixture>, ICollectionFixture<AnotherTestFixture>",
			string.Empty
		},
		{
			"TestFixture fixture, AnotherTestFixture anotherFixture",
			string.Empty,
			": ICollectionFixture<TestFixture>, ICollectionFixture<AnotherTestFixture>"
		},
	};

	public static TheoryData<string, string, string> InheritedDefinitionErrorCases = new()
	{
		{
			"TestFixture fixture, AnotherTestFixture anotherFixture",
			string.Empty,
			": ICollectionFixture<AnotherTestFixture>"
		},
		{
			"TestFixture fixture, AnotherTestFixture anotherFixture",
			", ICollectionFixture<TestFixture>",
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

	static readonly string InheritedDefinitionTemplate = @"
using Xunit;

[Collection(""Test collection definition"")]
public class TestClass
{{
    public TestClass({0}) {{ }}
}}

public class TestFixture {{ }}
public class AnotherTestFixture {{ }}

namespace TestNamespace {{
	public class BaseTestDefinition {2} {{ }}

    [CollectionDefinition(""Test collection definition"")]
    public class TestDefinition : BaseTestDefinition {1} {{ }}
}}";

	[Theory]
	[MemberData(nameof(BasicNoDiagnosticsCases))]
	public async void BasicCollectionDefinitionIsPresentInTheAssembly_NoDiagnostics(
		string classAttribute,
		string definitionAttribute,
		string classConstructorParams)
	{
		var source = string.Format(BasicCasesTemplate, classAttribute, definitionAttribute, classConstructorParams);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(InheritedDefinitionNoDiagnosticsCases))]
	public async void InheritedCollectionDefinitionIsPresentInTheAssembly_NoDiagnostics(
		string classConstructorParams,
		string definitionInherited,
		string baseDefinitionInherited)
	{
		var source = string.Format(InheritedDefinitionTemplate, classConstructorParams, definitionInherited, baseDefinitionInherited);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void BasicCollectionDefinitionIsMissingInTheAssembly_ReturnsError()
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

	[Theory]
	[MemberData(nameof(InheritedDefinitionErrorCases))]
	public async void InheritedCollectionDefinitionIsMissingInTheAssembly_ReturnsError(
		string classConstructorParams,
		string definitionInherited,
		string baseDefinitionInherited)
	{
		var source = string.Format(InheritedDefinitionTemplate, classConstructorParams, definitionInherited, baseDefinitionInherited);

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 14, 5, 23)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("Test collection definition", "TestProject");

		await Verify.VerifyAnalyzer(source, expected);
	}
}
