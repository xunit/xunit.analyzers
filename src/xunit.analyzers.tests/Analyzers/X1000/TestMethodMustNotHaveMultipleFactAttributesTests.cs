using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

public class TestMethodMustNotHaveMultipleFactAttributesTests
{
	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async Task MethodWithSingleAttribute_DoesNotTrigger(string attribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
				[Xunit.{0}]
				public void TestMethod() {{ }}
			}}
			""", attribute);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task MethodWithFactAndTheory_Triggers()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
				[Xunit.Fact]
				[Xunit.Theory]
				public void [|TestMethod|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task MethodWithFactAndCustomFactAttribute_Triggers()
	{
		var source1 = /* lang=c#-test */ """
			public class TestClass {
				[Xunit.Fact]
				[CustomFact]
				public void [|TestMethod|]() { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			public class CustomFactAttribute : Xunit.FactAttribute { }
			""";

		await Verify.VerifyAnalyzer([source1, source2]);
	}
}
