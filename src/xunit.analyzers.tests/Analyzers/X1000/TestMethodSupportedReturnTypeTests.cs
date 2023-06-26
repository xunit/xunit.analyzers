using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodSupportedReturnType>;

public class TestMethodSupportedReturnTypeTests
{
	[Fact]
	public async void NonTestMethod()
	{
		var source = @"
public class NonTestClass {
    public int Add(int x, int y) {
        return x + y;
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("int")]
	[InlineData("object")]
	public async void InvalidReturnType(string returnType)
	{
		var sourceTemplate = @"
using Xunit;

public class TestClass {{
    [Fact]
    public {0} TestMethod() {{
        return default({0});
	}}
}}";

		var source = string.Format(sourceTemplate, returnType);

		// v2
		var expectedV2 =
			Verify
				.Diagnostic()
				.WithSpan(6, 13 + returnType.Length, 6, 23 + returnType.Length)
				.WithArguments("void, Task");

		await Verify.VerifyAnalyzerV2(source, expectedV2);

		// v3
		var expectedV3 =
			Verify
				.Diagnostic()
				.WithSpan(6, 13 + returnType.Length, 6, 23 + returnType.Length)
				.WithArguments("void, Task, ValueTask");

		await Verify.VerifyAnalyzerV3(source, expectedV3);
	}

	[Fact]
	public async void V2DoesNotSupportValueTask()
	{
		var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public ValueTask TestMethod() {
        return default(ValueTask);
    }
}";

		var expectedV2 =
			Verify
				.Diagnostic()
				.WithSpan(7, 22, 7, 32)
				.WithArguments("void, Task");
		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7, source, expectedV2);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7, source);
	}
}
