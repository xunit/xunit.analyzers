using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualGenericShouldNotBeUsedForStringValue>;

public class X2006_AssertEqualGenericShouldNotBeUsedForStringValueTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void NonGenericEqual_DoesNotTrigger() {
					Assert.Equal(true.ToString(), "True");
					Assert.Equal(1.ToString(), "1");
					Assert.Equal("", null);
					Assert.Equal(null, "");
					Assert.Equal("", "");
					Assert.Equal("abc", "abc");
					Assert.Equal("TestClass", nameof(TestClass));
				}

				void GenericEqual_Triggers() {
					[|Assert.Equal<string>(true.ToString(), "True")|];
					[|Assert.Equal<string>(1.ToString(), "1")|];
					[|Assert.Equal<string>("", null)|];
					[|Assert.Equal<string>(null, "")|];
					[|Assert.Equal<string>("", "")|];
					[|Assert.Equal<string>("abc", "abc")|];
					[|Assert.Equal<string>("TestClass", nameof(TestClass))|];
				}

				void StrictEqual_Triggers() {
					[|Assert.StrictEqual(true.ToString(), "True")|];
					[|Assert.StrictEqual(1.ToString(), "1")|];
					[|Assert.StrictEqual("", null)|];
					[|Assert.StrictEqual(null, "")|];
					[|Assert.StrictEqual("", "")|];
					[|Assert.StrictEqual("abc", "abc")|];
					[|Assert.StrictEqual("TestClass", nameof(TestClass))|];
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void GenericStrictEqual_Triggers() {
					[|Assert.StrictEqual<string>(true.ToString(), "True")|];
					[|Assert.StrictEqual<string>(1.ToString(), "1")|];
					[|Assert.StrictEqual<string>("", null)|];
					[|Assert.StrictEqual<string>(null, "")|];
					[|Assert.StrictEqual<string>("", "")|];
					[|Assert.StrictEqual<string>("abc", "abc")|];
					[|Assert.StrictEqual<string>("TestClass", nameof(TestClass))|];
				}
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
	}
}
