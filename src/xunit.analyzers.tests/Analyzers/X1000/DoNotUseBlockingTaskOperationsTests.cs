using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseBlockingTaskOperations>;

public class DoNotUseBlockingTaskOperationsTests
{
	[Fact]
	public async Task SuccessCase()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
			    [Fact]
			    public async Task TestMethod() {
			        await Task.Delay(1);
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	public class IValueTaskSource_NonGeneric
	{
		[Fact]
		public async Task GetResult_Triggers()
		{
			var source = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks.Sources;
				using Xunit;

				public class TestClass {
				    [Fact]
				    public void TestMethod() {
				        default(IValueTaskSource).[|GetResult(0)|];
				        Action<IValueTaskSource> _ = vts => vts.GetResult(0);
				        void LocalFunction() {
				            default(IValueTaskSource).GetResult(0);
				        }
				    }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}
	}

	public class IValueTaskSource_Generic
	{
		[Fact]
		public async Task GetResult_Triggers()
		{
			var source = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks.Sources;
				using Xunit;

				public class TestClass {
				    [Fact]
				    public void TestMethod() {
				        default(IValueTaskSource<int>).[|GetResult(0)|];
				        Func<IValueTaskSource<int>, int> _ = vts => vts.GetResult(0);
				        void LocalFunction() {
				            default(IValueTaskSource<int>).GetResult(0);
				        }
				    }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}
	}

	public class Task_NonGeneric
	{
		public class Wait
		{
			[Fact]
			public async Task Wait_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System;
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        Task.Delay(1).[|Wait()|];
					        Action<Task> _ = t => t.Wait();
					        void LocalFunction() {
					            Task.Delay(1).Wait();
					        }
					    }
					}
					""";

				await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
			}

			[Fact]
			public async Task Wait_BeforeWhenAll_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task = Task.Delay(1);

					        task.[|Wait()|];

					        await Task.WhenAll(task);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Wait_ForUnawaitedTask_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        await Task.WhenAll(new[] { task1 });

					        task1.Wait();
					        task2.[|Wait()|];
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Wait_InLambda_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        Task.CompletedTask.ContinueWith(x => x.Wait());
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Wait_AfterWhenAll_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        await Task.WhenAll(task1, task2);

					        task1.Wait();
					        task2.Wait();
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Wait_AfterWhenAny_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        var finishedTask = await Task.WhenAny(task1, task2);

					        finishedTask.Wait();
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class WaitAny_WaitAll
		{
			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async Task WaitMethod_Triggers(string waitMethod)
			{
				var source = string.Format(/* lang=c#-test */ """
					using System;
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
					    [Fact]
					    public void TestMethod() {{
					        Task.[|{0}(Task.Delay(1))|];
					        Action<Task> _ = t => Task.{0}(t);
					        void LocalFunction() {{
					            Task.{0}(Task.Delay(1));
					        }}
					    }}
					}}
					""", waitMethod);

				await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async Task WaitMethod_BeforeWhenAll_Triggers(string waitMethod)
			{
				var source = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
					    [Fact]
					    public async Task TestMethod() {{
					        var task = Task.Delay(1);

					        Task.[|{0}(task)|];

					        await Task.WhenAll(task);
					    }}
					}}
					""", waitMethod);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async Task WaitMethod_ForUnawaitedTask_Triggers(string waitMethod)
			{
				var source = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
					    [Fact]
					    public async Task TestMethod() {{
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        await Task.WhenAll(new[] {{ task1 }});

					        Task.{0}(task1);
					        Task.[|{0}(task2)|];
					        Task.[|{0}(task1, task2)|];
					    }}
					}}
					""", waitMethod);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async Task WaitMethod_InLambda_DoesNotTrigger(string waitMethod)
			{
				var source = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
					    [Fact]
					    public void TestMethod() {{
					        Task.CompletedTask.ContinueWith(x => Task.{0}(x));
					    }}
					}}
					""", waitMethod);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async Task WaitMethod_AfterWhenAll_DoesNotTrigger(string waitMethod)
			{
				var source = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
					    [Fact]
					    public async Task TestMethod() {{
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        await Task.WhenAll(task1, task2);

					        Task.{0}(task1);
					        Task.{0}(task2);
					        Task.{0}(task1, task2);
					    }}
					}}
					""", waitMethod);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("WaitAny")]
			[InlineData("WaitAll")]
			public async Task WaitMethod_AfterWhenAny_DoesNotTrigger(string waitMethod)
			{
				var source = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
					    [Fact]
					    public async Task TestMethod() {{
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        var finishedTask = await Task.WhenAny(task1, task2);

					        Task.{0}(finishedTask);
					    }}
					}}
					""", waitMethod);

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class GetAwaiterGetResult
		{
			[Fact]
			public async Task GetResult_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System;
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        Task.CompletedTask.GetAwaiter().[|GetResult()|];
					        Action<Task> _ = t => t.GetAwaiter().GetResult();
					        void LocalFunction() {
					            Task.CompletedTask.GetAwaiter().GetResult();
					        }
					    }
					}
					""";

				await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
			}

			[Fact]
			public async Task GetResult_BeforeWhenAll_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task = Task.Delay(1);

					        task.GetAwaiter().[|GetResult()|];

					        await Task.WhenAll(task);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_OnUnawaitedTask_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        await Task.WhenAll(new[] { task1 });

					        task1.GetAwaiter().GetResult();
					        task2.GetAwaiter().[|GetResult()|];
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_InLambda_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        Task.CompletedTask.ContinueWith(x => x.GetAwaiter().GetResult());
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_AfterWhenAll_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        await Task.WhenAll(task1, task2);

					        task1.GetAwaiter().GetResult();
					        task2.GetAwaiter().GetResult();
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_AfterWhenAny_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.Delay(1);
					        var task2 = Task.Delay(2);

					        var finishedTask = await Task.WhenAny(task1, task2);

					        finishedTask.GetAwaiter().GetResult();
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}
		}
	}

	public class Task_Generic
	{
		public class Result
		{
			[Fact]
			public async Task Result_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System;
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        var _ = Task.FromResult(42).[|Result|];
					        Func<Task<int>, int> _2 = t => t.Result;
					        void LocalFunction() {
					            var _3 = Task.FromResult(42).Result;
					        }
					    }
					}
					""";

				await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
			}

			[Fact]
			public async Task Result_BeforeWhenAll_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task = Task.FromResult(42);

					        Assert.Equal(42, task.[|Result|]);

					        await Task.WhenAll(task);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Result_ForUnawaitedTask_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.FromResult(42);
					        var task2 = Task.FromResult(2112);

					        await Task.WhenAll(new[] { task1 });

					        Assert.Equal(42, task1.Result);
					        Assert.Equal(2112, task2.[|Result|]);
					        Assert.Equal(2154, task1.Result + task2.[|Result|]);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Result_InLambda_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        var _ = Task.FromResult(42).ContinueWith(x => x.Result);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Result_AfterWhenAll_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.FromResult(42);
					        var task2 = Task.FromResult(2112);

					        await Task.WhenAll(task1, task2);

					        Assert.Equal(42, task1.Result);
					        Assert.Equal(2112, task2.Result);
					        Assert.Equal(2154, task1.Result + task2.Result);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task Result_AfterWhenAny_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.FromResult(42);
					        var task2 = Task.FromResult(2112);

					        var finishedTask = await Task.WhenAny(task1, task2);

					        Assert.Equal(2600, finishedTask.Result);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class GetAwaiterGetResult
		{
			[Fact]
			public async Task GetResult_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System;
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        var _ = Task.FromResult(42).GetAwaiter().[|GetResult()|];
					        Func<Task<int>, int> _2 = t => t.GetAwaiter().GetResult();
					        void LocalFunction() {
					            var _3 = Task.FromResult(42).GetAwaiter().GetResult();
					        }
					    }
					}
					""";

				await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
			}

			[Fact]
			public async Task GetResult_BeforeWhenAll_Triggers()
			{
				var source = /* lang=c#-test */ """

					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task = Task.FromResult(42);

					        Assert.Equal(42, task.GetAwaiter().[|GetResult()|]);

					        await Task.WhenAll(task);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_OnUnawaitedTask_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.FromResult(42);
					        var task2 = Task.FromResult(2112);

					        await Task.WhenAll(new[] { task1 });

					        Assert.Equal(42, task1.GetAwaiter().GetResult());
					        Assert.Equal(2112, task2.GetAwaiter().[|GetResult()|]);
					        Assert.Equal(2154, task1.GetAwaiter().GetResult() + task2.GetAwaiter().[|GetResult()|]);
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_InLambda_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public void TestMethod() {
					        var _ = Task.FromResult(42).ContinueWith(x => x.GetAwaiter().GetResult());
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_AfterWhenAll_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.FromResult(42);
					        var task2 = Task.FromResult(2112);

					        await Task.WhenAll(task1, task2);

					        Assert.Equal(42, task1.GetAwaiter().GetResult());
					        Assert.Equal(2112, task2.GetAwaiter().GetResult());
					        Assert.Equal(2154, task1.GetAwaiter().GetResult() + task2.GetAwaiter().GetResult());
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task GetResult_AfterWhenAny_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
					    [Fact]
					    public async Task TestMethod() {
					        var task1 = Task.FromResult(42);
					        var task2 = Task.FromResult(2112);

					        var finishedTask = await Task.WhenAny(task1, task2);

					        Assert.Equal(2600, finishedTask.GetAwaiter().GetResult());
					    }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}
		}
	}

	public class ValueTask_NonGeneric
	{
		[Fact]
		public async Task GetResult_Triggers()
		{
			var source = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {
				    [Fact]
				    public void TestMethod() {
				        default(ValueTask).GetAwaiter().[|GetResult()|];
				        Action<ValueTask> _ = vt => vt.GetAwaiter().GetResult();
				        void LocalFunction() {
				            default(ValueTask).GetAwaiter().GetResult();
				        }
				    }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}
	}

	public class ValueTask_Generic
	{
		[Fact]
		public async Task Result_Triggers()
		{
			var source = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {
				    [Fact]
				    public void TestMethod() {
				        var _ = new ValueTask<int>(42).[|Result|];
				        Func<ValueTask<int>, int> _2 = vt => vt.Result;
				        void LocalFunction() {
				            var _3 = new ValueTask<int>(42).Result;
				        }
				    }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}

		[Fact]
		public async Task GetResult_Triggers()
		{
			var source = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {
				    [Fact]
				    public void TestMethod() {
				        var _ = new ValueTask<int>(42).GetAwaiter().[|GetResult()|];
				        Func<ValueTask<int>, int> _2 = vt => vt.GetAwaiter().GetResult();
				        void LocalFunction() {
				            var _3 = new ValueTask<int>(42).GetAwaiter().GetResult();
				        }
				    }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}
	}
}
