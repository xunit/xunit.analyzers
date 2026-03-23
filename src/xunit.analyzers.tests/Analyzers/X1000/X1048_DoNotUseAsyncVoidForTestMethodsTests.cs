using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

public class X1048_DoNotUseAsyncVoidForTestMethodsTests
{
	[Fact]
	public async ValueTask V2_only()
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
			}
			""";

		var expected = Verify.Diagnostic("xUnit1048").WithLocation(0);

		await Verify.VerifyAnalyzerV2(source, expected);
	}
}
