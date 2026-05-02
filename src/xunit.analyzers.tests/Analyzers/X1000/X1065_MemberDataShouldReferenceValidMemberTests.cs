using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1065_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1042

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<object[]> DataMethod() => DataMethod("Hello world");
				public static IEnumerable<object[]> DataMethod(string s) => new object[][] { new object[] { s } };

				[Theory]
				[{|#0:MemberData(nameof(DataMethod))|}]
				public void WithoutArgs(string _) { }

				[Theory]
				[{|#1:MemberData(nameof(DataMethod), "Heyo")|}]
				public void WithArgs(string _) { }

				public static IEnumerable<object[]> OverloadedDataMethod(string name, int scenarios = 444)
				{
					for (int i = 1; i <= scenarios; i++)
						yield return new object[] { name, i };
				}

				public static IEnumerable<object[]> OverloadedDataMethod(string name, string scenariosAsString = "444")
				{
					yield return new object[] { name, int.Parse(scenariosAsString) };
				}

				[Theory]
				[{|#2:MemberData(nameof(OverloadedDataMethod), "MyFirst")|}]
				public void WithUnresolvedAmbiguity(string _1, int _2)
				{ }
			}
			""";
		var expectedNonAot = new[] {
			Verify.Diagnostic("xUnit1065").WithLocation(2).WithArguments("TestClass", "OverloadedDataMethod", "Rename the members to use different names."),
		};

		await Verify.VerifyAnalyzerNonAot(source, expectedNonAot);

#if NETCOREAPP && ROSLYN_LATEST
		var expectedAot = new[] {
			Verify.Diagnostic("xUnit1065").WithLocation(0).WithArguments("TestClass", "DataMethod", "Native AOT does not support overloaded MemberData methods."),
			Verify.Diagnostic("xUnit1065").WithLocation(1).WithArguments("TestClass", "DataMethod", "Native AOT does not support overloaded MemberData methods."),
			Verify.Diagnostic("xUnit1065").WithLocation(2).WithArguments("TestClass", "OverloadedDataMethod", "Native AOT does not support overloaded MemberData methods."),
		};

		await Verify.VerifyAnalyzerV3Aot(source, expectedAot);
#endif
	}
}
