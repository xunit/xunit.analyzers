using System.Collections.Generic;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

namespace Xunit.Analyzers
{
	public class TestClassShouldHaveTFixtureArgumentTests
	{
		public static IEnumerable<object[]> CreateFactsInNonPublicClassCases()
		{
			foreach (var factAttribute in new[] { "Xunit.Fact", "Xunit.Theory" })
				foreach (var fixtureInterface in new[] { "Xunit.IClassFixture", "Xunit.ICollectionFixture" })
					yield return new object[] { factAttribute, fixtureInterface };
		}

		[Theory]
		[MemberData(nameof(CreateFactsInNonPublicClassCases))]
		public async void ForClassWithIClassFixtureWithoutConstructorArg_FindsInfo(string factRelatedAttribute, string fixtureInterface)
		{
			var source = @"
public class FixtureData { }
public class TestClass : " + fixtureInterface + @"<FixtureData> { " + $"[{factRelatedAttribute}]" + @"public void TestMethod() { } }";

			var expected = Verify.Diagnostic()
				.WithLocation(3, 14)
				.WithArguments("TestClass", "FixtureData");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[MemberData(nameof(CreateFactsInNonPublicClassCases))]
		public async void ForClassWithIClassFixtureWithConstructorArg_DonnotFindInfo(string factRelatedAttribute, string fixtureInterface)
		{
			var source = @"
public class FixtureData { }
public class TestClass : " + fixtureInterface + @"<FixtureData> 
{
	public TestClass(FixtureData fixtureData) { }

	[" + factRelatedAttribute + @"] public void TestMethod() { }
}";

			await Verify.VerifyAnalyzerAsync(source);
		}
	}
}
