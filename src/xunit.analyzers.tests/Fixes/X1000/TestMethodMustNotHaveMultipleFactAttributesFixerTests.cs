using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

public class TestMethodMustNotHaveMultipleFactAttributesFixerTests
{
	[Fact]
	public async Task FixAll_RemovesDuplicateAttributes()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class FactDerivedAttribute : FactAttribute { }

			public class TestClass {
				[Fact]
				[{|CS0579:Fact|}]
				public void [|TestMethod1|]() { }

				[Fact]
				[{|CS0579:Fact|}]
				public void [|TestMethod2|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class FactDerivedAttribute : FactAttribute { }

			public class TestClass {
				[Fact]
				public void TestMethod1() { }

				[Fact]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, TestMethodMustNotHaveMultipleFactAttributesFixer.Key_KeepAttribute("Fact"));
	}
}
