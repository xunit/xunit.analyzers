using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class DoNotUseConfigureAwaitTests
{
	[Fact]
	public async void SuccessCase()
	{
		var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        await Task.Delay(1);
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void FailureCase_Task_Async(string argumentValue)
	{
		var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        await Task.Delay(1).[|ConfigureAwait({argumentValue})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void FailureCase_Task_NonAsync(string argumentValue)
	{
		var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        Task.Delay(1).[|ConfigureAwait({argumentValue})|].GetAwaiter().GetResult();
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void FailureCase_TaskOfT(string argumentValue)
	{
		var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var task = Task.FromResult(42);
        await task.[|ConfigureAwait({argumentValue})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void FailureCase_ValueTask(string argumentValue)
	{
		var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var valueTask = default(ValueTask);
        await valueTask.[|ConfigureAwait({argumentValue})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("false")]
	[InlineData("1 == 2")]
	public async void FailureCase_ValueTaskOfT(string argumentValue)
	{
		var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var valueTask = default(ValueTask<int>);
        await valueTask.[|ConfigureAwait({argumentValue})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void IgnoredCase()
	{
		var source = @"
using System.Threading.Tasks;
using Xunit;

public class NonTestClass {
    public async Task NonTestMethod() {
        await Task.Delay(1).ConfigureAwait(false);
    }
}";

		await Verify.VerifyAnalyzer(source);
	}
}
