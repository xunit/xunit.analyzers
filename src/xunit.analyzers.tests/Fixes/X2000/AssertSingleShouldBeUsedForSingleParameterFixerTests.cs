using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class AssertSingleShouldBeUsedForSingleParameterFixerTests
{
	public static TheoryData<string, string> Statements = new()
	{
		{
			/* lang=c#-test */ """
					[|Assert.Collection(collection, item => Assert.NotNull(item))|]
			""",
			/* lang=c#-test */ """
					var item = Assert.Single(collection);
					Assert.NotNull(item);
			"""
		},
		{
			/* lang=c#-test */ """
					var item = 42;
					[|Assert.Collection(collection, item => Assert.NotNull(item))|]
			""",
			/* lang=c#-test */ """
					var item = 42;
					var item_2 = Assert.Single(collection);
					Assert.NotNull(item_2);
			"""
		},
		{
			/* lang=c#-test */ """
					[|Assert.Collection(collection, item => { Assert.NotNull(item); })|]
			""",
			/* lang=c#-test */ """
					var item = Assert.Single(collection);
					Assert.NotNull(item);
			"""
		},
		{
			/* lang=c#-test */ """
					var item = 42;
					[|Assert.Collection(collection, item => { Assert.NotNull(item); })|]
			""",
			/* lang=c#-test */ """
					var item = 42;
					var item_2 = Assert.Single(collection);
					Assert.NotNull(item_2);
			"""
		},
		{
			/* lang=c#-test */ """
					[|Assert.Collection(collection, item => { Assert.NotNull(item); Assert.NotNull(item); })|]
			""",
			/* lang=c#-test */ """
					var item = Assert.Single(collection);
					Assert.NotNull(item); Assert.NotNull(item);
			"""
		},
		{
			/* lang=c#-test */ """
					var item = 42;
					[|Assert.Collection(collection, item => { Assert.NotNull(item); Assert.NotNull(item); })|]
			""",
			/* lang=c#-test */ """
					var item = 42;
					var item_2 = Assert.Single(collection);
					Assert.NotNull(item_2); Assert.NotNull(item_2);
			"""
		},
		{
			/* lang=c#-test */ """
					[|Assert.Collection(collection, item => {
						if (item != null) {
							Assert.NotNull(item);
							Assert.NotNull(item);
						}
					})|]
			""",
			/* lang=c#-test */ """
					var item = Assert.Single(collection);
					if (item != null)
					{
						Assert.NotNull(item);
						Assert.NotNull(item);
					}
			"""
		},
		{
			/* lang=c#-test */ """
					var item = 42;
					[|Assert.Collection(collection, item => {
						if (item != null) {
							Assert.NotNull(item);
							Assert.NotNull(item);
						}
					})|]
			""",
			/* lang=c#-test */ """
					var item = 42;
					var item_2 = Assert.Single(collection);
					if (item_2 != null)
					{
						Assert.NotNull(item_2);
						Assert.NotNull(item_2);
					}
			"""
		},
		{
			/* lang=c#-test */ """
					[|Assert.Collection(collection, ElementInspector)|]
			""",
			/* lang=c#-test */ """
					var item = Assert.Single(collection);
					ElementInspector(item);
			"""
		},
		{
			/* lang=c#-test */ """
					var item = 42;
					var item_2 = 21.12;
					[|Assert.Collection(collection, ElementInspector)|]
			""",
			/* lang=c#-test */ """
					var item = 42;
					var item_2 = 21.12;
					var item_3 = Assert.Single(collection);
					ElementInspector(item_3);
			"""
		},
	};

	const string beforeTemplate = /* lang=c#-test */ """
		using Xunit;
		using System.Collections.Generic;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				IEnumerable<object> collection = new List<object>() {{ new object() }};

		{0};
			}}

			private void ElementInspector(object obj)
			{{ }}
		}}
		""";

	const string afterTemplate = /* lang=c#-test */ """
		using Xunit;
		using System.Collections.Generic;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				IEnumerable<object> collection = new List<object>() {{ new object() }};

		{0}
			}}

			private void ElementInspector(object obj)
			{{ }}
		}}
		""";

	[Theory]
	[MemberData(nameof(Statements))]
	public async Task ReplacesCollectionMethod(
		string statementBefore,
		string statementAfter)
	{
		var before = string.Format(beforeTemplate, statementBefore);
		var after = string.Format(afterTemplate, statementAfter);

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, AssertSingleShouldBeUsedForSingleParameterFixer.Key_UseSingleMethod);
	}
}
