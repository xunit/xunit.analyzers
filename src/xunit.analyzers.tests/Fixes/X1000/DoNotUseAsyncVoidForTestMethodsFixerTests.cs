using System.Threading.Tasks;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

namespace Xunit.Analyzers;

public class DoNotUseAsyncVoidForTestMethodsFixerTests
{
	[Fact]
	public async Task WithoutNamespace_ConvertsToTask()
	{
		var beforeV2 = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public async void {|xUnit1048:TestMethod|}() {
					await System.Threading.Tasks.Task.Yield();
				}
			}
			""";
		var beforeV3 = beforeV2.Replace("xUnit1048", "xUnit1049");
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public async System.Threading.Tasks.Task TestMethod() {
					await System.Threading.Tasks.Task.Yield();
				}
			}
			""";

		await Verify.VerifyCodeFixV2(beforeV2, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
		await Verify.VerifyCodeFixV3(beforeV3, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
	}

	[Fact]
	public async Task WithNamespace_ConvertsToTask()
	{
		var beforeV2 = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async void {|xUnit1048:TestMethod|}() {
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
				public async Task TestMethod() {
					await Task.Yield();
				}
			}
			""";

		await Verify.VerifyCodeFixV2(beforeV2, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
		await Verify.VerifyCodeFixV3(beforeV3, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
	}

	[Fact]
	public async Task WithoutNamespace_ConvertsToValueTask()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public async void {|xUnit1049:TestMethod|}() {
					await System.Threading.Tasks.Task.Yield();
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public async System.Threading.Tasks.ValueTask TestMethod() {
					await System.Threading.Tasks.Task.Yield();
				}
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToValueTask);
	}

	[Fact]
	public async Task WithNamespace_ConvertsToValueTask()
	{
		var before = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async void {|xUnit1049:TestMethod|}() {
					await Task.Yield();
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async ValueTask TestMethod() {
					await Task.Yield();
				}
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToValueTask);
	}
}
