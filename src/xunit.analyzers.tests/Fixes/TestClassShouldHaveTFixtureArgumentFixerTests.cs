using Xunit;
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

    TestClass(FixtureData fixtureData)
    {
        _fixtureData = fixtureData;
    }

    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsync(before, after);
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

    TestClass(FixtureData<object> fixtureData)
    {
        _fixtureData = fixtureData;
    }

    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
