using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

public class TestMethodMustNotHaveMultipleFactAttributesFixerTests
{
	[Fact]
	public async Task RemovesSecondAttribute()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class FactDerivedAttribute : FactAttribute { }

			public class TestClass {
			    [Fact]
			    [{|CS0579:Fact|}]
			    public void [|TestMethod|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class FactDerivedAttribute : FactAttribute { }

			public class TestClass {
			    [Fact]
			    public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, TestMethodMustNotHaveMultipleFactAttributesFixer.Key_KeepAttribute("Fact"));
	}
}
