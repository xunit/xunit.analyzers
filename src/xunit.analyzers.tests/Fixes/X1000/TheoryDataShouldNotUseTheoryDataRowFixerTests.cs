using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataShouldNotUseTheoryDataRow>;

public class TheoryDataShouldNotUseTheoryDataRowFixerTests
{
	const string myRowSource = /* lang=c#-test */ """
		public class MyRow : ITheoryDataRow {
			public object?[] GetData() { return null; }
			public bool? Explicit { get; }
			public string? Label { get; }
			public string? Skip { get; }
			public Type? SkipType { get; }
			public string? SkipUnless { get; }
			public string? SkipWhen { get; }
			public string? TestDisplayName { get; }
			public int? Timeout { get; }
			public Dictionary<string, HashSet<string>>? Traits { get; }
		}
		""";

	[Fact]
	public async Task AcceptanceTest_Fixable()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				private [|TheoryData<ITheoryDataRow>|] field1;
				private [|TheoryData<TheoryDataRow<int>>|] field2;
				private [|TheoryData<MyRow>|] field3;

				public [|TheoryData<ITheoryDataRow>|] property1 { get; set; }
				public [|TheoryData<TheoryDataRow<int>>|] property2 { get; set; }
				public [|TheoryData<MyRow>|] property3 { get; set; }

				public [|TheoryData<ITheoryDataRow>|] method1() {  [|TheoryData<ITheoryDataRow>|] data; return null; }
				public [|TheoryData<TheoryDataRow<int>>|] method2() { [|TheoryData<TheoryDataRow<int>>|] data; return null; }
				public [|TheoryData<MyRow>|] method3() { [|TheoryData<MyRow>|] data; return null; }
			}
			""" + myRowSource;
		var after = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				private IEnumerable<ITheoryDataRow> field1;
				private IEnumerable<TheoryDataRow<int>> field2;
				private IEnumerable<MyRow> field3;

				public IEnumerable<ITheoryDataRow> property1 { get; set; }
				public IEnumerable<TheoryDataRow<int>> property2 { get; set; }
				public IEnumerable<MyRow> property3 { get; set; }

				public IEnumerable<ITheoryDataRow> method1() { IEnumerable<ITheoryDataRow> data; return null; }
				public IEnumerable<TheoryDataRow<int>> method2() { IEnumerable<TheoryDataRow<int>> data; return null; }
				public IEnumerable<MyRow> method3() { IEnumerable<MyRow> data; return null; }
			}
			""" + myRowSource;

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp9, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}

	[Fact]
	public async Task AcceptanceTest_Unfixable()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				private [|TheoryData<ITheoryDataRow>|] field11 = new();
				private [|TheoryData<TheoryDataRow<int>>|] field12 = new();
				private [|TheoryData<MyRow>|] field13 = new();

				private [|TheoryData<ITheoryDataRow, int>|] field21;
				private [|TheoryData<TheoryDataRow<int>, int>|] field22;
				private [|TheoryData<MyRow, int>|] field23;

				public [|TheoryData<ITheoryDataRow>|] property11 { get; set; } = new();
				public [|TheoryData<TheoryDataRow<int>>|] property12 { get; set; } = new();
				public [|TheoryData<MyRow>|] property13 { get; set; } = new();

				public [|TheoryData<ITheoryDataRow, int>|] property21 { get; set; }
				public [|TheoryData<TheoryDataRow<int>, int>|] property22 { get; set; }
				public [|TheoryData<MyRow, int>|] property23 { get; set; }

				public [|TheoryData<ITheoryDataRow, int>|] method11() { [|TheoryData<ITheoryDataRow, int>|] data; return null; }
				public [|TheoryData<TheoryDataRow<int>, int>|] method12() { [|TheoryData<TheoryDataRow<int>, int>|] data; return null; }
				public [|TheoryData<MyRow, int>|] method13() { [|TheoryData<MyRow, int>|] data; return null; }
			}
			""" + myRowSource;

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp9, before, after: before, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}
}
