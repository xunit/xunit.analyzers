using System;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertNullShouldNotBeCalledOnValueTypesFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertNullShouldNotBeCalledOnValueTypes();
        readonly CodeFixProvider fixer = new AssertNullShouldNotBeCalledOnValueTypesFixer();

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

        Assert.NotNull(i);
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
            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, original);

            Assert.Equal(expected, actual);
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
            Assert.NotNull(i);
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
            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, original);

            // Code fixer always inserts \r\n even on Linux, so fix up the actual result
            if (Environment.NewLine != "\r\n")
                actual = actual.Replace("\r\n", Environment.NewLine);

            Assert.Equal(expected, actual);
        }
    }
}
