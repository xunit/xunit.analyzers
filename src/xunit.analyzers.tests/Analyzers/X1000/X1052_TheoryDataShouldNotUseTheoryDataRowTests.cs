using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataShouldNotUseTheoryDataRow>;

public class X1052_TheoryDataShouldNotUseTheoryDataRowTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class Triggers1 {
				// Constructors
				public Triggers1([|TheoryData<ITheoryDataRow>|] data) { }
				public Triggers1([|TheoryData<TheoryDataRow<int>>|] data) { }
				public Triggers1([|TheoryData<MyRow>|] data) { }

				// Fields
				private [|TheoryData<ITheoryDataRow>|] field1 = new [|TheoryData<ITheoryDataRow>|]();
				private [|TheoryData<TheoryDataRow<int>>|] field2 = new [|TheoryData<TheoryDataRow<int>>|]();
				private [|TheoryData<MyRow>|] field3 = new [|TheoryData<MyRow>|]();

				// Array fields
				public [|TheoryData<ITheoryDataRow>|][] field11 = null;
				public [|TheoryData<TheoryDataRow<int>>|][] field12 = null;
				public [|TheoryData<MyRow>|][] field13 = null;

				// Static fields
				private static [|TheoryData<ITheoryDataRow>|] field21 = null;
				private static [|TheoryData<TheoryDataRow<int>>|] field22 = null;
				private static [|TheoryData<MyRow>|] field23 = null;

				// Properties
				public [|TheoryData<ITheoryDataRow>|] property1 { get; set; } = new [|TheoryData<ITheoryDataRow>|]();
				public [|TheoryData<TheoryDataRow<int>>|] property2 { get; set; } = new [|TheoryData<TheoryDataRow<int>>|]();
				public [|TheoryData<MyRow>|] property3 { get; set; } = new [|TheoryData<MyRow>|]();

				// Methods
				public [|TheoryData<ITheoryDataRow>|] Method1() { return null; }
				public [|TheoryData<TheoryDataRow<int>>|] Method2() { return null; }
				public [|TheoryData<MyRow>|] Method3() { return null; }
			}

			// Generic constraints
			class Triggers2<T> where T : ITheoryDataRow {
				[|TheoryData<T>|] data = null;
			}

			class Triggers3<T> where T : MyRow {
				[|TheoryData<T>|] data = null;
			}

			public class DoesNotTrigger {
				IEnumerable<ITheoryDataRow> field1 = new List<ITheoryDataRow>();
				IEnumerable<TheoryDataRow<int>> field2 = new List<TheoryDataRow<int>>();
				IEnumerable<MyRow> field3 = new List<MyRow>();

				IEnumerable<ITheoryDataRow> property1 { get; set; } = new List<ITheoryDataRow>();
				IEnumerable<TheoryDataRow<int>> property2 { get; set; } = new List<TheoryDataRow<int>>();
				IEnumerable<MyRow> property3 { get; set; } = new List<MyRow>();

				IEnumerable<ITheoryDataRow> method1() { return null; }
				IEnumerable<TheoryDataRow<int>> method2() { return null; }
				IEnumerable<MyRow> method3() { return null; }
			}

			public partial class MyRow : ITheoryDataRow {
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
		var myRowNonAOT = /* lang=c#-test */ """
			using System;
			using Xunit;

			partial class MyRow {
				public string? SkipUnless { get; }
				public string? SkipWhen { get; }
			}
			""";

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, [source, myRowNonAOT]);

#if NETCOREAPP && ROSLYN_LATEST
		var myRowAOT = /* lang=c#-test */ """
			using System;
			using Xunit;

			partial class MyRow {
				public Func<bool>? SkipUnless { get; }
				public Func<bool>? SkipWhen { get; }
			}
			""";

		await Verify.VerifyAnalyzerV3Aot([source, myRowAOT]);
#endif
	}
}
