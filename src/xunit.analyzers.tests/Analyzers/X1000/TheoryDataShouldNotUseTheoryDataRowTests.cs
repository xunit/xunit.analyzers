using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataShouldNotUseTheoryDataRow>;

public class TheoryDataShouldNotUseTheoryDataRowTests
{
	const string strongerType = /* lang=c#-test */ """
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

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_Assignment(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;
			public class Test {{
				object data = new [|TheoryData<{0}>|]();
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_MethodReturnType(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;
			public class Test {{

				[|TheoryData<{0}>|] GetData() {{ return null; }}
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task ValidCombination_OnlyTheoryDataRow_DoesNotTrigger(string type)
	{
		var source = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				IEnumerable<ITheoryDataRow> rows = new List<{0}>();
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Fact]
	public async Task ValidCombination_OnlyTheoryData_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {
				TheoryData<int, int> rows = new TheoryData<int, int>();
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_GenericConstraintToStrongerType(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
		using Xunit;
		using System;
		using System.Collections.Generic;

		class Test<T> where T : {0} {{
			[|TheoryData<T>|] data = null;
		}}

		{1}
		""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_Property_UseOfITheoryDataRow(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
		using Xunit;
		using System;
		using System.Collections.Generic;

		class Test {{
			public [|TheoryData<{0}>|] Data {{ get; set; }}
		}}

		{1}
		""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_ArrayOfTheoryDataRow(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
		using Xunit;
		using System;
		using System.Collections.Generic;

		class Test {{
			public [|TheoryData<{0}>|][] AllData = null;
		}}

		{1}
		""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_MultipleMatches_ArrayOfTheoryDataRow(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
		using Xunit;
		using System;
		using System.Collections.Generic;

		class Test {{
			public {{|#0:TheoryData<{0}>|}}[] AllData = new {{|#1:TheoryData<{0}>|}}[1];
		}}

		{1}
		""", type, strongerType);


		var expected = new[] {
			Verify.Diagnostic(Descriptors.X1052_TheoryDataShouldNotUseITheoryDataRow.Id).WithLocation(0),
			Verify.Diagnostic(Descriptors.X1052_TheoryDataShouldNotUseITheoryDataRow.Id).WithLocation(1),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_StaticField(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
		using Xunit;
		using System;
		using System.Collections.Generic;

		class Test {{
			private static [|TheoryData<{0}>|] StaticData = null;
		}}

		{1}
		""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task InvalidCombination_ConstructorParameter(string type)
	{
		var source = string.Format(/* lang=C#-test */ """
		using Xunit;
		using System;
		using System.Collections.Generic;

		class Test {{
			public Test([|TheoryData<{0}>|] data) {{
			}}
		}}

		{1}
		""", type, strongerType);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}
}
