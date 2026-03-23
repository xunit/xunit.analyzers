using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class X1011_InlineDataMustMatchTheoryParameters_ExtraValueFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class UnusedDataValues {
				[Theory]
				[InlineData(42, {|xUnit1011:21.12|})]
				public void TestMethod1(int a) { }

				[Theory]
				[InlineData(1, {|xUnit1011:"extra"|})]
				public void TestMethod2(int p) { }
			}
			""";
		var afterRemove = /* lang=c#-test */ """
			using Xunit;

			public class UnusedDataValues {
				[Theory]
				[InlineData(42)]
				public void TestMethod1(int a) { }

				[Theory]
				[InlineData(1)]
				public void TestMethod2(int p) { }
			}
			""";
		var afterAdd = /* lang=c#-test */ """
			using Xunit;

			public class UnusedDataValues {
				[Theory]
				[InlineData(42, 21.12)]
				public void TestMethod1(int a, double p) { }

				[Theory]
				[InlineData(1, "extra")]
				public void TestMethod2(int p, string p_2) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, afterRemove, InlineDataMustMatchTheoryParameters_ExtraValueFixer.Key_RemoveExtraDataValue);
		await Verify.VerifyCodeFixFixAll(before, afterAdd, InlineDataMustMatchTheoryParameters_ExtraValueFixer.Key_AddTheoryParameter);
	}
}
