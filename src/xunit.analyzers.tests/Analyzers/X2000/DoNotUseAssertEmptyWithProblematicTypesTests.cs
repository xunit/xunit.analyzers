using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAssertEmptyWithProblematicTypes>;

public class DoNotUseAssertEmptyWithProblematicTypesTests
{
	public static TheoryData<string, string, string> ProblematicTypes = new()
	{
		/* lang=c#-test */ { "StringValues.Empty", "Microsoft.Extensions.Primitives.StringValues", "it is implicitly cast to a string, not a collection" },
		/* lang=c#-test */ { "new ArraySegment<int>()", "System.ArraySegment<int>", "its implementation of GetEnumerator() can throw" },
	};

	[Theory]
	[InlineData(/* lang=c#-test */ "new int[0]")]
	[InlineData(/* lang=c#-test */ "new List<int>()")]
	[InlineData(/* lang=c#-test */ "new Dictionary<string, int>()")]
	public async Task NonProblematicCollection_DoesNotTrigger(string invocation)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {{
				public void TestMethod() {{
					Assert.Empty({0});
					Assert.NotEmpty({0});
				}}
			}}
			""", invocation);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(ProblematicTypes))]
	public async Task ConvertingToCollection_DoesNotTrigger(
		string invocation,
		string _1,
		string _2)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using System.Linq;
			using Microsoft.Extensions.Primitives;
			using Xunit;

			public class TestClass {{
				public void TestMethod() {{
					Assert.Empty({0}.ToArray());
					Assert.NotEmpty({0}.ToArray());
				}}
			}}
			""", invocation);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(ProblematicTypes))]
	public async Task UsingProblematicType_Triggers(
		string invocation,
		string typeName,
		string problem)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using Microsoft.Extensions.Primitives;
			using Xunit;

			public class TestClass {{
				public void TestMethod() {{
					{{|#0:Assert.Empty({0})|}};
					{{|#1:Assert.NotEmpty({0})|}};
				}}
			}}
			""", invocation);
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("Empty", typeName, problem),
			Verify.Diagnostic().WithLocation(1).WithArguments("NotEmpty", typeName, problem),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
