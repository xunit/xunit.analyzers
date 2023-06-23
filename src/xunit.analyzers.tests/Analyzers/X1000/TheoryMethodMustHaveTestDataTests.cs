using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

public class TheoryMethodMustHaveTestDataTests
{
	[Fact]
	public async void DoesNotFindErrorForFactMethod()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async void DoesNotFindErrorForTheoryMethodWithDataAttributes(string dataAttribute)
	{
		var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.{dataAttribute}]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void FindsErrorForTheoryMethodMissingData()
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

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}
}
