using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class DoNotUseConfigureAwaitFixerTests
{
	public class ConfigureAwait_Boolean
	{
		public static TheoryData<string> InvalidValues = new()
		{
			"false",       // Literal false
			"1 == 2",      // Logical false (we don't compute)
			"1 == 1",      // Logical true (we don't compute)
			"booleanVar",  // Reference value (we don't do lookup)
		};

		public class RemoveConfigureAwait
		{
			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async void Task_Async(string argumentValue)
			{
				var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
        await Task.Delay(1).[|ConfigureAwait({argumentValue})|];
    }}
}}";

				var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        var booleanVar = true;
        await Task.Delay(1);
    }
}";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async void Task_NonAsync(string argumentValue)
			{
				var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var booleanVar = true;
        Task.Delay(1).[|ConfigureAwait({argumentValue})|].GetAwaiter().GetResult();
    }}
}}";

				var after = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var booleanVar = true;
        Task.Delay(1).GetAwaiter().GetResult();
    }
}";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async void TaskOfT(string argumentValue)
			{
				var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
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
        var booleanVar = true;
        var task = Task.FromResult(42);
        await task;
    }
}";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async void ValueTask(string argumentValue)
			{
				var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
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
        var booleanVar = true;
        var valueTask = default(ValueTask);
        await valueTask;
    }
}";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async void ValueTaskOfT(string argumentValue)
			{
				var before = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
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
        var booleanVar = true;
        var valueTask = default(ValueTask<object>);
        await valueTask;
    }
}";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}
		}
	}
}
