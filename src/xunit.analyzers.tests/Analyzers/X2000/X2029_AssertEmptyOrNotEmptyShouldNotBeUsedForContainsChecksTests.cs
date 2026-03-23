using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks>;

public class X2029_AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksTests
{
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
					Assert.Empty(new int[0]);
					Assert.Empty(new List<int>());
					Assert.Empty(new HashSet<int>());
					Assert.Empty(new Collection<int>());
					Assert.Empty(Enumerable.Empty<int>());

					Assert.Empty(new string[0]);
					Assert.Empty(new List<string>());
					Assert.Empty(new HashSet<string>());
					Assert.Empty(new Collection<string>());
					Assert.Empty(Enumerable.Empty<string>());
				}

				void WithIndexedWhereClause_DoesNotTrigger() {
					Assert.Empty(new int[0].Where((f, i) => f > 0 && i > 0));
					Assert.Empty(new List<int>().Where((f, i) => f > 0 && i > 0));
					Assert.Empty(new HashSet<int>().Where((f, i) => f > 0 && i > 0));
					Assert.Empty(new Collection<int>().Where((f, i) => f > 0 && i > 0));
					Assert.Empty(Enumerable.Empty<int>().Where((f, i) => f > 0 && i > 0));

					Assert.Empty(new string[0].Where((f, i) => f.Length > 0 && i > 0));
					Assert.Empty(new List<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.Empty(new HashSet<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.Empty(new Collection<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.Empty(Enumerable.Empty<string>().Where((f, i) => f.Length > 0 && i > 0));
				}

				void WithWhereClause_WithChainedLinq_DoesNotTrigger() {
					Assert.Empty(new int[0].Where(f => f > 0).Select(f => f));
					Assert.Empty(new List<int>().Where(f => f > 0).Select(f => f));
					Assert.Empty(new HashSet<int>().Where(f => f > 0).Select(f => f));
					Assert.Empty(new Collection<int>().Where(f => f > 0).Select(f => f));
					Assert.Empty(Enumerable.Empty<int>().Where(f => f > 0).Select(f => f));

					Assert.Empty(new string[0].Where(f => f.Length > 0).Select(f => f));
					Assert.Empty(new List<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.Empty(new HashSet<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.Empty(new Collection<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.Empty(Enumerable.Empty<string>().Where(f => f.Length > 0).Select(f => f));
				}

				void WithWhereClause_Triggers() {
					{|xUnit2029:Assert.Empty(new int[0].Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty(new List<int>().Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty(new HashSet<int>().Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty(new Collection<int>().Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty(Enumerable.Empty<int>().Where(f => f > 0))|};

					{|xUnit2029:Assert.Empty(new string[0].Where(f => f.Length > 0))|};
					{|xUnit2029:Assert.Empty(new List<string>().Where(f => f.Length > 0))|};
					{|xUnit2029:Assert.Empty(new HashSet<string>().Where(f => f.Length > 0))|};
					{|xUnit2029:Assert.Empty(new Collection<string>().Where(f => f.Length > 0))|};
					{|xUnit2029:Assert.Empty(Enumerable.Empty<string>().Where(f => f.Length > 0))|};
				}

				void Strings_WithWhereClause_Triggers() {
					{|xUnit2029:Assert.Empty("".Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty("123".Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty("abc\n\t".Where(f => f > 0))|};
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
