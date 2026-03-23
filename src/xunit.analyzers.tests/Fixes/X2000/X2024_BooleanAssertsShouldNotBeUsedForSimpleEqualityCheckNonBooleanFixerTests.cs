using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class X2024_BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			class TestClass {
				void TestMethod() {
					var stringValue = "bacon";

					{|xUnit2024:Assert.True(stringValue == "bacon")|};
					{|xUnit2024:Assert.True(stringValue != "bacon")|};
					{|xUnit2024:Assert.True("bacon" == stringValue)|};
					{|xUnit2024:Assert.True("bacon" != stringValue)|};

					{|xUnit2024:Assert.False(stringValue == "bacon")|};
					{|xUnit2024:Assert.False(stringValue != "bacon")|};
					{|xUnit2024:Assert.False("bacon" == stringValue)|};
					{|xUnit2024:Assert.False("bacon" != stringValue)|};

					var charValue = '5';

					{|xUnit2024:Assert.True(charValue == '5')|};
					{|xUnit2024:Assert.True(charValue != '5')|};
					{|xUnit2024:Assert.True('5' == charValue)|};
					{|xUnit2024:Assert.True('5' != charValue)|};

					{|xUnit2024:Assert.False(charValue == '5')|};
					{|xUnit2024:Assert.False(charValue != '5')|};
					{|xUnit2024:Assert.False('5' == charValue)|};
					{|xUnit2024:Assert.False('5' != charValue)|};

					var intValue = 5;

					{|xUnit2024:Assert.True(intValue == 5)|};
					{|xUnit2024:Assert.True(intValue != 5)|};
					{|xUnit2024:Assert.True(5 == intValue)|};
					{|xUnit2024:Assert.True(5 != intValue)|};

					{|xUnit2024:Assert.False(intValue == 5)|};
					{|xUnit2024:Assert.False(intValue != 5)|};
					{|xUnit2024:Assert.False(5 == intValue)|};
					{|xUnit2024:Assert.False(5 != intValue)|};

					var enumValue = MyEnum.Bacon;

					{|xUnit2024:Assert.True(enumValue == MyEnum.Bacon)|};
					{|xUnit2024:Assert.True(enumValue != MyEnum.Bacon)|};
					{|xUnit2024:Assert.True(MyEnum.Bacon == enumValue)|};
					{|xUnit2024:Assert.True(MyEnum.Bacon != enumValue)|};

					{|xUnit2024:Assert.False(enumValue == MyEnum.Bacon)|};
					{|xUnit2024:Assert.False(enumValue != MyEnum.Bacon)|};
					{|xUnit2024:Assert.False(MyEnum.Bacon == enumValue)|};
					{|xUnit2024:Assert.False(MyEnum.Bacon != enumValue)|};

					string? nullStringValue = null;

					{|xUnit2024:Assert.True(nullStringValue == null)|};
					{|xUnit2024:Assert.True(nullStringValue != null)|};
					{|xUnit2024:Assert.True(null == nullStringValue)|};
					{|xUnit2024:Assert.True(null != nullStringValue)|};

					{|xUnit2024:Assert.False(nullStringValue == null)|};
					{|xUnit2024:Assert.False(nullStringValue != null)|};
					{|xUnit2024:Assert.False(null == nullStringValue)|};
					{|xUnit2024:Assert.False(null != nullStringValue)|};

					int? nullIntValue = null;

					{|xUnit2024:Assert.True(nullIntValue == null)|};
					{|xUnit2024:Assert.True(nullIntValue != null)|};
					{|xUnit2024:Assert.True(null == nullIntValue)|};
					{|xUnit2024:Assert.True(null != nullIntValue)|};

					{|xUnit2024:Assert.False(nullIntValue == null)|};
					{|xUnit2024:Assert.False(nullIntValue != null)|};
					{|xUnit2024:Assert.False(null == nullIntValue)|};
					{|xUnit2024:Assert.False(null != nullIntValue)|};
				}
			}

			enum MyEnum { None, Bacon, Veggie }
			""";
		var after = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			class TestClass {
				void TestMethod() {
					var stringValue = "bacon";

					Assert.Equal("bacon", stringValue);
					Assert.NotEqual("bacon", stringValue);
					Assert.Equal("bacon", stringValue);
					Assert.NotEqual("bacon", stringValue);

					Assert.NotEqual("bacon", stringValue);
					Assert.Equal("bacon", stringValue);
					Assert.NotEqual("bacon", stringValue);
					Assert.Equal("bacon", stringValue);

					var charValue = '5';

					Assert.Equal('5', charValue);
					Assert.NotEqual('5', charValue);
					Assert.Equal('5', charValue);
					Assert.NotEqual('5', charValue);

					Assert.NotEqual('5', charValue);
					Assert.Equal('5', charValue);
					Assert.NotEqual('5', charValue);
					Assert.Equal('5', charValue);

					var intValue = 5;

					Assert.Equal(5, intValue);
					Assert.NotEqual(5, intValue);
					Assert.Equal(5, intValue);
					Assert.NotEqual(5, intValue);

					Assert.NotEqual(5, intValue);
					Assert.Equal(5, intValue);
					Assert.NotEqual(5, intValue);
					Assert.Equal(5, intValue);

					var enumValue = MyEnum.Bacon;

					Assert.Equal(MyEnum.Bacon, enumValue);
					Assert.NotEqual(MyEnum.Bacon, enumValue);
					Assert.Equal(MyEnum.Bacon, enumValue);
					Assert.NotEqual(MyEnum.Bacon, enumValue);

					Assert.NotEqual(MyEnum.Bacon, enumValue);
					Assert.Equal(MyEnum.Bacon, enumValue);
					Assert.NotEqual(MyEnum.Bacon, enumValue);
					Assert.Equal(MyEnum.Bacon, enumValue);

					string? nullStringValue = null;

					Assert.Null(nullStringValue);
					Assert.NotNull(nullStringValue);
					Assert.Null(nullStringValue);
					Assert.NotNull(nullStringValue);

					Assert.NotNull(nullStringValue);
					Assert.Null(nullStringValue);
					Assert.NotNull(nullStringValue);
					Assert.Null(nullStringValue);

					int? nullIntValue = null;

					Assert.Null(nullIntValue);
					Assert.NotNull(nullIntValue);
					Assert.Null(nullIntValue);
					Assert.NotNull(nullIntValue);

					Assert.NotNull(nullIntValue);
					Assert.Null(nullIntValue);
					Assert.NotNull(nullIntValue);
					Assert.Null(nullIntValue);
				}
			}

			enum MyEnum { None, Bacon, Veggie }
			""";

		await Verify.VerifyCodeFixFixAll(LanguageVersion.CSharp8, before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}
}
