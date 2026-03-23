using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

public class X2015_UseGenericOverloadFixTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					Action func = () => { };

					[|Assert.Throws(typeof(DivideByZeroException), func)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					Action func = () => { };

					Assert.Throws<DivideByZeroException>(func);
				}
			}
			""";

		await Verify.VerifyCodeFix(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}
}
