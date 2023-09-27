using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseBlockingTaskOperations>;

public class DoNotUseBlockingTaskOperationsTests
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

	public class IValueTaskSource_NonGeneric
	{
		[Fact]
		public async void FailureCase_GetResult()
		{
			var source = @"
using System.Threading.Tasks.Sources;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        default(IValueTaskSource).[|GetResult(0)|];
    }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class IValueTaskSource_Generic
	{
		[Fact]
		public async void FailureCase_GetResult()
		{
			var source = @"
using System.Threading.Tasks.Sources;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        default(IValueTaskSource<int>).[|GetResult(0)|];
    }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class Task_NonGeneric
	{
		public class Wait
		{
			[Fact]
			public async void FailureCase()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.Delay(1).[|Wait()|];
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_InContinueWithLambda()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.CompletedTask.ContinueWith(x => x.Wait());
    }
}";

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class WaitAny_WaitAll
		{
			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async void FailureCase(string waitMethod)
			{
				var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        Task.[|{waitMethod}(Task.Delay(1))|];
    }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async void SuccessCase_InContinueWithLambda(string waitMethod)
			{
				var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        Task.CompletedTask.ContinueWith(x => Task.{waitMethod}(x));
    }}
}}";

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class GetAwaiterGetResult
		{
			[Fact]
			public async void FailureCase()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.CompletedTask.GetAwaiter().[|GetResult()|];
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_InContinueWithLambda()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.CompletedTask.ContinueWith(x => x.GetAwaiter().GetResult());
    }
}";

				await Verify.VerifyAnalyzer(source);
			}
		}
	}

	public class Task_Generic
	{
		public class Result
		{
			[Fact]
			public async void FailureCase()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = Task.FromResult(42).[|Result|];
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_InContinueWithLambda()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = Task.FromResult(42).ContinueWith(x => x.Result);
    }
}";

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class GetAwaiterGetResult
		{
			[Fact]
			public async void FailureCase()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = Task.FromResult(42).GetAwaiter().[|GetResult()|];
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_InContinueWithLambda()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = Task.FromResult(42).ContinueWith(x => x.GetAwaiter().GetResult());
    }
}";

				await Verify.VerifyAnalyzer(source);
			}
		}
	}

	public class ValueTask_NonGeneric
	{
		[Fact]
		public async void FailureCase_GetAwaiterGetResult()
		{
			var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        default(ValueTask).GetAwaiter().[|GetResult()|];
    }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class ValueTask_Generic
	{
		[Fact]
		public async void FailureCase_Result()
		{
			var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = new ValueTask<int>(42).[|Result|];
    }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void FailureCase_GetAwaiterGetResult()
		{
			var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = new ValueTask<int>(42).GetAwaiter().[|GetResult()|];
    }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}
}
