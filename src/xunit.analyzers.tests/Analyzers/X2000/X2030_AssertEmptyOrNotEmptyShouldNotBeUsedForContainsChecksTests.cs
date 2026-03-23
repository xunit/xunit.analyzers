using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks>;

public class X2030_AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksTests
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
					Assert.NotEmpty(new int[0]);
					Assert.NotEmpty(new List<int>());
					Assert.NotEmpty(new HashSet<int>());
					Assert.NotEmpty(new Collection<int>());
					Assert.NotEmpty(Enumerable.Empty<int>());

					Assert.NotEmpty(new string[0]);
					Assert.NotEmpty(new List<string>());
					Assert.NotEmpty(new HashSet<string>());
					Assert.NotEmpty(new Collection<string>());
					Assert.NotEmpty(Enumerable.Empty<string>());
				}

				void WithIndexedWhereClause_DoesNotTrigger() {
					Assert.NotEmpty(new int[0].Where((f, i) => f > 0 && i > 0));
					Assert.NotEmpty(new List<int>().Where((f, i) => f > 0 && i > 0));
					Assert.NotEmpty(new HashSet<int>().Where((f, i) => f > 0 && i > 0));
					Assert.NotEmpty(new Collection<int>().Where((f, i) => f > 0 && i > 0));
					Assert.NotEmpty(Enumerable.Empty<int>().Where((f, i) => f > 0 && i > 0));

					Assert.NotEmpty(new string[0].Where((f, i) => f.Length > 0 && i > 0));
					Assert.NotEmpty(new List<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.NotEmpty(new HashSet<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.NotEmpty(new Collection<string>().Where((f, i) => f.Length > 0 && i > 0));
					Assert.NotEmpty(Enumerable.Empty<string>().Where((f, i) => f.Length > 0 && i > 0));
				}

				void WithWhereClause_WithChainedLinq_DoesNotTrigger() {
					Assert.NotEmpty(new int[0].Where(f => f > 0).Select(f => f));
					Assert.NotEmpty(new List<int>().Where(f => f > 0).Select(f => f));
					Assert.NotEmpty(new HashSet<int>().Where(f => f > 0).Select(f => f));
					Assert.NotEmpty(new Collection<int>().Where(f => f > 0).Select(f => f));
					Assert.NotEmpty(Enumerable.Empty<int>().Where(f => f > 0).Select(f => f));

					Assert.NotEmpty(new string[0].Where(f => f.Length > 0).Select(f => f));
					Assert.NotEmpty(new List<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.NotEmpty(new HashSet<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.NotEmpty(new Collection<string>().Where(f => f.Length > 0).Select(f => f));
					Assert.NotEmpty(Enumerable.Empty<string>().Where(f => f.Length > 0).Select(f => f));
				}

				void WithWhereClause_Triggers() {
					{|xUnit2030:Assert.NotEmpty(new int[0].Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty(new List<int>().Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty(new HashSet<int>().Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty(new Collection<int>().Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty(Enumerable.Empty<int>().Where(f => f > 0))|};

					{|xUnit2030:Assert.NotEmpty(new string[0].Where(f => f.Length > 0))|};
					{|xUnit2030:Assert.NotEmpty(new List<string>().Where(f => f.Length > 0))|};
					{|xUnit2030:Assert.NotEmpty(new HashSet<string>().Where(f => f.Length > 0))|};
					{|xUnit2030:Assert.NotEmpty(new Collection<string>().Where(f => f.Length > 0))|};
					{|xUnit2030:Assert.NotEmpty(Enumerable.Empty<string>().Where(f => f.Length > 0))|};
				}

				void Strings_WithWhereClause_Triggers() {
					{|xUnit2030:Assert.NotEmpty("".Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty("123".Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty("abc\n\t".Where(f => f > 0))|};
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
