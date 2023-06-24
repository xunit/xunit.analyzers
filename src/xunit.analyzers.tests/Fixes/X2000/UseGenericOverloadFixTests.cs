using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X2007 = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;
using Verify_X2015 = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

public class UseGenericOverloadFixTests
{
	[Fact]
	public async void X2007_SwitchesToGenericIsType()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var result = 123;

        [|Assert.IsType(typeof(int), result)|];
    }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var result = 123;

        Assert.IsType<int>(result);
    }
}";

		await Verify_X2007.VerifyCodeFixAsyncV2(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}

	[Fact]
	public async void X2015_SwitchesToGenericThrows()
	{
		var before = @"
using System;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Action func = () => { };

        [|Assert.Throws(typeof(DivideByZeroException), func)|];
    }
}";

		var after = @"
using System;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Action func = () => { };

        Assert.Throws<DivideByZeroException>(func);
    }
}";

		await Verify_X2015.VerifyCodeFixAsyncV2(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}
}
