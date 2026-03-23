using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualPrecisionShouldBeInRange>;

public class X2016_AssertEqualPrecisionShouldBeInRangeTest
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void DoublePrecisionInRange_DoesNotTrigger() {
					Assert.Equal(0.133d, 0.13d, 0);
					Assert.Equal(0.133d, 0.13d, 8);
					Assert.Equal(0.133d, 0.13d, 15);
				}

				void DoublePrecisionOutOfRange_Triggers() {
					Assert.Equal(0.133d, 0.13d, {|#0:int.MinValue|});
					Assert.Equal(0.133d, 0.13d, {|#1:-1|});
					Assert.Equal(0.133d, 0.13d, {|#2:16|});
					Assert.Equal(0.133d, 0.13d, {|#3:int.MaxValue|});
				}

				void DecimalPrecisionInRange_DoesNotTrigger() {
					Assert.Equal(0.133m, 0.13m, 0);
					Assert.Equal(0.133m, 0.13m, 14);
					Assert.Equal(0.133m, 0.13m, 28);
				}

				void DecimalPrecisionOutOfRange_Triggers() {
					Assert.Equal(0.133m, 0.13m, {|#10:int.MinValue|});
					Assert.Equal(0.133m, 0.13m, {|#11:-1|});
					Assert.Equal(0.133m, 0.13m, {|#12:29|});
					Assert.Equal(0.133m, 0.13m, {|#13:int.MaxValue|});
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("[0..15]", "double"),
			Verify.Diagnostic().WithLocation(1).WithArguments("[0..15]", "double"),
			Verify.Diagnostic().WithLocation(2).WithArguments("[0..15]", "double"),
			Verify.Diagnostic().WithLocation(3).WithArguments("[0..15]", "double"),

			Verify.Diagnostic().WithLocation(10).WithArguments("[0..28]", "decimal"),
			Verify.Diagnostic().WithLocation(11).WithArguments("[0..28]", "decimal"),
			Verify.Diagnostic().WithLocation(12).WithArguments("[0..28]", "decimal"),
			Verify.Diagnostic().WithLocation(13).WithArguments("[0..28]", "decimal"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
