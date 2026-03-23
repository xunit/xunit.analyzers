using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

public class X1049_DoNotUseAsyncVoidForTestMethodsFixerTests
{
	[Fact]
	public async ValueTask V3_only()
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
		var afterTask = /* lang=c#-test */ """
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
		var afterValueTask = /* lang=c#-test */ """
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

		await Verify.VerifyCodeFixV3FixAll(before, afterTask, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
		await Verify.VerifyCodeFixV3FixAll(before, afterValueTask, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToValueTask);
	}
}
