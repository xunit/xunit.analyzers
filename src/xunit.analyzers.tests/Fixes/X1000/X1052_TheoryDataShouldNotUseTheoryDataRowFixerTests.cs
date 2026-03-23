using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataShouldNotUseTheoryDataRow>;

public class X1052_TheoryDataShouldNotUseTheoryDataRowFixerTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class Fixable {
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

			public class Unfixable {
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

			public class MyRow : {|CS0535:{|CS0535:ITheoryDataRow|}|} {
				public object?[] GetData() { return null; }
				public bool? Explicit { get; }
				public string? Label { get; }
				public string? Skip { get; }
				public Type? SkipType { get; }
				public string? TestDisplayName { get; }
				public int? Timeout { get; }
				public Dictionary<string, HashSet<string>>? Traits { get; }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class Fixable {
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

			public class Unfixable {
				private {|#0:TheoryData<ITheoryDataRow>|} field11 = new();
				private {|#1:TheoryData<TheoryDataRow<int>>|} field12 = new();
				private {|#2:TheoryData<MyRow>|} field13 = new();

				private {|#10:TheoryData<ITheoryDataRow, int>|} field21;
				private {|#11:TheoryData<TheoryDataRow<int>, int>|} field22;
				private {|#12:TheoryData<MyRow, int>|} field23;

				public {|#20:TheoryData<ITheoryDataRow>|} property11 { get; set; } = new();
				public {|#21:TheoryData<TheoryDataRow<int>>|} property12 { get; set; } = new();
				public {|#22:TheoryData<MyRow>|} property13 { get; set; } = new();

				public {|#30:TheoryData<ITheoryDataRow, int>|} property21 { get; set; }
				public {|#31:TheoryData<TheoryDataRow<int>, int>|} property22 { get; set; }
				public {|#32:TheoryData<MyRow, int>|} property23 { get; set; }

				public {|#40:TheoryData<ITheoryDataRow, int>|} method11() { {|#41:TheoryData<ITheoryDataRow, int>|} data; return null; }
				public {|#42:TheoryData<TheoryDataRow<int>, int>|} method12() { {|#43:TheoryData<TheoryDataRow<int>, int>|} data; return null; }
				public {|#44:TheoryData<MyRow, int>|} method13() { {|#45:TheoryData<MyRow, int>|} data; return null; }
			}

			public class MyRow : {|CS0535:{|CS0535:ITheoryDataRow|}|} {
				public object?[] GetData() { return null; }
				public bool? Explicit { get; }
				public string? Label { get; }
				public string? Skip { get; }
				public Type? SkipType { get; }
				public string? TestDisplayName { get; }
				public int? Timeout { get; }
				public Dictionary<string, HashSet<string>>? Traits { get; }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0),
			Verify.Diagnostic().WithLocation(1),
			Verify.Diagnostic().WithLocation(2),

			Verify.Diagnostic().WithLocation(10),
			Verify.Diagnostic().WithLocation(11),
			Verify.Diagnostic().WithLocation(12),

			Verify.Diagnostic().WithLocation(20),
			Verify.Diagnostic().WithLocation(21),
			Verify.Diagnostic().WithLocation(22),

			Verify.Diagnostic().WithLocation(30),
			Verify.Diagnostic().WithLocation(31),
			Verify.Diagnostic().WithLocation(32),

			Verify.Diagnostic().WithLocation(40),
			Verify.Diagnostic().WithLocation(41),
			Verify.Diagnostic().WithLocation(42),
			Verify.Diagnostic().WithLocation(43),
			Verify.Diagnostic().WithLocation(44),
			Verify.Diagnostic().WithLocation(45),
		};

		await Verify.VerifyCodeFixV3FixAll(LanguageVersion.CSharp9, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable, expected);
	}
}
