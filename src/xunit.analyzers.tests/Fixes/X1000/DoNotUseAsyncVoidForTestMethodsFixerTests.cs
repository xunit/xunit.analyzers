using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

namespace Xunit.Analyzers;

public class DoNotUseAsyncVoidForTestMethodsFixerTests
{
	[Fact]
	public async Task FixAll_ConvertsMultipleMethodsToTask()
	{
		var beforeV2 = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async void {|xUnit1048:TestMethod1|}() {
					await Task.Yield();
				}

				[Fact]
				public async void {|xUnit1048:TestMethod2|}() {
					await Task.Yield();
				}
			}
			""";
		var beforeV3 = beforeV2.Replace("xUnit1048", "xUnit1049");
		var after = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task TestMethod1() {
					await Task.Yield();
				}

				[Fact]
				public async Task TestMethod2() {
					await Task.Yield();
				}
			}
			""";

		await Verify.VerifyCodeFixV2FixAll(beforeV2, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
		await Verify.VerifyCodeFixV3FixAll(beforeV3, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
	}

	[Fact]
	public async Task FixAll_ConvertsMultipleMethodsToValueTask()
	{
		var before = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async void {|xUnit1049:TestMethod1|}() {
					await Task.Yield();
				}

				[Fact]
				public async void {|xUnit1049:TestMethod2|}() {
					await Task.Yield();
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async ValueTask TestMethod1() {
					await Task.Yield();
				}

				[Fact]
				public async ValueTask TestMethod2() {
					await Task.Yield();
				}
			}
			""";

		await Verify.VerifyCodeFixV3FixAll(before, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToValueTask);
	}
}
