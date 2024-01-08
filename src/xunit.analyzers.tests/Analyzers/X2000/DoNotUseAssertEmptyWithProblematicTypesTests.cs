using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAssertEmptyWithProblematicTypes>;

public class DoNotUseAssertEmptyWithProblematicTypesTests
{
	public static TheoryData<string, string, string> ProblematicTypes = new()
	{
		{ "StringValues.Empty", "StringValues", "it is implicitly cast to a string, not a collection" },
		{ "new ArraySegment<int>()", "ArraySegment<int>", "its implementation of GetEnumerator() can throw" },
	};

	[Theory]
	[InlineData("new int[0]")]
	[InlineData("new List<int>()")]
	[InlineData("new Dictionary<string, int>()")]
	public async Task NonProblematicCollection_DoesNotTrigger(string invocation)
	{
		var source = @$"
using System;
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public void TestMethod() {{
        Assert.Empty({invocation});
        Assert.NotEmpty({invocation});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(ProblematicTypes))]
	public async Task ConvertingToCollection_DoesNotTrigger(
		string invocation,
		string _1,
		string _2)
	{
		var source = @$"
using System;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Xunit;

public class TestClass {{
    public void TestMethod() {{
        Assert.Empty({invocation}.ToArray());
        Assert.NotEmpty({invocation}.ToArray());
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(ProblematicTypes))]
	public async Task UsingProblematicType_Triggers(
		string invocation,
		string typeName,
		string problem)
	{
		var source = @$"
using System;
using Microsoft.Extensions.Primitives;
using Xunit;

public class TestClass {{
    public void TestMethod() {{
        Assert.Empty({invocation});
        Assert.NotEmpty({invocation});
    }}
}}";

		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 23 + invocation.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Empty", typeName, problem),
			Verify
				.Diagnostic()
				.WithSpan(9, 9, 9, 26 + invocation.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("NotEmpty", typeName, problem),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
