using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

public class TestClassShouldHaveTFixtureArgumentFixerTests
{
	[Fact]
	public async void ForClassWithoutField_GenerateFieldAndConstructor()
	{
		var before = @"
public class FixtureData { }

public class [|TestClass|]: Xunit.IClassFixture<FixtureData> {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		var after = @"
public class FixtureData { }

public class [|TestClass|]: Xunit.IClassFixture<FixtureData> {
    private readonly FixtureData _fixtureData;

    public TestClass(FixtureData fixtureData)
    {
        _fixtureData = fixtureData;
    }

    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, TestClassShouldHaveTFixtureArgumentFixer.Key_GenerateConstructor);
	}

	[Fact]
	public async void ForGenericTFixture_GenerateFieldAndConstructor()
	{
		var before = @"
public class FixtureData<T> { }

public class [|TestClass|]: Xunit.IClassFixture<FixtureData<object>> {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		var after = @"
public class FixtureData<T> { }

public class [|TestClass|]: Xunit.IClassFixture<FixtureData<object>> {
    private readonly FixtureData<object> _fixtureData;

    public TestClass(FixtureData<object> fixtureData)
    {
        _fixtureData = fixtureData;
    }

    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, TestClassShouldHaveTFixtureArgumentFixer.Key_GenerateConstructor);
	}
}
