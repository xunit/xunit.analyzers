using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMemberTests
{
	public class X1014_MemberDataShouldUseNameOfOperator
	{
		static readonly string sharedCode = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public partial class TestClass {
			    public static TheoryData<int> Data { get; set; }
			}

			public class OtherClass {
			    public static TheoryData<int> OtherData { get; set; }
			}
			""";

		[Fact]
		public async Task NameofOnSameClass_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public partial class TestClass {
				    [Xunit.MemberData(nameof(Data))]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer([source, sharedCode]);
		}

		[Fact]
		public async Task NameofOnOtherClass_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public partial class TestClass {
				    [Xunit.MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer([source, sharedCode]);
		}

		[Fact]
		public async Task StringNameOnSameClass_Triggers()
		{
			var source = /* lang=c#-test */ """
				public partial class TestClass {
				    [Xunit.MemberData({|#0:"Data"|})]
				    public void TestMethod(int _) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1014").WithLocation(0).WithArguments("Data", "TestClass");

			await Verify.VerifyAnalyzer([source, sharedCode], expected);
		}

		[Fact]
		public async Task StringNameOnOtherClass_Triggers()
		{
			var source = /* lang=c#-test */ """
				public partial class TestClass {
				    [Xunit.MemberData({|#0:"OtherData"|}, MemberType = typeof(OtherClass))]
				    public void TestMethod(int _) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1014").WithLocation(0).WithArguments("OtherData", "OtherClass");

			await Verify.VerifyAnalyzer([source, sharedCode], expected);
		}
	}

	public class X1015_MemberDataMustReferenceExistingMember
	{
		[Theory]
		[InlineData(/* lang=c#-test */ "")]
		[InlineData(/* lang=c#-test */ ", MemberType = typeof(TestClass)")]
		public async Task InvalidStringNameOnSameClass_Triggers(string memberType)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    [{{|#0:Xunit.MemberData("BogusName"{0})|}}]
				    public void TestMethod() {{ }}
				}}
				""", memberType);
			var expected = Verify.Diagnostic("xUnit1015").WithLocation(0).WithArguments("BogusName", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task InvalidStringNameOnOtherClass_Triggers()
		{
			var source1 = /* lang=c#-test */ """
				public class TestClass {
				    [{|#0:Xunit.MemberData("BogusName", MemberType = typeof(OtherClass))|}]
				    public void TestMethod() { }
				}
				""";
			var source2 = /* lang=c#-test */ """
				public class OtherClass { }
				""";
			var expected = Verify.Diagnostic("xUnit1015").WithLocation(0).WithArguments("BogusName", "OtherClass");

			await Verify.VerifyAnalyzer([source1, source2], expected);
		}

		[Fact]
		public async Task InvalidNameofOnOtherClass_Triggers()
		{
			var source1 = /* lang=c#-test */ """
				public class TestClass {
				    [{|#0:Xunit.MemberData(nameof(TestClass.TestMethod), MemberType = typeof(OtherClass))|}]
				    public void TestMethod() { }
				}
				""";
			var source2 = /* lang=c#-test */ """
				public class OtherClass { }
				""";
			var expected = Verify.Diagnostic("xUnit1015").WithLocation(0).WithArguments("TestMethod", "OtherClass");

			await Verify.VerifyAnalyzer([source1, source2], expected);
		}
	}

	public class X1016_MemberDataMustReferencePublicMember
	{
		[Fact]
		public async Task PublicMember_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public static Xunit.TheoryData<int> Data = null;

				    [Xunit.MemberData(nameof(Data))]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		public static MatrixTheoryData<string, string> NonPublicTestData =>
			new(
				/* lang=c#-test */ ["", "private", "protected", "internal", "protected internal"],
				/* lang=c#-test */ ["{|xUnit1014:\"Data\"|}", "DataNameConst", "DataNameofConst", "nameof(Data)", "nameof(TestClass.Data)", "OtherClass.Data", "nameof(OtherClass.Data)"]
			);

		[Theory]
		[MemberData(nameof(NonPublicTestData))]
		public async Task NonPublicNameExpression_Triggers(
			string accessModifier,
			string dataNameExpression)
		{
			TestFileMarkupParser.GetPositionsAndSpans(dataNameExpression, out var parsedDataNameExpression, out _, out _);
			var dataNameExpressionLength = parsedDataNameExpression.Length;

			var source1 = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    const string DataNameConst = "Data";
				    const string DataNameofConst = nameof(Data);

				    {0} static Xunit.TheoryData<int> Data = null;

				    [{{|xUnit1016:Xunit.MemberData({1})|}}]
				    public void TestMethod(int _) {{ }}
				}}
				""", accessModifier, dataNameExpression);
			var source2 = /* lang=c#-test */ """
				public static class OtherClass { public const string Data = "Data"; }
				""";

			await Verify.VerifyAnalyzer([source1, source2]);
		}
	}

	public class X1017_MemberDataMustReferenceStaticMember
	{
		[Fact]
		public async Task StaticMember_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public static Xunit.TheoryData<int> Data = null;

				    [Xunit.MemberData(nameof(Data))]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task InstanceMember_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public Xunit.TheoryData<int> Data = null;

				    [{|xUnit1017:Xunit.MemberData(nameof(Data))|}]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1018_MemberDataMustReferenceValidMemberKind
	{
		[Theory]
		[InlineData(/* lang=c#-test */ "Data;")]
		[InlineData(/* lang=c#-test */ "Data { get; set; }")]
		[InlineData(/* lang=c#-test */ "Data() { return null; }")]
		public async Task ValidMemberKind_DoesNotTrigger(string member)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    public static Xunit.TheoryData<int> {0}

				    [Xunit.MemberData(nameof(Data))]
				    public void TestMethod(int _) {{ }}
				}}
				""", member);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "public delegate System.Collections.Generic.IEnumerable<object[]> Data();")]
		[InlineData(/* lang=c#-test */ "public static class Data { }")]
		[InlineData(/* lang=c#-test */ "public static event System.EventHandler Data;")]
		public async Task InvalidMemberKind_Triggers(string member)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    {0}

				    [{{|xUnit1018:Xunit.MemberData(nameof(Data))|}}]
				    public void TestMethod() {{ }}
				}}
				""", member);

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1019_MemberDataMustReferenceMemberOfValidType
	{
		// The base type of IEnumerable<object[]> and IEnumerable<ITheoryDataRow> trigger xUnit1042,
		// which is covered below in X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis,
		// so we'll only test TheoryData<> and IEnumerable<TheoryDataRow<>> here.

		[Fact]
		public async Task TheoryData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
				    public static TheoryData<int> Data;

				    [MemberData(nameof(Data))]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "Task")]
		[InlineData(/* lang=c#-test */ "ValueTask")]
		public async Task Async_TheoryData_TriggersInV2_DoesNotTriggerInV3(string taskType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    public static {0}<TheoryData<int>> Data;

				    [{{|#0:MemberData(nameof(Data))|}}]
				    public void TestMethod(int _) {{ }}
				}}
				""", taskType);
			var expectedV2 = Verify.Diagnostic("xUnit1019").WithLocation(0).WithArguments("'System.Collections.Generic.IEnumerable<object[]>'", $"System.Threading.Tasks.{taskType}<Xunit.TheoryData<int>>");

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, source, expectedV2);
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "List<TheoryDataRow<int, string>>")]
		[InlineData(/* lang=c#-test */ "IAsyncEnumerable<TheoryDataRow<int, string>>")]
		public async Task GenericTheoryDataRow_DoesNotTrigger(string dataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using System.Collections.Generic;
				using Xunit;
				using Xunit.Sdk;

				public class TestClass {{
				    [Theory]
				    [MemberData(nameof(DataRowSource))]
				    public void SkippedDataRow(int x, string y) {{ }}

				    public static {0} DataRowSource() {{
				        throw new NotImplementedException();
				    }}
				}}
				""", dataType);

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "Task<List<TheoryDataRow<int, string>>>")]
		[InlineData(/* lang=c#-test */ "ValueTask<IAsyncEnumerable<TheoryDataRow<int, string>>>")]
		public async Task Async_GenericTheoryDataRow_DoesNotTrigger(string taskType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using System.Collections.Generic;
				using System.Threading.Tasks;
				using Xunit;
				using Xunit.Sdk;

				public class TestClass {{
				    [Theory]
				    [MemberData(nameof(DataRowSource))]
				    public void SkippedDataRow(int x, string y) {{ }}

				    public static async {0} DataRowSource() {{
				        throw new NotImplementedException();
				    }}
				}}
				""", taskType);

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "System.Collections.Generic.IEnumerable<object>")]
		[InlineData(/* lang=c#-test */ "object[]")]
		[InlineData(/* lang=c#-test */ "object")]
		[InlineData(/* lang=c#-test */ "System.Tuple<string, int>")]
		[InlineData(/* lang=c#-test */ "System.Tuple<string, int>[]")]
		public async Task InvalidMemberType_Triggers(string memberType)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    public static {0} Data;

				    [{{|#0:Xunit.MemberData(nameof(Data))|}}]
				    public void TestMethod() {{ }}
				}}
				""", memberType);
			var expectedV2 = Verify.Diagnostic("xUnit1019").WithLocation(0).WithArguments("'System.Collections.Generic.IEnumerable<object[]>'", memberType);
			var expectedV3 = Verify.Diagnostic("xUnit1019").WithLocation(0).WithArguments("'System.Collections.Generic.IEnumerable<object[]>', 'System.Collections.Generic.IAsyncEnumerable<object[]>', 'System.Collections.Generic.IEnumerable<Xunit.ITheoryDataRow>', or 'System.Collections.Generic.IAsyncEnumerable<Xunit.ITheoryDataRow>'", memberType);

			await Verify.VerifyAnalyzerV2(source, expectedV2);
			await Verify.VerifyAnalyzerV3(source, expectedV3);
		}
	}

	public class X1020_MemberDataPropertyMustHaveGetter
	{
		[Fact]
		public async Task PropertyWithoutGetter_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public static Xunit.TheoryData<int> Data { set { } }

				    [{|xUnit1020:Xunit.MemberData(nameof(Data))|}]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "internal")]
		[InlineData(/* lang=c#-test */ "protected")]
		[InlineData(/* lang=c#-test */ "private")]
		public async Task PropertyWithNonPublicGetter_Triggers(string visibility)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    public static Xunit.TheoryData<int> Data {{ {0} get {{ return null; }} set {{ }} }}

				    [{{|xUnit1020:Xunit.MemberData(nameof(Data))|}}]
				    public void TestMethod(int _) {{ }}
				}}
				""", visibility);

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1021_MemberDataNonMethodShouldNotHaveParameters
	{
		[Theory]
		[InlineData(/* lang=c#-test */ "1")]                   // implicit params
		[InlineData(/* lang=c#-test */ "new object[] { 1 }")]  // explicit params
		public async Task MethodMemberWithParameters_DoesNotTrigger(string parameter)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    private static void TestData() {{ }}

				    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

				    [Xunit.MemberData(nameof(TestData), {0})]
				    public void TestMethod(int n) {{ }}
				}}
				""", parameter);
			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "1, 2")]                   // implicit params
		[InlineData(/* lang=c#-test */ "new object[] { 1, 2 }")]  // explicit params
		public async Task MethodMemberWithParamsArrayParameters_DoesNotTrigger(string parameters)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    public static Xunit.TheoryData<int> TestData(params int[] n) => new Xunit.TheoryData<int> {{ n[0] }};

				    [Xunit.MemberData(nameof(TestData), {0})]
				    public void TestMethod(int n) {{ }}
				}}
				""", parameters);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "1")]                   // implicit params
		[InlineData(/* lang=c#-test */ "new object[] { 1 }")]  // explicit params
		public async Task MethodMemberOnBaseType_DoesNotTrigger(string parameter)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClassBase {{
				    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};
				}}

				public class TestClass : TestClassBase {{
				    private static void TestData() {{ }}

				    [Xunit.MemberData(nameof(TestData), {0})]
				    public void TestMethod(int n) {{ }}
				}}
				""", parameter);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "'a', 123")]
		[InlineData(/* lang=c#-test */ "new object[] {{ 'a', 123 }}")]
		[InlineData(/* lang=c#-test */ "{0}: new object[] {{ 'a', 123 }}")]
		public async Task FieldMemberWithParameters_Triggers(string paramsArgument)
		{
			var sourceTemplate = /* lang=c#-test */ """
				public class TestClass {{
				    public static Xunit.TheoryData<int> Data;

				    [Xunit.MemberData(nameof(Data), {{|xUnit1021:{0}|}}, MemberType = typeof(TestClass))]
				    public void TestMethod(int _) {{ }}
				}}
				""";

			await Verify.VerifyAnalyzerV2(string.Format(sourceTemplate, string.Format(paramsArgument, "parameters")));
			await Verify.VerifyAnalyzerV3(string.Format(sourceTemplate, string.Format(paramsArgument, "arguments")));
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "'a', 123")]
		[InlineData(/* lang=c#-test */ "new object[] {{ 'a', 123 }}")]
		[InlineData(/* lang=c#-test */ "{0}: new object[] {{ 'a', 123 }}")]
		public async Task PropertyMemberWithParameters_Triggers(string paramsArgument)
		{
			var sourceTemplate = /* lang=c#-test */ """
				public class TestClass {{
				    public static Xunit.TheoryData<int> Data {{ get; set; }}

				    [Xunit.MemberData(nameof(Data), {{|xUnit1021:{0}|}}, MemberType = typeof(TestClass))]
				    public void TestMethod(int _) {{ }}
				}}
				""";

			await Verify.VerifyAnalyzerV2(string.Format(sourceTemplate, string.Format(paramsArgument, "parameters")));
			await Verify.VerifyAnalyzerV3(string.Format(sourceTemplate, string.Format(paramsArgument, "arguments")));
		}
	}

	public class X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter
	{
		[Theory]
		[InlineData(/* lang=c#-test */ "", "string")]
		[InlineData(/* lang=c#-test */ "#nullable enable", "string?")]
		public async Task PassingNullForNullableReferenceType_DoesNotTrigger(
			string header,
			string argumentType)
		{
			var source = string.Format(/* lang=c#-test */ """
				{0}

				public class TestClass {{
				    public static Xunit.TheoryData<int> TestData({1} f) => new Xunit.TheoryData<int> {{ 42 }};

				    [Xunit.MemberData(nameof(TestData), new object[] {{ null }})]
				    public void TestMethod(int _) {{ }}
				}}
				""", header, argumentType);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task PassingNullForStructType_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> { n };

				    [Xunit.MemberData(nameof(TestData), new object[] { {|#0:null|} })]
				    public void TestMethod(int _) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1034").WithLocation(0).WithArguments("n", "int");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task PassingNullForNonNullableReferenceType_Triggers()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				public class TestClass {
				    public static Xunit.TheoryData<string> TestData(string f) => new Xunit.TheoryData<string> { f };

				    [Xunit.MemberData(nameof(TestData), new object[] { {|#0:null|} })]
				    public void TestMethod(string _) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1034").WithLocation(0).WithArguments("f", "string");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X1035_MemberDataArgumentsMustMatchMethodParameters_IncompatibleValueType
	{
		// https://github.com/xunit/xunit/issues/2817
		[Theory]
		[InlineData(/* lang=c#-test */ "Foo.Bar")]
		[InlineData(/* lang=c#-test */ "(Foo)42")]
		public async Task ValidEnumValue_DoesNotTrigger(string enumValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    [Theory]
				    [MemberData(nameof(SomeData), {0})]
				    public void TestMethod(int _) {{ }}

				    public enum Foo {{ Bar }}

				    public static Xunit.TheoryData<int> SomeData(Foo foo) => new Xunit.TheoryData<int>();
				}}
				""", enumValue);

			await Verify.VerifyAnalyzer(source);
		}

		// https://github.com/xunit/xunit/issues/2852
		[Theory]
		[InlineData(/* lang=c#-test */ "")]
		[InlineData(/* lang=c#-test */ "#nullable enable")]
		public async Task ArrayInitializerWithCorrectType_DoesNotTrigger(string header)
		{
			var source = string.Format(/* lang=c#-test */ """
				{0}

				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static TheoryData<int> GetSequences(IEnumerable<int> seq) => new TheoryData<int> {{ 42, 2112 }};

				    [Theory]
				    [MemberData(nameof(GetSequences), new int[] {{ 1, 2 }})]
				    public void Test(int value) {{ }}
				}}
				""", header);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "")]
		[InlineData(/* lang=c#-test */ "#nullable enable")]
		public async Task ArrayInitializerWithIncorrectType_Triggers(string header)
		{
			var source = string.Format(/* lang=c#-test */ """
				{0}

				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static TheoryData<int> GetSequences(IEnumerable<int> seq) => new TheoryData<int> {{ 42, 2112 }};

				    [Theory]
				    [MemberData(nameof(GetSequences), {{|#0:new char[] {{ 'a', 'b' }}|}})]
				    public void Test(int value) {{ }}
				}}
				""", header);
			var expected = Verify.Diagnostic("xUnit1035").WithLocation(0).WithArguments("seq", "System.Collections.Generic.IEnumerable<int>");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		[Fact]
		public async Task ValidMemberWithIncorrectArgumentTypes_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public static Xunit.TheoryData<int> TestData(string n) => new Xunit.TheoryData<int> { n.Length };

				    [Xunit.MemberData(nameof(TestData), new object[] { {|#0:1|} })]
				    public void TestMethod(int n) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1035").WithLocation(0).WithArguments("n", "string");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task ValidMemberWithIncorrectArgumentTypesParams_Triggers()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public static Xunit.TheoryData<int> TestData(params int[] n) => new Xunit.TheoryData<int> { n[0] };

				    [Xunit.MemberData(nameof(TestData), new object[] { 1, {|#0:"bob"|} })]
				    public void TestMethod(int n) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1035").WithLocation(0).WithArguments("n", "int");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue
	{
		[Theory]
		[InlineData(/* lang=c#-test */ "1")]
		[InlineData(/* lang=c#-test */ "new object[] { 1 }")]
		public async Task ValidArgumentCount_DoesNotTrigger(string parameter)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    private static void TestData() {{ }}

				    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

				    [Xunit.MemberData(nameof(TestData), {0})]
				    public void TestMethod(int n) {{ }}
				}}
				""", parameter);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "1")]
		[InlineData(/* lang=c#-test */ "new object[] { 1 }")]
		public async Task ValidArgumentCount_InNullableContext_DoesNotTrigger(string parameter)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				public class TestClass {{
				    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

				    [Xunit.MemberData(nameof(TestData), {0})]
				    public void TestMethod(int n) {{ }}
				}}
				""", parameter);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "1, {|#0:2|}")]
		[InlineData(/* lang=c#-test */ "new object[] { 1, {|#0:2|} }")]
		public async Task TooManyArguments_Triggers(string parameters)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

				    [Xunit.MemberData(nameof(TestData), {0})]
				    public void TestMethod(int n) {{ }}
				}}
				""", parameters);
			var expected = Verify.Diagnostic("xUnit1036").WithLocation(0).WithArguments("2");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters
	{
		public static TheoryData<string, string> MemberSyntaxAndArgs = new()
		{
			/* lang=c#-test */ { " = ", "" },              // Field
			/* lang=c#-test */ { " => ", "" },             // Property
			/* lang=c#-test */ { "() => ", "" },           // Method w/o args
			/* lang=c#-test */ { "(int n) => ", ", 42" },  // Method w/ args
		};

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class TestClass {{
				    public static TheoryData<int> TestData{0}new TheoryData<int>();

				    [{{|#0:MemberData(nameof(TestData){1})|}}]
				    public void TestMethod(int n, string f) {{ }}
				}}
				""", memberSyntax, memberArgs);
			var expected = Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static IEnumerable<TheoryDataRow<int>> TestData{0}null;

				    [{{|#0:MemberData(nameof(TestData){1})|}}]
				    public void TestMethod(int n, string f) {{ }}
				}}
				""", memberSyntax, memberArgs);
			var expected = Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidSubclassedTheoryDataMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData<T, U> : TheoryData<T> {{ }}

				public class TestClass {{
				    public static DerivedTheoryData<int, string> TestData{0}new DerivedTheoryData<int, string>();

				    [{{|#0:MemberData(nameof(TestData){1})|}}]
				    public void TestMethod(int n, string f) {{ }}
				}}
				""", memberSyntax, memberArgs);
			var expected = Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters
	{
		public static TheoryData<string, string> MemberSyntaxAndArgs() => new()
		{
			{ " = ", "" },              // Field
			{ " => ", "" },             // Property
			{ "() => ", "" },           // Method w/o args
			{ "(int n) => ", ", 42" },  // Method w/ args
		};

		public static MatrixTheoryData<(string syntax, string args), string> MemberSyntaxAndArgs_WithTheoryDataType(string theoryDataTypes) => new(
			[
				(" = ", ""),              // Field
				(" => ", ""),             // Property
				("() => ", ""),           // Method w/o args
				("(int n) => ", ", 42"),  // Method w/ args
			],
			[
				$"TheoryData<{theoryDataTypes}>",
				"DerivedTheoryData",
				$"DerivedTheoryData<{theoryDataTypes}>"
			]
		);

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryData_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData : TheoryData<int> {{ }}
				public class DerivedTheoryData<T> : TheoryData<T> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRow_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static IEnumerable<TheoryDataRow<int>> TestData{0}new List<TheoryDataRow<int>>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n) {{ }}
				}}
				""", memberSyntax, memberArgs);

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithOptionalParameters_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData : TheoryData<int> {{ }}
				public class DerivedTheoryData<T> : TheoryData<T> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n, int a = 0) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithOptionalParameters_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static TheoryDataRow<int>[] TestData{0}new TheoryDataRow<int>[0];

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n, int a = 0) {{ }}
				}}
				""", memberSyntax, memberArgs);

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithNoValuesForParamsArray_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData : TheoryData<int> {{ }}
				public class DerivedTheoryData<T> : TheoryData<T> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n, params int[] a) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithNoValuesForParamsArray_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static ICollection<TheoryDataRow<int>> TestData{0}new List<TheoryDataRow<int>>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n, params int[] a) {{ }}
				}}
				""", memberSyntax, memberArgs);

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithSingleValueForParamsArray_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData : TheoryData<int, int> {{ }}
				public class DerivedTheoryData<T1, T2> : TheoryData<T1, T2> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n, params int[] a) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithSingleValueForParamsArray_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static IEnumerable<TheoryDataRow<int, int>> TestData{0}new List<TheoryDataRow<int, int>>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n, params int[] a) {{ }}
				}}
				""", memberSyntax, memberArgs);

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithGenericTestParameter_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData : TheoryData<int> {{ }}
				public class DerivedTheoryData<T> : TheoryData<T> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod<T>(T n) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithGenericTestParameter_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static ISet<TheoryDataRow<int>> TestData{0}new HashSet<TheoryDataRow<int>>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod<T>(T n) {{ }}
				}}
				""", memberSyntax, memberArgs);

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithNullableGenericTestParameter_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				using Xunit;

				public class DerivedTheoryData : TheoryData<int> {{ }}
				public class DerivedTheoryData<T> : TheoryData<T> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [Xunit.MemberData(nameof(TestData){1})]
				    public void TestMethod<T>(T? n) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithNullableGenericTestParameter_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static IEnumerable<TheoryDataRow<int>> TestData{0}new List<TheoryDataRow<int>>();

				    [Xunit.MemberData(nameof(TestData){1})]
				    public void TestMethod<T>(T? n) {{ }}
				}}
				""", memberSyntax, memberArgs);

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataDoubleGenericSubclassMember_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData<T, U> : TheoryData<T> {{ }}

				public class TestClass {{
				    public static DerivedTheoryData<int, string> TestData{0}new DerivedTheoryData<int, string>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod(int n) {{ }}
				}}
				""", memberSyntax, memberArgs);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WithIntArrayArguments_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
				    public static TheoryData<IEnumerable<int>> GetSequences(IEnumerable<int> seq) => new TheoryData<IEnumerable<int>> { seq };

				    [Theory]
				    [MemberData(nameof(GetSequences), new[] { 1, 2 })]
				    [MemberData(nameof(GetSequences), new[] { 3, 4, 5 })]
				    public void Test(IEnumerable<int> seq) {
				        Assert.NotEmpty(seq);
				    }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, string", DisableDiscoveryEnumeration = true)]
		public async Task ValidSubclassTheoryDataMemberWithTooManyTypeParameters_Triggers(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData : TheoryData<int, string> {{ }}
				public class DerivedTheoryData<T1, T2> : TheoryData<T1, T2> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [{{|#0:MemberData(nameof(TestData){1})|}}]
				    public void TestMethod(int n) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);
			var expected = Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidSubclassTheoryDataRowMemberWithTooManyTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static IEnumerable<TheoryDataRow<int, string>> TestData{0}new List<TheoryDataRow<int, string>>();

				    [{{|#0:MemberData(nameof(TestData){1})|}}]
				    public void TestMethod(int n) {{ }}
				}}
				""", memberSyntax, memberArgs);
			var expected = Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, string[], string", DisableDiscoveryEnumeration = true)]
		public async Task ExtraTheoryDataTypeExistsPastArrayForParamsArray_Triggers(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData : TheoryData<int, string[], string> {{ }}
				public class DerivedTheoryData<T1, T2, T3> : TheoryData<T1, T2, T3> {{ }}

				public class TestClass {{
				    public static {2} TestData{0}new {2}();

				    [{{|#0:MemberData(nameof(TestData){1})|}}]
				    public void PuzzleOne(int _1, params string[] _2) {{ }}
				}}
				""", member.syntax, member.args, theoryDataType);
			var expected = Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ExtraTheoryDataRowTypeExistsPastArrayForParamsArray_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static ICollection<TheoryDataRow<int, string[], string>> TestData{0}new TheoryDataRow<int, string[], string>[0];

				    [{{|#0:MemberData(nameof(TestData){1})|}}]
				    public void PuzzleOne(int _1, params string[] _2) {{ }}
				}}
				""", memberSyntax, memberArgs);
			var expected = Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(source, expected);
		}
	}

	public class X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes
	{
		public static MatrixTheoryData<(string syntax, string args), string> TypeWithMemberSyntaxAndArgs = new(
			[
				(" = ", ""),              // Field
				(" => ", ""),             // Property
				("() => ", ""),           // Method w/o args
				("(int n) => ", ", 42"),  // Method w/ args
			],
			[
				"int",
				"System.Exception",
			]
		);

		[Fact]
		public async Task WhenPassingMultipleValuesForParamsArray_TheoryData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int, string, string> TestData = new TheoryData<int, string, string>();

				    [MemberData(nameof(TestData))]
				    public void PuzzleOne(int _1, params string[] _2) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WhenPassingMultipleValuesForParamsArray_TheoryDataRow_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static IEnumerable<TheoryDataRow<int, string, string>> TestData = new List<TheoryDataRow<int, string, string>>();

				    [MemberData(nameof(TestData))]
				    public void PuzzleOne(int _1, params string[] _2) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async Task WhenPassingArrayForParamsArray_TheoryData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int, string[]> TestData = new TheoryData<int, string[]>();

				    [MemberData(nameof(TestData))]
				    public void PuzzleOne(int _1, params string[] _2) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WhenPassingArrayForParamsArray_TheoryDataRow_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryDataRow<int, string[]>[] TestData = new TheoryDataRow<int, string[]>[0];

				    [MemberData(nameof(TestData))]
				    public void PuzzleOne(int _1, params string[] _2) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async Task WhenPassingTupleWithoutFieldNames_TheoryData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<(int, int)> TestData = new TheoryData<(int, int)>();

				    [MemberData(nameof(TestData))]
				    public void TestMethod((int a, int b) x) { }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task WhenPassingTupleWithoutFieldNames_TheoryDataRow_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static IList<TheoryDataRow<(int, int)>> TestData = new List<TheoryDataRow<(int, int)>>();

				    [MemberData(nameof(TestData))]
				    public void TestMethod((int a, int b) x) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task WhenPassingTupleWithDifferentFieldNames_TheoryData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<(int c, int d)> TestData = new TheoryData<(int, int)>();

				    [MemberData(nameof(TestData))]
				    public void TestMethod((int a, int b) x) { }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task WhenPassingTupleWithDifferentFieldNames_TheoryDataRow_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static IEnumerable<TheoryDataRow<(int c, int d)>> TestData = new List<TheoryDataRow<(int, int)>>();

				    [MemberData(nameof(TestData))]
				    public void TestMethod((int a, int b) x) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}

		// https://github.com/xunit/xunit/issues/3007
		[Theory]
		[InlineData("T[]")]
		[InlineData("IEnumerable<T>")]
		public async Task WhenPassingArrayToGenericTheory_TheoryData_DoesNotTrigger(string enumerable)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
					public static TheoryData<object[]> TestData = new TheoryData<object[]>();
		
					[MemberData(nameof(TestData))]
					public void PuzzleOne<T>({0} _1) {{ }}
				}}
				""", enumerable);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WithExtraValueNotCompatibleWithParamsArray_TheoryData_Triggers()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int, string, int> TestData = new TheoryData<int, string, int>();

				    [MemberData(nameof(TestData))]
				    public void PuzzleOne(int _1, params {|#0:string[]|} _2) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.TestData", "_2");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task WithExtraValueNotCompatibleWithParamsArray_TheoryDataRow_Triggers()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static IEnumerable<TheoryDataRow<int, string, int>> TestData = new List<TheoryDataRow<int, string, int>>();

				    [MemberData(nameof(TestData))]
				    public void PuzzleOne(int _1, params {|#0:string[]|} _2) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.TestData", "_2");

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(TypeWithMemberSyntaxAndArgs), DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataMemberWithIncompatibleTypeParameters_Triggers(
			(string syntax, string args) member,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class TestClass {{
				    public static TheoryData<{2}> TestData{0}new TheoryData<{2}>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod({{|#0:string|}} f) {{ }}
				}}
				""", member.syntax, member.args, type);
			var expected = Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments(type, "TestClass.TestData", "f");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(TypeWithMemberSyntaxAndArgs), DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataRowMemberWithIncompatibleTypeParameters_Triggers(
			(string syntax, string args) member,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static IList<TheoryDataRow<{2}>> TestData{0}new List<TheoryDataRow<{2}>>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod({{|#0:string|}} f) {{ }}
				}}
				""", member.syntax, member.args, type);
			var expected = Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments(type, "TestClass.TestData", "f");

			await Verify.VerifyAnalyzerV3(source, expected);
		}
	}

	public class X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
	{
		public static TheoryData<string, string> MemberSyntaxAndArgs = new()
		{
			/* lang=c#-test */ { " = ", "" },              // Field
			/* lang=c#-test */ { " => ", "" },             // Property
			/* lang=c#-test */ { "() => ", "" },           // Method w/o args
			/* lang=c#-test */ { "(int n) => ", ", 42" },  // Method w/ args
		};

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataMemberWithMismatchedNullability_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				using Xunit;

				public class TestClass {{
				    public static TheoryData<string?> TestData{0}new TheoryData<string?>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod({{|#0:string|}} f) {{ }}
				}}
				""", memberSyntax, memberArgs);
			var expected = Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.TestData", "f");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowMemberWithMismatchedNullability_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static IEnumerable<TheoryDataRow<string?>> TestData{0}new List<TheoryDataRow<string?>>();

				    [MemberData(nameof(TestData){1})]
				    public void TestMethod({{|#0:string|}} f) {{ }}
				}}
				""", memberSyntax, memberArgs);
			var expected = Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.TestData", "f");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis
	{
		[Fact]
		public async Task TheoryData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
				    public static TheoryData<int> Data;

				    [MemberData(nameof(Data))]
				    public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MatrixTheoryData_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
				    public static MatrixTheoryData<int, string> Data;

				    [MemberData(nameof(Data))]
				    public void TestMethod(int _1, string _2) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[InlineData(/* lang=c#-test */ "IEnumerable<TheoryDataRow<int>>")]
		[InlineData(/* lang=c#-test */ "IAsyncEnumerable<TheoryDataRow<int>>")]
		[InlineData(/* lang=c#-test */ "List<TheoryDataRow<int>>")]
		[InlineData(/* lang=c#-test */ "TheoryDataRow<int>[]")]
		public async Task GenericTheoryDataRow_DoesNotTrigger(string memberType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;
				using Xunit.v3;

				public class TestClass {{
				    public static {0} Data;

				    [MemberData(nameof(Data))]
				    public void TestMethod(int _) {{ }}
				}}
				""", memberType);

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[InlineData("IEnumerable<object[]>")]
		[InlineData("List<object[]>")]
		public async Task ValidTypesWhichAreNotTheoryData_Trigger(string memberType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    public static {0} Data;

				    [{{|#0:MemberData(nameof(Data))|}}]
				    public void TestMethod(int _) {{ }}
				}}
				""", memberType);
			var expectedV2 = Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments("TheoryData<>");
			var expectedV3 = Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments("TheoryData<> or IEnumerable<TheoryDataRow<>>");

			await Verify.VerifyAnalyzerV2(source, expectedV2);
			await Verify.VerifyAnalyzerV3(source, expectedV3);
		}

		// For v2, we test for xUnit1019 above, since it's incompatible rather than "compatible,
		// but you could do better".
		[Theory]
		[InlineData(/* lang=c#-test */ "IAsyncEnumerable<object[]>")]
		[InlineData(/* lang=c#-test */ "Task<IEnumerable<object[]>>")]
		[InlineData(/* lang=c#-test */ "ValueTask<List<object[]>>")]
		[InlineData(/* lang=c#-test */ "IEnumerable<TheoryDataRow>")]
		[InlineData(/* lang=c#-test */ "IAsyncEnumerable<TheoryDataRow>")]
		[InlineData(/* lang=c#-test */ "Task<IEnumerable<TheoryDataRow>>")]
		[InlineData(/* lang=c#-test */ "Task<IAsyncEnumerable<TheoryDataRow>>")]
		[InlineData(/* lang=c#-test */ "ValueTask<List<TheoryDataRow>>")]
		[InlineData(/* lang=c#-test */ "IEnumerable<ITheoryDataRow>")]
		[InlineData(/* lang=c#-test */ "Task<IEnumerable<ITheoryDataRow>>")]
		[InlineData(/* lang=c#-test */ "Task<IAsyncEnumerable<ITheoryDataRow>>")]
		[InlineData(/* lang=c#-test */ "ValueTask<EnumerableOfITheoryDataRow>")]
		public async Task ValidTypesWhichAreNotTheoryDataOrGenericTheoryDataRow_TriggersInV3(string memberType)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Collections;
				using System.Collections.Generic;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    public static {0} Data;

				    [{{|#0:MemberData(nameof(Data))|}}]
				    public void TestMethod(int _) {{ }}
				}}

				public class EnumerableOfITheoryDataRow : IEnumerable<ITheoryDataRow> {{
				    public IEnumerator<ITheoryDataRow> GetEnumerator() => null;
				    IEnumerator IEnumerable.GetEnumerator() => null;
				}}
				""", memberType);
			var expected = Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments("TheoryData<> or IEnumerable<TheoryDataRow<>>");

			await Verify.VerifyAnalyzerV3(source, expected);
		}
	}
}
