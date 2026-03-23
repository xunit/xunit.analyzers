using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassCannotBeNestedInGenericClass>;

public class X1032_TestClassCannotBeNestedInGenericClassFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public abstract class OpenGenericType<T>
			{
				public class [|NestedTestClass|]
				{
					[Fact]
					public void TestMethod() { }
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public abstract class OpenGenericType<T>
			{
			}

			public class NestedTestClass
			{
				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, TestClassCannotBeNestedInGenericClassFixer.Key_ExtractTestClass);
	}
}
