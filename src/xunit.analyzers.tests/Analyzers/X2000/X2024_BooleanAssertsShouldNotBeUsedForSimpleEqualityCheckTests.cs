using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;
using Verify_v3_Pre_301 = CSharpVerifier<X2024_BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckTests.Analyzer_v3_Pre301>;

public class X2024_BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void ComparingAgainstNonLiteral_DoesNotTrigger() {
					var value1 = 42;
					var value2 = 2112;
					var value3 = new { innerValue = 2600 };

					Assert.True(value1 == value2);
					Assert.True(value1 != value2);
					Assert.True(value1 == value3.innerValue);
					Assert.True(value1 != value3.innerValue);

					Assert.False(value1 == value2);
					Assert.False(value1 != value2);
					Assert.False(value1 == value3.innerValue);
					Assert.False(value1 != value3.innerValue);
				}

				void ComparingAgainstLiteral_WithMessage_DoesNotTrigger() {
					var stringValue = "bacon";

					Assert.True(stringValue == "bacon", "message");
					Assert.True(stringValue != "bacon", "message");
					Assert.True("bacon" == stringValue, "message");
					Assert.True("bacon" != stringValue, "message");

					Assert.False(stringValue == "bacon", "message");
					Assert.False(stringValue != "bacon", "message");
					Assert.False("bacon" == stringValue, "message");
					Assert.False("bacon" != stringValue, "message");

					var charValue = '5';

					Assert.True(charValue == '5', "message");
					Assert.True(charValue != '5', "message");
					Assert.True('5' == charValue, "message");
					Assert.True('5' != charValue, "message");

					Assert.False(charValue == '5', "message");
					Assert.False(charValue != '5', "message");
					Assert.False('5' == charValue, "message");
					Assert.False('5' != charValue, "message");

					var intValue = 5;

					Assert.True(intValue == 5, "message");
					Assert.True(intValue != 5, "message");
					Assert.True(5 == intValue, "message");
					Assert.True(5 != intValue, "message");

					Assert.False(intValue == 5, "message");
					Assert.False(intValue != 5, "message");
					Assert.False(5 == intValue, "message");
					Assert.False(5 != intValue, "message");

					var enumValue = MyEnum.Bacon;

					Assert.True(enumValue == MyEnum.Bacon, "message");
					Assert.True(enumValue != MyEnum.Bacon, "message");
					Assert.True(MyEnum.Bacon == enumValue, "message");
					Assert.True(MyEnum.Bacon != enumValue, "message");

					Assert.False(enumValue == MyEnum.Bacon, "message");
					Assert.False(enumValue != MyEnum.Bacon, "message");
					Assert.False(MyEnum.Bacon == enumValue, "message");
					Assert.False(MyEnum.Bacon != enumValue, "message");
				}

				void ComparingAgainstLiteral_WithoutMessage_Triggers() {
					var stringValue = "bacon";

					{|#0:Assert.True(stringValue == "bacon")|};
					{|#1:Assert.True(stringValue != "bacon")|};
					{|#2:Assert.True("bacon" == stringValue)|};
					{|#3:Assert.True("bacon" != stringValue)|};

					{|#4:Assert.False(stringValue == "bacon")|};
					{|#5:Assert.False(stringValue != "bacon")|};
					{|#6:Assert.False("bacon" == stringValue)|};
					{|#7:Assert.False("bacon" != stringValue)|};

					var charValue = '5';

					{|#10:Assert.True(charValue == '5')|};
					{|#11:Assert.True(charValue != '5')|};
					{|#12:Assert.True('5' == charValue)|};
					{|#13:Assert.True('5' != charValue)|};

					{|#14:Assert.False(charValue == '5')|};
					{|#15:Assert.False(charValue != '5')|};
					{|#16:Assert.False('5' == charValue)|};
					{|#17:Assert.False('5' != charValue)|};

					var intValue = 5;

					{|#20:Assert.True(intValue == 5)|};
					{|#21:Assert.True(intValue != 5)|};
					{|#22:Assert.True(5 == intValue)|};
					{|#23:Assert.True(5 != intValue)|};

					{|#24:Assert.False(intValue == 5)|};
					{|#25:Assert.False(intValue != 5)|};
					{|#26:Assert.False(5 == intValue)|};
					{|#27:Assert.False(5 != intValue)|};

					var enumValue = MyEnum.Bacon;

					{|#30:Assert.True(enumValue == MyEnum.Bacon)|};
					{|#31:Assert.True(enumValue != MyEnum.Bacon)|};
					{|#32:Assert.True(MyEnum.Bacon == enumValue)|};
					{|#33:Assert.True(MyEnum.Bacon != enumValue)|};

					{|#34:Assert.False(enumValue == MyEnum.Bacon)|};
					{|#35:Assert.False(enumValue != MyEnum.Bacon)|};
					{|#36:Assert.False(MyEnum.Bacon == enumValue)|};
					{|#37:Assert.False(MyEnum.Bacon != enumValue)|};
				}
			}

			#nullable enable

			class NullableTestClass {
				void ComparingAgainstNull_WithMessage_DoesNotTrigger() {
					string? stringValue = null;

					Assert.True(stringValue == null, "message");
					Assert.True(stringValue != null, "message");
					Assert.True(null == stringValue, "message");
					Assert.True(null != stringValue, "message");

					Assert.False(stringValue == null, "message");
					Assert.False(stringValue != null, "message");
					Assert.False(null == stringValue, "message");
					Assert.False(null != stringValue, "message");

					int? intValue = null;

					Assert.True(intValue == null, "message");
					Assert.True(intValue != null, "message");
					Assert.True(null == intValue, "message");
					Assert.True(null != intValue, "message");

					Assert.False(intValue == null, "message");
					Assert.False(intValue != null, "message");
					Assert.False(null == intValue, "message");
					Assert.False(null != intValue, "message");
				}

				void ComparingAgainstNull_WithoutMessage_Triggers() {
					string? stringValue = null;

					{|#40:Assert.True(stringValue == null)|};
					{|#41:Assert.True(stringValue != null)|};
					{|#42:Assert.True(null == stringValue)|};
					{|#43:Assert.True(null != stringValue)|};

					{|#44:Assert.False(stringValue == null)|};
					{|#45:Assert.False(stringValue != null)|};
					{|#46:Assert.False(null == stringValue)|};
					{|#47:Assert.False(null != stringValue)|};

					int? intValue = null;

					{|#50:Assert.True(intValue == null)|};
					{|#51:Assert.True(intValue != null)|};
					{|#52:Assert.True(null == intValue)|};
					{|#53:Assert.True(null != intValue)|};

					{|#54:Assert.False(intValue == null)|};
					{|#55:Assert.False(intValue != null)|};
					{|#56:Assert.False(null == intValue)|};
					{|#57:Assert.False(null != intValue)|};
				}
			}

			enum MyEnum { None, Bacon, Veggie }
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit2024").WithLocation(0).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(1).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(2).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(3).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(4).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(5).WithArguments("False", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(6).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(7).WithArguments("False", "Equal"),

			Verify.Diagnostic("xUnit2024").WithLocation(10).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(11).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(12).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(13).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(14).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(15).WithArguments("False", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(16).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(17).WithArguments("False", "Equal"),

			Verify.Diagnostic("xUnit2024").WithLocation(20).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(21).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(22).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(23).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(24).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(25).WithArguments("False", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(26).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(27).WithArguments("False", "Equal"),

			Verify.Diagnostic("xUnit2024").WithLocation(30).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(31).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(32).WithArguments("True", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(33).WithArguments("True", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(34).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(35).WithArguments("False", "Equal"),
			Verify.Diagnostic("xUnit2024").WithLocation(36).WithArguments("False", "NotEqual"),
			Verify.Diagnostic("xUnit2024").WithLocation(37).WithArguments("False", "Equal"),

			Verify.Diagnostic("xUnit2024").WithLocation(40).WithArguments("True", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(41).WithArguments("True", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(42).WithArguments("True", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(43).WithArguments("True", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(44).WithArguments("False", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(45).WithArguments("False", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(46).WithArguments("False", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(47).WithArguments("False", "Null"),

			Verify.Diagnostic("xUnit2024").WithLocation(50).WithArguments("True", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(51).WithArguments("True", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(52).WithArguments("True", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(53).WithArguments("True", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(54).WithArguments("False", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(55).WithArguments("False", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(56).WithArguments("False", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(57).WithArguments("False", "Null"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async ValueTask V2_and_V3_PrePointerSupport()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				unsafe void ComparingAgainstNullPointer_DoesNotTrigger() {
					var value = 42;
					var ptr = &value;

					Assert.True(ptr == null);
					Assert.True(null == ptr);
					Assert.True(ptr != null);
					Assert.True(null != ptr);

					Assert.False(ptr == null);
					Assert.False(null == ptr);
					Assert.False(ptr != null);
					Assert.False(null != ptr);
				}
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
		await Verify_v3_Pre_301.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				unsafe void ComparingAgainstNullPointer_Triggers() {
					var value = 42;
					var ptr = &value;

					{|#0:Assert.True(ptr == null)|};
					{|#1:Assert.True(null == ptr)|};
					{|#2:Assert.True(ptr != null)|};
					{|#3:Assert.True(null != ptr)|};

					{|#10:Assert.False(ptr == null)|};
					{|#11:Assert.False(null == ptr)|};
					{|#12:Assert.False(ptr != null)|};
					{|#13:Assert.False(null != ptr)|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit2024").WithLocation(0).WithArguments("True", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(1).WithArguments("True", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(2).WithArguments("True", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(3).WithArguments("True", "NotNull"),

			Verify.Diagnostic("xUnit2024").WithLocation(10).WithArguments("False", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(11).WithArguments("False", "NotNull"),
			Verify.Diagnostic("xUnit2024").WithLocation(12).WithArguments("False", "Null"),
			Verify.Diagnostic("xUnit2024").WithLocation(13).WithArguments("False", "Null"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}

	public class Analyzer_v3_Pre301 : BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new(3, 0, 0));
	}
}
