using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

public class X2002_AssertNullShouldNotBeCalledOnValueTypesFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		const string before = /* lang=c#-test */ """
			using Xunit;

			public class Tests {
				[Fact]
				public void RemovesAssertion() {
					int i = 1;

					[|Assert.NotNull(i)|];
				}

				// https://github.com/xunit/xunit/issues/1753
				[Fact]
				public void ForAssertionWithTrivia_RemovesAssertionAndLeavesLeadingTriviaInPlace() {
					int i = 1;

					// I am a comment which gets deleted by the quick fix
					// Assert
					[|Assert.NotNull(i)|];
					Assert.Null(null);
				}
			}
			""";
		const string after = /* lang=c#-test */ """
			using Xunit;

			public class Tests {
				[Fact]
				public void RemovesAssertion() {
					int i = 1;
				}

				// https://github.com/xunit/xunit/issues/1753
				[Fact]
				public void ForAssertionWithTrivia_RemovesAssertionAndLeavesLeadingTriviaInPlace() {
					int i = 1;

					// I am a comment which gets deleted by the quick fix
					// Assert
					Assert.Null(null);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertNullShouldNotBeCalledOnValueTypesFixer.Key_RemoveAssert);
	}
}
