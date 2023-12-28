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
using System;
using System.Threading.Tasks.Sources;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        default(IValueTaskSource).[|GetResult(0)|];
        Action<IValueTaskSource> _ = vts => vts.GetResult(0);
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
using System;
using System.Threading.Tasks.Sources;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        default(IValueTaskSource<int>).[|GetResult(0)|];
        Func<IValueTaskSource<int>, int> _ = vts => vts.GetResult(0);
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
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.Delay(1).[|Wait()|];
        Action<Task> _ = t => t.Wait();
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_BeforeWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task = Task.Delay(1);

        task.[|Wait()|];

        await Task.WhenAll(task);
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_WhenAllForOtherTask()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        await Task.WhenAll(new[] { task1 });

        task1.Wait();
        task2.[|Wait()|];
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

			[Fact]
			public async void SuccessCase_AfterWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        await Task.WhenAll(task1, task2);

        task1.Wait();
        task2.Wait();
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_AfterWhenAny()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        var finishedTask = await Task.WhenAny(task1, task2);

        finishedTask.Wait();
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
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        Task.[|{waitMethod}(Task.Delay(1))|];
        Action<Task> _ = t => Task.{waitMethod}(t);
    }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async void FailureCase_BeforeWhenAll(string waitMethod)
			{
				var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async void TestMethod() {{
        var task = Task.Delay(1);

        Task.[|{waitMethod}(task)|];

        await Task.WhenAll(task);
    }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async void FailureCase_WhenAllForOtherTask(string waitMethod)
			{
				var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async void TestMethod() {{
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        await Task.WhenAll(new[] {{ task1 }});

        Task.{waitMethod}(task1);
        Task.[|{waitMethod}(task2)|];
        Task.[|{waitMethod}(task1, task2)|];
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

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async void SuccessCase_AfterWhenAll(string waitMethod)
			{
				var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async void TestMethod() {{
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        await Task.WhenAll(task1, task2);

        Task.{waitMethod}(task1);
        Task.{waitMethod}(task2);
        Task.{waitMethod}(task1, task2);
    }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async void SuccessCase_AfterWhenAny(string waitMethod)
			{
				var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async void TestMethod() {{
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        var finishedTask = await Task.WhenAny(task1, task2);

        Task.{waitMethod}(finishedTask);
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
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.CompletedTask.GetAwaiter().[|GetResult()|];
        Action<Task> _ = t => t.GetAwaiter().GetResult();
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_BeforeWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task = Task.Delay(1);

        task.GetAwaiter().[|GetResult()|];

        await Task.WhenAll(task);
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_WhenAllForOtherTask()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        await Task.WhenAll(new[] { task1 });

        task1.GetAwaiter().GetResult();
        task2.GetAwaiter().[|GetResult()|];
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

			[Fact]
			public async void SuccessCase_AfterWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        await Task.WhenAll(task1, task2);

        task1.GetAwaiter().GetResult();
        task2.GetAwaiter().GetResult();
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_AfterWhenAny()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.Delay(1);
        var task2 = Task.Delay(2);

        var finishedTask = await Task.WhenAny(task1, task2);

        finishedTask.GetAwaiter().GetResult();
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
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = Task.FromResult(42).[|Result|];
        Func<Task<int>, int> _2 = t => t.Result;
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_BeforeWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task = Task.FromResult(42);

        Assert.Equal(42, task.[|Result|]);

        await Task.WhenAll(task);
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_WhenAllForOtherTask()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.FromResult(42);
        var task2 = Task.FromResult(2112);

        await Task.WhenAll(new[] { task1 });

        Assert.Equal(42, task1.Result);
        Assert.Equal(2112, task2.[|Result|]);
        Assert.Equal(2154, task1.Result + task2.[|Result|]);
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

			[Fact]
			public async void SuccessCase_AfterWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.FromResult(42);
        var task2 = Task.FromResult(2112);

        await Task.WhenAll(task1, task2);

        Assert.Equal(42, task1.Result);
        Assert.Equal(2112, task2.Result);
        Assert.Equal(2154, task1.Result + task2.Result);
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_AfterWhenAny()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.FromResult(42);
        var task2 = Task.FromResult(2112);

        var finishedTask = await Task.WhenAny(task1, task2);

        Assert.Equal(2600, finishedTask.Result);
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
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = Task.FromResult(42).GetAwaiter().[|GetResult()|];
        Func<Task<int>, int> _2 = t => t.GetAwaiter().GetResult();
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_BeforeWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task = Task.FromResult(42);

        Assert.Equal(42, task.GetAwaiter().[|GetResult()|]);

        await Task.WhenAll(task);
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void FailureCase_WhenAllForOtherTask()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.FromResult(42);
        var task2 = Task.FromResult(2112);

        await Task.WhenAll(new[] { task1 });

        Assert.Equal(42, task1.GetAwaiter().GetResult());
        Assert.Equal(2112, task2.GetAwaiter().[|GetResult()|]);
        Assert.Equal(2154, task1.GetAwaiter().GetResult() + task2.GetAwaiter().[|GetResult()|]);
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

			[Fact]
			public async void SuccessCase_AfterWhenAll()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.FromResult(42);
        var task2 = Task.FromResult(2112);

        await Task.WhenAll(task1, task2);

        Assert.Equal(42, task1.GetAwaiter().GetResult());
        Assert.Equal(2112, task2.GetAwaiter().GetResult());
        Assert.Equal(2154, task1.GetAwaiter().GetResult() + task2.GetAwaiter().GetResult());
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async void SuccessCase_AfterWhenAny()
			{
				var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        var task1 = Task.FromResult(42);
        var task2 = Task.FromResult(2112);

        var finishedTask = await Task.WhenAny(task1, task2);

        Assert.Equal(2600, finishedTask.GetAwaiter().GetResult());
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
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        default(ValueTask).GetAwaiter().[|GetResult()|];
        Action<ValueTask> _ = vt => vt.GetAwaiter().GetResult();
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
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = new ValueTask<int>(42).[|Result|];
        Func<ValueTask<int>, int> _2 = vt => vt.Result;
    }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void FailureCase_GetAwaiterGetResult()
		{
			var source = @"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var _ = new ValueTask<int>(42).GetAwaiter().[|GetResult()|];
        Func<ValueTask<int>, int> _2 = vt => vt.GetAwaiter().GetResult();
    }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}
}
