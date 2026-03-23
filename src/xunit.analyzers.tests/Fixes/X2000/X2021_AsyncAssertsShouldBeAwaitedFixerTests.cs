using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AsyncAssertsShouldBeAwaited>;

public class X2021_AsyncAssertsShouldBeAwaitedFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.ComponentModel;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass : INotifyPropertyChanged {
				public int Property { get; set; }

				public event PropertyChangedEventHandler? PropertyChanged;
				public event EventHandler? SimpleEvent;
				public event EventHandler<int>? SimpleIntEvent;

				void TestMethod1() {
					[|Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException())|];
				}

				void TestMethod2() {
					[|Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException())|];
				}

				void TestMethod3() {
					[|Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())|];
				}

				void TestMethod4() {
					[|Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())|];
				}

				void TestMethod5() {
					[|Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException())|];
				}

				void TestMethod6() {
					[|Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException())|];
				}

				void TestMethod7() {
					[|Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException())|];
				}

				void TestMethod8() {
					[|Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException())|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.ComponentModel;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass : INotifyPropertyChanged {
				public int Property { get; set; }

				public event PropertyChangedEventHandler? PropertyChanged;
				public event EventHandler? SimpleEvent;
				public event EventHandler<int>? SimpleIntEvent;

				async Task TestMethod1() {
					await Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException());
				}

				async Task TestMethod2() {
					await Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException());
				}

				async Task TestMethod3() {
					await Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException());
				}

				async Task TestMethod4() {
					await Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException());
				}

				async Task TestMethod5() {
					await Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException());
				}

				async Task TestMethod6() {
					await Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException());
				}

				async Task TestMethod7() {
					await Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException());
				}

				async Task TestMethod8() {
					await Assert.ThrowsAsync<ArgumentException>("argName", async () => throw new DivideByZeroException());
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(LanguageVersion.CSharp8, before, after, AsyncAssertsShouldBeAwaitedFixer.Key_AddAwait);
	}
}
