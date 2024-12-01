using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataShouldBeUniqueWithinTheory>;

public abstract class InlineDataShouldBeUniqueWithinTheoryTests
{
	public class ForNonRelatedToInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
	{
		[Fact]
		public async Task WithNoDataAttributes_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Fact]
					public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("MemberData(\"\")")]
		[InlineData("ClassData(typeof(string))")]
		public async Task WithDataAttributesOtherThanInline_DoesNotTrigger(
			string dataAttribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.{0}]
					public void TestMethod() {{ }}
				}}
				""", dataAttribute);

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class ForUniqueInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
	{
		[Fact]
		public async Task NonTheory_SingleInlineData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Fact]
					[Xunit.InlineData]
					public void TestMethod(int x) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task NonTheory_DoubledInlineData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Fact]
					[Xunit.InlineData]
					[Xunit.InlineData]
					public void TestMethod(int x) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task SingleInlineData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(10)]
					public void TestMethod(int x) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MultipleInlineData_DifferentValues_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(10)]
					[Xunit.InlineData(20)]
					public void TestMethod(int x) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "new object[] { 1, 3 }")]
		[InlineData(/* lang=c#-test */ "data: new object[] { 1, 3 }")]
		[InlineData(/* lang=c#-test */ "new object[] { }")]
		[InlineData(/* lang=c#-test */ "data: new object[] { 1 }")]
		public async Task UniqueValues_WithParamsInitializerValues_DoesNotTrigger(string data)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData(1, 2)]
					[Xunit.InlineData({0})]
					public void TestMethod(params int[] args) {{ }}
				}}
				""", data);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UniqueValues_WithOverridingDefaultValues_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(1)]
					[Xunit.InlineData(1, "non-default-val")]
					public void TestMethod(int x, string a = "default-val") { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task NullAndEmpty_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(null)]
					[Xunit.InlineData]
					public void TestMethod(string s) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task NullAndArray_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass{
					[Xunit.Theory]
					[Xunit.InlineData(new[] { 0 })]
					[Xunit.InlineData(null)]
					public void TestMethod(int[] arr) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task ArrayOrderVariance_DoesNotTrigger()
		{
			// Specially crafted InlineData values that will cause the InlineDataUniquenessComparer
			// to return same hashcodes, because GetFlattenedArgumentPrimitives ignores empty arrays.
			// This will trigger the actual bug, where the first parameter object array being equal
			// would cause the other parameters to not be evaluated for equality at all.
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(new int[] { 1 }, new int[0], new int[] { 1 })]
					[Xunit.InlineData(new int[] { 1 }, new int[] { 1 }, new int[0])]
					public static void Test(int[] x, int[] y, int[] z) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class ForDuplicatedInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
	{
		[Fact]
		public async Task DoubleEmptyInlineData_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData]
					[{|#0:Xunit.InlineData|}]
					public void TestMethod(int x) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task DoubleNullInlineData_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(null)]
					[{|#0:Xunit.InlineData(null)|}]
					public void TestMethod(string x) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task DoubleValues_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(10)]
					[{|#0:Xunit.InlineData(10)|}]
					public void TestMethod(int x) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task ValueFromConstant_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					private const int X = 10;

					[Xunit.Theory]
					[Xunit.InlineData(10)]
					[{|#0:Xunit.InlineData(X)|}]
					public void TestMethod(int x) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "new object[] { 10, 20 }")]
		[InlineData(/* lang=c#-test */ "data: new object[] { 10, 20 }")]
		public async Task TwoParams_RawValuesVsArgumentArray_Triggers(string data)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData(10, 20)]
					[{{|#0:Xunit.InlineData({0})|}}]
					public void TestMethod(int x, int y) {{ }}
				}}
				""", data);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "new object[] { 10, 20 }")]
		[InlineData(/* lang=c#-test */ "data: new object[] { 10, 20 }")]
		public async Task ParamsArray_RawValuesVsArgumentArray_Triggers(string data)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData(10, 20)]
					[{{|#0:Xunit.InlineData({0})|}}]
					public void TestMethod(params int[] args) {{ }}
				}}
				""", data);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task DoubledArgumentArrays_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(new object[] { 10, 20 })]
					[{|#0:Xunit.InlineData(new object[] { 10, 20 })|}]
					public void TestMethod(int x, int y) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task DoubledComplexValuesForObject_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]
					[{|#0:Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})|}]
					public void TestMethod(object x, object y) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task DoubledComplexValues_RawValuesVsArgumentArray_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(10, new object[] { new object[] {20}, 30}, 40)]
					[{|#0:Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})|}]
					public void TestMethod(object x, object y, int z = 40) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		// The value 1 doesn't seem to trigger bugs related to comparing boxed values, but 2 does
		public static TheoryData<int> DefaultValueData = [1, 2];

		[Theory]
		[MemberData(nameof(DefaultValueData))]
		public async Task DefaultValueVsExplicitValue_Triggers(int defaultValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData]
					[{{|#0:Xunit.InlineData({0})|}}]
					public void TestMethod(int y = {0}) {{ }}
				}}
				""", defaultValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(DefaultValueData))]
		public async Task ExplicitValueVsDefaultValue_Triggers(int defaultValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData({0})]
					[{{|#0:Xunit.InlineData|}}]
					public void TestMethod(int y = {0}) {{ }}
				}}
				""", defaultValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(DefaultValueData))]
		public async Task DefaultValueVsDefaultValue_Triggers(int defaultValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData]
					[{{|#0:Xunit.InlineData|}}]
					public void TestMethod(int y = {0}) {{ }}
				}}
				""", defaultValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("null", "null")]
		[InlineData("null", "")]
		[InlineData("", "null")]
		[InlineData("", "")]
		public async Task DefaultValueVsNull_Triggers(
			string firstArg,
			string secondArg)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData({0})]
					[{{|#0:Xunit.InlineData({1})|}}]
					public void TestMethod(string x = null) {{ }}
				}}
				""", firstArg, secondArg);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task Null_RawValuesVsExplicitArray_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(1, null)]
					[{|#0:Xunit.InlineData(new object[] { 1, null })|}]
					public void TestMethod(object x, object y) { }
				}
				""";
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("", "")]
		[InlineData("", ", default")]
		[InlineData(", default", "")]
		[InlineData(", default", ", default")]
		public async Task DefaultOfStruct_Triggers(
			string firstDefaultOverride,
			string secondDefaultOverride)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData(1{0})]
					[{{|#0:Xunit.InlineData(1{1})|}}]
					public void TestMethod(int x, System.DateTime date = default) {{ }}
				}}
				""", firstDefaultOverride, secondDefaultOverride);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7_1, source, expected);
		}

		[Theory]
		[InlineData("", "")]
		[InlineData("", ", null")]
		[InlineData(", null", "")]
		[InlineData(", null", ", null")]
		public async Task DefaultOfString_Triggers(
			string firstDefaultOverride,
			string secondDefaultOverride)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData(1{0})]
					[{{|#0:Xunit.InlineData(1{1})|}}]
					public void TestMethod(int x, string y = null) {{ }}
				}}
				""", firstDefaultOverride, secondDefaultOverride);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task Tripled_TriggersTwice()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(10)]
					[{|#0:Xunit.InlineData(10)|}]
					[{|#1:Xunit.InlineData(10)|}]
					public void TestMethod(int x) { }
				}
				""";
			var expected = new[]
			{
				Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass"),
				Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "TestClass"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task DoubledTwice_TriggersTwice()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
					[Xunit.Theory]
					[Xunit.InlineData(10)]
					[Xunit.InlineData(20)]
					[{|#0:Xunit.InlineData(10)|}]
					[{|#1:Xunit.InlineData(20)|}]
					public void TestMethod(int x) { }
				}
				""";
			var expected = new[]
			{
				Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass"),
				Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "TestClass"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
