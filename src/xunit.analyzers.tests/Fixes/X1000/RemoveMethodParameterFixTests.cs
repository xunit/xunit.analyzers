using System;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify_X1022 = CSharpVerifier<RemoveMethodParameterFixTests.Analyzer_X1022>;
using Verify_X1026 = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldUseAllParameters>;

public class RemoveMethodParameterFixTests
{
	[Fact]
	public async void X1022_RemoveParamsArray()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1, 2, 3)]
    public void TestMethod([|params int[] values|]) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1, 2, 3)]
    public void TestMethod() { }
}";

		await Verify_X1022.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void X1026_RemovesUnusedParameter()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int [|arg|]) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod() { }
}";

		await Verify_X1026.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void X1026_DoesNotCrashWhenParameterDeclarationIsMissing()
	{
		var before = @"
using Xunit;

public class TestClass {
	[Theory]
	[InlineData(1, 1)]
	public void Test1(int x, )
	{
		var x1 = x;
	}
}";

		var after = @"
using Xunit;

public class TestClass {
	[Theory]
	[InlineData(1, 1)]
	public void Test1(int x, )
	{
		var x1 = x;
	}
}";

		var expected = new[]
		{
			Verify_X1026
				.Diagnostic()
				.WithSpan(7, 27, 7, 27)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Test1", "TestClass", ""),
			Verify_X1026
				.CompilerError("CS1001")
				.WithSpan(7, 27, 7, 28)
				.WithMessage("Identifier expected"),
			Verify_X1026
				.CompilerError("CS1031")
				.WithSpan(7, 27, 7, 28)
				.WithMessage("Type expected"),
		};

		await Verify_X1026.VerifyCodeFixAsyncV2(before, after, diagnostics: expected);
	}

	internal class Analyzer_X1022 : TheoryMethodCannotHaveParamsArray
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Core(compilation, new Version(2, 1, 999));
	}
}
