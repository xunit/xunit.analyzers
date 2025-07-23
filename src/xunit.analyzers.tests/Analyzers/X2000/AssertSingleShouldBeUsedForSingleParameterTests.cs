using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class AssertSingleShouldBeUsedForSingleParameterTests
{
	[Fact]
	public async ValueTask EnumerableAcceptanceTest()
	{
		var code = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task TestMethod() {
					{|#0:Assert.Collection(
						default(IEnumerable<object>),
						item => Assert.NotNull(item)
					)|};
					Assert.Collection(
						default(IEnumerable<object>),
						item => Assert.NotNull(item),
						item => Assert.NotNull(item)
					);

					await {|#1:Assert.CollectionAsync(
						default(IEnumerable<Task<int>>),
						async item => Assert.NotNull(item)
					)|};
					await Assert.CollectionAsync(
						default(IEnumerable<Task<int>>),
						async item => Assert.Equal(42, await item),
						async item => Assert.Equal(2112, await item)
					);
				}
			}
			""";
		var expected = new DiagnosticResult[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Collection"),
			Verify.Diagnostic().WithLocation(1).WithArguments("CollectionAsync"),
		};

		await Verify.VerifyAnalyzer(code, expected);
	}

#if NETCOREAPP3_0_OR_GREATER

	[Fact]
	public async ValueTask AsyncEnumerableAcceptanceTest()
	{
		var code = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task TestMethod() {
					{|#0:Assert.Collection(
						default(IAsyncEnumerable<object>),
						item => Assert.NotNull(item)
					)|};
					Assert.Collection(
						default(IAsyncEnumerable<object>),
						item => Assert.NotNull(item),
						item => Assert.NotNull(item)
					);

					await {|#1:Assert.CollectionAsync(
						default(IAsyncEnumerable<Task<int>>),
						async item => Assert.NotNull(item)
					)|};
					await Assert.CollectionAsync(
						default(IAsyncEnumerable<Task<int>>),
						async item => Assert.Equal(42, await item),
						async item => Assert.Equal(2112, await item)
					);
				}
			}
			""";
		var expected = new DiagnosticResult[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Collection"),
			Verify.Diagnostic().WithLocation(1).WithArguments("CollectionAsync"),
		};

		await Verify.VerifyAnalyzer(code, expected);
	}

#endif  // NETCOREAPP3_0_OR_GREATER
}
