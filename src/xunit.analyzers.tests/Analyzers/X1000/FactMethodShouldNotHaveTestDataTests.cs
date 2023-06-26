using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

public class FactMethodShouldNotHaveTestDataTests
{
	[Fact]
	public async void DoesNotFindErrorForFactMethodWithNoDataAttributes()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(source);
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

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async void DoesNotFindErrorForDerivedFactMethodWithDataAttributes(string dataAttribute)
	{
		var source1 = "public class DerivedFactAttribute: Xunit.FactAttribute {}";
		var source2 = $@"
public class TestClass {{
    [DerivedFactAttribute]
    [Xunit.{dataAttribute}]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzer(new[] { source1, source2 });
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async void FindsErrorForFactMethodsWithDataAttributes(string dataAttribute)
	{
		var source = $@"
public class TestClass {{
    [Xunit.Fact]
    [Xunit.{dataAttribute}]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 17, 5, 27);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
