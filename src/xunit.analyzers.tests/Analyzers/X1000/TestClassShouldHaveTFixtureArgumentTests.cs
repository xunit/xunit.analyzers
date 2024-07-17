using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

public class TestClassShouldHaveTFixtureArgumentTests
{
	public static MatrixTheoryData<string, string> CreateFactsInNonPublicClassCases =
		new(
			/* lang=c#-test */ ["Xunit.Fact", "Xunit.Theory"],
			/* lang=c#-test */ ["Xunit.IClassFixture", "Xunit.ICollectionFixture"]
		);

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async Task ForClassWithIClassFixtureWithoutConstructorArg_Triggers(
		string attribute,
		string @interface)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class FixtureData {{ }}

			public class {{|#0:TestClass|}}: {1}<FixtureData> {{
			    [{0}]
			    public void TestMethod() {{ }}
			}}
			""", attribute, @interface);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestClass", "FixtureData");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async Task ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger(
		string attribute,
		string @interface)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class FixtureData {{ }}

			public class TestClass: {1}<FixtureData> {{
			    public TestClass(FixtureData fixtureData) {{ }}

			    [{0}]
			    public void TestMethod() {{ }}
			}}
			""", attribute, @interface);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async Task ForClassWithIClassFixtureWithConstructorMultipleArg_DoesNotTrigger(
		string attribute,
		string @interface)
	{
		var source = /* lang=c#-test */ """
			public class FixtureData {{ }}

			public class TestClass: {1}<FixtureData> {{
			    public TestClass(FixtureData fixtureData, {2}.ITestOutputHelper output) {{ }}

			    [{0}]
			    public void TestMethod() {{ }}
			}}
			""";

		await Verify.VerifyAnalyzerV2(string.Format(source, attribute, @interface, "Xunit.Abstractions"));
		await Verify.VerifyAnalyzerV3(string.Format(source, attribute, @interface, "Xunit"));
	}
}
