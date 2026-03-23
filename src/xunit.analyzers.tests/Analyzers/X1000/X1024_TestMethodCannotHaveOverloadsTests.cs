using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodCannotHaveOverloads>;

public class X1024_TestMethodCannotHaveOverloadsTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source1 = /* lang=c#-test */ """
			using Xunit;

			public class ForInstanceMethodOverloads_InSameInstanceClass_Triggers {
				[Fact]
				public void {|#0:TestMethod|}() { }

				[Theory]
				public void {|#1:TestMethod|}(int a) { }
			}

			public static class ForStaticMethodOverloads_InSameStaticClass_Triggers {
				[Fact]
				public static void {|#10:TestMethod|}() { }

				[Theory]
				public static void {|#11:TestMethod|}(int a) { }
			}

			public class ForInstanceMethodOverload_InDerivedClass_Triggers : InstanceBaseClass {
				[Theory]
				public void {|#20:TestMethod|}(int a) { }
			}

			public class ForStaticAndInstanceMethodOverload_Triggers : StaticBaseClass {
				[Theory]
				public void {|#30:TestMethod|}(int a) { }
			}

			public class ForMethodOverrides_DoesNotTrigger : OverrideBaseClass {
				[Fact]
				public override void TestMethod() { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			using Xunit;

			public class InstanceBaseClass {
				[Fact]
				public void TestMethod() { }
			}

			public class StaticBaseClass {
				[Fact]
				public static void TestMethod() { }
			}

			public class OverrideBaseClass {
				[Fact]
				public virtual void TestMethod() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers"),
			Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers"),

			Verify.Diagnostic().WithLocation(10).WithArguments("TestMethod", "ForStaticMethodOverloads_InSameStaticClass_Triggers", "ForStaticMethodOverloads_InSameStaticClass_Triggers"),
			Verify.Diagnostic().WithLocation(11).WithArguments("TestMethod", "ForStaticMethodOverloads_InSameStaticClass_Triggers", "ForStaticMethodOverloads_InSameStaticClass_Triggers"),

			Verify.Diagnostic().WithLocation(20).WithArguments("TestMethod", "ForInstanceMethodOverload_InDerivedClass_Triggers", "InstanceBaseClass"),

			Verify.Diagnostic().WithLocation(30).WithArguments("TestMethod", "ForStaticAndInstanceMethodOverload_Triggers", "StaticBaseClass"),
		};

		await Verify.VerifyAnalyzer([source1, source2], expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source1 = /* lang=c#-test */ """
			using Xunit;

			public class ForInstanceMethodOverloads_InSameInstanceClass_Triggers {
				[CulturedFact(new[] { "en-US" })]
				public void {|#0:TestMethod|}() { }

				[CulturedTheory(new[] { "en-US" })]
				public void {|#1:TestMethod|}(int a) { }
			}

			public static class ForStaticMethodOverloads_InSameStaticClass_Triggers {
				[CulturedFact(new[] { "en-US" })]
				public static void {|#10:TestMethod|}() { }

				[CulturedTheory(new[] { "en-US" })]
				public static void {|#11:TestMethod|}(int a) { }
			}

			public class ForInstanceMethodOverload_InDerivedClass_Triggers : InstanceBaseClass {
				[CulturedTheory(new[] { "en-US" })]
				public void {|#20:TestMethod|}(int a) { }
			}

			public class ForStaticAndInstanceMethodOverload_Triggers : StaticBaseClass {
				[CulturedTheory(new[] { "en-US" })]
				public void {|#30:TestMethod|}(int a) { }
			}

			public class ForMethodOverrides_DoesNotTrigger : OverrideBaseClass {
				[CulturedFact(new[] { "en-US" })]
				public override void TestMethod() { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			using Xunit;

			public class InstanceBaseClass {
				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public class StaticBaseClass {
				[CulturedFact(new[] { "en-US" })]
				public static void TestMethod() { }
			}

			public class OverrideBaseClass {
				[CulturedFact(new[] { "en-US" })]
				public virtual void TestMethod() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers"),
			Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers", "ForInstanceMethodOverloads_InSameInstanceClass_Triggers"),

			Verify.Diagnostic().WithLocation(10).WithArguments("TestMethod", "ForStaticMethodOverloads_InSameStaticClass_Triggers", "ForStaticMethodOverloads_InSameStaticClass_Triggers"),
			Verify.Diagnostic().WithLocation(11).WithArguments("TestMethod", "ForStaticMethodOverloads_InSameStaticClass_Triggers", "ForStaticMethodOverloads_InSameStaticClass_Triggers"),

			Verify.Diagnostic().WithLocation(20).WithArguments("TestMethod", "ForInstanceMethodOverload_InDerivedClass_Triggers", "InstanceBaseClass"),

			Verify.Diagnostic().WithLocation(30).WithArguments("TestMethod", "ForStaticAndInstanceMethodOverload_Triggers", "StaticBaseClass"),
		};

		await Verify.VerifyAnalyzerV3([source1, source2], expected);
	}
}
