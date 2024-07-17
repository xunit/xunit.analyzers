using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

public class PublicMethodShouldBeMarkedAsTestTests
{
	[Fact]
	public async Task PublicMethodInNonTestClass_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
			    public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData(/* lang=c#-test */ "Xunit.Fact")]
	[InlineData(/* lang=c#-test */ "Xunit.Theory")]
	public async Task TestMethods_DoesNotTrigger(string attribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
			    [{0}]
			    public void TestMethod() {{ }}
			}}
			""", attribute);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task IDisposableDisposeMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass: System.IDisposable {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    public void Dispose() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task PublicAbstractMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public abstract class TestClass {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    public abstract void AbstractMethod();
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task DerivedMethodWithFactOnBaseAbstractMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public abstract class BaseClass {
			    [Xunit.Fact]
			    public abstract void TestMethod();
			}

			public class TestClass : BaseClass {
			    public override void TestMethod() { }

			    [Xunit.Fact]
			    public void TestMethod2() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task PublicAbstractMethodMarkedWithFact_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public abstract class TestClass {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    [Xunit.Fact]
			    public abstract void AbstractMethod();
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task IDisposableDisposeMethodOverrideFromParentClass_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class BaseClass: System.IDisposable {
			    public virtual void Dispose() { }
			}

			public class TestClass: BaseClass {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    public override void Dispose() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task IDisposableDisposeMethodOverrideFromParentClassWithRepeatedInterfaceDeclaration_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class BaseClass: System.IDisposable {
			    public virtual void Dispose() { }
			}

			public class TestClass: BaseClass, System.IDisposable {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    public override void Dispose() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task IDisposableDisposeMethodOverrideFromGrandParentClass_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public abstract class BaseClass: System.IDisposable {
			    public abstract void Dispose();
			}

			public abstract class IntermediateClass: BaseClass { }

			public class TestClass: IntermediateClass {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    public override void Dispose() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task IAsyncLifetimeMethods_V2_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass: Xunit.IAsyncLifetime {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    public System.Threading.Tasks.Task DisposeAsync()
			    {
			        throw new System.NotImplementedException();
			    }

			    public System.Threading.Tasks.Task InitializeAsync()
			    {
			        throw new System.NotImplementedException();
			    }
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async Task IAsyncLifetimeMethods_V3_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass: Xunit.IAsyncLifetime {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    public System.Threading.Tasks.ValueTask DisposeAsync()
			    {
			        throw new System.NotImplementedException();
			    }

			    public System.Threading.Tasks.ValueTask InitializeAsync()
			    {
			        throw new System.NotImplementedException();
			    }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task PublicMethodMarkedWithAttributeWhichIsMarkedWithIgnoreXunitAnalyzersRule1013_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class IgnoreXunitAnalyzersRule1013Attribute: System.Attribute { }

			[IgnoreXunitAnalyzersRule1013]
			public class CustomTestTypeAttribute: System.Attribute { }

			public class TestClass {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    [CustomTestType]
			    public void CustomTestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task PublicMethodMarkedWithAttributeWhichInheritsFromAttributeMarkedWithIgnoreXunitAnalyzersRule1013_Triggers()
	{
		var source = /* lang=c#-test */ """
			public class IgnoreXunitAnalyzersRule1013Attribute: System.Attribute { }

			[IgnoreXunitAnalyzersRule1013]
			public class BaseCustomTestTypeAttribute: System.Attribute { }

			public class DerivedCustomTestTypeAttribute: BaseCustomTestTypeAttribute { }

			public class TestClass {
			    [Xunit.Fact]
			    public void TestMethod() { }

			    [DerivedCustomTestType]
			    public void {|#0:CustomTestMethod|}() { }
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("CustomTestMethod", "TestClass", "Fact");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(/* lang=c#-test */ "Xunit.Fact")]
	[InlineData(/* lang=c#-test */ "Xunit.Theory")]
	public async Task PublicMethodWithoutParametersInTestClass_Triggers(string attribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
			    [{0}]
			    public void TestMethod() {{ }}

			    public void {{|#0:Method|}}() {{ }}
			}}
			""", attribute);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Method", "TestClass", "Fact");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(/* lang=c#-test */ "Xunit.Fact")]
	[InlineData(/* lang=c#-test */ "Xunit.Theory")]
	public async Task PublicMethodWithParametersInTestClass_Triggers(string attribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
			    [{0}]
			    public void TestMethod() {{ }}

			    public void {{|#0:Method|}}(int a) {{ }}
			}}
			""", attribute);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Method", "TestClass", "Theory");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(/* lang=c#-test */ "Fact")]
	[InlineData(/* lang=c#-test */ "Theory")]
	public async Task OverridenMethod_FromParentNonTestClass_DoesNotTrigger(string attribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			using Xunit;

			public abstract class ParentClass {{
			    public abstract void ParentMethod();
			}}

			public class TestClass : ParentClass {{
			    [{0}]
			    public void TestMethod() {{ }}

			    public override void ParentMethod() {{ }}
			
				public override void {{|CS0115:MissingMethod|}}() {{ }}
			}}
			""", attribute);

		await Verify.VerifyAnalyzer(source);
	}
}
