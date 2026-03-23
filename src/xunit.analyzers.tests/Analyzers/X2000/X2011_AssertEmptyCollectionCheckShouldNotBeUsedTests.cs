using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyCollectionCheckShouldNotBeUsed>;

public class X2011_AssertEmptyCollectionCheckShouldNotBeUsedTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Collections.ObjectModel;
			using System.Linq;
			using System.Threading.Tasks;
			using Xunit;

			class IntList : List<int> { }

			class TestClass {
				async Task CollectionCheckWithoutAction_Triggers() {
					[|Assert.Collection(new int[0])|];
					[|Assert.Collection(new List<int>())|];
					[|Assert.Collection(new HashSet<int>())|];
					[|Assert.Collection(new Collection<int>())|];
					[|Assert.Collection(Enumerable.Empty<int>())|];
					[|Assert.Collection(new IntList())|];

					await [|Assert.CollectionAsync(new int[0])|];
					await [|Assert.CollectionAsync(new List<int>())|];
					await [|Assert.CollectionAsync(new HashSet<int>())|];
					await [|Assert.CollectionAsync(new Collection<int>())|];
					await [|Assert.CollectionAsync(Enumerable.Empty<int>())|];
					await [|Assert.CollectionAsync(new IntList())|];
				}

				async Task CollectionCheckWithAction_DoesNotTrigger() {
					Assert.Collection(new int[0], i => Assert.True(true));
					Assert.Collection(new List<int>(), i => Assert.True(true));
					Assert.Collection(new HashSet<int>(), i => Assert.True(true));
					Assert.Collection(new Collection<int>(), i => Assert.True(true));
					Assert.Collection(Enumerable.Empty<int>(), i => Assert.True(true));
					Assert.Collection(new IntList(), i => Assert.True(true));

					await Assert.CollectionAsync(new int[0], async i => { await Task.Yield(); Assert.True(true); });
					await Assert.CollectionAsync(new List<int>(), async i => { await Task.Yield(); Assert.True(true); });
					await Assert.CollectionAsync(new HashSet<int>(), async i => { await Task.Yield(); Assert.True(true); });
					await Assert.CollectionAsync(new Collection<int>(), async i => { await Task.Yield(); Assert.True(true); });
					await Assert.CollectionAsync(Enumerable.Empty<int>(), async i => { await Task.Yield(); Assert.True(true); });
					await Assert.CollectionAsync(new IntList(), async i => { await Task.Yield(); Assert.True(true); });
				}
			}
			""";
#if NETCOREAPP3_0_OR_GREATER
		source += /* lang=c#-test */ """
			class AsyncTestClass {
				async Task CollectionCheckWithoutAction_Triggers() {
					[|Assert.Collection(default(IAsyncEnumerable<int>))|];

					await [|Assert.CollectionAsync(default(IAsyncEnumerable<int>))|];
				}

				async Task CollectionCheckWithAction_DoesNotTrigger() {
					Assert.Collection(default(IAsyncEnumerable<int>), i => Assert.True(true));

					await Assert.CollectionAsync(default(IAsyncEnumerable<int>), async i => { await Task.Yield(); Assert.True(true); });
				}
			}
			""";
#endif

		await Verify.VerifyAnalyzer(source);
	}
}
