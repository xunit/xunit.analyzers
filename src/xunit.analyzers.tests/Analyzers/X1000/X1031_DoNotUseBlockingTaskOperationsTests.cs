using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseBlockingTaskOperations>;

// TODO: Combine these tests once we've determined why some are failing

public class X1031_DoNotUseBlockingTaskOperationsTests
{
	[Fact]
	public async ValueTask SuccessCase()
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
		public async ValueTask GetResult_Triggers()
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
		public async ValueTask GetResult_Triggers()
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
			public async ValueTask Wait_Triggers()
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
			public async ValueTask Wait_BeforeWhenAll_Triggers()
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
			public async ValueTask Wait_ForUnawaitedTask_Triggers()
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
			public async ValueTask Wait_InLambda_DoesNotTrigger()
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
			public async ValueTask Wait_AfterWhenAll_DoesNotTrigger()
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
			public async ValueTask Wait_AfterWhenAny_DoesNotTrigger()
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
			[Fact]
			public async ValueTask WaitMethod_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System;
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public void TestMethod1() {
							Task.[|WaitAny(Task.Delay(1))|];
							Action<Task> _ = t => Task.WaitAny(t);
							void LocalFunction() {
								Task.WaitAny(Task.Delay(1));
							}
						}

						[Fact]
						public void TestMethod2() {
							Task.[|WaitAll(Task.Delay(1))|];
							Action<Task> _ = t => Task.WaitAll(t);
							void LocalFunction() {
								Task.WaitAll(Task.Delay(1));
							}
						}
					}
					""";

				await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
			}

			[Fact]
			public async ValueTask WaitMethod_BeforeWhenAll_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod1() {
							var task = Task.Delay(1);

							Task.[|WaitAny(task)|];

							await Task.WhenAll(task);
						}

						[Fact]
						public async Task TestMethod2() {
							var task = Task.Delay(1);

							Task.[|WaitAll(task)|];

							await Task.WhenAll(task);
						}
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async ValueTask WaitMethod_ForUnawaitedTask_Triggers()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod1() {
							var task1 = Task.Delay(1);
							var task2 = Task.Delay(2);

							await Task.WhenAll(new[] { task1 });

							Task.WaitAny(task1);
							Task.[|WaitAny(task2)|];
							Task.[|WaitAny(task1, task2)|];
						}

						[Fact]
						public async Task TestMethod2() {
							var task1 = Task.Delay(1);
							var task2 = Task.Delay(2);

							await Task.WhenAll(new[] { task1 });

							Task.WaitAll(task1);
							Task.[|WaitAll(task2)|];
							Task.[|WaitAll(task1, task2)|];
						}
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async ValueTask WaitMethod_InLambda_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public void TestMethod() {
							Task.CompletedTask.ContinueWith(x => Task.WaitAny(x));
							Task.CompletedTask.ContinueWith(x => Task.WaitAll(x));
						}
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async ValueTask WaitMethod_AfterWhenAll_DoesNotTrigger()
			{
				var source = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod1() {
							var task1 = Task.Delay(1);
							var task2 = Task.Delay(2);

							await Task.WhenAll(task1, task2);

							Task.WaitAny(task1);
							Task.WaitAny(task2);
							Task.WaitAny(task1, task2);
						}

						[Fact]
						public async Task TestMethod2() {
							var task1 = Task.Delay(1);
							var task2 = Task.Delay(2);

							await Task.WhenAll(task1, task2);

							Task.WaitAll(task1);
							Task.WaitAll(task2);
							Task.WaitAll(task1, task2);
						}
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async ValueTask WaitMethod_AfterWhenAny_DoesNotTrigger()
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

							Task.WaitAny(finishedTask);
							Task.WaitAll(finishedTask);
						}
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class GetAwaiterGetResult
		{
			[Fact]
			public async ValueTask GetResult_Triggers()
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
			public async ValueTask GetResult_BeforeWhenAll_Triggers()
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
			public async ValueTask GetResult_OnUnawaitedTask_Triggers()
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
			public async ValueTask GetResult_InLambda_DoesNotTrigger()
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
			public async ValueTask GetResult_AfterWhenAll_DoesNotTrigger()
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
			public async ValueTask GetResult_AfterWhenAny_DoesNotTrigger()
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
			public async ValueTask Result_Triggers()
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
			public async ValueTask Result_BeforeWhenAll_Triggers()
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
			public async ValueTask Result_ForUnawaitedTask_Triggers()
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
			public async ValueTask Result_InLambda_DoesNotTrigger()
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
			public async ValueTask Result_AfterWhenAll_DoesNotTrigger()
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
			public async ValueTask Result_AfterWhenAny_DoesNotTrigger()
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
			public async ValueTask GetResult_Triggers()
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
			public async ValueTask GetResult_BeforeWhenAll_Triggers()
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
			public async ValueTask GetResult_OnUnawaitedTask_Triggers()
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
			public async ValueTask GetResult_InLambda_DoesNotTrigger()
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
			public async ValueTask GetResult_AfterWhenAll_DoesNotTrigger()
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
			public async ValueTask GetResult_AfterWhenAny_DoesNotTrigger()
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
		public async ValueTask GetResult_Triggers()
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
		public async ValueTask Result_Triggers()
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
		public async ValueTask GetResult_Triggers()
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
