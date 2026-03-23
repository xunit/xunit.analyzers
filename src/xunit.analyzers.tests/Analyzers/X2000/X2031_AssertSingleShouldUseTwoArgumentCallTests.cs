using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldUseTwoArgumentCall>;

public class X2031_AssertSingleShouldUseTwoArgumentCallTests
{
	public static TheoryData<string, string> GetEnumerables(
		string typeName,
		string comparison) =>
			new()
			{
				{ $"new System.Collections.Generic.List<{typeName}>()", comparison },
				{ $"new System.Collections.Generic.HashSet<{typeName}>()", comparison },
				{ $"new System.Collections.ObjectModel.Collection<{typeName}>()", comparison },
				{ $"new {typeName}[0]", comparison },
				{ $"System.Linq.Enumerable.Empty<{typeName}>()", comparison },
			};

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Collections.ObjectModel;
			using System.Linq;
			using Xunit;

			class TestClass {
				void WithoutWhereClause_DoesNotTrigger() {
					Assert.Single(new int[0]);
					Assert.Single(new List<int>());
					Assert.Single(new HashSet<int>());
					Assert.Single(new Collection<int>());
					Assert.Single(Enumerable.Empty<int>());

					Assert.Single(new string[0]);
					Assert.Single(new List<string>());
					Assert.Single(new HashSet<string>());
					Assert.Single(new Collection<string>());
					Assert.Single(Enumerable.Empty<string>());
				}

				void WithIndexedWhereClause_DoesNotTrigger() {
					Assert.Single(new int[0].Where((f, i) => f > 0 && i > 0));
					Assert.Single(new List<int>().Where((f, i) => f > 0 && i > 0));
					Assert.Single(new HashSet<int>().Where((f, i) => f > 0 && i > 0));
					Assert.Single(new Collection<int>().Where((f, i) => f > 0 && i > 0));
					Assert.Single(Enumerable.Empty<int>().Where((f, i) => f > 0 && i > 0));

					Assert.Single(new string[0].Where((f, i) => f.Length > 0 && i > 0));
					Assert.Single(new List<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.Single(new HashSet<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.Single(new Collection<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.Single(Enumerable.Empty<string>().Where((f, i) => f.Length > 0 && i > 0));
				}

				void WithWhereClause_WithChainedLinq_DoesNotTrigger() {
					Assert.Single(new int[0].Where(f => f > 0).Select(f => f));
					Assert.Single(new List<int>().Where(f => f > 0).Select(f => f));
					Assert.Single(new HashSet<int>().Where(f => f > 0).Select(f => f));
					Assert.Single(new Collection<int>().Where(f => f > 0).Select(f => f));
					Assert.Single(Enumerable.Empty<int>().Where(f => f > 0).Select(f => f));

					Assert.Single(new string[0].Where(f => f.Length > 0).Select(f => f));
					Assert.Single(new List<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.Single(new HashSet<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.Single(new Collection<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.Single(Enumerable.Empty<string>().Where(f => f.Length > 0).Select(f => f));
				}

				void WithWhereClause_Triggers() {
					[|Assert.Single(new int[0].Where(f => f > 0))|];
					[|Assert.Single(new List<int>().Where(f => f > 0))|];
					[|Assert.Single(new HashSet<int>().Where(f => f > 0))|];
					[|Assert.Single(new Collection<int>().Where(f => f > 0))|];
					[|Assert.Single(Enumerable.Empty<int>().Where(f => f > 0))|];

					[|Assert.Single(new string[0].Where(f => f.Length > 0))|];
					[|Assert.Single(new List<string>().Where(f => f.Length > 0))|];
					[|Assert.Single(new HashSet<string>().Where(f => f.Length > 0))|];
					[|Assert.Single(new Collection<string>().Where(f => f.Length > 0))|];
					[|Assert.Single(Enumerable.Empty<string>().Where(f => f.Length > 0))|];
				}

				void Strings_WithWhereClause_Triggers() {
					[|Assert.Single("".Where(f => f > 0))|];
					[|Assert.Single("123".Where(f => f > 0))|];
					[|Assert.Single("abc\n\t".Where(f => f > 0))|];
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
