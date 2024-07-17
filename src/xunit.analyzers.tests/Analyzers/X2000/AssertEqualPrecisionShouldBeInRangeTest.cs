using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualPrecisionShouldBeInRange>;

public class AssertEqualPrecisionShouldBeInRangeTest
{
	static readonly string Template = /* lang=c#-test */ """
		class TestClass {{
		    void TestMethod() {{
		        {0}
		    }}
		}}
		""";

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(8)]
	[InlineData(14)]
	[InlineData(15)]
	public async Task DoesNotFindError_ForDoubleArgumentWithPrecisionProvidedInRange(int precision)
	{
		var source = string.Format(
			Template,
			$"double num = 0.133d; Xunit.Assert.Equal(0.13d, num, {precision});"
		);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData(int.MinValue)]
	[InlineData(-2000)]
	[InlineData(-1)]
	[InlineData(16)]
	[InlineData(17000)]
	[InlineData(int.MaxValue)]
	public async Task FindsError_ForDoubleArgumentWithPrecisionProvidedOutOfRange(int precision)
	{
		var source = string.Format(
			Template,
			$"double num = 0.133d; Xunit.Assert.Equal(0.13d, num, {{|#0:{precision}|}});"
		);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("[0..15]", "double");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(14)]
	[InlineData(27)]
	[InlineData(28)]
	public async Task DoesNotFindError_ForDecimalArgumentWithPrecisionProvidedInRange(int precision)
	{
		var source = string.Format(
			Template,
			$"decimal num = 0.133m; Xunit.Assert.Equal(0.13m, num, {precision});"
		);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData(int.MinValue)]
	[InlineData(-2000)]
	[InlineData(-1)]
	[InlineData(29)]
	[InlineData(30000)]
	[InlineData(int.MaxValue)]
	public async Task FindsError_ForDecimalArgumentWithPrecisionProvidedOutOfRange(int precision)
	{
		var source = string.Format(
			Template,
			$"decimal num = 0.133m; Xunit.Assert.Equal(0.13m, num, {{|#0:{precision}|}});"
		);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("[0..28]", "decimal");

		await Verify.VerifyAnalyzer(source, expected);
	}
}
