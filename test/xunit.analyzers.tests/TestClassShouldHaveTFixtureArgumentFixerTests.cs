using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

namespace Xunit.Analyzers
{
	public class TestClassShouldHaveTFixtureArgumentFixerTests
	{
		[Fact]
		public async void ForClassWithoutField_GenerateFieldAndConstructor()
		{
			var source = @"
public class FixtureData {}
public class [|TestClass|] : Xunit.IClassFixture<FixtureData>
{
    [Xunit.Fact]
    public void TestMethod() {}
}";
			var fixedSource = @"
public class FixtureData {}
public class [|TestClass|] : Xunit.IClassFixture<FixtureData>
{
    private readonly FixtureData _fixtureData;

    TestClass(FixtureData fixtureData)
    {
        _fixtureData = fixtureData;
    }

    [Xunit.Fact]
    public void TestMethod() {}
}";

			await Verify.VerifyCodeFixAsync(source, fixedSource);
		}
	}
}
