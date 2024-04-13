using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

public class TheoryMethodShouldHaveParametersTests
{
	[Fact]
	public async Task DoesNotFindErrorForFactMethod()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task DoesNotFindErrorForTheoryMethodWithParameters()
	{
		var source = @"
public class TestClass {
    [Xunit.Theory]
    public void TestMethod(string s) { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task FindsErrorForTheoryMethodWithoutParameters()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 17, 4, 27);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
