using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

public class X1049_DoNotUseAsyncVoidForTestMethodsTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
				public async void NonTestMethod_DoesNotTrigger() {
					await Task.Yield();
				}
			}

			public class TestClass {
				[Fact]
				public void NonAsyncTestMethod_DoesNotTrigger() { }

				[Fact]
				public async Task AsyncTaskMethod_DoesNotTrigger() {
					await Task.Yield();
				}

				[Fact]
				public async void {|#0:AsyncVoidMethod_Triggers|}() {
					await Task.Yield();
				}

				[Fact]
				public async ValueTask AsyncValueTaskMethod_DoesNotTrigger() {
					await Task.Yield();
				}
			}
			""";

		var expected = Verify.Diagnostic("xUnit1049").WithLocation(0);

		await Verify.VerifyAnalyzerV3(source, expected);
	}
}
