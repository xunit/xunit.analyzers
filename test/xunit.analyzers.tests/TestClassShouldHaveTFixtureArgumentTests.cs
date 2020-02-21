using System.Collections.Generic;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

namespace Xunit.Analyzers
{
	public class TestClassShouldHaveTFixtureArgumentTests
	{
		public static IEnumerable<object[]> CreateFactsInNonPublicClassCases()
		{
			foreach (var factAttribute in new[] { "Xunit.Fact", "Xunit.Theory" })
				yield return new object[] { factAttribute };
		}

		[Theory]
		[MemberData(nameof(CreateFactsInNonPublicClassCases))]
		public async void ForClassWithIClassFixtureWithoutConstructorArg_FindsInfo(string factRelatedAttribute)
		{
			var source = @"
public class FixtureData { }
public class TestClass : Xunit.IClassFixture<FixtureData> { " + $"[{factRelatedAttribute}]" + @"public void TestMethod() { } }";

			var expected = Verify.Diagnostic()
				.WithLocation(3, 14)
				.WithArguments("TestClass", "FixtureData");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async void ForClassWithIClassFixtureWithConstructorArg_FindsInfo()
		{
			var source = @"
public class FixtureData { }
public class TestClass : Xunit.IClassFixture<FixtureData> 
{
    public TestClass(FixtureData fixtureData) { }
    [Xunit.Fact] public void TestMethod() { } 
}";

			await Verify.VerifyAnalyzerAsync(source);
		}
	}
}
