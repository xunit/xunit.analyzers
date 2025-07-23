using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class AssertSingleShouldBeUsedForSingleParameterFixerTests
{
	[Fact]
	public async ValueTask CannotFixInsideLambda()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;
			
			public class TestClass {
				[Fact]
				public static void TestMethod()
				{
					Record.Exception(
						() => [|Assert.Collection(default(IEnumerable<object>), item => Assert.True(false))|]
					);
				}
			}
			""";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, source, source, AssertSingleShouldBeUsedForSingleParameterFixer.Key_UseSingleMethod);
	}

	[Fact]
	public async ValueTask EnumerableAcceptanceTest()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task WithInlineLambda() {
					[|Assert.Collection(default(IEnumerable<object>), item => Assert.NotNull(item))|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), async item => Assert.Equal(42, await item))|];
				}

				[Fact]
				public async Task WithOneStatementLambda() {
					[|Assert.Collection(default(IEnumerable<object>), item => { Assert.NotNull(item); })|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), async item => { Assert.Equal(42, await item); })|];
				}

				[Fact]
				public async Task WithTwoStatementLambda() {
					[|Assert.Collection(default(IEnumerable<object>), item => { Assert.NotNull(item); Assert.NotNull(item); })|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), async item => { Assert.Equal(42, await item); Assert.Equal(42, await item); })|];
				}

				[Fact]
				public async Task WithMultiLineLambda() {
					[|Assert.Collection(default(IEnumerable<object>), item => {
						if (item != null)
							Assert.NotNull(item);
					})|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), async item => {
						if (item != null)
							Assert.Equal(42, await item);
					})|];
				}

				[Fact]
				public async Task WithNameCollision() {
					var item = 42;
					[|Assert.Collection(default(IEnumerable<object>), item => Assert.NotNull(item))|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), async item => Assert.Equal(2112, await item))|];
				}

				[Fact]
				public async Task WithInspector() {
					[|Assert.Collection(default(IEnumerable<object>), ElementInspector)|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), AsyncElementInspector)|];
				}

				void ElementInspector(object obj)
				{ }

				Task AsyncElementInspector(Task<int> obj) =>
					Task.CompletedTask;
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task WithInlineLambda() {
					var item = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithOneStatementLambda() {
					var item = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithTwoStatementLambda() {
					var item = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item); Assert.NotNull(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					Assert.Equal(42, await item_2); Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithMultiLineLambda() {
					var item = Assert.Single(default(IEnumerable<object>));
					if (item != null)
						Assert.NotNull(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					if (item_2 != null)
						Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithNameCollision() {
					var item = 42;
					var item_2 = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item_2);
					var item_3 = Assert.Single(default(IEnumerable<Task<int>>));
					Assert.Equal(2112, await item_3);
				}

				[Fact]
				public async Task WithInspector() {
					var item = Assert.Single(default(IEnumerable<object>));
					ElementInspector(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					await AsyncElementInspector(item_2);
				}

				void ElementInspector(object obj)
				{ }

				Task AsyncElementInspector(Task<int> obj) =>
					Task.CompletedTask;
			}
			""";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, AssertSingleShouldBeUsedForSingleParameterFixer.Key_UseSingleMethod);
	}

#if NETCOREAPP3_0_OR_GREATER

	[Fact]
	public async ValueTask AsyncEnumerableAcceptanceTest()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task WithInlineLambda() {
					[|Assert.Collection(default(IAsyncEnumerable<object>), item => Assert.NotNull(item))|];
					await [|Assert.CollectionAsync(default(IAsyncEnumerable<Task<int>>), async item => Assert.Equal(42, await item))|];
				}

				[Fact]
				public async Task WithOneStatementLambda() {
					[|Assert.Collection(default(IAsyncEnumerable<object>), item => { Assert.NotNull(item); })|];
					await [|Assert.CollectionAsync(default(IAsyncEnumerable<Task<int>>), async item => { Assert.Equal(42, await item); })|];
				}

				[Fact]
				public async Task WithTwoStatementLambda() {
					[|Assert.Collection(default(IAsyncEnumerable<object>), item => { Assert.NotNull(item); Assert.NotNull(item); })|];
					await [|Assert.CollectionAsync(default(IAsyncEnumerable<Task<int>>), async item => { Assert.Equal(42, await item); Assert.Equal(42, await item); })|];
				}

				[Fact]
				public async Task WithMultiLineLambda() {
					[|Assert.Collection(default(IAsyncEnumerable<object>), item => {
						if (item != null)
							Assert.NotNull(item);
					})|];
					await [|Assert.CollectionAsync(default(IAsyncEnumerable<Task<int>>), async item => {
						if (item != null)
							Assert.Equal(42, await item);
					})|];
				}

				[Fact]
				public async Task WithNameCollision() {
					var item = 42;
					[|Assert.Collection(default(IAsyncEnumerable<object>), item => Assert.NotNull(item))|];
					await [|Assert.CollectionAsync(default(IAsyncEnumerable<Task<int>>), async item => Assert.Equal(42, await item))|];
				}

				[Fact]
				public async Task WithInspector() {
					[|Assert.Collection(default(IAsyncEnumerable<object>), ElementInspector)|];
					await [|Assert.CollectionAsync(default(IAsyncEnumerable<Task<int>>), AsyncElementInspector)|];
				}

				void ElementInspector(object obj)
				{ }

				Task AsyncElementInspector(Task<int> obj) =>
					Task.CompletedTask;
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task WithInlineLambda() {
					var item = Assert.Single(default(IAsyncEnumerable<object>));
					Assert.NotNull(item);
					var item_2 = Assert.Single(default(IAsyncEnumerable<Task<int>>));
					Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithOneStatementLambda() {
					var item = Assert.Single(default(IAsyncEnumerable<object>));
					Assert.NotNull(item);
					var item_2 = Assert.Single(default(IAsyncEnumerable<Task<int>>));
					Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithTwoStatementLambda() {
					var item = Assert.Single(default(IAsyncEnumerable<object>));
					Assert.NotNull(item); Assert.NotNull(item);
					var item_2 = Assert.Single(default(IAsyncEnumerable<Task<int>>));
					Assert.Equal(42, await item_2); Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithMultiLineLambda() {
					var item = Assert.Single(default(IAsyncEnumerable<object>));
					if (item != null)
						Assert.NotNull(item);
					var item_2 = Assert.Single(default(IAsyncEnumerable<Task<int>>));
					if (item_2 != null)
						Assert.Equal(42, await item_2);
				}

				[Fact]
				public async Task WithNameCollision() {
					var item = 42;
					var item_2 = Assert.Single(default(IAsyncEnumerable<object>));
					Assert.NotNull(item_2);
					var item_3 = Assert.Single(default(IAsyncEnumerable<Task<int>>));
					Assert.Equal(42, await item_3);
				}

				[Fact]
				public async Task WithInspector() {
					var item = Assert.Single(default(IAsyncEnumerable<object>));
					ElementInspector(item);
					var item_2 = Assert.Single(default(IAsyncEnumerable<Task<int>>));
					await AsyncElementInspector(item_2);
				}

				void ElementInspector(object obj)
				{ }

				Task AsyncElementInspector(Task<int> obj) =>
					Task.CompletedTask;
			}
			""";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, AssertSingleShouldBeUsedForSingleParameterFixer.Key_UseSingleMethod);
	}

#endif  // NETCOREAPP3_0_OR_GREATER
}
