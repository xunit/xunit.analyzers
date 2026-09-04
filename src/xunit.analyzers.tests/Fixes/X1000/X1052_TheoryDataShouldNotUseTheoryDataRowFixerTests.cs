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

				public [|TheoryData<ITheoryDataRow>|] property11 { get; set; } = new();
				public [|TheoryData<TheoryDataRow<int>>|] property12 { get; set; } = new();
				public [|TheoryData<MyRow>|] property13 { get; set; } = new();
			}

			public class MyRow : {|CS0535:{|CS0535:ITheoryDataRow|}|} {
				public object?[] GetData() { return null; }
				public bool? DisableParallelization { get; }
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

				public {|#10:TheoryData<ITheoryDataRow>|} property11 { get; set; } = new();
				public {|#11:TheoryData<TheoryDataRow<int>>|} property12 { get; set; } = new();
				public {|#12:TheoryData<MyRow>|} property13 { get; set; } = new();
			}

			public class MyRow : {|CS0535:{|CS0535:ITheoryDataRow|}|} {
				public object?[] GetData() { return null; }
				public bool? DisableParallelization { get; }
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
		};

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp9, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable, expected);
	}
}
