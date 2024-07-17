using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

public class FactMethodShouldNotHaveTestDataTests
{
	[Fact]
	public async Task FactWithNoDataAttributes_DoesNotTrigger()
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
	public async Task TheoryWithDataAttributes_DoesNotTrigger(string dataAttribute)
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
	public async Task FactDerivedMethodWithDataAttributes_DoesNotTrigger(string dataAttribute)
	{
		var source1 = /* lang=c#-test */ "public class DerivedFactAttribute: Xunit.FactAttribute {}";
		var source2 = string.Format(/* lang=c#-test */ """
			public class TestClass {{
			    [DerivedFactAttribute]
			    [Xunit.{0}]
			    public void TestMethod() {{ }}
			}}
			""", dataAttribute);

		await Verify.VerifyAnalyzer([source1, source2]);
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async Task FactWithDataAttributes_Triggers(string dataAttribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
			    [Xunit.Fact]
			    [Xunit.{0}]
			    public void [|TestMethod|]() {{ }}
			}}
			""", dataAttribute);

		await Verify.VerifyAnalyzer(source);
	}
}
