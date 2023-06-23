using System.Collections.Generic;
using System.Linq;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

public class TestClassShouldHaveTFixtureArgumentTests
{
	public static IEnumerable<object[]> CreateFactsInNonPublicClassCases =
		from attribute in new[] { "Xunit.Fact", "Xunit.Theory" }
		from @interface in new[] { "Xunit.IClassFixture", "Xunit.ICollectionFixture" }
		select new[] { attribute, @interface };

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async void ForClassWithIClassFixtureWithoutConstructorArg_FindsInfo(
		string attribute,
		string @interface)
	{
		var source = $@"
public class FixtureData {{ }}

public class TestClass: {@interface}<FixtureData> {{
    [{attribute}]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 14, 4, 23)
				.WithArguments("TestClass", "FixtureData");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async void ForClassWithIClassFixtureWithConstructorArg_DonnotFindInfo(
		string attribute,
		string @interface)
	{
		var source = $@"
public class FixtureData {{ }}

public class TestClass: {@interface}<FixtureData> {{
    public TestClass(FixtureData fixtureData) {{ }}

    [{attribute}]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async void ForClassWithIClassFixtureWithConstructorMultipleArg_DonnotFindInfo(
		string attribute,
		string @interface)
	{
		var source = $@"
public class FixtureData {{ }}

public class TestClass: {@interface}<FixtureData> {{
    public TestClass(FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper output) {{ }}

    [{attribute}]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
