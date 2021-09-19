using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

public class TestMethodMustNotHaveMultipleFactAttributesTests
{
	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async void DoesNotFindErrorForMethodWithSingleAttribute(string attribute)
	{
		var source = $@"
public class TestClass {{
    [Xunit.{attribute}]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void FindsErrorForMethodWithTheoryAndFact()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.Theory]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 17, 5, 27);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsErrorForMethodWithCustomFactAttribute()
	{
		var source1 = @"
public class TestClass {
    [Xunit.Fact]
    [CustomFact]
    public void TestMethod() { }
}";
		var source2 = "public class CustomFactAttribute : Xunit.FactAttribute { }";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 17, 5, 27);

		await Verify.VerifyAnalyzerAsyncV2(new[] { source1, source2 }, expected);
	}
}
