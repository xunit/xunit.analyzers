using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X2007 = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;
using Verify_X2015 = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

public class UseGenericOverloadFixTests
{
	[Fact]
	public async Task X2007_SwitchesToGenericIsType()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void TestMethod() {
			        var result = 123;

			        [|Assert.IsType(typeof(int), result)|];
			    }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void TestMethod() {
			        var result = 123;

			        Assert.IsType<int>(result);
			    }
			}
			""";

		await Verify_X2007.VerifyCodeFix(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}

	[Fact]
	public async Task X2015_SwitchesToGenericThrows()
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

		await Verify_X2015.VerifyCodeFix(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}
}
