using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AsyncAssertsShouldBeAwaited>;

public class X2021_AsyncAssertsShouldBeAwaitedTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var code = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.ComponentModel;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : INotifyPropertyChanged {
				void UnawaitedNonAssertion_DoesNotTrigger() {
					Task.Delay(1);
				}

				public int Property { get; set; }

				public event PropertyChangedEventHandler? PropertyChanged;
				public event EventHandler? SimpleEvent;
				public event EventHandler<int>? SimpleIntEvent;

				async void AwaitedAssert_DoesNotTrigger() {
					await Assert.AllAsync(default(IEnumerable<int>), i => Task.FromResult(true));
					await Assert.CollectionAsync(default(IEnumerable<int>));
					await Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException());
					await Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException());
					await Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException());
					await Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException());
					await Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException());
					await Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException());
					await Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException());
					await Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException());
				}

				async void AssertionWithConsumption_DoesNotTrigger() {
					MyTaskExtensions.ConsumeTask(Assert.AllAsync(default(IEnumerable<int>), i => Task.FromResult(true)));
					MyTaskExtensions.ConsumeTask(Assert.CollectionAsync(default(IEnumerable<int>)));
					MyTaskExtensions.ConsumeTask(Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException()));
					MyTaskExtensions.ConsumeTask(Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException()));
					MyTaskExtensions.ConsumeTask(Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException()));
					MyTaskExtensions.ConsumeTask(Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException()));
					MyTaskExtensions.ConsumeTask(Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException()));
					MyTaskExtensions.ConsumeTask(Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException()));
					MyTaskExtensions.ConsumeTask(Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException()));
					MyTaskExtensions.ConsumeTask(Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException()));
				}

				async void AssertionWithConsumptionViaExtension_DoesNotTrigger() {
					Assert.AllAsync(default(IEnumerable<int>), i => Task.FromResult(true)).ConsumeTask();
					Assert.CollectionAsync(default(IEnumerable<int>)).ConsumeTask();
					Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException()).ConsumeTask();
					Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException()).ConsumeTask();
					Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException()).ConsumeTask();
					Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException()).ConsumeTask();
					Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException()).ConsumeTask();
					Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException()).ConsumeTask();
					Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException()).ConsumeTask();
					Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException()).ConsumeTask();
				}

				async void StoredTask_DoesNotTrigger() {
					var task0 = Assert.AllAsync(default(IEnumerable<int>), i => Task.FromResult(true));
					var task1 = Assert.CollectionAsync(default(IEnumerable<int>));
					var task2 = Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException());
					var task3 = Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException());
					var task4 = Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException());
					var task5 = Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException());
					var task6 = Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException());
					var task7 = Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException());
					var task8 = Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException());
					var task9 = Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException());
				}

				async void AssertionWithoutAwait_Triggers() {
					{|#0:Assert.AllAsync(default(IEnumerable<int>), i => Task.FromResult(true))|};
					{|#1:Assert.CollectionAsync(default(IEnumerable<int>))|};
					{|#2:Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException())|};
					{|#3:Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException())|};
					{|#4:Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())|};
					{|#5:Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())|};
					{|#6:Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException())|};
					{|#7:Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException())|};
					{|#8:Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException())|};
					{|#9:Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException())|};
				}

				async void AssertionWithUnawaitedContinuation_Triggers() {
					{|#10:Assert.AllAsync(default(IEnumerable<int>), i => Task.FromResult(true))|}.ContinueWith(t => { });
					{|#11:Assert.CollectionAsync(default(IEnumerable<int>))|}.ContinueWith(t => { });
					{|#12:Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
					{|#13:Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
					{|#14:Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
					{|#15:Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
					{|#16:Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
					{|#17:Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
					{|#18:Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
					{|#19:Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException())|}.ContinueWith(t => { });
				}
			}

			public static class MyTaskExtensions {
				public static void ConsumeTask(this Task t) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("AllAsync"),
			Verify.Diagnostic().WithLocation(1).WithArguments("CollectionAsync"),
			Verify.Diagnostic().WithLocation(2).WithArguments("PropertyChangedAsync"),
			Verify.Diagnostic().WithLocation(3).WithArguments("RaisesAnyAsync"),
			Verify.Diagnostic().WithLocation(4).WithArguments("RaisesAnyAsync"),
			Verify.Diagnostic().WithLocation(5).WithArguments("RaisesAsync"),
			Verify.Diagnostic().WithLocation(6).WithArguments("ThrowsAnyAsync"),
			Verify.Diagnostic().WithLocation(7).WithArguments("ThrowsAsync"),
			Verify.Diagnostic().WithLocation(8).WithArguments("ThrowsAsync"),
			Verify.Diagnostic().WithLocation(9).WithArguments("ThrowsAsync"),

			Verify.Diagnostic().WithLocation(10).WithArguments("AllAsync"),
			Verify.Diagnostic().WithLocation(11).WithArguments("CollectionAsync"),
			Verify.Diagnostic().WithLocation(12).WithArguments("PropertyChangedAsync"),
			Verify.Diagnostic().WithLocation(13).WithArguments("RaisesAnyAsync"),
			Verify.Diagnostic().WithLocation(14).WithArguments("RaisesAnyAsync"),
			Verify.Diagnostic().WithLocation(15).WithArguments("RaisesAsync"),
			Verify.Diagnostic().WithLocation(16).WithArguments("ThrowsAnyAsync"),
			Verify.Diagnostic().WithLocation(17).WithArguments("ThrowsAsync"),
			Verify.Diagnostic().WithLocation(18).WithArguments("ThrowsAsync"),
			Verify.Diagnostic().WithLocation(19).WithArguments("ThrowsAsync"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code, expected);
	}

#if NETCOREAPP

	[Fact]
	public async ValueTask V2_and_V3_AsyncEnumerable()
	{
		var code = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				async void AwaitedAssert_DoesNotTrigger() {
					await Assert.AllAsync(default(IAsyncEnumerable<int>), i => Task.FromResult(true));
					await Assert.CollectionAsync(default(IAsyncEnumerable<int>));
				}

				async void AssertionWithConsumption_DoesNotTrigger() {
					MyTaskExtensions.ConsumeTask(Assert.AllAsync(default(IAsyncEnumerable<int>), i => Task.FromResult(true)));
					MyTaskExtensions.ConsumeTask(Assert.CollectionAsync(default(IAsyncEnumerable<int>)));
				}

				async void AssertionWithConsumptionViaExtension_DoesNotTrigger() {
					Assert.AllAsync(default(IAsyncEnumerable<int>), i => Task.FromResult(true)).ConsumeTask();
					Assert.CollectionAsync(default(IAsyncEnumerable<int>)).ConsumeTask();
				}

				async void StoredTask_DoesNotTrigger() {
					var task0 = Assert.AllAsync(default(IAsyncEnumerable<int>), i => Task.FromResult(true));
					var task1 = Assert.CollectionAsync(default(IAsyncEnumerable<int>));
				}

				async void AssertionWithoutAwait_Triggers() {
					{|#0:Assert.AllAsync(default(IAsyncEnumerable<int>), i => Task.FromResult(true))|};
					{|#1:Assert.CollectionAsync(default(IAsyncEnumerable<int>))|};
				}

				async void AssertionWithUnawaitedContinuation_Triggers() {
					{|#10:Assert.AllAsync(default(IAsyncEnumerable<int>), i => Task.FromResult(true))|}.ContinueWith(t => { });
					{|#11:Assert.CollectionAsync(default(IAsyncEnumerable<int>))|}.ContinueWith(t => { });
				}
			}

			public static class MyTaskExtensions {
				public static void ConsumeTask(this Task t) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("AllAsync"),
			Verify.Diagnostic().WithLocation(1).WithArguments("CollectionAsync"),

			Verify.Diagnostic().WithLocation(10).WithArguments("AllAsync"),
			Verify.Diagnostic().WithLocation(11).WithArguments("CollectionAsync"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code, expected);
	}

#endif  // NETCOREAPP
}
