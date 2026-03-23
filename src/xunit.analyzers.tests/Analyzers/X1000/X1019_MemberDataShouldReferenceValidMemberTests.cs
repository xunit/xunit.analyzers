using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1019_MemberDataShouldReferenceValidMemberTests
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
			using System.Collections;
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class NamedTypeForIEnumerableStringArray : IEnumerable<string[]>
			{
				private readonly List<string[]> _items = new List<string[]>();

				public void Add(string sdl)
				{
					_items.Add(new string[] { sdl });
				}

				public IEnumerator<string[]> GetEnumerator()
				{
					return _items.GetEnumerator();
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}
			}

			public class NamedSubtypeForIEnumerableStringArray : NamedTypeForIEnumerableStringArray {}

			public class TestClass {
				public static IEnumerable<object> ObjectSource;
				public static object NakedObjectSource;
				public static object[] NakedObjectArraySource;
				public static object[][] NakedObjectMatrixSource;

				public static IEnumerable<object[]> ObjectArraySource;
				public static IEnumerable<string[]> StringArraySource;
				public static NamedTypeForIEnumerableStringArray NamedTypeForIEnumerableStringArraySource;
				public static NamedSubtypeForIEnumerableStringArray NamedSubtypeForIEnumerableStringArraySource;

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
				[MemberData(nameof(NakedObjectMatrixSource))]

				[MemberData(nameof(ObjectArraySource))]
				[MemberData(nameof(StringArraySource))]
				[MemberData(nameof(NamedTypeForIEnumerableStringArraySource))]
				[MemberData(nameof(NamedSubtypeForIEnumerableStringArraySource))]

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
