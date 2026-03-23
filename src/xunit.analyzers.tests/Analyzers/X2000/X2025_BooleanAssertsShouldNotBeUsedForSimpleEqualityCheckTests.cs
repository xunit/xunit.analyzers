using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class X2025_BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				void SimplifiableExpressions() {
					var trueValue = true;

					{|#0:Assert.True(trueValue == true)|};
					{|#1:Assert.True(trueValue != true)|};
					{|#2:Assert.True(trueValue == true, "message")|};
					{|#3:Assert.True(trueValue != true, "message")|};
					{|#4:Assert.True(true == trueValue)|};
					{|#5:Assert.True(true != trueValue)|};
					{|#6:Assert.True(true == trueValue, "message")|};
					{|#7:Assert.True(true != trueValue, "message")|};

					{|#10:Assert.True(trueValue == false)|};
					{|#11:Assert.True(trueValue != false)|};
					{|#12:Assert.True(trueValue == false, "message")|};
					{|#13:Assert.True(trueValue != false, "message")|};
					{|#14:Assert.True(false == trueValue)|};
					{|#15:Assert.True(false != trueValue)|};
					{|#16:Assert.True(false == trueValue, "message")|};
					{|#17:Assert.True(false != trueValue, "message")|};

					{|#20:Assert.False(trueValue == true)|};
					{|#21:Assert.False(trueValue != true)|};
					{|#22:Assert.False(trueValue == true, "message")|};
					{|#23:Assert.False(trueValue != true, "message")|};
					{|#24:Assert.False(true == trueValue)|};
					{|#25:Assert.False(true != trueValue)|};
					{|#26:Assert.False(true == trueValue, "message")|};
					{|#27:Assert.False(true != trueValue, "message")|};

					{|#30:Assert.False(trueValue == false)|};
					{|#31:Assert.False(trueValue != false)|};
					{|#32:Assert.False(trueValue == false, "message")|};
					{|#33:Assert.False(trueValue != false, "message")|};
					{|#34:Assert.False(false == trueValue)|};
					{|#35:Assert.False(false != trueValue)|};
					{|#36:Assert.False(false == trueValue, "message")|};
					{|#37:Assert.False(false != trueValue, "message")|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit2025").WithLocation(0).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(1).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(2).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(3).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(4).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(5).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(6).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(7).WithArguments("True"),

			Verify.Diagnostic("xUnit2025").WithLocation(10).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(11).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(12).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(13).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(14).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(15).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(16).WithArguments("True"),
			Verify.Diagnostic("xUnit2025").WithLocation(17).WithArguments("True"),

			Verify.Diagnostic("xUnit2025").WithLocation(20).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(21).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(22).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(23).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(24).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(25).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(26).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(27).WithArguments("False"),

			Verify.Diagnostic("xUnit2025").WithLocation(30).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(31).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(32).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(33).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(34).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(35).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(36).WithArguments("False"),
			Verify.Diagnostic("xUnit2025").WithLocation(37).WithArguments("False"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
