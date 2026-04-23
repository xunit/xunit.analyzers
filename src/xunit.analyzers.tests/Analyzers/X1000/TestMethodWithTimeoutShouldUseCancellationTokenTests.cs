using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodWithTimeoutShouldUseCancellationToken>;

public class TestMethodWithTimeoutShouldUseCancellationTokenTests
{
	[Fact]
	public async Task NoTimeout_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					Thread.Sleep(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task ZeroTimeout_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using Xunit;

			public class TestClass {
				[Fact(Timeout = 0)]
				public void TestMethod() {
					Thread.Sleep(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task TimeoutWithDirectCancellationTokenReference_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact(Timeout = 1000)]
				public async Task TestMethod() {
					await Task.Delay(1, TestContext.Current.CancellationToken);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task TimeoutWithCancellationTokenViaLocal_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact(Timeout = 1000)]
				public async Task TestMethod() {
					var token = TestContext.Current.CancellationToken;
					await Task.Delay(1, token);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task TimeoutWithCancellationTokenReferencedInLambda_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact(Timeout = 1000)]
				public async Task TestMethod() {
					await Assert.ThrowsAsync<System.OperationCanceledException>(
						async () => await Task.Delay(1000, TestContext.Current.CancellationToken));
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task TimeoutWithoutCancellationTokenReference_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact({|#0:Timeout = 1000|})]
				public async Task TestMethod() {
					await Task.Delay(1);
				}
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod");

		await Verify.VerifyAnalyzerV3(source, expected);
	}

	[Fact]
	public async Task TheoryWithTimeoutWithoutCancellationTokenReference_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Theory({|#0:Timeout = 1000|})]
				[InlineData(1)]
				public async Task TestMethod(int _) {
					await Task.Delay(1);
				}
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod");

		await Verify.VerifyAnalyzerV3(source, expected);
	}

	[Fact]
	public async Task DerivedFactWithTimeoutWithoutCancellationTokenReference_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public sealed class MyFactAttribute : FactAttribute {
				public MyFactAttribute(
					[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
					[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
					: base(sourceFilePath, sourceLineNumber) { }
			}

			public class TestClass {
				[MyFact({|#0:Timeout = 1000|})]
				public async Task TestMethod() {
					await Task.Delay(1);
				}
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod");

		await Verify.VerifyAnalyzerV3(source, expected);
	}

	[Fact]
	public async Task NonTestMethodWithoutCancellationToken_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				public async Task HelperMethod() {
					await Task.Delay(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
