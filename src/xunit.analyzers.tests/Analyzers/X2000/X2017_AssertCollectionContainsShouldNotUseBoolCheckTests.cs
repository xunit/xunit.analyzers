using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertCollectionContainsShouldNotUseBoolCheck>;

public class X2017_AssertCollectionContainsShouldNotUseBoolCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Collections.ObjectModel;
			using System.Linq;
			using Xunit;

			class IntList : List<int> { }

			class TestClass {
				void AssertTrueContainsCheck_Triggers() {
					{|#0:Assert.True(new int[0].Contains(1))|};
					{|#1:Assert.True(Enumerable.Empty<int>().Contains(1))|};
					{|#2:Assert.True(new List<int>().Contains(1))|};
					{|#3:Assert.True(new HashSet<int>().Contains(1))|};
					{|#4:Assert.True(new Collection<int>().Contains(1))|};
					{|#5:Assert.True(new IntList().Contains(1))|};

					{|#10:Assert.True(new int[0].Contains(1, EqualityComparer<int>.Default))|};
					{|#11:Assert.True(Enumerable.Empty<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#12:Assert.True(new List<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#13:Assert.True(new HashSet<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#14:Assert.True(new Collection<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#15:Assert.True(new IntList().Contains(1, EqualityComparer<int>.Default))|};
				}

				void AssertFalseContainsCheck_Triggers() {
					{|#20:Assert.False(new int[0].Contains(1))|};
					{|#21:Assert.False(Enumerable.Empty<int>().Contains(1))|};
					{|#22:Assert.False(new List<int>().Contains(1))|};
					{|#23:Assert.False(new HashSet<int>().Contains(1))|};
					{|#24:Assert.False(new Collection<int>().Contains(1))|};
					{|#25:Assert.False(new IntList().Contains(1))|};

					{|#30:Assert.False(new int[0].Contains(1, EqualityComparer<int>.Default))|};
					{|#31:Assert.False(Enumerable.Empty<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#32:Assert.False(new List<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#33:Assert.False(new HashSet<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#34:Assert.False(new Collection<int>().Contains(1, EqualityComparer<int>.Default))|};
					{|#35:Assert.False(new IntList().Contains(1, EqualityComparer<int>.Default))|};
				}

				void AssertTrueWithMessage_DoesNotTrigger() {
					Assert.True(new int[0].Contains(1), "Custom message");
					Assert.True(Enumerable.Empty<int>().Contains(1), "Custom message");
					Assert.True(new List<int>().Contains(1), "Custom message");
					Assert.True(new HashSet<int>().Contains(1), "Custom message");
					Assert.True(new Collection<int>().Contains(1), "Custom message");
					Assert.True(new IntList().Contains(1), "Custom message");
				}

				void AssertFalseWithMessage_DoesNotTrigger() {
					Assert.False(new int[0].Contains(1), "Custom message");
					Assert.False(Enumerable.Empty<int>().Contains(1), "Custom message");
					Assert.False(new List<int>().Contains(1), "Custom message");
					Assert.False(new HashSet<int>().Contains(1), "Custom message");
					Assert.False(new Collection<int>().Contains(1), "Custom message");
					Assert.False(new IntList().Contains(1), "Custom message");
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(3).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(4).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(5).WithArguments("Assert.True()", Constants.Asserts.Contains),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(12).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(13).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(14).WithArguments("Assert.True()", Constants.Asserts.Contains),
			Verify.Diagnostic().WithLocation(15).WithArguments("Assert.True()", Constants.Asserts.Contains),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(22).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(23).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(24).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(25).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),

			Verify.Diagnostic().WithLocation(30).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(31).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(32).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(33).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(34).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
			Verify.Diagnostic().WithLocation(35).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
