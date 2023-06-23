using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class DataAttributeShouldBeUsedOnATheoryTests
{
	[Fact]
	public async void DoesNotFindErrorForFactMethodWithNoDataAttributes()
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
	public async void DoesNotFindErrorForFactMethodWithDataAttributes(string dataAttribute)
	{
		var source = $@"
public class TestClass {{
    [Xunit.Fact]
    [Xunit.{dataAttribute}]
    public void TestMethod() {{ }}
}}";

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

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async void FindsErrorForMethodsWithDataAttributesButNotFactOrTheory(string dataAttribute)
	{
		var source = $@"
public class TestClass {{
    [Xunit.{dataAttribute}]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 17, 4, 27);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}
}
