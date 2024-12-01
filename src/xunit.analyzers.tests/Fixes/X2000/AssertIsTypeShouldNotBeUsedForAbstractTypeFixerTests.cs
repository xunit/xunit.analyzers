using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;
using Verify_v2_Pre2_9_3 = CSharpVerifier<AssertIsTypeShouldNotBeUsedForAbstractTypeFixerTests.Analyzer_v2_Pre2_9_3>;
using Verify_v3_Pre0_6_0 = CSharpVerifier<AssertIsTypeShouldNotBeUsedForAbstractTypeFixerTests.Analyzer_v3_Pre0_6_0>;

public class AssertIsTypeShouldNotBeUsedForAbstractTypeFixerTests
{
	[Fact]
	public async Task Conversions_WithoutExactMatch()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public abstract class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new object();
			
					[|Assert.IsType<IDisposable>(data)|];
					[|Assert.IsType<TestClass>(data)|];
					[|Assert.IsNotType<IDisposable>(data)|];
					[|Assert.IsNotType<TestClass>(data)|];
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

					Assert.IsAssignableFrom<IDisposable>(data);
					Assert.IsAssignableFrom<TestClass>(data);
					Assert.IsNotAssignableFrom<IDisposable>(data);
					Assert.IsNotAssignableFrom<TestClass>(data);
				}
			}
			""";

		await Verify_v2_Pre2_9_3.VerifyCodeFix(before, after, AssertIsTypeShouldNotBeUsedForAbstractTypeFixer.Key_UseAlternateAssert);
		await Verify_v3_Pre0_6_0.VerifyCodeFix(before, after, AssertIsTypeShouldNotBeUsedForAbstractTypeFixer.Key_UseAlternateAssert);
	}

	[Fact]
	public async Task Conversions_WithExactMatch()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public abstract class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new object();
			
					[|Assert.IsType<IDisposable>(data)|];
					[|Assert.IsType<IDisposable>(data, true)|];
					[|Assert.IsType<IDisposable>(data, exactMatch: true)|];
					[|Assert.IsType<TestClass>(data)|];
					[|Assert.IsType<TestClass>(data, true)|];
					[|Assert.IsType<TestClass>(data, exactMatch: true)|];
					[|Assert.IsNotType<IDisposable>(data)|];
					[|Assert.IsNotType<IDisposable>(data, true)|];
					[|Assert.IsNotType<IDisposable>(data, exactMatch: true)|];
					[|Assert.IsNotType<TestClass>(data)|];
					[|Assert.IsNotType<TestClass>(data, true)|];
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
					Assert.IsType<IDisposable>(data, exactMatch: false);
					Assert.IsType<IDisposable>(data, exactMatch: false);
					Assert.IsType<TestClass>(data, exactMatch: false);
					Assert.IsType<TestClass>(data, exactMatch: false);
					Assert.IsType<TestClass>(data, exactMatch: false);
					Assert.IsNotType<IDisposable>(data, exactMatch: false);
					Assert.IsNotType<IDisposable>(data, exactMatch: false);
					Assert.IsNotType<IDisposable>(data, exactMatch: false);
					Assert.IsNotType<TestClass>(data, exactMatch: false);
					Assert.IsNotType<TestClass>(data, exactMatch: false);
					Assert.IsNotType<TestClass>(data, exactMatch: false);
				}
			}
			""";

		await Verify.VerifyCodeFix(before, after, AssertIsTypeShouldNotBeUsedForAbstractTypeFixer.Key_UseAlternateAssert);
	}

	internal class Analyzer_v2_Pre2_9_3 : AssertIsTypeShouldNotBeUsedForAbstractType
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 9, 2));
	}

	internal class Analyzer_v3_Pre0_6_0 : AssertIsTypeShouldNotBeUsedForAbstractType
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(0, 5, 999));
	}
}
