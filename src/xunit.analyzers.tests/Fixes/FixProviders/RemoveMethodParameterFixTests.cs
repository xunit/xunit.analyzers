using Xunit;
using Xunit.Analyzers;
using Verify_X1022 = CSharpVerifier<RemoveMethodParameterFixTests.Analyzer_X1022>;
using Verify_X1023 = CSharpVerifier<RemoveMethodParameterFixTests.Analyzer_X1023>;
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

		await Verify_X1022.VerifyCodeFixAsync(before, after);
	}

	[Fact]
	public async void X1023_RemovesDefaultValue()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int arg [|= 0|]) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int arg) { }
}";

		await Verify_X1023.VerifyCodeFixAsync(before, after);
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

		await Verify_X1026.VerifyCodeFixAsync(before, after);
	}

	internal class Analyzer_X1022 : TheoryMethodCannotHaveParamsArray
	{
		public Analyzer_X1022()
			: base("2.1.99")
		{ }
	}

	internal class Analyzer_X1023 : TheoryMethodCannotHaveDefaultParameter
	{
		public Analyzer_X1023()
			: base("2.1.99")
		{ }
	}
}
