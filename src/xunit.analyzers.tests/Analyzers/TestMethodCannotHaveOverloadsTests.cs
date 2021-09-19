using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodCannotHaveOverloads>;

public class TestMethodCannotHaveOverloadsTests
{
	[Fact]
	public async void FindsErrors_ForInstanceMethodOverloads_InSameInstanceClass()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }

    [Xunit.Theory]
    public void TestMethod(int a) { }
}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(4, 17, 4, 27)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "TestClass"),
			Verify
				.Diagnostic()
				.WithSpan(7, 17, 7, 27)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "TestClass"),
		};

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsErrors_ForStaticMethodOverloads_InSameStaticClass()
	{
		var source = @"
public static class TestClass {
    [Xunit.Fact]
    public static void TestMethod() { }

    [Xunit.Theory]
    public static void TestMethod(int a) { }
}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(4, 24, 4, 34)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "TestClass"),
			Verify
				.Diagnostic()
				.WithSpan(7, 24, 7, 34)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "TestClass"),
		};

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsErrors_ForInstanceMethodOverload_InDerivedClass()
	{
		var source1 = @"
public class TestClass : BaseClass {
    [Xunit.Theory]
    public void TestMethod(int a) { }

    private void TestMethod(int a, byte c) { }
}";
		var source2 = @"
public class BaseClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(4, 17, 4, 27)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "BaseClass"),
			Verify
				.Diagnostic()
				.WithSpan(6, 18, 6, 28)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "BaseClass"),
		};

		await Verify.VerifyAnalyzerAsyncV2(new[] { source1, source2 }, expected);
	}

	[Fact]
	public async void FindsError_ForStaticAndInstanceMethodOverload()
	{
		var source1 = @"
public class TestClass : BaseClass {
    [Xunit.Theory]
    public void TestMethod(int a) { }
}";
		var source2 = @"
public class BaseClass {
    [Xunit.Fact]
    public static void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 17, 4, 27)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "BaseClass");

		await Verify.VerifyAnalyzerAsyncV2(new[] { source1, source2 }, expected);
	}

	[Fact]
	public async void DoesNotFindError_ForMethodOverrides()
	{
		var source1 = @"
public class BaseClass {
    [Xunit.Fact]
    public virtual void TestMethod() { }
}";
		var source2 = @"
public class TestClass : BaseClass {
    [Xunit.Fact]
    public override void TestMethod() { }
}";

		await Verify.VerifyAnalyzerAsyncV2(new[] { source1, source2 });
	}
}
