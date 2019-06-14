namespace Xunit.Analyzers
{
    using Verify = CSharpVerifier<AssertNullShouldNotBeCalledOnValueTypes>;

    public class AssertNullShouldNotBeCalledOnValueTypesFixerTests
    {
        [Fact]
        public async void ForValueTypeNullAssert_RemovesAssertion()
        {
            const string original = @"
using Xunit;

public class Tests
{
    [Fact]
    public void TestMethod()
    {
        int i = 1;

        [|Assert.NotNull(i)|];
    }
}";

            const string expected = @"
using Xunit;

public class Tests
{
    [Fact]
    public void TestMethod()
    {
        int i = 1;
    }
}";

            await Verify.VerifyCodeFixAsync(original, expected);
        }

        [Fact]
        // https://github.com/xunit/xunit/issues/1753
        public async void ForAssertionWithTrivia_RemovesAssertionAndLeavesLeadingTriviaInPlace()
        {
            const string original = @"
using System;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            int i = 1;

            // I am a comment which gets deleted by the quick fix
            // Assert
            [|Assert.NotNull(i)|];
            Assert.Null(null);
        }
    }
}";
            const string expected = @"
using System;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            int i = 1;

            // I am a comment which gets deleted by the quick fix
            // Assert
            Assert.Null(null);
        }
    }
}";

            await Verify.VerifyCodeFixAsync(original, expected);
        }
    }
}
