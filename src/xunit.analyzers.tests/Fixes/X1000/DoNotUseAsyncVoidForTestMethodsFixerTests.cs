using System.Threading.Tasks;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

namespace Xunit.Analyzers;

public class DoNotUseAsyncVoidForTestMethodsFixerTests
{
	[Fact]
	public async Task WithoutNamespace_ConvertsToTask()
	{
		var beforeV2 = @"
using Xunit;

public class TestClass {
    [Fact]
    public async void {|xUnit1048:TestMethod|}() {
        await System.Threading.Tasks.Task.Yield();
    }
}";
		var beforeV3 = @"
using Xunit;

public class TestClass {
    [Fact]
    public async void {|xUnit1049:TestMethod|}() {
        await System.Threading.Tasks.Task.Yield();
    }
}";
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public async System.Threading.Tasks.Task TestMethod() {
        await System.Threading.Tasks.Task.Yield();
    }
}";

		await Verify.VerifyCodeFixV2(beforeV2, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
		await Verify.VerifyCodeFixV3(beforeV3, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
	}

	[Fact]
	public async Task WithNamespace_ConvertsToTask()
	{
		var beforeV2 = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void {|xUnit1048:TestMethod|}() {
        await Task.Yield();
    }
}";
		var beforeV3 = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void {|xUnit1049:TestMethod|}() {
        await Task.Yield();
    }
}";
		var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        await Task.Yield();
    }
}";

		await Verify.VerifyCodeFixV2(beforeV2, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
		await Verify.VerifyCodeFixV3(beforeV3, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToTask);
	}

	[Fact]
	public async Task WithoutNamespace_ConvertsToValueTask()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Fact]
    public async void {|xUnit1049:TestMethod|}() {
        await System.Threading.Tasks.Task.Yield();
    }
}";
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public async System.Threading.Tasks.ValueTask TestMethod() {
        await System.Threading.Tasks.Task.Yield();
    }
}";

		await Verify.VerifyCodeFixV3(before, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToValueTask);
	}

	[Fact]
	public async Task WithNamespace_ConvertsToValueTask()
	{
		var before = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void {|xUnit1049:TestMethod|}() {
        await Task.Yield();
    }
}";
		var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async ValueTask TestMethod() {
        await Task.Yield();
    }
}";

		await Verify.VerifyCodeFixV3(before, after, DoNotUseAsyncVoidForTestMethodsFixer.Key_ConvertToValueTask);
	}
}
