using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAssertEmptyWithProblematicTypes>;

public class X2028_DoNotUseAssertEmptyWithProblematicTypesTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.Linq;
			using Microsoft.Extensions.Primitives;
			using Xunit;

			public class TestClass {
				public void NonProblematicCollection_DoesNotTrigger() {
					Assert.Empty(new int[0]);
					Assert.NotEmpty(new int[0]);
					Assert.Empty(new List<int>());
					Assert.NotEmpty(new List<int>());
					Assert.Empty(new Dictionary<string, int>());
					Assert.NotEmpty(new Dictionary<string, int>());
				}

				public void ProblematicCollection_Triggers() {
					{|#0:Assert.Empty(StringValues.Empty)|};
					{|#1:Assert.NotEmpty(StringValues.Empty)|};
					{|#2:Assert.Empty(new ArraySegment<int>())|};
					{|#3:Assert.NotEmpty(new ArraySegment<int>())|};
				}

				public void ProblematicCollection_ConvertedToCollection_DoesNotTrigger() {
					Assert.Empty(StringValues.Empty.ToArray());
					Assert.NotEmpty(StringValues.Empty.ToArray());
					Assert.Empty(new ArraySegment<int>().ToArray());
					Assert.NotEmpty(new ArraySegment<int>().ToArray());
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Empty", "Microsoft.Extensions.Primitives.StringValues", "it is implicitly cast to a string, not a collection"),
			Verify.Diagnostic().WithLocation(1).WithArguments("NotEmpty", "Microsoft.Extensions.Primitives.StringValues", "it is implicitly cast to a string, not a collection"),
			Verify.Diagnostic().WithLocation(2).WithArguments("Empty", "System.ArraySegment<int>", "its implementation of GetEnumerator() can throw"),
			Verify.Diagnostic().WithLocation(3).WithArguments("NotEmpty", "System.ArraySegment<int>", "its implementation of GetEnumerator() can throw"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
