using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

public class AssertNullShouldNotBeCalledOnValueTypesFixerTests
{
	[Fact]
	public async void ForValueTypeNullAssert_RemovesAssertion()
	{
		const string before = @"
using Xunit;

public class Tests {
    [Fact]
    public void TestMethod() {
        int i = 1;

        [|Assert.NotNull(i)|];
    }
}";
		const string after = @"
using Xunit;

public class Tests {
    [Fact]
    public void TestMethod() {
        int i = 1;
    }
}";

		await Verify.VerifyCodeFix(before, after, AssertNullShouldNotBeCalledOnValueTypesFixer.Key_RemoveAssert);
	}

	[Fact]
	// https://github.com/xunit/xunit/issues/1753
	public async void ForAssertionWithTrivia_RemovesAssertionAndLeavesLeadingTriviaInPlace()
	{
		const string before = @"
using System;
using Xunit;

namespace XUnitTestProject1 {
    public class UnitTest1 {
        [Fact]
        public void Test1() {
            int i = 1;

            // I am a comment which gets deleted by the quick fix
            // Assert
            [|Assert.NotNull(i)|];
            Assert.Null(null);
        }
    }
}";
		const string after = @"
using System;
using Xunit;

namespace XUnitTestProject1 {
    public class UnitTest1 {
        [Fact]
        public void Test1() {
            int i = 1;

            // I am a comment which gets deleted by the quick fix
            // Assert
            Assert.Null(null);
        }
    }
}";

		await Verify.VerifyCodeFix(before, after, AssertNullShouldNotBeCalledOnValueTypesFixer.Key_RemoveAssert);
	}
}
