using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodCannotHaveOverloads>;

public class TestMethodCannotHaveOverloadsTests
{
	[Fact]
	public async Task ForInstanceMethodOverloads_InSameInstanceClass_Triggers()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
			    [Xunit.Fact]
			    public void {|#0:TestMethod|}() { }

			    [Xunit.Theory]
			    public void {|#1:TestMethod|}(int a) { }
			}
			""";
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "TestClass"),
			Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "TestClass", "TestClass"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ForStaticMethodOverloads_InSameStaticClass_Triggers()
	{
		var source = /* lang=c#-test */ """
			public static class TestClass {
			    [Xunit.Fact]
			    public static void {|#0:TestMethod|}() { }

			    [Xunit.Theory]
			    public static void {|#1:TestMethod|}(int a) { }
			}
			""";
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "TestClass"),
			Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "TestClass", "TestClass"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ForInstanceMethodOverload_InDerivedClass_Triggers()
	{
		var source1 = /* lang=c#-test */ """
			public class TestClass : BaseClass {
			    [Xunit.Theory]
			    public void {|#0:TestMethod|}(int a) { }

			    private void {|#1:TestMethod|}(int a, byte c) { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			public class BaseClass {
			    [Xunit.Fact]
			    public void TestMethod() { }
			}
			""";
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "BaseClass"),
			Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "TestClass", "BaseClass"),
		};

		await Verify.VerifyAnalyzer([source1, source2], expected);
	}

	[Fact]
	public async Task ForStaticAndInstanceMethodOverload_Triggers()
	{
		var source1 = /* lang=c#-test */ """
			public class TestClass : BaseClass {
			    [Xunit.Theory]
			    public void {|#0:TestMethod|}(int a) { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			public class BaseClass {
			    [Xunit.Fact]
			    public static void TestMethod() { }
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "BaseClass");

		await Verify.VerifyAnalyzer([source1, source2], expected);
	}

	[Fact]
	public async Task ForMethodOverrides_DoesNotTrigger()
	{
		var source1 = /* lang=c#-test */ """
			public class BaseClass {
			    [Xunit.Fact]
			    public virtual void TestMethod() { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			public class TestClass : BaseClass {
			    [Xunit.Fact]
			    public override void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer([source1, source2]);
	}
}
