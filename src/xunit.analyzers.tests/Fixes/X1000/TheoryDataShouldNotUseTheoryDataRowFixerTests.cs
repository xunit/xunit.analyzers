using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataShouldNotUseTheoryDataRow>;

public class TheoryDataShouldNotUseTheoryDataRowFixerTests
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
	public async Task VariableDeclaration_ToIEnumerable(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				private [|TheoryData<{0}>|] data;
			}}

			{1}
			""", type, strongerType);

		var after = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				private IEnumerable<{0}> data;
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}


	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task PropertyDeclaration_ToIEnumerable(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public [|TheoryData<{0}>|] data {{ get; set; }}
			}}

			{1}
			""", type, strongerType);

		var after = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public IEnumerable<{0}> data {{ get; set; }}
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task Parameter_ToIEnumerable(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public TestClass([|TheoryData<{0}>|] data) {{ }}
			}}

			{1}
			""", type, strongerType);

		var after = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public TestClass(IEnumerable<{0}> data) {{ }}
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task MethodDeclaration_ToIEnumerable(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public [|TheoryData<{0}>|] GetData() {{ return null; }}
			}}

			{1}
			""", type, strongerType);

		var after = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public IEnumerable<{0}> GetData() {{ return null; }}
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}


	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task LocalVariableDeclaration_ToIEnumerable(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public TestClass()
				{{
					[|TheoryData<{0}>|] data;
				}}
			}}

			{1}
			""", type, strongerType);

		var after = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public TestClass()
				{{
					IEnumerable<{0}> data;
				}}
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task LocalVariableDeclaration_WithMultipleGenericArguments_DoesNotFix(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public TestClass()
				{{
					[|TheoryData<{0}, int>|] data;
				}}
			}}

			{1}
			""", type, strongerType);

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after: before, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task VariableDeclaration_WithAssignment_DoesNotFix(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public TestClass()
				{{
					[|TheoryData<{0}>|] data = new [|TheoryData<{0}>|]();
				}}
			}}

			{1}
			""", type, strongerType);


		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after: before, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}

	[Theory]
	[InlineData("ITheoryDataRow")]
	[InlineData("MyRow")]
	public async Task PropertyDeclaration_WithAssignment_DoesNotFix(string type)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System;
			using System.Collections.Generic;

			public class TestClass {{
				public [|TheoryData<{0}>|] data {{ get; set; }} = new [|TheoryData<{0}>|]();
			}}

			{1}
			""", type, strongerType);


		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after: before, TheoryDataShouldNotUseTheoryDataRowFixer.Key_UseIEnumerable);
	}
}
