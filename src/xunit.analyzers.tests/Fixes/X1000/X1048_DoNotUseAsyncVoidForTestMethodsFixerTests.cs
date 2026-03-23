using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

public class X1048_DoNotUseAsyncVoidForTestMethodsFixerTests
{
	[Fact]
	public async ValueTask V2_only()
	{
		var before = /* lang=c#-test */ """
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

		await Verify.VerifyCodeFixV2FixAll(before, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
	}
}
