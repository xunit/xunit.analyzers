using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class X2023_AssertSingleShouldBeUsedForSingleParameterFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public static void CannotFixInsideLambda()
				{
					Record.Exception(
						() => [|Assert.Collection(default(IEnumerable<object>), item => Assert.True(false))|]
					);
				}

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

				// https://github.com/xunit/xunit/issues/3336
				[Fact]
				public void WithLeadingComment() {
					// Assert
					[|Assert.Collection(default(IEnumerable<object>), item => Assert.NotNull(item))|];
				}

				[Fact]
				public void WithMultiLineCollection() {
					[|Assert.Collection(
						default(IEnumerable<object>),
						item => Assert.NotNull(item))|];
				}

				[Fact]
				public void WithMultiLineInspector() {
					[|Assert.Collection(default(IEnumerable<string>),
						item => Assert.Equal(
							"abc",
							item))|];
				}

				// https://github.com/xunit/xunit/issues/3659
				[Fact]
				public async Task WithParenthesizedLambda() {
					[|Assert.Collection(default(IEnumerable<object>), (item) => Assert.NotNull(item))|];
					[|Assert.Collection(default(IEnumerable<string>), (object item) => Assert.NotNull(item))|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), async (item) => Assert.Equal(42, await item))|];
				}

				[Fact]
				public async Task WithAwaitedLambda() {
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), async item => await AsyncElementInspector(item))|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), item => AsyncElementInspector(item))|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), item => item ?? Task.CompletedTask)|];
				}

				[Fact]
				public async Task WithMemberAccessInspector() {
					[|Assert.Collection(default(IEnumerable<object>), this.ElementInspector)|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), this.AsyncElementInspector)|];
				}

				[Fact]
				public async Task WithDelegateInspector() {
					Action<object> inspector = ElementInspector;
					Func<Task<int>, Task> asyncInspector = AsyncElementInspector;
					Action<object>[] inspectors = new Action<object>[] { ElementInspector };
					[|Assert.Collection(default(IEnumerable<object>), inspector)|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), asyncInspector)|];
					[|Assert.Collection(default(IEnumerable<object>), inspectors[0])|];
					[|Assert.Collection(default(IEnumerable<object>), GetElementInspector())|];
				}

				Action<object> GetElementInspector() => ElementInspector;

				[Fact]
				public void WithThrowingLambda() {
					[|Assert.Collection(default(IEnumerable<object>), item => throw new InvalidOperationException())|];
				}

				[Fact]
				public void WithReturnInNestedFunction() {
					[|Assert.Collection(default(IEnumerable<object>), item => {
						Func<bool> isNull = () => { return item == null; };
						bool IsNull() { return item == null; }
						Assert.False(isNull() || IsNull());
					})|];
				}

				[Fact]
				public async Task CannotFixLambdaWithReturn() {
					[|Assert.Collection(default(IEnumerable<object>), item => { if (item == null) return; Assert.NotNull(item); })|];
					await [|Assert.CollectionAsync(default(IEnumerable<Task<int>>), item => { return AsyncElementInspector(item); })|];
				}

				[Fact]
				public void CannotFixAnonymousMethod() {
					[|Assert.Collection(default(IEnumerable<object>), delegate (object item) { Assert.NotNull(item); })|];
				}

				[Fact]
				public void WithComplexCollectionExpressions() {
					var holder = default(CollectionHolder);
					[|Assert.Collection(holder.Items,
						error => Assert.Equal(
							"Response status code does not indicate success: 404 (Not Found).",
							error))|];
					[|Assert.Collection(
						holder.GetItems(),
						t => Assert.Equal("Query", t))|];
					[|Assert.Collection(
						holder.Map["address"].Items,
						item => Assert.Equal("external", item))|];
				}

				[Fact]
				public async Task WithGenericAndNamedArguments() {
					[|Assert.Collection<object>(default(IEnumerable<object>), item => Assert.NotNull(item))|];
					await [|Assert.CollectionAsync<Task<int>>(default(IEnumerable<Task<int>>), async item => Assert.Equal(42, await item))|];
					[|Assert.Collection(collection: default(IEnumerable<object>), elementInspectors: item => Assert.NotNull(item))|];
				}

				[Theory]
				[InlineData(42)]
				public void WithParameterAndNestedScopeCollision(int item) {
					var holder = default(CollectionHolder);
					foreach (var nested in holder.Items) { }
					[|Assert.Collection(default(IEnumerable<string>), item => Assert.Equal(holder.item, item))|];
					[|Assert.Collection(default(IEnumerable<string>), nested => Assert.Equal(holder.item, nested))|];
				}

				class CollectionHolder {
					public string item => "abc";
					public IEnumerable<string> Items => default(IEnumerable<string>);
					public IDictionary<string, CollectionHolder> Map => default(IDictionary<string, CollectionHolder>);
					public IEnumerable<string> GetItems() => default(IEnumerable<string>);
				}
			}

			namespace UsingStaticTests {
				using static Xunit.Assert;

				public class StaticAssertTestClass {
					[Fact]
					public void WithUsingStaticAssert() {
						[|Collection(default(IEnumerable<object>), item => NotNull(item))|];
						[|Collection<object>(default(IEnumerable<object>), item => NotNull(item))|];
					}
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public static void CannotFixInsideLambda()
				{
					Record.Exception(
						() => {|#0:Assert.Collection(default(IEnumerable<object>), item => Assert.True(false))|}
					);
				}

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

				// https://github.com/xunit/xunit/issues/3336
				[Fact]
				public void WithLeadingComment() {
					// Assert
					var item = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item);
				}

				[Fact]
				public void WithMultiLineCollection() {
					var item = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item);
				}

				[Fact]
				public void WithMultiLineInspector() {
					var item = Assert.Single(default(IEnumerable<string>));
					Assert.Equal("abc", item);
				}

				// https://github.com/xunit/xunit/issues/3659
				[Fact]
				public async Task WithParenthesizedLambda() {
					var item = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item);
					object item_2 = Assert.Single(default(IEnumerable<string>));
					Assert.NotNull(item_2);
					var item_3 = Assert.Single(default(IEnumerable<Task<int>>));
					Assert.Equal(42, await item_3);
				}

				[Fact]
				public async Task WithAwaitedLambda() {
					var item = Assert.Single(default(IEnumerable<Task<int>>));
					await AsyncElementInspector(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					await AsyncElementInspector(item_2);
					var item_3 = Assert.Single(default(IEnumerable<Task<int>>));
					await (item_3 ?? Task.CompletedTask);
				}

				[Fact]
				public async Task WithMemberAccessInspector() {
					var item = Assert.Single(default(IEnumerable<object>));
					this.ElementInspector(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					await this.AsyncElementInspector(item_2);
				}

				[Fact]
				public async Task WithDelegateInspector() {
					Action<object> inspector = ElementInspector;
					Func<Task<int>, Task> asyncInspector = AsyncElementInspector;
					Action<object>[] inspectors = new Action<object>[] { ElementInspector };
					var item = Assert.Single(default(IEnumerable<object>));
					inspector(item);
					var item_2 = Assert.Single(default(IEnumerable<Task<int>>));
					await asyncInspector(item_2);
					var item_3 = Assert.Single(default(IEnumerable<object>));
					inspectors[0](item_3);
					var item_4 = Assert.Single(default(IEnumerable<object>));
					GetElementInspector()(item_4);
				}

				Action<object> GetElementInspector() => ElementInspector;

				[Fact]
				public void WithThrowingLambda() {
					var item = Assert.Single(default(IEnumerable<object>));
					throw new InvalidOperationException();
				}

				[Fact]
				public void WithReturnInNestedFunction() {
					var item = Assert.Single(default(IEnumerable<object>));
					Func<bool> isNull = () => { return item == null; };
					bool IsNull() { return item == null; }
					Assert.False(isNull() || IsNull());
				}

				[Fact]
				public async Task CannotFixLambdaWithReturn() {
					{|#1:Assert.Collection(default(IEnumerable<object>), item => { if (item == null) return; Assert.NotNull(item); })|};
					await {|#2:Assert.CollectionAsync(default(IEnumerable<Task<int>>), item => { return AsyncElementInspector(item); })|};
				}

				[Fact]
				public void CannotFixAnonymousMethod() {
					{|#3:Assert.Collection(default(IEnumerable<object>), delegate (object item) { Assert.NotNull(item); })|};
				}

				[Fact]
				public void WithComplexCollectionExpressions() {
					var holder = default(CollectionHolder);
					var error = Assert.Single(holder.Items);
					Assert.Equal("Response status code does not indicate success: 404 (Not Found).", error);
					var t = Assert.Single(holder.GetItems());
					Assert.Equal("Query", t);
					var item = Assert.Single(holder.Map["address"].Items);
					Assert.Equal("external", item);
				}

				[Fact]
				public async Task WithGenericAndNamedArguments() {
					var item = Assert.Single<object>(default(IEnumerable<object>));
					Assert.NotNull(item);
					var item_2 = Assert.Single<Task<int>>(default(IEnumerable<Task<int>>));
					Assert.Equal(42, await item_2);
					var item_3 = Assert.Single(default(IEnumerable<object>));
					Assert.NotNull(item_3);
				}

				[Theory]
				[InlineData(42)]
				public void WithParameterAndNestedScopeCollision(int item) {
					var holder = default(CollectionHolder);
					foreach (var nested in holder.Items) { }
					var item_2 = Assert.Single(default(IEnumerable<string>));
					Assert.Equal(holder.item, item_2);
					var nested_2 = Assert.Single(default(IEnumerable<string>));
					Assert.Equal(holder.item, nested_2);
				}

				class CollectionHolder {
					public string item => "abc";
					public IEnumerable<string> Items => default(IEnumerable<string>);
					public IDictionary<string, CollectionHolder> Map => default(IDictionary<string, CollectionHolder>);
					public IEnumerable<string> GetItems() => default(IEnumerable<string>);
				}
			}

			namespace UsingStaticTests {
				using static Xunit.Assert;

				public class StaticAssertTestClass {
					[Fact]
					public void WithUsingStaticAssert() {
						var item = Single(default(IEnumerable<object>));
						NotNull(item);
						var item_2 = Single<object>(default(IEnumerable<object>));
						NotNull(item_2);
					}
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Collection"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Collection"),
			Verify.Diagnostic().WithLocation(2).WithArguments("CollectionAsync"),
			Verify.Diagnostic().WithLocation(3).WithArguments("Collection"),
		};

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, AssertSingleShouldBeUsedForSingleParameterFixer.Key_UseSingleMethod, expected);
	}

#if NETCOREAPP3_0_OR_GREATER

	[Fact]
	public async ValueTask V2_and_V3_AsyncEnumerable()
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
