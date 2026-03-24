using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseBlockingTaskOperations>;

public class X1031_DoNotUseBlockingTaskOperationsTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using System.Threading.Tasks.Sources;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task WithoutProblematicWait_DoesNotTrigger() {
					await Task.Delay(1);
				}

				// IValueTaskSource.GetResult

				[Fact]
				public void IValueTaskSource_NonGeneric_GetResult_Triggers() {
					default(IValueTaskSource).[|GetResult(0)|];
					Action<IValueTaskSource> _ = vts => vts.GetResult(0);
					void LocalFunction() {
						default(IValueTaskSource).GetResult(0);
					}
				}

				// IValueTaskSource<T>.GetResult

				[Fact]
				public void IValueTaskSourceOfT_GetResult_Triggers() {
					default(IValueTaskSource<int>).[|GetResult(0)|];
					Func<IValueTaskSource<int>, int> _ = vts => vts.GetResult(0);
					void LocalFunction() {
						default(IValueTaskSource<int>).GetResult(0);
					}
				}

				// Task.Wait

				[Fact]
				public void Task_Wait_Triggers() {
					Task.Delay(1).[|Wait()|];
					Action<Task> _ = t => t.Wait();
					void LocalFunction() {
						Task.Delay(1).Wait();
					}
				}

				[Fact]
				public async Task Task_Wait_BeforeWhenAll_Triggers() {
					var task = Task.Delay(1);

					task.[|Wait()|];

					await Task.WhenAll(task);
				}

				[Fact]
				public async Task Task_Wait_ForUnawaitedTask_Triggers() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(new[] { task1 });

					task1.Wait();
					task2.[|Wait()|];
				}

				[Fact]
				public void Task_Wait_InLambda_DoesNotTrigger() {
					Task.CompletedTask.ContinueWith(x => x.Wait());
				}

				[Fact]
				public async Task Task_Wait_AfterWhenAll_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(task1, task2);

					task1.Wait();
					task2.Wait();
				}

				[Fact]
				public async Task Task_Wait_AfterWhenAny_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					var finishedTask = await Task.WhenAny(task1, task2);

					finishedTask.Wait();
				}

				// Task.WaitAny

				[Fact]
				public void Task_WaitAny_Triggers() {
					Task.[|WaitAny(Task.Delay(1))|];
					Action<Task> _ = t => Task.WaitAny(t);
					void LocalFunction() {
						Task.WaitAny(Task.Delay(1));
					}
				}

				[Fact]
				public async Task Task_WaitAny_BeforeWhenAll_Triggers() {
					var task = Task.Delay(1);

					Task.[|WaitAny(task)|];

					await Task.WhenAll(task);
				}

				[Fact]
				public async Task Task_WaitAny_ForUnawaitedTask_Triggers() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(new[] { task1 });

					Task.WaitAny(task1);
					Task.[|WaitAny(task2)|];
					Task.[|WaitAny(task1, task2)|];
				}

				[Fact]
				public void Task_WaitAny_InLambda_DoesNotTrigger() {
					Task.CompletedTask.ContinueWith(x => Task.WaitAny(x));
				}

				[Fact]
				public async Task Task_WaitAny_AfterWhenAll_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(task1, task2);

					Task.WaitAny(task1);
					Task.WaitAny(task2);
					Task.WaitAny(task1, task2);
				}

				[Fact]
				public async Task Task_WaitAny_AfterWhenAny_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					var finishedTask = await Task.WhenAny(task1, task2);

					Task.WaitAny(finishedTask);
				}

				// Task.WaitAll

				[Fact]
				public void Task_WaitAll_Triggers() {
					Task.[|WaitAll(Task.Delay(1))|];
					Action<Task> _ = t => Task.WaitAll(t);
					void LocalFunction() {
						Task.WaitAll(Task.Delay(1));
					}
				}

				[Fact]
				public async Task Task_WaitAll_BeforeWhenAll_Triggers() {
					var task = Task.Delay(1);

					Task.[|WaitAll(task)|];

					await Task.WhenAll(task);
				}

				[Fact]
				public async Task Task_WaitAll_ForUnawaitedTask_Triggers() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(new[] { task1 });

					Task.WaitAll(task1);
					Task.[|WaitAll(task2)|];
					Task.[|WaitAll(task1, task2)|];
				}

				[Fact]
				public void Task_WaitAll_InLambda_DoesNotTrigger() {
					Task.CompletedTask.ContinueWith(x => Task.WaitAll(x));
				}

				[Fact]
				public async Task Task_WaitAll_AfterWhenAll_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(task1, task2);

					Task.WaitAll(task1);
					Task.WaitAll(task2);
					Task.WaitAll(task1, task2);
				}

				[Fact]
				public async Task Task_WaitAll_AfterWhenAny_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					var finishedTask = await Task.WhenAny(task1, task2);

					Task.WaitAll(finishedTask);
				}

				// Task.GetAwaiter.GetResult

				[Fact]
				public void Task_GetResult_Triggers() {
					Task.CompletedTask.GetAwaiter().[|GetResult()|];
					Action<Task> _ = t => t.GetAwaiter().GetResult();
					void LocalFunction() {
						Task.CompletedTask.GetAwaiter().GetResult();
					}
				}

				[Fact]
				public async Task Task_GetResult_BeforeWhenAll_Triggers() {
					var task = Task.Delay(1);

					task.GetAwaiter().[|GetResult()|];

					await Task.WhenAll(task);
				}

				[Fact]
				public async Task Task_GetResult_OnUnawaitedTask_Triggers() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(new[] { task1 });

					task1.GetAwaiter().GetResult();
					task2.GetAwaiter().[|GetResult()|];
				}

				[Fact]
				public void Task_GetResult_InLambda_DoesNotTrigger() {
					Task.CompletedTask.ContinueWith(x => x.GetAwaiter().GetResult());
				}

				[Fact]
				public async Task Task_GetResult_AfterWhenAll_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					await Task.WhenAll(task1, task2);

					task1.GetAwaiter().GetResult();
					task2.GetAwaiter().GetResult();
				}

				[Fact]
				public async Task Task_GetResult_AfterWhenAny_DoesNotTrigger() {
					var task1 = Task.Delay(1);
					var task2 = Task.Delay(2);

					var finishedTask = await Task.WhenAny(task1, task2);

					finishedTask.GetAwaiter().GetResult();
				}

				// Task<T>.Result

				[Fact]
				public void TaskOfT_Result_Triggers() {
					var _ = Task.FromResult(42).[|Result|];
					Func<Task<int>, int> _2 = t => t.Result;
					void LocalFunction() {
						var _3 = Task.FromResult(42).Result;
					}
				}

				[Fact]
				public async Task TaskOfT_Result_BeforeWhenAll_Triggers() {
					var task = Task.FromResult(42);

					Assert.Equal(42, task.[|Result|]);

					await Task.WhenAll(task);
				}

				[Fact]
				public async Task TaskOfT_Result_ForUnawaitedTask_Triggers() {
					var task1 = Task.FromResult(42);
					var task2 = Task.FromResult(2112);

					await Task.WhenAll(new[] { task1 });

					Assert.Equal(42, task1.Result);
					Assert.Equal(2112, task2.[|Result|]);
					Assert.Equal(2154, task1.Result + task2.[|Result|]);
				}

				[Fact]
				public void TaskOfT_Result_InLambda_DoesNotTrigger() {
					var _ = Task.FromResult(42).ContinueWith(x => x.Result);
				}

				[Fact]
				public async Task TaskOfT_Result_AfterWhenAll_DoesNotTrigger() {
					var task1 = Task.FromResult(42);
					var task2 = Task.FromResult(2112);

					await Task.WhenAll(task1, task2);

					Assert.Equal(42, task1.Result);
					Assert.Equal(2112, task2.Result);
					Assert.Equal(2154, task1.Result + task2.Result);
				}

				[Fact]
				public async Task TaskOfT_Result_AfterWhenAny_DoesNotTrigger() {
					var task1 = Task.FromResult(42);
					var task2 = Task.FromResult(2112);

					var finishedTask = await Task.WhenAny(task1, task2);

					Assert.Equal(2600, finishedTask.Result);
				}

				// Task<T>.GetAwaiter.GetResult

				[Fact]
				public void TaskOfT_GetResult_Triggers() {
					var _ = Task.FromResult(42).GetAwaiter().[|GetResult()|];
					Func<Task<int>, int> _2 = t => t.GetAwaiter().GetResult();
					void LocalFunction() {
						var _3 = Task.FromResult(42).GetAwaiter().GetResult();
					}
				}

				[Fact]
				public async Task TaskOfT_GetResult_BeforeWhenAll_Triggers() {
					var task = Task.FromResult(42);

					Assert.Equal(42, task.GetAwaiter().[|GetResult()|]);

					await Task.WhenAll(task);
				}

				[Fact]
				public async Task TaskOfT_GetResult_OnUnawaitedTask_Triggers() {
					var task1 = Task.FromResult(42);
					var task2 = Task.FromResult(2112);

					await Task.WhenAll(new[] { task1 });

					Assert.Equal(42, task1.GetAwaiter().GetResult());
					Assert.Equal(2112, task2.GetAwaiter().[|GetResult()|]);
					Assert.Equal(2154, task1.GetAwaiter().GetResult() + task2.GetAwaiter().[|GetResult()|]);
				}

				[Fact]
				public void TaskOfT_GetResult_InLambda_DoesNotTrigger() {
					var _ = Task.FromResult(42).ContinueWith(x => x.GetAwaiter().GetResult());
				}

				[Fact]
				public async Task TaskOfT_GetResult_AfterWhenAll_DoesNotTrigger() {
					var task1 = Task.FromResult(42);
					var task2 = Task.FromResult(2112);

					await Task.WhenAll(task1, task2);

					Assert.Equal(42, task1.GetAwaiter().GetResult());
					Assert.Equal(2112, task2.GetAwaiter().GetResult());
					Assert.Equal(2154, task1.GetAwaiter().GetResult() + task2.GetAwaiter().GetResult());
				}

				[Fact]
				public async Task TaskOfT_GetResult_AfterWhenAny_DoesNotTrigger() {
					var task1 = Task.FromResult(42);
					var task2 = Task.FromResult(2112);

					var finishedTask = await Task.WhenAny(task1, task2);

					Assert.Equal(2600, finishedTask.GetAwaiter().GetResult());
				}

				// ValueTask.GetAwaiter.GetResult

				[Fact]
				public void ValueTask_GetResult_Triggers() {
					default(ValueTask).GetAwaiter().[|GetResult()|];
					Action<ValueTask> _ = vt => vt.GetAwaiter().GetResult();
					void LocalFunction() {
						default(ValueTask).GetAwaiter().GetResult();
					}
				}

				// ValueTask<T>.Result

				[Fact]
				public void ValueTaskOfT_Result_Triggers() {
					var _ = new ValueTask<int>(42).[|Result|];
					Func<ValueTask<int>, int> _2 = vt => vt.Result;
					void LocalFunction() {
						var _3 = new ValueTask<int>(42).Result;
					}
				}

				// ValueTask<T>.GetAwaiter.GetResult

				[Fact]
				public void ValueTaskOfT_GetResult_Triggers() {
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
