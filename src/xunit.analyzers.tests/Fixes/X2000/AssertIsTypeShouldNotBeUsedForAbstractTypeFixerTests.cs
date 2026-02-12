using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;

public class AssertIsTypeShouldNotBeUsedForAbstractTypeFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllAbstractTypeChecks()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public abstract class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new object();

					[|Assert.IsType<IDisposable>(data)|];
					[|Assert.IsType<TestClass>(data, true)|];
					[|Assert.IsNotType<IDisposable>(data)|];
					[|Assert.IsNotType<TestClass>(data, exactMatch: true)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit;

			public abstract class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new object();

					Assert.IsType<IDisposable>(data, exactMatch: false);
					Assert.IsType<TestClass>(data, exactMatch: false);
					Assert.IsNotType<IDisposable>(data, exactMatch: false);
					Assert.IsNotType<TestClass>(data, exactMatch: false);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertIsTypeShouldNotBeUsedForAbstractTypeFixer.Key_UseAlternateAssert);
	}
}
