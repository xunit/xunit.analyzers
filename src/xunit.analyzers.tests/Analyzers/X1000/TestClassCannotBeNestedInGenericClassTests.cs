using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassCannotBeNestedInGenericClass>;

public class TestClassCannotBeNestedInGenericClassTests
{
	[Fact]
	public async Task WhenTestClassIsNestedInOpenGenericType_Triggers()
	{
		var source = /* lang=c#-test */ """
			public abstract class OpenGenericType<T> {
			    public class [|NestedTestClass|] {
			        [Xunit.Fact]
			        public void TestMethod() { }
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task WhenDerivedTestClassIsNestedInOpenGenericType_Triggers()
	{
		var source = /* lang=c#-test */ """
			public abstract class BaseTestClass {
			    [Xunit.Fact]
			    public void TestMethod() { }
			}

			public abstract class OpenGenericType<T> {
			    public class [|NestedTestClass|] : BaseTestClass { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task WhenTestClassIsNestedInClosedGenericType_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public abstract class OpenGenericType<T> { }

			public abstract class ClosedGenericType : OpenGenericType<int> {
			    public class NestedTestClass {
			        [Xunit.Fact]
			        public void TestMethod() { }
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task WhenNestedClassIsNotTestClass_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public abstract class OpenGenericType<T> {
			    public class NestedClass { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task WhenTestClassIsNotNestedInOpenGenericType_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public abstract class NonGenericType {
			    public class NestedTestClass {
			        [Xunit.Fact]
			        public void TestMethod() { }
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
