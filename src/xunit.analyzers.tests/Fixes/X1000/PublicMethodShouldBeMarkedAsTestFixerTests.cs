using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

public class PublicMethodShouldBeMarkedAsTestFixerTests
{
	const string beforeNoParams = /* lang=c#-test */ """
		using Xunit;

		public class TestClass {
			[Fact]
			public void TestMethod() { }

			public void [|TestMethod2|]() { }
		}
		""";
	const string beforeWithParams = /* lang=c#-test */ """
		using Xunit;

		public class TestClass {
			[Fact]
			public void TestMethod() { }

			public void [|TestMethod2|](int _) { }
		}
		""";

	[Fact]
	public async Task AddsFactToPublicMethodWithoutParameters()
	{
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				[Fact]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFix(beforeNoParams, after, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToFact);
	}

	[Fact]
	public async Task AddsFactToPublicMethodWithParameters()
	{
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				[Theory]
				public void TestMethod2(int _) { }
			}
			""";

		await Verify.VerifyCodeFix(beforeWithParams, after, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToTheory);
	}

	[Theory]
	[InlineData(beforeNoParams)]
	[InlineData(beforeWithParams)]
	public async Task MarksMethodAsInternal(string before)
	{
		var after = before.Replace(/* lang=c#-test */ "public void [|TestMethod2|]", /* lang=c#-test */ "internal void TestMethod2");

		await Verify.VerifyCodeFix(before, after, PublicMethodShouldBeMarkedAsTestFixer.Key_MakeMethodInternal);
	}
}
