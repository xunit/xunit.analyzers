using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMemberTests
{
	public class X1014_MemberDataShouldUseNameOfOperator
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1053
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static TheoryData<int> Data { get; set; }

					[MemberData(nameof(Data))]
					[MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))]
					public void TestMethod1(int _) { }

					[MemberData({|#0:"Data"|})]
					[MemberData({|#1:"OtherData"|}, MemberType = typeof(OtherClass))]
					public void TestMethod2(int _) { }
				}

				public class OtherClass {
					public static TheoryData<int> OtherData { get; set; }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1014").WithLocation(0).WithArguments("Data", "TestClass"),
				Verify.Diagnostic("xUnit1014").WithLocation(1).WithArguments("OtherData", "OtherClass"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1015_MemberDataMustReferenceExistingMember
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source1 = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					[{|#0:MemberData("BogusName")|}]
					public void TestMethod1() { }

					[{|#1:MemberData("BogusName", MemberType = typeof(TestClass))|}]
					public void TestMethod2() { }

					[{|#2:MemberData("BogusName", MemberType = typeof(OtherClass))|}]
					public void TestMethod3() { }

					[{|#3:MemberData(nameof(TestClass.TestMethod4), MemberType = typeof(OtherClass))|}]
					public void TestMethod4() { }
				}
				""";
			var source2 = /* lang=c#-test */ "public class OtherClass { }";
			var expected = new[] {
				Verify.Diagnostic("xUnit1015").WithLocation(0).WithArguments("BogusName", "TestClass"),
				Verify.Diagnostic("xUnit1015").WithLocation(1).WithArguments("BogusName", "TestClass"),
				Verify.Diagnostic("xUnit1015").WithLocation(2).WithArguments("BogusName", "OtherClass"),
				Verify.Diagnostic("xUnit1015").WithLocation(3).WithArguments("TestMethod4", "OtherClass"),
			};

			await Verify.VerifyAnalyzer([source1, source2], expected);
		}
	}

	public class X1016_MemberDataMustReferencePublicMember
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int> PublicData = null;

					[MemberData(nameof(PublicData))]
					public void TestMethod1a(int _) { }

					const string PrivateDataNameConst = "PrivateData";
					const string PrivateDataNameofConst = nameof(PrivateData);
					private static TheoryData<int> PrivateData = null;

					[{|xUnit1016:MemberData(nameof(PrivateData))|}]
					public void TestMethod2a(int _) { }

					[{|xUnit1016:MemberData(PrivateDataNameConst)|}]
					public void TestMethod2b(int _) { }

					[{|xUnit1016:MemberData(PrivateDataNameofConst)|}]
					public void TestMethod2c(int _) { }

					internal static TheoryData<int> InternalData = null;

					[{|xUnit1016:MemberData(nameof(InternalData))|}]
					public void TestMethod3(int _) { }

					protected static TheoryData<int> ProtectedData = null;

					[{|xUnit1016:MemberData(nameof(ProtectedData))|}]
					public void TestMethod4(int _) { }

					protected internal static TheoryData<int> ProtectedInternalData = null;

					[{|xUnit1016:MemberData(nameof(ProtectedInternalData))|}]
					public void TestMethod5(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1017_MemberDataMustReferenceStaticMember
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int> StaticData = null;
					public TheoryData<int> NonStaticData = null;

					[MemberData(nameof(StaticData))]
					[{|xUnit1017:MemberData(nameof(NonStaticData))|}]
					public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1018_MemberDataMustReferenceValidMemberKind
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1053
				using System;
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static TheoryData<int> FieldData;
					public static TheoryData<int> PropertyData { get; set; }
					public static TheoryData<int> MethodData() { return null; }

					public static class ClassData { }
					public delegate IEnumerable<object[]> DelegateData();
					public static event EventHandler EventData;

					[MemberData(nameof(FieldData))]
					public void TestMethod1(int _) { }

					[MemberData(nameof(PropertyData))]
					public void TestMethod2(int _) { }

					[MemberData(nameof(MethodData))]
					public void TestMethod3(int _) { }

					[{|xUnit1018:MemberData(nameof(ClassData))|}]
					public void TestMethod4(int _) { }

					[{|xUnit1018:MemberData(nameof(DelegateData))|}]
					public void TestMethod5(int _) { }

					[{|xUnit1018:MemberData(nameof(EventData))|}]
					public void TestMethod6(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1019_MemberDataMustReferenceMemberOfValidType
	{
		const string V2AllowedTypes = "'System.Collections.Generic.IEnumerable<object[]>'";
		const string V3AllowedTypes = "'System.Collections.Generic.IEnumerable<object[]>', 'System.Collections.Generic.IAsyncEnumerable<object[]>', 'System.Collections.Generic.IEnumerable<Xunit.ITheoryDataRow>', 'System.Collections.Generic.IAsyncEnumerable<Xunit.ITheoryDataRow>', 'System.Collections.Generic.IEnumerable<System.Runtime.CompilerServices.ITuple>', or 'System.Collections.Generic.IAsyncEnumerable<System.Runtime.CompilerServices.ITuple>'";

		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1042
				#pragma warning disable xUnit1053

				using System;
				using System.Collections.Generic;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {
					public static IEnumerable<object> ObjectSource;
					public static object NakedObjectSource;
					public static object[] NakedObjectArraySource;

					public static IEnumerable<object[]> ObjectArraySource;
					public static Task<IEnumerable<object[]>> TaskObjectArraySource;
					public static ValueTask<IEnumerable<object[]>> ValueTaskObjectArraySource;

					public static IAsyncEnumerable<object[]> AsyncObjectArraySource;
					public static Task<IAsyncEnumerable<object[]>> TaskAsyncObjectArraySource;
					public static ValueTask<IAsyncEnumerable<object[]>> ValueTaskAsyncObjectArraySource;

					public static TheoryData<string, int> TheoryDataSource;
					public static Task<TheoryData<string, int>> TaskTheoryDataSource;
					public static ValueTask<TheoryData<string, int>> ValueTaskTheoryDataSource;

					public static IEnumerable<(string, int)> UntypedTupleSource;
					public static Task<IEnumerable<(string, int)>> TaskUntypedTupleSource;
					public static ValueTask<IEnumerable<(string, int)>> ValueTaskUntypedTupleSource;

					public static IAsyncEnumerable<(string, int)> AsyncUntypedTupleSource;
					public static Task<IAsyncEnumerable<(string, int)>> TaskAsyncUntypedTupleSource;
					public static ValueTask<IAsyncEnumerable<(string, int)>> ValueTaskAsyncUntypedTupleSource;

					public static IEnumerable<Tuple<string, int>> TypedTupleSource;
					public static Task<IEnumerable<Tuple<string, int>>> TaskTypedTupleSource;
					public static ValueTask<IEnumerable<Tuple<string, int>>> ValueTaskTypedTupleSource;

					public static IAsyncEnumerable<Tuple<string, int>> AsyncTypedTupleSource;
					public static Task<IAsyncEnumerable<Tuple<string, int>>> TaskAsyncTypedTupleSource;
					public static ValueTask<IAsyncEnumerable<Tuple<string, int>>> ValueTaskAsyncTypedTupleSource;

					[{|#0:MemberData(nameof(ObjectSource))|}]
					[{|#1:MemberData(nameof(NakedObjectSource))|}]
					[{|#2:MemberData(nameof(NakedObjectArraySource))|}]

					[MemberData(nameof(ObjectArraySource))]
					[{|#10:MemberData(nameof(TaskObjectArraySource))|}]
					[{|#11:MemberData(nameof(ValueTaskObjectArraySource))|}]

					[{|#20:MemberData(nameof(AsyncObjectArraySource))|}]
					[{|#21:MemberData(nameof(TaskAsyncObjectArraySource))|}]
					[{|#22:MemberData(nameof(ValueTaskAsyncObjectArraySource))|}]

					[MemberData(nameof(TheoryDataSource))]
					[{|#30:MemberData(nameof(TaskTheoryDataSource))|}]
					[{|#31:MemberData(nameof(ValueTaskTheoryDataSource))|}]

					[{|#40:MemberData(nameof(UntypedTupleSource))|}]
					[{|#41:MemberData(nameof(TaskUntypedTupleSource))|}]
					[{|#42:MemberData(nameof(ValueTaskUntypedTupleSource))|}]

					[{|#50:MemberData(nameof(AsyncUntypedTupleSource))|}]
					[{|#51:MemberData(nameof(TaskAsyncUntypedTupleSource))|}]
					[{|#52:MemberData(nameof(ValueTaskAsyncUntypedTupleSource))|}]

					[{|#60:MemberData(nameof(TypedTupleSource))|}]
					[{|#61:MemberData(nameof(TaskTypedTupleSource))|}]
					[{|#62:MemberData(nameof(ValueTaskTypedTupleSource))|}]

					[{|#70:MemberData(nameof(AsyncTypedTupleSource))|}]
					[{|#71:MemberData(nameof(TaskAsyncTypedTupleSource))|}]
					[{|#72:MemberData(nameof(ValueTaskAsyncTypedTupleSource))|}]

					public void TestMethod(string _1, int _2) { }
				}
				""";
			var expectedV2 = new[] {
				// Generally invalid types
				Verify.Diagnostic("xUnit1019").WithLocation(0).WithArguments(V2AllowedTypes, $"System.Collections.Generic.IEnumerable<object>"),
				Verify.Diagnostic("xUnit1019").WithLocation(1).WithArguments(V2AllowedTypes, $"object"),
				Verify.Diagnostic("xUnit1019").WithLocation(2).WithArguments(V2AllowedTypes, $"object[]"),

				// v2 does not support tuples, wrapping in Task/ValueTask, and does not support IAsyncEnumerable
				Verify.Diagnostic("xUnit1019").WithLocation(10).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<object[]>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(11).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<object[]>>"),

				Verify.Diagnostic("xUnit1019").WithLocation(20).WithArguments(V2AllowedTypes, $"System.Collections.Generic.IAsyncEnumerable<object[]>"),
				Verify.Diagnostic("xUnit1019").WithLocation(21).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.Task<System.Collections.Generic.IAsyncEnumerable<object[]>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(22).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.ValueTask<System.Collections.Generic.IAsyncEnumerable<object[]>>"),

				Verify.Diagnostic("xUnit1019").WithLocation(30).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.Task<Xunit.TheoryData<string, int>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(31).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.ValueTask<Xunit.TheoryData<string, int>>"),

				Verify.Diagnostic("xUnit1019").WithLocation(40).WithArguments(V2AllowedTypes, $"System.Collections.Generic.IEnumerable<(string, int)>"),
				Verify.Diagnostic("xUnit1019").WithLocation(41).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<(string, int)>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(42).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<(string, int)>>"),

				Verify.Diagnostic("xUnit1019").WithLocation(50).WithArguments(V2AllowedTypes, $"System.Collections.Generic.IAsyncEnumerable<(string, int)>"),
				Verify.Diagnostic("xUnit1019").WithLocation(51).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.Task<System.Collections.Generic.IAsyncEnumerable<(string, int)>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(52).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.ValueTask<System.Collections.Generic.IAsyncEnumerable<(string, int)>>"),

				Verify.Diagnostic("xUnit1019").WithLocation(60).WithArguments(V2AllowedTypes, $"System.Collections.Generic.IEnumerable<System.Tuple<string, int>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(61).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<System.Tuple<string, int>>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(62).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<System.Tuple<string, int>>>"),

				Verify.Diagnostic("xUnit1019").WithLocation(70).WithArguments(V2AllowedTypes, $"System.Collections.Generic.IAsyncEnumerable<System.Tuple<string, int>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(71).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.Task<System.Collections.Generic.IAsyncEnumerable<System.Tuple<string, int>>>"),
				Verify.Diagnostic("xUnit1019").WithLocation(72).WithArguments(V2AllowedTypes, $"System.Threading.Tasks.ValueTask<System.Collections.Generic.IAsyncEnumerable<System.Tuple<string, int>>>"),
			};
			var expectedV3 = new[] {
				// Generally invalid types
				Verify.Diagnostic("xUnit1019").WithLocation(0).WithArguments(V3AllowedTypes, $"System.Collections.Generic.IEnumerable<object>"),
				Verify.Diagnostic("xUnit1019").WithLocation(1).WithArguments(V3AllowedTypes, $"object"),
				Verify.Diagnostic("xUnit1019").WithLocation(2).WithArguments(V3AllowedTypes, $"object[]"),
			};

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, source, expectedV2);
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source, expectedV3);
		}

		[Fact]
		public async ValueTask V3_only()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1042
				#pragma warning disable xUnit1053

				using System.Collections.Generic;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {
					public static List<ITheoryDataRow> ITheoryDataRowSource;
					public static Task<List<ITheoryDataRow>> TaskITheoryDataRowSource;
					public static ValueTask<List<ITheoryDataRow>> ValueTaskITheoryDataRowSource;

					public static List<TheoryDataRow<int, string>> TheoryDataRowSource;
					public static Task<List<TheoryDataRow<int, string>>> TaskTheoryDataRowSource;
					public static ValueTask<List<TheoryDataRow<int, string>>> ValueTaskTheoryDataRowSource;

					public static IAsyncEnumerable<ITheoryDataRow> AsyncITheoryDataRowSource;
					public static Task<IAsyncEnumerable<ITheoryDataRow>> TaskAsyncITheoryDataRowSource;
					public static ValueTask<IAsyncEnumerable<ITheoryDataRow>> ValueTaskAsyncITheoryDataRowSource;

					public static IAsyncEnumerable<TheoryDataRow<int, string>> AsyncTheoryDataRowSource;
					public static Task<IAsyncEnumerable<TheoryDataRow<int, string>>> TaskAsyncTheoryDataRowSource;
					public static ValueTask<IAsyncEnumerable<TheoryDataRow<int, string>>> ValueTaskAsyncTheoryDataRowSource;

					[MemberData(nameof(ITheoryDataRowSource))]
					[MemberData(nameof(TaskITheoryDataRowSource))]
					[MemberData(nameof(ValueTaskITheoryDataRowSource))]

					[MemberData(nameof(TheoryDataRowSource))]
					[MemberData(nameof(TaskTheoryDataRowSource))]
					[MemberData(nameof(ValueTaskTheoryDataRowSource))]

					[MemberData(nameof(AsyncITheoryDataRowSource))]
					[MemberData(nameof(TaskAsyncITheoryDataRowSource))]
					[MemberData(nameof(ValueTaskAsyncITheoryDataRowSource))]

					[MemberData(nameof(AsyncTheoryDataRowSource))]
					[MemberData(nameof(TaskAsyncTheoryDataRowSource))]
					[MemberData(nameof(ValueTaskAsyncTheoryDataRowSource))]

					public void TestMethod(int _1, string _2) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}
	}

	public class X1020_MemberDataPropertyMustHaveGetter
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1053
				using Xunit;

				public class TestClass {
					public static TheoryData<int> PublicWithGetter => new();
					public static TheoryData<int> PublicWithoutGetter { set { } }
					public static TheoryData<int> ProtectedGetter { protected get { return null; } set { } }
					public static TheoryData<int> InternalGetter { internal get { return null; } set { } }
					public static TheoryData<int> PrivateGetter { private get { return null; } set { } }

					[MemberData(nameof(PublicWithGetter))]
					[{|xUnit1020:MemberData(nameof(PublicWithoutGetter))|}]
					[{|xUnit1020:MemberData(nameof(ProtectedGetter))|}]
					[{|xUnit1020:MemberData(nameof(InternalGetter))|}]
					[{|xUnit1020:MemberData(nameof(PrivateGetter))|}]
					public void TestMethod(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
		}
	}

	public class X1021_MemberDataNonMethodShouldNotHaveParameters
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1053
				using Xunit;

				public class TestClassBase {
					public static TheoryData<int> BaseTestData(int n) => new TheoryData<int> { n };
				}

				public class TestClass : TestClassBase {
					private static void TestData() { }

					public static TheoryData<int> SingleData(int n) => new TheoryData<int> { n };

					[MemberData(nameof(SingleData), 1)]
					[MemberData(nameof(SingleData), new object[] { 1 })]
					public void TestMethod1(int n) { }

					public static TheoryData<int> ParamsData(params int[] n) => new TheoryData<int> { n[0] };

					[MemberData(nameof(ParamsData), 1, 2)]
					[MemberData(nameof(ParamsData), new object[] { 1, 2 })]
					public void TestMethod2(int n) { }

					[MemberData(nameof(BaseTestData), 1)]
					[MemberData(nameof(BaseTestData), new object[] { 1 })]
					public void TestMethod3(int n) { }

					public static TheoryData<int> FieldData;

					[MemberData(nameof(FieldData), {|xUnit1021:'a', 123|})]
					public void TestMethod4a(int _) { }

					[MemberData(nameof(FieldData), {|xUnit1021:new object[] { 'a', 123 }|})]
					public void TestMethod4b(int _) { }

					public static TheoryData<int> PropertyData { get; set; }

					[MemberData(nameof(PropertyData), {|xUnit1021:'a', 123|})]
					public void TestMethod5a(int _) { }

					[MemberData(nameof(PropertyData), {|xUnit1021:new object[] { 'a', 123 }|})]
					public void TestMethod5b(int _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				using Xunit;

				public class TestClass {
					public static TheoryData<string?> NullableReferenceData(string? s) => new TheoryData<string?> { s };

					[MemberData(nameof(NullableReferenceData), default(string))]
					public void TestMethod1(string? _) { }

					public static TheoryData<string> NonNullableReferenceData(string s) => new TheoryData<string> { s };

					[MemberData(nameof(NonNullableReferenceData), {|#0:default(string)|})]
					public void TestMethod(string _) { }

				#nullable disable
					public static TheoryData<string> MaybeNullableReferenceData(string s) => new TheoryData<string> { s };
				#nullable enable

					[MemberData(nameof(MaybeNullableReferenceData), default(string))]
					public void TestMethod3(string? _) { }

					public static TheoryData<int?> NullableStructData(int? n) => new TheoryData<int?> { n };

					[MemberData(nameof(NullableStructData), new object[] { null })]
					public void TestMethod4(int? _) { }

					public static TheoryData<int> NonNullableStructData(int n) => new TheoryData<int> { n };

					[MemberData(nameof(NonNullableStructData), new object[] { {|#1:null|} })]
					public void TestMethod5(int _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1034").WithLocation(0).WithArguments("s", "string"),
				Verify.Diagnostic("xUnit1034").WithLocation(1).WithArguments("n", "int"),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X1035_MemberDataArgumentsMustMatchMethodParameters_IncompatibleValueType
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using System;
				using System.Collections.Generic;
				using Xunit;

				public enum Foo { Bar }

				public class TestClass {
					public static TheoryData<int> StringData(string s) => new TheoryData<int> { s.Length };

					[MemberData(nameof(StringData), {|#0:1|})]
					public void TestMethod1(int _) { }

					public static TheoryData<int> ParamsIntData(params int[] n) => new TheoryData<int> { n[0] };

					[MemberData(nameof(ParamsIntData), {|#1:"bob"|})]
					public void TestMethod2(int _) { }

					// https://github.com/xunit/xunit/issues/2817
					public static TheoryData<int> EnumData(Foo foo) => new TheoryData<int> { (int)foo };

					[Theory]
					[MemberData(nameof(EnumData), Foo.Bar)]
					[MemberData(nameof(EnumData), (Foo)42)]
					public void TestMethod3(int _) { }

					// https://github.com/xunit/xunit/issues/2852
					public static TheoryData<int> IntegerSequenceData(IEnumerable<int> seq) => new TheoryData<int> { 42, 2112 };

					[Theory]
					[MemberData(nameof(IntegerSequenceData), new int[] { 1, 2 })]
					[MemberData(nameof(IntegerSequenceData), {|#2:new char[] { 'a', 'b' }|})]
					public void TestMethod4(int _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1035").WithLocation(0).WithArguments("s", "string"),
				Verify.Diagnostic("xUnit1035").WithLocation(1).WithArguments("n", "int"),
				Verify.Diagnostic("xUnit1035").WithLocation(2).WithArguments("seq", "System.Collections.Generic.IEnumerable<int>"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int> TestData(int n) => new TheoryData<int> { n };

					[MemberData(nameof(TestData), 1)]
					public void TestMethod1(int _) { }

					[MemberData(nameof(TestData), new object[] { 1 })]
					public void TestMethod2(int _) { }

					[MemberData(nameof(TestData), 1, {|#0:2|})]
					public void TestMethod3(int _) { }

					[MemberData(nameof(TestData), new object[] { 1, {|#1:2|} })]
					public void TestMethod4(int _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1036").WithLocation(0).WithArguments("2"),
				Verify.Diagnostic("xUnit1036").WithLocation(1).WithArguments("2"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class DerivedTheoryData<T, U> : TheoryData<T> { }

				public class TestClass {
					public static TheoryData<int> FieldData = new TheoryData<int>();
					public static TheoryData<int> PropertyData => new TheoryData<int>();
					public static TheoryData<int> MethodData() => new TheoryData<int>();
					public static TheoryData<int> MethodDataWithArgs(int n) => new TheoryData<int>();

					[{|#0:MemberData(nameof(FieldData))|}]
					[{|#1:MemberData(nameof(PropertyData))|}]
					[{|#2:MemberData(nameof(MethodData))|}]
					[{|#3:MemberData(nameof(MethodDataWithArgs), 42)|}]
					public void TestMethod1(int n, string f) { }

					public static DerivedTheoryData<int, string> DerivedFieldData = new DerivedTheoryData<int, string>();
					public static DerivedTheoryData<int, string> DerivedPropertyData => new DerivedTheoryData<int, string>();
					public static DerivedTheoryData<int, string> DerivedMethodData() => new DerivedTheoryData<int, string>();
					public static DerivedTheoryData<int, string> DerivedMethodDataWithArgs(int n) => new DerivedTheoryData<int, string>();

					[{|#10:MemberData(nameof(DerivedFieldData))|}]
					[{|#11:MemberData(nameof(DerivedPropertyData))|}]
					[{|#12:MemberData(nameof(DerivedMethodData))|}]
					[{|#13:MemberData(nameof(DerivedMethodDataWithArgs), 42)|}]
					public void TestMethod3(int n, string f) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1037").WithLocation(1).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1037").WithLocation(2).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1037").WithLocation(3).WithArguments("Xunit.TheoryData"),

				Verify.Diagnostic("xUnit1037").WithLocation(10).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1037").WithLocation(11).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1037").WithLocation(12).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1037").WithLocation(13).WithArguments("Xunit.TheoryData"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async ValueTask V3_only()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static IEnumerable<TheoryDataRow<int>> NullFieldData = null;
					public static IEnumerable<TheoryDataRow<int>> NullPropertyData => null;
					public static IEnumerable<TheoryDataRow<int>> NullMethodData() => null;
					public static IEnumerable<TheoryDataRow<int>> NullMethodDataWithArgs(int n) => null;

					[{|#0:MemberData(nameof(NullFieldData))|}]
					[{|#1:MemberData(nameof(NullPropertyData))|}]
					[{|#2:MemberData(nameof(NullMethodData))|}]
					[{|#3:MemberData(nameof(NullMethodDataWithArgs), 42)|}]
					public void TestMethod2(int n, string f) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1037").WithLocation(1).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1037").WithLocation(2).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1037").WithLocation(3).WithArguments("Xunit.TheoryDataRow"),
			};

			await Verify.VerifyAnalyzerV3(source, expected);
		}
	}

	public class X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				using Xunit;

				public class DerivedTheoryData : TheoryData<int> { }
				public class DerivedTheoryData<T> : TheoryData<T> { }
				public class DerivedTheoryData<T, U> : TheoryData<T> { }

				public class DerivedTheoryData2 : TheoryData<int, int> { }
				public class DerivedTheoryData2<T1, T2> : TheoryData<T1, T2> { }

				public class DerivedTheoryData3 : TheoryData<int, string[], string> { }
				public class DerivedTheoryData3<T1, T2, T3> : TheoryData<T1, T2, T3> { }

				public class TestClass {
					// ===== Direct TheoryData<> usage =====

					public static TheoryData<int> FieldTheoryData = new TheoryData<int>();
					public static TheoryData<int> PropertyTheoryData => new TheoryData<int>();
					public static TheoryData<int> MethodTheoryData() => new TheoryData<int>();
					public static TheoryData<int> MethodWithArgsTheoryData(int _) => new TheoryData<int>();

					// Exact match
					[MemberData(nameof(FieldTheoryData))]
					[MemberData(nameof(PropertyTheoryData))]
					[MemberData(nameof(MethodTheoryData))]
					[MemberData(nameof(MethodWithArgsTheoryData), 42)]
					public void TestMethod1a(int _) { }

					// Optional paramter, no argument from data source
					[MemberData(nameof(FieldTheoryData))]
					[MemberData(nameof(PropertyTheoryData))]
					[MemberData(nameof(MethodTheoryData))]
					[MemberData(nameof(MethodWithArgsTheoryData), 42)]
					public void TestMethod1b(int _1, int _2 = 0) { }

					// Params array, no argument from data source
					[MemberData(nameof(FieldTheoryData))]
					[MemberData(nameof(PropertyTheoryData))]
					[MemberData(nameof(MethodTheoryData))]
					[MemberData(nameof(MethodWithArgsTheoryData), 42)]
					public void TestMethod1c(int _1, params int[] _2) { }

					// Generic match
					[MemberData(nameof(FieldTheoryData))]
					[MemberData(nameof(PropertyTheoryData))]
					[MemberData(nameof(MethodTheoryData))]
					[MemberData(nameof(MethodWithArgsTheoryData), 42)]
					public void TestMethod1d<T>(T _) { }

					// Generic nullable match
					[MemberData(nameof(FieldTheoryData))]
					[MemberData(nameof(PropertyTheoryData))]
					[MemberData(nameof(MethodTheoryData))]
					[MemberData(nameof(MethodWithArgsTheoryData), 42)]
					public void TestMethod1e<T>(T? _) { }

					public static TheoryData<int, int> FieldTheoryData2 = new TheoryData<int, int>();
					public static TheoryData<int, int> PropertyTheoryData2 => new TheoryData<int, int>();
					public static TheoryData<int, int> MethodTheoryData2() => new TheoryData<int, int>();
					public static TheoryData<int, int> MethodWithArgsTheoryData2(int _) => new TheoryData<int, int>();

					// Params array, single non-array argument from data source
					[MemberData(nameof(FieldTheoryData2))]
					[MemberData(nameof(PropertyTheoryData2))]
					[MemberData(nameof(MethodTheoryData2))]
					[MemberData(nameof(MethodWithArgsTheoryData2), 42)]
					public void TestMethod1f(int _1, params int[] _2) { }

					// Too many arguments
					[{|#0:MemberData(nameof(FieldTheoryData2))|}]
					[{|#1:MemberData(nameof(PropertyTheoryData2))|}]
					[{|#2:MemberData(nameof(MethodTheoryData2))|}]
					[{|#3:MemberData(nameof(MethodWithArgsTheoryData2), 42)|}]
					public void TestMethod1g(int _) { }

					public static TheoryData<int, string[], string> FieldTheoryData3 = new TheoryData<int, string[], string>();
					public static TheoryData<int, string[], string> PropertyTheoryData3 => new TheoryData<int, string[], string>();
					public static TheoryData<int, string[], string> MethodTheoryData3() => new TheoryData<int, string[], string>();
					public static TheoryData<int, string[], string> MethodWithArgsTheoryData3(int _) => new TheoryData<int, string[], string>();

					// Extra parameter type on data source
					[{|#4:MemberData(nameof(FieldTheoryData3))|}]
					[{|#5:MemberData(nameof(PropertyTheoryData3))|}]
					[{|#6:MemberData(nameof(MethodTheoryData3))|}]
					[{|#7:MemberData(nameof(MethodWithArgsTheoryData3), 42)|}]
					public void TestMethod1h(int _1, params string[] _2) { }

					// ===== Indirect TheoryData<> without generics =====

					public static DerivedTheoryData FieldDerivedTheoryData = new DerivedTheoryData();
					public static DerivedTheoryData PropertyDerivedTheoryData => new DerivedTheoryData();
					public static DerivedTheoryData MethodDerivedTheoryData() => new DerivedTheoryData();
					public static DerivedTheoryData MethodWithArgsDerivedTheoryData(int _) => new DerivedTheoryData();

					// Exact match
					[MemberData(nameof(FieldDerivedTheoryData))]
					[MemberData(nameof(PropertyDerivedTheoryData))]
					[MemberData(nameof(MethodDerivedTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
					public void TestMethod2a(int _) { }

					// Optional paramter, no argument from data source
					[MemberData(nameof(FieldDerivedTheoryData))]
					[MemberData(nameof(PropertyDerivedTheoryData))]
					[MemberData(nameof(MethodDerivedTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
					public void TestMethod2b(int _1, int _2 = 0) { }

					// Params array, no argument from data source
					[MemberData(nameof(FieldDerivedTheoryData))]
					[MemberData(nameof(PropertyDerivedTheoryData))]
					[MemberData(nameof(MethodDerivedTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
					public void TestMethod2c(int _1, params int[] _2) { }

					// Generic match
					[MemberData(nameof(FieldDerivedTheoryData))]
					[MemberData(nameof(PropertyDerivedTheoryData))]
					[MemberData(nameof(MethodDerivedTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
					public void TestMethod2d<T>(T _) { }

					// Generic nullable match
					[MemberData(nameof(FieldDerivedTheoryData))]
					[MemberData(nameof(PropertyDerivedTheoryData))]
					[MemberData(nameof(MethodDerivedTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
					public void TestMethod2e<T>(T? _) { }

					public static DerivedTheoryData2 FieldDerivedTheoryData2 = new DerivedTheoryData2();
					public static DerivedTheoryData2 PropertyDerivedTheoryData2 => new DerivedTheoryData2();
					public static DerivedTheoryData2 MethodDerivedTheoryData2() => new DerivedTheoryData2();
					public static DerivedTheoryData2 MethodWithArgsDerivedTheoryData2(int _) => new DerivedTheoryData2();

					// Params array, single non-array argument from data source
					[MemberData(nameof(FieldDerivedTheoryData2))]
					[MemberData(nameof(PropertyDerivedTheoryData2))]
					[MemberData(nameof(MethodDerivedTheoryData2))]
					[MemberData(nameof(MethodWithArgsDerivedTheoryData2), 42)]
					public void TestMethod2f(int _1, params int[] _2) { }

					// Too many arguments
					[{|#10:MemberData(nameof(FieldDerivedTheoryData2))|}]
					[{|#11:MemberData(nameof(PropertyDerivedTheoryData2))|}]
					[{|#12:MemberData(nameof(MethodDerivedTheoryData2))|}]
					[{|#13:MemberData(nameof(MethodWithArgsDerivedTheoryData2), 42)|}]
					public void TestMethod2g(int _) { }

					public static DerivedTheoryData3 FieldDerivedTheoryData3 = new DerivedTheoryData3();
					public static DerivedTheoryData3 PropertyDerivedTheoryData3 => new DerivedTheoryData3();
					public static DerivedTheoryData3 MethodDerivedTheoryData3() => new DerivedTheoryData3();
					public static DerivedTheoryData3 MethodWithArgsDerivedTheoryData3(int _) => new DerivedTheoryData3();

					// Extra parameter type on data source
					[{|#14:MemberData(nameof(FieldDerivedTheoryData3))|}]
					[{|#15:MemberData(nameof(PropertyDerivedTheoryData3))|}]
					[{|#16:MemberData(nameof(MethodDerivedTheoryData3))|}]
					[{|#17:MemberData(nameof(MethodWithArgsDerivedTheoryData3), 42)|}]
					public void TestMethod2h(int _1, params string[] _2) { }

					// ===== Indirect TheoryData<> with generics =====

					public static DerivedTheoryData<int> FieldDerivedGenericTheoryData = new DerivedTheoryData<int>();
					public static DerivedTheoryData<int> PropertyDerivedGenericTheoryData => new DerivedTheoryData<int>();
					public static DerivedTheoryData<int> MethodDerivedGenericTheoryData() => new DerivedTheoryData<int>();
					public static DerivedTheoryData<int> MethodWithArgsDerivedGenericTheoryData(int _) => new DerivedTheoryData<int>();

					// Exact match
					[MemberData(nameof(FieldDerivedGenericTheoryData))]
					[MemberData(nameof(PropertyDerivedGenericTheoryData))]
					[MemberData(nameof(MethodDerivedGenericTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
					public void TestMethod3a(int _) { }

					// Optional paramter, no argument from data source
					[MemberData(nameof(FieldDerivedGenericTheoryData))]
					[MemberData(nameof(PropertyDerivedGenericTheoryData))]
					[MemberData(nameof(MethodDerivedGenericTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
					public void TestMethod3b(int _1, int _2 = 0) { }

					// Params array, no argument from data source
					[MemberData(nameof(FieldDerivedGenericTheoryData))]
					[MemberData(nameof(PropertyDerivedGenericTheoryData))]
					[MemberData(nameof(MethodDerivedGenericTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
					public void TestMethod3c(int _1, params int[] _2) { }

					// Generic match
					[MemberData(nameof(FieldDerivedGenericTheoryData))]
					[MemberData(nameof(PropertyDerivedGenericTheoryData))]
					[MemberData(nameof(MethodDerivedGenericTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
					public void TestMethod3d<T>(T _) { }

					// Generic nullable match
					[MemberData(nameof(FieldDerivedGenericTheoryData))]
					[MemberData(nameof(PropertyDerivedGenericTheoryData))]
					[MemberData(nameof(MethodDerivedGenericTheoryData))]
					[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
					public void TestMethod3e<T>(T? _) { }

					public static DerivedTheoryData2<int, int> FieldDerivedGenericTheoryData2 = new DerivedTheoryData2<int, int>();
					public static DerivedTheoryData2<int, int> PropertyDerivedGenericTheoryData2 => new DerivedTheoryData2<int, int>();
					public static DerivedTheoryData2<int, int> MethodDerivedGenericTheoryData2() => new DerivedTheoryData2<int, int>();
					public static DerivedTheoryData2<int, int> MethodWithArgsDerivedGenericTheoryData2(int _) => new DerivedTheoryData2<int, int>();

					// Params array, single non-array argument from data source
					[MemberData(nameof(FieldDerivedGenericTheoryData2))]
					[MemberData(nameof(PropertyDerivedGenericTheoryData2))]
					[MemberData(nameof(MethodDerivedGenericTheoryData2))]
					[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData2), 42)]
					public void TestMethod3f(int _1, params int[] _2) { }

					// Too many arguments
					[{|#20:MemberData(nameof(FieldDerivedGenericTheoryData2))|}]
					[{|#21:MemberData(nameof(PropertyDerivedGenericTheoryData2))|}]
					[{|#22:MemberData(nameof(MethodDerivedGenericTheoryData2))|}]
					[{|#23:MemberData(nameof(MethodWithArgsDerivedGenericTheoryData2), 42)|}]
					public void TestMethod3g(int _) { }

					public static DerivedTheoryData3<int, string[], string> FieldDerivedGenericTheoryData3 = new DerivedTheoryData3<int, string[], string>();
					public static DerivedTheoryData3<int, string[], string> PropertyDerivedGenericTheoryData3 => new DerivedTheoryData3<int, string[], string>();
					public static DerivedTheoryData3<int, string[], string> MethodDerivedGenericTheoryData3() => new DerivedTheoryData3<int, string[], string>();
					public static DerivedTheoryData3<int, string[], string> MethodWithArgsDerivedGenericTheoryData3(int _) => new DerivedTheoryData3<int, string[], string>();

					// Extra parameter type on data source
					[{|#24:MemberData(nameof(FieldDerivedGenericTheoryData3))|}]
					[{|#25:MemberData(nameof(PropertyDerivedGenericTheoryData3))|}]
					[{|#26:MemberData(nameof(MethodDerivedGenericTheoryData3))|}]
					[{|#27:MemberData(nameof(MethodWithArgsDerivedGenericTheoryData3), 42)|}]
					public void TestMethod3h(int _1, params string[] _2) { }

					// ===== Indirect TheoryData<> with generic type reduction =====

					public static DerivedTheoryData<int, string> FieldTheoryDataTypeReduced = new DerivedTheoryData<int, string>();
					public static DerivedTheoryData<int, string> PropertyTheoryDataTypeReduced => new DerivedTheoryData<int, string>();
					public static DerivedTheoryData<int, string> MethodTheoryDataTypeReduced() => new DerivedTheoryData<int, string>();
					public static DerivedTheoryData<int, string> MethodWithArgsTheoryDataTypeReduced(int _) => new DerivedTheoryData<int, string>();

					// Exact match
					[MemberData(nameof(FieldTheoryDataTypeReduced))]
					[MemberData(nameof(PropertyTheoryDataTypeReduced))]
					[MemberData(nameof(MethodTheoryDataTypeReduced))]
					[MemberData(nameof(MethodWithArgsTheoryDataTypeReduced), 42)]
					public void TestMethod4a(int _) { }

					// Generic match
					[MemberData(nameof(FieldTheoryDataTypeReduced))]
					[MemberData(nameof(PropertyTheoryDataTypeReduced))]
					[MemberData(nameof(MethodTheoryDataTypeReduced))]
					[MemberData(nameof(MethodWithArgsTheoryDataTypeReduced), 42)]
					public void TestMethod4d<T>(T _) { }

					// Generic nullable match
					[MemberData(nameof(FieldTheoryDataTypeReduced))]
					[MemberData(nameof(PropertyTheoryDataTypeReduced))]
					[MemberData(nameof(MethodTheoryDataTypeReduced))]
					[MemberData(nameof(MethodWithArgsTheoryDataTypeReduced), 42)]
					public void TestMethod4e<T>(T? _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(1).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(2).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(3).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(4).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(5).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(6).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(7).WithArguments("Xunit.TheoryData"),

				Verify.Diagnostic("xUnit1038").WithLocation(10).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(11).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(12).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(13).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(14).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(15).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(16).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(17).WithArguments("Xunit.TheoryData"),

				Verify.Diagnostic("xUnit1038").WithLocation(20).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(21).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(22).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(23).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(24).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(25).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(26).WithArguments("Xunit.TheoryData"),
				Verify.Diagnostic("xUnit1038").WithLocation(27).WithArguments("Xunit.TheoryData"),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source, expected);
		}

		[Fact]
		public async ValueTask V3_only()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static TheoryDataRow<int>[] FieldData = new TheoryDataRow<int>[0];
					public static TheoryDataRow<int>[] PropertyData => new TheoryDataRow<int>[0];
					public static TheoryDataRow<int>[] MethodData() => new TheoryDataRow<int>[0];
					public static TheoryDataRow<int>[] MethodWithArgsData(int _) => new TheoryDataRow<int>[0];

					// Exact match
					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1a(int _) { }

					// Optional paramter, no argument from data source
					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1b(int _1, int _2 = 0) { }

					// Params array, no argument from data source
					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1c(int _1, params int[] _2) { }

					// Generic match
					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1d<T>(T _) { }

					// Generic nullable match
					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1e<T>(T? _) { }

					public static TheoryDataRow<int, int>[] FieldData2 = new TheoryDataRow<int, int>[0];
					public static TheoryDataRow<int, int>[] PropertyData2 => new TheoryDataRow<int, int>[0];
					public static TheoryDataRow<int, int>[] MethodData2() => new TheoryDataRow<int, int>[0];
					public static TheoryDataRow<int, int>[] MethodWithArgsData2(int _) => new TheoryDataRow<int, int>[0];

					// Params array, single non-array argument from data source
					[MemberData(nameof(FieldData2))]
					[MemberData(nameof(PropertyData2))]
					[MemberData(nameof(MethodData2))]
					[MemberData(nameof(MethodWithArgsData2), 42)]
					public void TestMethod1f(int _1, params int[] _2) { }

					// Too many arguments
					[{|#0:MemberData(nameof(FieldData2))|}]
					[{|#1:MemberData(nameof(PropertyData2))|}]
					[{|#2:MemberData(nameof(MethodData2))|}]
					[{|#3:MemberData(nameof(MethodWithArgsData2), 42)|}]
					public void TestMethod1g(int _) { }

					public static TheoryDataRow<int, string[], string>[] FieldData3 = new TheoryDataRow<int, string[], string>[0];
					public static TheoryDataRow<int, string[], string>[] PropertyData3 => new TheoryDataRow<int, string[], string>[0];
					public static TheoryDataRow<int, string[], string>[] MethodData3() => new TheoryDataRow<int, string[], string>[0];
					public static TheoryDataRow<int, string[], string>[] MethodWithArgsData3(int _) => new TheoryDataRow<int, string[], string>[0];

					// Extra parameter type on data source
					[{|#4:MemberData(nameof(FieldData3))|}]
					[{|#5:MemberData(nameof(PropertyData3))|}]
					[{|#6:MemberData(nameof(MethodData3))|}]
					[{|#7:MemberData(nameof(MethodWithArgsData3), 42)|}]
					public void TestMethod1h(int _1, params string[] _2) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1038").WithLocation(1).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1038").WithLocation(2).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1038").WithLocation(3).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1038").WithLocation(4).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1038").WithLocation(5).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1038").WithLocation(6).WithArguments("Xunit.TheoryDataRow"),
				Verify.Diagnostic("xUnit1038").WithLocation(7).WithArguments("Xunit.TheoryDataRow"),
			};

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source, expected);
		}
	}

	public class X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static TheoryData<int, string[]> FieldData = new TheoryData<int, string[]>();
					public static TheoryData<int, string[]> PropertyData => new TheoryData<int, string[]>();
					public static TheoryData<int, string[]> MethodData() => new TheoryData<int, string[]>();
					public static TheoryData<int, string[]> MethodWithArgsData(int _) => new TheoryData<int, string[]>();

					// Exact match
					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1(int _1, params string[] _2) { }

					public static TheoryData<int, string, string> FieldDataCollapse = new TheoryData<int, string, string>();
					public static TheoryData<int, string, string> PropertyDataCollapse => new TheoryData<int, string, string>();
					public static TheoryData<int, string, string> MethodDataCollapse() => new TheoryData<int, string, string>();
					public static TheoryData<int, string, string> MethodWithArgsDataCollapse(int _) => new TheoryData<int, string, string>();

					// Multiple values can be collapsed into the params array
					[MemberData(nameof(FieldDataCollapse))]
					[MemberData(nameof(PropertyDataCollapse))]
					[MemberData(nameof(MethodDataCollapse))]
					[MemberData(nameof(MethodWithArgsDataCollapse), 42)]
					public void TestMethod2(int _1, params string[] _2) { }

					public static TheoryData<(int, int)> FieldNamelessTupleData = new TheoryData<(int, int)>();
					public static TheoryData<(int, int)> PropertyNamelessTupleData => new TheoryData<(int, int)>();
					public static TheoryData<(int, int)> MethodNamelessTupleData() => new TheoryData<(int, int)>();
					public static TheoryData<(int, int)> MethodWithArgsNamelessTupleData(int _) => new TheoryData<(int, int)>();

					// Nameless anonymous tuples
					[MemberData(nameof(FieldNamelessTupleData))]
					[MemberData(nameof(PropertyNamelessTupleData))]
					[MemberData(nameof(MethodNamelessTupleData))]
					[MemberData(nameof(MethodWithArgsNamelessTupleData), 42)]
					public void TestMethod3((int a, int b) _) { }

					public static TheoryData<(int x, int y)> FieldNamedTupleData = new TheoryData<(int x, int y)>();
					public static TheoryData<(int x, int y)> PropertyNamedTupleData => new TheoryData<(int x, int y)>();
					public static TheoryData<(int x, int y)> MethodNamedTupleData() => new TheoryData<(int x, int y)>();
					public static TheoryData<(int x, int y)> MethodWithArgsNamedTupleData(int _) => new TheoryData<(int x, int y)>();

					// Named anonymous tuples (names don't need to match, just the shape)
					[MemberData(nameof(FieldNamedTupleData))]
					[MemberData(nameof(PropertyNamedTupleData))]
					[MemberData(nameof(MethodNamedTupleData))]
					[MemberData(nameof(MethodWithArgsNamedTupleData), 42)]
					public void TestMethod4((int a, int b) _) { }

					public static TheoryData<object[]> FieldArrayData = new TheoryData<object[]>();
					public static TheoryData<object[]> PropertyArrayData => new TheoryData<object[]>();
					public static TheoryData<object[]> MethodArrayData() => new TheoryData<object[]>();
					public static TheoryData<object[]> MethodWithArgsArrayData(int _) => new TheoryData<object[]>();

					// https://github.com/xunit/xunit/issues/3007
					[MemberData(nameof(FieldArrayData))]
					[MemberData(nameof(PropertyArrayData))]
					[MemberData(nameof(MethodArrayData))]
					[MemberData(nameof(MethodWithArgsArrayData), 42)]
					public void TestMethod5a<T>(T[] _1) {{ }}

					[MemberData(nameof(FieldArrayData))]
					[MemberData(nameof(PropertyArrayData))]
					[MemberData(nameof(MethodArrayData))]
					[MemberData(nameof(MethodWithArgsArrayData), 42)]
					public void TestMethod5b<T>(IEnumerable<T> _1) {{ }}

					public static TheoryData<int, string, int> FieldWithExtraArgData = new TheoryData<int, string, int>();
					public static TheoryData<int, string, int> PropertyWithExtraArgData => new TheoryData<int, string, int>();
					public static TheoryData<int, string, int> MethodWithExtraArgData() => new TheoryData<int, string, int>();
					public static TheoryData<int, string, int> MethodWithArgsWithExtraArgData(int _) => new TheoryData<int, string, int>();

					// Extra argument does not match params array type
					[MemberData(nameof(FieldWithExtraArgData))]
					[MemberData(nameof(PropertyWithExtraArgData))]
					[MemberData(nameof(MethodWithExtraArgData))]
					[MemberData(nameof(MethodWithArgsWithExtraArgData))]
					public void TestMethod6(int _1, params {|#0:string[]|} _2) { }

					public static TheoryData<int> FieldIncompatibleData = new TheoryData<int>();
					public static TheoryData<int> PropertyIncompatibleData => new TheoryData<int>();
					public static TheoryData<int> MethodIncompatibleData() => new TheoryData<int>();
					public static TheoryData<int> MethodWithArgsIncompatibleData(int _) => new TheoryData<int>();

					// Incompatible data type
					[MemberData(nameof(FieldIncompatibleData))]
					[MemberData(nameof(PropertyIncompatibleData))]
					[MemberData(nameof(MethodIncompatibleData))]
					[MemberData(nameof(MethodWithArgsIncompatibleData))]
					public void TestMethod7({|#1:string|} _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.FieldWithExtraArgData", "_2"),
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.PropertyWithExtraArgData", "_2"),
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithExtraArgData", "_2"),
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithArgsWithExtraArgData", "_2"),

				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.FieldIncompatibleData", "_"),
				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.PropertyIncompatibleData", "_"),
				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodIncompatibleData", "_"),
				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodWithArgsIncompatibleData", "_"),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source, expected);
		}

		[Fact]
		public async ValueTask V3_only()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static IEnumerable<TheoryDataRow<int, string[]>> FieldData = new List<TheoryDataRow<int, string[]>>();
					public static IEnumerable<TheoryDataRow<int, string[]>> PropertyData => new List<TheoryDataRow<int, string[]>>();
					public static IEnumerable<TheoryDataRow<int, string[]>> MethodData() => new List<TheoryDataRow<int, string[]>>();
					public static IEnumerable<TheoryDataRow<int, string[]>> MethodWithArgsData(int _) => new List<TheoryDataRow<int, string[]>>();

					// Exact match
					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1(int _1, params string[] _2) { }

					public static IEnumerable<TheoryDataRow<int, string, string>> FieldDataCollapse = new List<TheoryDataRow<int, string, string>>();
					public static IEnumerable<TheoryDataRow<int, string, string>> PropertyDataCollapse => new List<TheoryDataRow<int, string, string>>();
					public static IEnumerable<TheoryDataRow<int, string, string>> MethodDataCollapse() => new List<TheoryDataRow<int, string, string>>();
					public static IEnumerable<TheoryDataRow<int, string, string>> MethodWithArgsDataCollapse(int _) => new List<TheoryDataRow<int, string, string>>();

					// Multiple values can be collapsed into the params array
					[MemberData(nameof(FieldDataCollapse))]
					[MemberData(nameof(PropertyDataCollapse))]
					[MemberData(nameof(MethodDataCollapse))]
					[MemberData(nameof(MethodWithArgsDataCollapse), 42)]
					public void TestMethod2(int _1, params string[] _2) { }

					public static IEnumerable<TheoryDataRow<(int, int)>> FieldNamelessTupleData = new List<TheoryDataRow<(int, int)>>();
					public static IEnumerable<TheoryDataRow<(int, int)>> PropertyNamelessTupleData => new List<TheoryDataRow<(int, int)>>();
					public static IEnumerable<TheoryDataRow<(int, int)>> MethodNamelessTupleData() => new List<TheoryDataRow<(int, int)>>();
					public static IEnumerable<TheoryDataRow<(int, int)>> MethodWithArgsNamelessTupleData(int _) => new List<TheoryDataRow<(int, int)>>();

					// Nameless anonymous tuples
					[MemberData(nameof(FieldNamelessTupleData))]
					[MemberData(nameof(PropertyNamelessTupleData))]
					[MemberData(nameof(MethodNamelessTupleData))]
					[MemberData(nameof(MethodWithArgsNamelessTupleData), 42)]
					public void TestMethod3((int a, int b) _) { }

					public static IEnumerable<TheoryDataRow<(int x, int y)>> FieldNamedTupleData = new List<TheoryDataRow<(int x, int y)>>();
					public static IEnumerable<TheoryDataRow<(int x, int y)>> PropertyNamedTupleData => new List<TheoryDataRow<(int x, int y)>>();
					public static IEnumerable<TheoryDataRow<(int x, int y)>> MethodNamedTupleData() => new List<TheoryDataRow<(int x, int y)>>();
					public static IEnumerable<TheoryDataRow<(int x, int y)>> MethodWithArgsNamedTupleData(int _) => new List<TheoryDataRow<(int x, int y)>>();

					// Named anonymous tuples (names don't need to match, just the shape)
					[MemberData(nameof(FieldNamedTupleData))]
					[MemberData(nameof(PropertyNamedTupleData))]
					[MemberData(nameof(MethodNamedTupleData))]
					[MemberData(nameof(MethodWithArgsNamedTupleData), 42)]
					public void TestMethod4((int a, int b) _) { }

					public static IEnumerable<TheoryDataRow<object[]>> FieldArrayData = new List<TheoryDataRow<object[]>>();
					public static IEnumerable<TheoryDataRow<object[]>> PropertyArrayData => new List<TheoryDataRow<object[]>>();
					public static IEnumerable<TheoryDataRow<object[]>> MethodArrayData() => new List<TheoryDataRow<object[]>>();
					public static IEnumerable<TheoryDataRow<object[]>> MethodWithArgsArrayData(int _) => new List<TheoryDataRow<object[]>>();

					// https://github.com/xunit/xunit/issues/3007
					[MemberData(nameof(FieldArrayData))]
					[MemberData(nameof(PropertyArrayData))]
					[MemberData(nameof(MethodArrayData))]
					[MemberData(nameof(MethodWithArgsArrayData), 42)]
					public void TestMethod5a<T>(T[] _1) {{ }}

					[MemberData(nameof(FieldArrayData))]
					[MemberData(nameof(PropertyArrayData))]
					[MemberData(nameof(MethodArrayData))]
					[MemberData(nameof(MethodWithArgsArrayData), 42)]
					public void TestMethod5b<T>(IEnumerable<T> _1) {{ }}

					public static IEnumerable<TheoryDataRow<int, string, int>> FieldWithExtraArgData = new List<TheoryDataRow<int, string, int>>();
					public static IEnumerable<TheoryDataRow<int, string, int>> PropertyWithExtraArgData => new List<TheoryDataRow<int, string, int>>();
					public static IEnumerable<TheoryDataRow<int, string, int>> MethodWithExtraArgData() => new List<TheoryDataRow<int, string, int>>();
					public static IEnumerable<TheoryDataRow<int, string, int>> MethodWithArgsWithExtraArgData(int _) => new List<TheoryDataRow<int, string, int>>();

					// Extra argument does not match params array type
					[MemberData(nameof(FieldWithExtraArgData))]
					[MemberData(nameof(PropertyWithExtraArgData))]
					[MemberData(nameof(MethodWithExtraArgData))]
					[MemberData(nameof(MethodWithArgsWithExtraArgData))]
					public void TestMethod6(int _1, params {|#0:string[]|} _2) { }

					public static IEnumerable<TheoryDataRow<int>> FieldIncompatibleData = new List<TheoryDataRow<int>>();
					public static IEnumerable<TheoryDataRow<int>> PropertyIncompatibleData => new List<TheoryDataRow<int>>();
					public static IEnumerable<TheoryDataRow<int>> MethodIncompatibleData() => new List<TheoryDataRow<int>>();
					public static IEnumerable<TheoryDataRow<int>> MethodWithArgsIncompatibleData(int _) => new List<TheoryDataRow<int>>();

					// Incompatible data type
					[MemberData(nameof(FieldIncompatibleData))]
					[MemberData(nameof(PropertyIncompatibleData))]
					[MemberData(nameof(MethodIncompatibleData))]
					[MemberData(nameof(MethodWithArgsIncompatibleData))]
					public void TestMethod7({|#1:string|} _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.FieldWithExtraArgData", "_2"),
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.PropertyWithExtraArgData", "_2"),
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithExtraArgData", "_2"),
				Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithArgsWithExtraArgData", "_2"),

				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.FieldIncompatibleData", "_"),
				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.PropertyIncompatibleData", "_"),
				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodIncompatibleData", "_"),
				Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodWithArgsIncompatibleData", "_"),
			};

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7, source, expected);
		}
	}

	public class X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				using Xunit;

				public class TestClass {
					public static TheoryData<string?> FieldData = new TheoryData<string?>();
					public static TheoryData<string?> PropertyData => new TheoryData<string?>();
					public static TheoryData<string?> MethodData() => new TheoryData<string?>();
					public static TheoryData<string?> MethodWithArgsData(int _) => new TheoryData<string?>();

					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod({|#0:string|} _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.FieldData", "_"),
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.PropertyData", "_"),
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodData", "_"),
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodWithArgsData", "_"),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		[Fact]
		public async ValueTask V3_only()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static IEnumerable<TheoryDataRow<string?>> FieldData = new List<TheoryDataRow<string?>>();
					public static IEnumerable<TheoryDataRow<string?>> PropertyData => new List<TheoryDataRow<string?>>();
					public static IEnumerable<TheoryDataRow<string?>> MethodData() => new List<TheoryDataRow<string?>>();
					public static IEnumerable<TheoryDataRow<string?>> MethodWithArgsData(int _) => new List<TheoryDataRow<string?>>();

					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod({|#0:string|} _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.FieldData", "_"),
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.PropertyData", "_"),
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodData", "_"),
				Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodWithArgsData", "_"),
			};

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis
	{
		const string V2AllowedTypes = "TheoryData<>";
		const string V3AllowedTypes = "TheoryData<> or IEnumerable<TheoryDataRow<>>";

		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1053
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {
					public static TheoryData<int> FieldData;
					public static TheoryData<int> PropertyData { get; set; }
					public static TheoryData<int> MethodData() => null;
					public static TheoryData<int> MethodWithArgsData(int _) => null;

					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1(int _) { }

					public static IEnumerable<object[]> FieldUntypedData;
					public static IEnumerable<object[]> PropertyUntypedData { get; set; }
					public static List<object[]> MethodUntypedData() => null;
					public static object[][] MethodWithArgsUntypedData(int _) => null;

					[{|#0:MemberData(nameof(FieldUntypedData))|}]
					[{|#1:MemberData(nameof(PropertyUntypedData))|}]
					[{|#2:MemberData(nameof(MethodUntypedData))|}]
					[{|#3:MemberData(nameof(MethodWithArgsUntypedData), 42)|}]
					public void TestMethod2(int _) { }
				}
				""";
			var expectedV2 = new[] {
				Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments(V2AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(1).WithArguments(V2AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(2).WithArguments(V2AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(3).WithArguments(V2AllowedTypes),
			};
			var expectedV3 = new[] {
				Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(1).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(2).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(3).WithArguments(V3AllowedTypes),
			};

			await Verify.VerifyAnalyzerV2(source, expectedV2);
			await Verify.VerifyAnalyzerV3(source, expectedV3);
		}

		[Fact]
		public async Task V3_only()
		{
			var source = /* lang=c#-test */ """
				#pragma warning disable xUnit1053
				using System.Collections.Generic;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {
					public static MatrixTheoryData<int, string> FieldData;
					public static MatrixTheoryData<int, string> PropertyData { get; set; }
					public static MatrixTheoryData<int, string> MethodData() => null;
					public static MatrixTheoryData<int, string> MethodWithArgsData(int _) => null;

					[MemberData(nameof(FieldData))]
					[MemberData(nameof(PropertyData))]
					[MemberData(nameof(MethodData))]
					[MemberData(nameof(MethodWithArgsData), 42)]
					public void TestMethod1(int _1, string _2) { }

					public static IEnumerable<TheoryDataRow<int>> FieldEnumerableData;
					public static IAsyncEnumerable<TheoryDataRow<int>> PropertyEnumerableData { get; set; }
					public static List<TheoryDataRow<int>> MethodEnumerableData() => null;
					public static TheoryDataRow<int>[] MethodWithArgsEnumerableData(int _) => null;

					[MemberData(nameof(FieldEnumerableData))]
					[MemberData(nameof(PropertyEnumerableData))]
					[MemberData(nameof(MethodEnumerableData))]
					[MemberData(nameof(MethodWithArgsEnumerableData), 42)]
					public void TestMethod2(int _) { }

					public static Task<IEnumerable<object[]>> FieldUntypedTaskData;
					public static Task<IAsyncEnumerable<object[]>> PropertyUntypedTaskData { get; set; }
					public static Task<List<object[]>> MethodUntypedTaskData() => null;
					public static Task<object[][]> MethodWithArgsUntypedTaskData(int _) => null;

					[{|#0:MemberData(nameof(FieldUntypedTaskData))|}]
					[{|#1:MemberData(nameof(PropertyUntypedTaskData))|}]
					[{|#2:MemberData(nameof(MethodUntypedTaskData))|}]
					[{|#3:MemberData(nameof(MethodWithArgsUntypedTaskData), 42)|}]
					public void TestMethod3(int _) { }

					public static ValueTask<IEnumerable<object[]>> FieldUntypedValueTaskData;
					public static ValueTask<IAsyncEnumerable<object[]>> PropertyUntypedValueTaskData { get; set; }
					public static ValueTask<List<object[]>> MethodUntypedValueTaskData() => default;
					public static ValueTask<object[][]> MethodWithArgsUntypedValueTaskData(int _) => default;

					[{|#10:MemberData(nameof(FieldUntypedValueTaskData))|}]
					[{|#11:MemberData(nameof(PropertyUntypedValueTaskData))|}]
					[{|#12:MemberData(nameof(MethodUntypedValueTaskData))|}]
					[{|#13:MemberData(nameof(MethodWithArgsUntypedValueTaskData), 42)|}]
					public void TestMethod4(int _) { }

					public static Task<IEnumerable<ITheoryDataRow>> FieldUntypedTaskData2;
					public static Task<IAsyncEnumerable<ITheoryDataRow>> PropertyUntypedTaskData2 { get; set; }
					public static Task<List<TheoryDataRow>> MethodUntypedTaskData2() => null;
					public static Task<TheoryDataRow[]> MethodWithArgsUntypedTaskData2(int _) => null;

					[{|#20:MemberData(nameof(FieldUntypedTaskData2))|}]
					[{|#21:MemberData(nameof(PropertyUntypedTaskData2))|}]
					[{|#22:MemberData(nameof(MethodUntypedTaskData2))|}]
					[{|#23:MemberData(nameof(MethodWithArgsUntypedTaskData2), 42)|}]
					public void TestMethod5(int _) { }

					public static ValueTask<IEnumerable<ITheoryDataRow>> FieldUntypedValueTaskData2;
					public static ValueTask<IAsyncEnumerable<ITheoryDataRow>> PropertyUntypedValueTaskData2 { get; set; }
					public static ValueTask<List<TheoryDataRow>> MethodUntypedValueTaskData2() => default;
					public static ValueTask<TheoryDataRow[]> MethodWithArgsUntypedValueTaskData2(int _) => default;

					[{|#30:MemberData(nameof(FieldUntypedValueTaskData2))|}]
					[{|#31:MemberData(nameof(PropertyUntypedValueTaskData2))|}]
					[{|#32:MemberData(nameof(MethodUntypedValueTaskData2))|}]
					[{|#33:MemberData(nameof(MethodWithArgsUntypedValueTaskData2), 42)|}]
					public void TestMethod6(int _) { }
				}
				""";
			var expected = new[] {
				Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(1).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(2).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(3).WithArguments(V3AllowedTypes),

				Verify.Diagnostic("xUnit1042").WithLocation(10).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(11).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(12).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(13).WithArguments(V3AllowedTypes),

				Verify.Diagnostic("xUnit1042").WithLocation(20).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(21).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(22).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(23).WithArguments(V3AllowedTypes),

				Verify.Diagnostic("xUnit1042").WithLocation(30).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(31).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(32).WithArguments(V3AllowedTypes),
				Verify.Diagnostic("xUnit1042").WithLocation(33).WithArguments(V3AllowedTypes),
			};

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source, expected);
		}
	}

	public class X1053_MemberDataMemberMustBeStaticallyWrittenTo
	{
		[Fact]
		public async ValueTask Initializers_AndGetExpressions_MarkAsWrittenTo()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int> Field = null;
					public static TheoryData<int> Property { get; } = null;
					public static TheoryData<int> PropertyWithGetBody { get { return null; } }
					public static TheoryData<int> PropertyWithGetExpression => null;

					[Theory]
					[MemberData(nameof(Field))]
					[MemberData(nameof(Property))]
					[MemberData(nameof(PropertyWithGetBody))]
					[MemberData(nameof(PropertyWithGetExpression))]
					public void TestCase(int _) {}
				}
			""";

			await Verify.VerifyAnalyzer(source, []);
		}

		[Fact]
		public async ValueTask SimpleCase_GeneratesResult()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public static TheoryData<int> {|#0:Field|};
					public static TheoryData<int> {|#1:Property|} { get; set; }

					public TestClass()
					{
						Field = null;
						Property = null;
					}

					[Theory]
					[MemberData(nameof(Field)), MemberData(nameof(Property))]
					public void TestCase(int _) {}
				}
			""";

			var expected = new[] {
				Verify.Diagnostic("xUnit1053").WithLocation(0).WithArguments("Field"),
				Verify.Diagnostic("xUnit1053").WithLocation(1).WithArguments("Property"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
