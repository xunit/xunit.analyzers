using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassCannotBeNestedInGenericClass>;

public class TestClassCannotBeNestedInGenericClassFixerTests
{
	[Fact]
	public async void MovesTestClassOutOfGenericParent()
	{
		const string before = @"
public abstract class OpenGenericType<T>
{
    public class NestedTestClass
    {
        [Xunit.Fact]
        public void TestMethod() { }
    }
}";
		const string after = @"
public abstract class OpenGenericType<T>
{
}

public class NestedTestClass
{
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFix(
			before,
			after,
			TestClassCannotBeNestedInGenericClassFixer.Key_ExtractTestClass,
			new DiagnosticResult(Descriptors.X1032_TestClassCannotBeNestedInGenericClass).WithLocation(4, 18));
	}
}
