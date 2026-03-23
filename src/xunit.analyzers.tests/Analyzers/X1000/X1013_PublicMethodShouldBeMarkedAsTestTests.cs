using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

public class X1013_PublicMethodShouldBeMarkedAsTestTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class NonTestClass {
				public void PublicMethodInNonTestClass_DoesNotTrigger() { }
			}

			public class FactTestClass {
				[Fact]
				public void FactMethod_DoesNotTrigger() { }

				public void {|#0:NonTestMethod|}() { }

				public void {|#1:NonTestMethodWithParameters|}(int n) { }
			}

			public class TheoryTestClass {
				[Theory]
				public void TheoryMethod_DoesNotTrigger() { }

				public void {|#10:NonTestMethod|}() { }

				public void {|#11:NonTestMethodWithParameters|}(int n) { }
			}

			// Implementation of IDisposable does not trigger

			public class DisposableTestClass : IDisposable {
				[Fact]
				public void TestMethod() { }

				public void Dispose() { }
			}

			// Abstract methods in an abstract class don't trigger

			public abstract class AbstractTestClass {
				[Fact]
				public void TestMethod() { }

				public abstract void AbstractMethod_DoesNotTrigger();

				[Fact]
				public abstract void AbstractTestMethod_DoesNotTrigger();
			}

			// Override of test method from base class should not trigger

			public abstract class BaseClassWithAbstractTest {
				[Fact]
				public abstract void TestMethod();
			}

			public class DerivedMethodWithFactOnBaseAbstractMethod_DoesNotTrigger : BaseClassWithAbstractTest {
				public override void TestMethod() { }

				[Fact]
				public void TestMethod2() { }

				public override void {|CS0115:OverridingMissingMethodFromBase_DoesNotTrigger|}() { }
			}

			// Override of non-test method from base class should not trigger

			public class BaseDisposableClass : IDisposable {
				public virtual void Dispose() { }
			}

			public class DerivedDisposableClass : BaseDisposableClass {
				[Fact]
				public void TestMethod() { }

				public override void Dispose() { }
			}

			public abstract class IntermediateDisposableClass: BaseDisposableClass { }

			public class DerivedFromIntermediateDisposableClass: IntermediateDisposableClass {
				[Fact]
				public void TestMethod() { }

				public override void Dispose() { }
			}

			// Allow users to opt out of the analyzer

			public class IgnoreXunitAnalyzersRule1013Attribute: Attribute { }

			[IgnoreXunitAnalyzersRule1013]
			public class CustomTestTypeAttribute: Attribute { }

			public class DerivedCustomTestTypeAttribute: CustomTestTypeAttribute { }

			public class TestClassWithCustomTestType {
				[Fact]
				public void TestMethod() { }

				[CustomTestType]
				public void DirectIgnoreXunitAnalyzersRule1013Attribute_DoesNotTrigger() { }

				[DerivedCustomTestType]
				public void {|#20:IndirectIgnoreXunitAnalyzersRule1013Attribute_Triggers|}() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("NonTestMethod", "FactTestClass", "Fact"),
			Verify.Diagnostic().WithLocation(1).WithArguments("NonTestMethodWithParameters", "FactTestClass", "Theory"),

			Verify.Diagnostic().WithLocation(10).WithArguments("NonTestMethod", "TheoryTestClass", "Fact"),
			Verify.Diagnostic().WithLocation(11).WithArguments("NonTestMethodWithParameters", "TheoryTestClass", "Theory"),

			Verify.Diagnostic().WithLocation(20).WithArguments("IndirectIgnoreXunitAnalyzersRule1013Attribute_Triggers", "TestClassWithCustomTestType", "Fact"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V2_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			public class IAsyncLifetimeMethods_DoNotTrigger : IAsyncLifetime {
				[Fact]
				public void TestMethod() { }

				public Task DisposeAsync() => Task.FromResult(0);

				public Task InitializeAsync() => Task.FromResult(0);
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			public class FactTestClass {
				[CulturedFact(new[] { "en-US" })]
				public void CulturedFactMethod_DoesNotTrigger() { }

				public void {|#0:NonTestMethod|}() { }

				public void {|#1:NonTestMethodWithParameters|}(int n) { }
			}

			public class TheoryTestClass {
				[CulturedTheory(new[] { "en-US" })]
				public void CulturedTheoryMethod_DoesNotTrigger() { }

				public void {|#10:NonTestMethod|}() { }

				public void {|#11:NonTestMethodWithParameters|}(int n) { }
			}

			public class IAsyncLifetimeMethods_DoNotTrigger : IAsyncLifetime {
				[Fact]
				public void TestMethod() { }

				public ValueTask DisposeAsync() => default(ValueTask);

				public ValueTask InitializeAsync() => default(ValueTask);
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("NonTestMethod", "FactTestClass", "Fact"),
			Verify.Diagnostic().WithLocation(1).WithArguments("NonTestMethodWithParameters", "FactTestClass", "Theory"),

			Verify.Diagnostic().WithLocation(10).WithArguments("NonTestMethod", "TheoryTestClass", "Fact"),
			Verify.Diagnostic().WithLocation(11).WithArguments("NonTestMethodWithParameters", "TheoryTestClass", "Theory"),
		};

		await Verify.VerifyAnalyzerV3(source, expected);
	}
}
