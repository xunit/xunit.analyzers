using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertStringEqualityCheckShouldNotUseBoolCheck>;

public class X2010_AssertStringEqualityCheckShouldNotUseBoolCheckTest
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				void InstanceEquals_Triggers() {
					{|#0:Assert.True("abc".Equals("a"))|};
					{|#1:Assert.False("abc".Equals("a"))|};
				}

				void True_InstanceEquals_OrdinalStringComparison_Triggers() {
					{|#10:Assert.True("abc".Equals("a", StringComparison.Ordinal))|};
					{|#11:Assert.True("abc".Equals("a", StringComparison.OrdinalIgnoreCase))|};
				}

				void True_InstanceEquals_NonOrdinalStringComparison_Triggers() {
					Assert.True("abc".Equals("a", StringComparison.CurrentCulture));
					Assert.True("abc".Equals("a", StringComparison.CurrentCultureIgnoreCase));
					Assert.True("abc".Equals("a", StringComparison.InvariantCulture));
					Assert.True("abc".Equals("a", StringComparison.InvariantCultureIgnoreCase));
				}

				void False_InstanceEquals_StringComparison_DoesNotTrigger() {
					Assert.False("abc".Equals("a", StringComparison.Ordinal));
					Assert.False("abc".Equals("a", StringComparison.OrdinalIgnoreCase));
					Assert.False("abc".Equals("a", StringComparison.CurrentCulture));
					Assert.False("abc".Equals("a", StringComparison.CurrentCultureIgnoreCase));
					Assert.False("abc".Equals("a", StringComparison.InvariantCulture));
					Assert.False("abc".Equals("a", StringComparison.InvariantCultureIgnoreCase));
				}

				void StaticEquals_Triggers() {
					{|#20:Assert.True(string.Equals("abc", "a"))|};
					{|#21:Assert.False(string.Equals("abc", "a"))|};
				}

				void True_StaticEquals_OrdinalStringComparison_Triggers() {
					{|#30:Assert.True(string.Equals("abc", "a", StringComparison.Ordinal))|};
					{|#31:Assert.True(string.Equals("abc", "a", StringComparison.OrdinalIgnoreCase))|};
				}

				void True_StaticEquals_NonOrdinalStringComparison_Triggers() {
					Assert.True(string.Equals("abc", "a", StringComparison.CurrentCulture));
					Assert.True(string.Equals("abc", "a", StringComparison.CurrentCultureIgnoreCase));
					Assert.True(string.Equals("abc", "a", StringComparison.InvariantCulture));
					Assert.True(string.Equals("abc", "a", StringComparison.InvariantCultureIgnoreCase));
				}

				void False_StaticEquals_StringComparison_DoesNotTrigger() {
					Assert.False(string.Equals("abc", "a", StringComparison.Ordinal));
					Assert.False(string.Equals("abc", "a", StringComparison.OrdinalIgnoreCase));
					Assert.False(string.Equals("abc", "a", StringComparison.CurrentCulture));
					Assert.False(string.Equals("abc", "a", StringComparison.CurrentCultureIgnoreCase));
					Assert.False(string.Equals("abc", "a", StringComparison.InvariantCulture));
					Assert.False(string.Equals("abc", "a", StringComparison.InvariantCultureIgnoreCase));
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", "Equal"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.False()", "NotEqual"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.True()", "Equal"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.True()", "Equal"),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.True()", "Equal"),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.False()", "NotEqual"),

			Verify.Diagnostic().WithLocation(30).WithArguments("Assert.True()", "Equal"),
			Verify.Diagnostic().WithLocation(31).WithArguments("Assert.True()", "Equal"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
