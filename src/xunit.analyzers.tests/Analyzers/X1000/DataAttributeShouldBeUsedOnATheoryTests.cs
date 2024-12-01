using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class DataAttributeShouldBeUsedOnATheoryTests
{
	[Fact]
	public async Task FactMethodWithNoDataAttributes_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
				[Xunit.Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async Task FactMethodWithDataAttributes_DoesNotTrigger(string dataAttribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
				[Xunit.Fact]
				[Xunit.{0}]
				public void TestMethod() {{ }}
			}}
			""", dataAttribute);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async Task TheoryMethodWithDataAttributes_DoesNotTrigger(string dataAttribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
				[Xunit.Theory]
				[Xunit.{0}]
				public void TestMethod() {{ }}
			}}
			""", dataAttribute);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async Task MethodsWithDataAttributesButNotFactOrTheory_Triggers(string dataAttribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
				[Xunit.{0}]
				public void [|TestMethod|]() {{ }}
			}}
			""", dataAttribute);

		await Verify.VerifyAnalyzer(source);
	}
}
