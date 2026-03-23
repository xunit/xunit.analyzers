using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ConstructorsOnFactAttributeSubclassShouldBePublic>;

public class X1043_ConstructorsOnFactAttributeSubclassShouldBePublicTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
			internal sealed class DefaultConstructor_DoesNotTrigger : FactAttribute { }

			[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
			internal sealed class ParameterlessPublicConstructor_DoesNotTrigger : FactAttribute {
				public ParameterlessPublicConstructor_DoesNotTrigger() {
					this.Skip = "xxx";
				}
			}

			[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
			internal sealed class ParameteredPublicConstructor_DoesNotTrigger : FactAttribute {
				public ParameteredPublicConstructor_DoesNotTrigger(string skip) {
					this.Skip = skip;
				}
			}

			[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
			internal sealed class PublicConstructorWithOtherConstructors_DoesNotTrigger : FactAttribute {
				public PublicConstructorWithOtherConstructors_DoesNotTrigger() {
					this.Skip = "xxx";
				}

				internal PublicConstructorWithOtherConstructors_DoesNotTrigger(string skip) {
					this.Skip = skip;
				}
			}

			[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
			internal sealed class InternalConstructor_Triggers : FactAttribute {
				internal InternalConstructor_Triggers(string skip, params int[] values) { }
			}

			[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
			internal sealed class ProtectedInternalConstructor_Triggers : FactAttribute {
				protected internal ProtectedInternalConstructor_Triggers() {
					this.Skip = "xxx";
				}
			}

			public class TestClass {
				[DefaultConstructor_DoesNotTrigger]
				public void TestDefaultConstructor() { }

				[ParameterlessPublicConstructor_DoesNotTrigger]
				public void TestParameterlessPublicConstructor() { }

				[ParameteredPublicConstructor_DoesNotTrigger("skip message")]
				public void TestParameteredPublicConstructor() { }

				[PublicConstructorWithOtherConstructors_DoesNotTrigger]
				public void TestPublicConstructorWithOtherConstructors() { }

				[{|#0:InternalConstructor_Triggers("skip message", 42)|}]
				public void TestInternalConstructor() { }

				[{|#1:ProtectedInternalConstructor_Triggers|}]
				public void TestProtectedInternalConstructor() { }

				[Fact]
				public void TestFact() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("InternalConstructor_Triggers.InternalConstructor_Triggers(string, params int[])"),
			Verify.Diagnostic().WithLocation(1).WithArguments("ProtectedInternalConstructor_Triggers.ProtectedInternalConstructor_Triggers()"),
		};

		await Verify.VerifyAnalyzerNonAot(source, expected);
	}
}
