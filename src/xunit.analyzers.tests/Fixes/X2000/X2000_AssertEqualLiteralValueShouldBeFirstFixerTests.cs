using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

public class X2000_AssertEqualLiteralValueShouldBeFirstFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var i = 0;

					[|Assert.Equal(i, 0)|];
					[|Assert.Equal(actual: 0, expected: i)|];
					[|Assert.Equal(expected: i, actual: 0)|];
					[|Assert.Equal(comparer: default(IEqualityComparer<int>), actual: 0, expected: i)|];
					[|Assert.Equal(comparer: (x, y) => true, actual: 0, expected: i)|];
					[|Assert.Equal(expected: i, 0)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var i = 0;

					Assert.Equal(0, i);
					Assert.Equal(actual: i, expected: 0);
					Assert.Equal(expected: 0, actual: i);
					Assert.Equal(comparer: default(IEqualityComparer<int>), actual: i, expected: 0);
					Assert.Equal(comparer: (x, y) => true, actual: i, expected: 0);
					Assert.Equal(expected: 0, i);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(LanguageVersion.CSharp7_2, before, after, AssertEqualLiteralValueShouldBeFirstFixer.Key_SwapArguments);
	}
}
