using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassCannotBeNestedInGenericClass>;

public class TestClassCannotBeNestedInGenericClassTests
{
	[Fact]
	public async Task ReportsDiagnostic_WhenTestClassIsNestedInOpenGenericType()
	{
		var source = @"
public abstract class OpenGenericType<T>
{
    public class NestedTestClass
    {
        [Xunit.Fact]
        public void TestMethod() { }
    }
}";

		var expected = new DiagnosticResult(Descriptors.X1032_TestClassCannotBeNestedInGenericClass)
			.WithLocation(4, 18);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ReportsDiagnostic_WhenDerivedTestClassIsNestedInOpenGenericType()
	{
		var source = @"
public abstract class BaseTestClass
{
    [Xunit.Fact]
    public void TestMethod() { }
}

public abstract class OpenGenericType<T>
{
    public class NestedTestClass : BaseTestClass
    {
    }
}";

		var expected = new DiagnosticResult(Descriptors.X1032_TestClassCannotBeNestedInGenericClass)
			.WithLocation(10, 18);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task DoesNotReportDiagnostic_WhenTestClassIsNestedInClosedGenericType()
	{
		var source = @"
public abstract class OpenGenericType<T>
{
}

public abstract class ClosedGenericType : OpenGenericType<int>
{
    public class NestedTestClass
    {
        [Xunit.Fact]
        public void TestMethod() { }
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task DoesNotReportDiagnostic_WhenNestedClassIsNotTestClass()
	{
		var source = @"
public abstract class OpenGenericType<T>
{
    public class NestedClass
    {
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task DoesNotReportDiagnostic_WhenTestClassIsNotNestedInOpenGenericType()
	{
		var source = @"
public abstract class NonGenericType
{
    public class NestedTestClass
    {
        [Xunit.Fact]
        public void TestMethod() { }
    }
}";

		await Verify.VerifyAnalyzer(source);
	}
}
