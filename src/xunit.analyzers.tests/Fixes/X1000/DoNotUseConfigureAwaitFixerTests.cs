using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class DoNotUseConfigureAwaitFixerTests
{
	[Theory]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void RemovesConfigureAwait_Task_Async(string argumentValue)
	{
		var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        await Task.Delay(1).[|ConfigureAwait({argumentValue})|];
    }}
}}";

		var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        await Task.Delay(1);
    }
}";

		await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
	}

	[Theory]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void RemovesConfigureAwait_Task_NonAsync(string argumentValue)
	{
		var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        Task.Delay(1).[|ConfigureAwait({argumentValue})|].GetAwaiter().GetResult();
    }}
}}";

		var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.Delay(1).GetAwaiter().GetResult();
    }
}";

		await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
	}

	[Theory]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void RemovesConfigureAwait_TaskOfT(string argumentValue)
	{
		var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var task = Task.FromResult(42);
        await task.[|ConfigureAwait({argumentValue})|];
    }}
}}";

		var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        var task = Task.FromResult(42);
        await task;
    }
}";

		await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
	}

	[Theory]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void RemovesConfigureAwait_ValueTask(string argumentValue)
	{
		var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var valueTask = default(ValueTask);
        await valueTask.[|ConfigureAwait({argumentValue})|];
    }}
}}";

		var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        var valueTask = default(ValueTask);
        await valueTask;
    }
}";

		await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
	}

	[Theory]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void RemovesConfigureAwait_ValueTaskOfT(string argumentValue)
	{
		var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var valueTask = default(ValueTask<object>);
        await valueTask.[|ConfigureAwait({argumentValue})|];
    }}
}}";

		var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        var valueTask = default(ValueTask<object>);
        await valueTask;
    }
}";

		await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
	}
}
