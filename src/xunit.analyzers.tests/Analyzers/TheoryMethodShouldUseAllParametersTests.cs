using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldUseAllParameters>;

public class TheoryMethodShouldUseAllParametersTests
{
	[Fact]
	public async void FindsWarning_ParameterNotReferenced()
	{
		var source = @"
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(int unused) { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 25, 6, 31)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "unused");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsWarning_ParameterUnread()
	{
		var source = @"
using System;
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(int unused) {
        unused = 3;
        int.TryParse(""123"", out unused);
    }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(7, 25, 7, 31)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "unused");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsWarning_MultipleUnreadParameters()
	{
		var source = @"
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(int foo, int bar, int baz) { }
}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(6, 25, 6, 28)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "foo"),
			Verify
				.Diagnostic()
				.WithSpan(6, 34, 6, 37)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "bar"),
			Verify
				.Diagnostic()
				.WithSpan(6, 43, 6, 46)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "baz"),
		};

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsWarning_SomeUnreadParameters()
	{
		var source = @"
using System;
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(int foo, int bar, int baz) {
        Console.WriteLine(bar);
        baz = 3;
    }
}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(7, 25, 7, 28)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "foo"),
			Verify
				.Diagnostic()
				.WithSpan(7, 43, 7, 46)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "baz"),
		};

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsWarning_ExpressionBodiedMethod()
	{
		var source = @"
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(int unused) => Assert.Equal(5, 2 + 2);
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 25, 6, 31)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("TestMethod", "TestClass", "unused");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void DoesNotFindWarning_ParameterRead()
	{
		var source = @"
using System;
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(int used) {
        Console.WriteLine(used);
    }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void DoesNotFindWarning_ParameterCapturedAsOutParameterInMockSetup()
	{
		var source = @"
using System;
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(string used, int usedOut) {
        // mimicking mock setup use case
        // var mock = new Mock<IHaveOutParameter>();
        // mock.Setup(m => m.SomeMethod(out used));
        Action setup = () => int.TryParse(used, out usedOut);
    }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void DoesNotFindWarning_ExpressionBodiedMethod()
	{
		var source = @"
using Xunit;

class TestClass {
    [Theory]
    void TestMethod(int used) => Assert.Equal(used, 2 + 2);
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void DoesNotCrash_MethodWithoutBody()
	{
		var source = @"
using Xunit;

class TestClass {
    [Theory]
    extern void TestMethod(int foo);
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
