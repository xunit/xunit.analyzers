using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassCannotBeNestedInGenericClass>;

public class X1032_TestClassCannotBeNestedInGenericClassTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public abstract class NonTestClass_NestedInOpenGeneric_DoesNotTrigger<T> {
				public class NestedClass { }
			}

			public class TestClass_NestedInNonGeneric_DoesNotTrigger {
				public class NestedClass {
					[Fact]
					public void TestMethod() { }
				}
			}

			public abstract class Fact_NestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] {
					[Fact]
					public void TestMethod() { }
				}
			}

			public abstract class Theory_NestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] {
					[Theory]
					public void TestMethod() { }
				}
			}

			public abstract class Fact_BaseTestClass {
				[Fact]
				public void TestMethod() { }
			}

			public abstract class Fact_DerivedTestClassNestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] : Fact_BaseTestClass { }
			}

			public abstract class Theory_BaseTestClass {
				[Theory]
				public void TestMethod() { }
			}

			public abstract class Theory_DerivedTestClassNestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] : Theory_BaseTestClass { }
			}

			public abstract class OpenGenericType<T> { }

			public abstract class Fact_TestClassNestedInClosedGenericType_DoesNotTrigger : OpenGenericType<int> {
				public class NestedTestClass {
					[Fact]
					public void TestMethod() { }
				}
			}

			public abstract class Theory_TestClassNestedInClosedGenericType_DoesNotTrigger : OpenGenericType<int> {
				public class NestedTestClass {
					[Theory]
					public void TestMethod() { }
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass_NestedInNonGeneric_DoesNotTrigger {
				public class NestedClass {
					[CulturedFact(new[] { "en-US" })]
					public void TestMethod() { }
				}
			}

			public abstract class Fact_NestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] {
					[CulturedFact(new[] { "en-US" })]
					public void TestMethod() { }
				}
			}

			public abstract class Theory_NestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] {
					[CulturedTheory(new[] { "en-US" })]
					public void TestMethod() { }
				}
			}

			public abstract class Fact_BaseTestClass {
				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public abstract class Fact_DerivedTestClassNestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] : Fact_BaseTestClass { }
			}

			public abstract class Theory_BaseTestClass {
				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public abstract class Theory_DerivedTestClassNestedInOpenGenericType_Triggers<T> {
				public class [|TestClass|] : Theory_BaseTestClass { }
			}

			public abstract class OpenGenericType<T> { }

			public abstract class Fact_TestClassNestedInClosedGenericType_DoesNotTrigger : OpenGenericType<int> {
				public class NestedTestClass {
					[CulturedFact(new[] { "en-US" })]
					public void TestMethod() { }
				}
			}

			public abstract class Theory_TestClassNestedInClosedGenericType_DoesNotTrigger : OpenGenericType<int> {
				public class NestedTestClass {
					[CulturedTheory(new[] { "en-US" })]
					public void TestMethod() { }
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
