using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				Action ThrowingMethod = () => { throw new NotImplementedException(); };
				Task AsyncThrowingMethod() { throw new NotImplementedException(); }

				void Throws_WithNonAsyncLambda() {
					Assert.Throws(typeof(NotImplementedException), ThrowingMethod);
					Assert.Throws(typeof(NotImplementedException), () => 1);

					Assert.Throws<NotImplementedException>(ThrowingMethod);
					Assert.Throws<NotImplementedException>(() => 1);

					Assert.Throws<ArgumentException>("param1", ThrowingMethod);
					Assert.Throws<ArgumentException>("param1", () => 1);
				}

				void Throws_WithAsyncLambda() {
					{|CS0619:{|#0:Assert.Throws(typeof(NotImplementedException), AsyncThrowingMethod)|}|};
					{|CS0619:{|#1:Assert.Throws(typeof(NotImplementedException), () => Task.Delay(0))|}|};
					{|CS0619:{|#2:Assert.Throws(typeof(NotImplementedException), async () => Task.Delay(0))|}|};
					{|CS0619:{|#3:Assert.Throws(typeof(NotImplementedException), async () => Task.Delay(0).ConfigureAwait(false))|}|};

					{|CS0619:{|#10:Assert.Throws<NotImplementedException>(AsyncThrowingMethod)|}|};
					{|CS0619:{|#11:Assert.Throws<NotImplementedException>(() => Task.Delay(0))|}|};
					{|CS0619:{|#12:Assert.Throws<NotImplementedException>(async () => Task.Delay(0))|}|};
					{|CS0619:{|#13:Assert.Throws<NotImplementedException>(async () => Task.Delay(0).ConfigureAwait(false))|}|};

					{|CS0619:{|#20:Assert.Throws<ArgumentException>("param1", AsyncThrowingMethod)|}|};
					{|CS0619:{|#21:Assert.Throws<ArgumentException>("param1", () => Task.Delay(0))|}|};
					{|CS0619:{|#22:Assert.Throws<ArgumentException>("param1", async () => Task.Delay(0))|}|};
					{|CS0619:{|#23:Assert.Throws<ArgumentException>("param1", async () => Task.Delay(0).ConfigureAwait(false))|}|};
				}

				void ThrowsAsync_WithAsyncLambda() {
					Assert.ThrowsAsync(typeof(NotImplementedException), AsyncThrowingMethod);
					Assert.ThrowsAsync(typeof(NotImplementedException), () => Task.Delay(0));
					Assert.ThrowsAsync(typeof(NotImplementedException), async () => Task.Delay(0));
					Assert.ThrowsAsync(typeof(NotImplementedException), async () => Task.Delay(0).ConfigureAwait(false));

					Assert.ThrowsAsync<NotImplementedException>(AsyncThrowingMethod);
					Assert.ThrowsAsync<NotImplementedException>(() => Task.Delay(0));
					Assert.ThrowsAsync<NotImplementedException>(async () => Task.Delay(0));
					Assert.ThrowsAsync<NotImplementedException>(async () => Task.Delay(0).ConfigureAwait(false));

					Assert.ThrowsAsync<ArgumentException>("param1", AsyncThrowingMethod);
					Assert.ThrowsAsync<ArgumentException>("param1", () => Task.Delay(0));
					Assert.ThrowsAsync<ArgumentException>("param1", async () => Task.Delay(0));
					Assert.ThrowsAsync<ArgumentException>("param1", async () => Task.Delay(0).ConfigureAwait(false));
				}

				void ThrowsAny_WithNonAsyncLambda() {
					Assert.ThrowsAny<NotImplementedException>(ThrowingMethod);
					Assert.ThrowsAny<NotImplementedException>(() => 1);
				}

				void ThrowsAny_WithAsyncLambda() {
					{|CS0619:{|#30:Assert.ThrowsAny<NotImplementedException>(AsyncThrowingMethod)|}|};
					{|CS0619:{|#31:Assert.ThrowsAny<NotImplementedException>(() => Task.Delay(0))|}|};
					{|CS0619:{|#32:Assert.ThrowsAny<NotImplementedException>(async () => Task.Delay(0))|}|};
					{|CS0619:{|#33:Assert.ThrowsAny<NotImplementedException>(async () => Task.Delay(0).ConfigureAwait(false))|}|};
				}

				void ThrowsAnyAsync_WithAsyncLambda() {
					Assert.ThrowsAnyAsync<NotImplementedException>(AsyncThrowingMethod);
					Assert.ThrowsAnyAsync<NotImplementedException>(() => Task.Delay(0));
					Assert.ThrowsAnyAsync<NotImplementedException>(async () => Task.Delay(0));
					Assert.ThrowsAnyAsync<NotImplementedException>(async () => Task.Delay(0).ConfigureAwait(false));
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(3).WithArguments("Assert.Throws()", "ThrowsAsync"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(12).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(13).WithArguments("Assert.Throws()", "ThrowsAsync"),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(22).WithArguments("Assert.Throws()", "ThrowsAsync"),
			Verify.Diagnostic().WithLocation(23).WithArguments("Assert.Throws()", "ThrowsAsync"),

			Verify.Diagnostic().WithLocation(30).WithArguments("Assert.ThrowsAny()", "ThrowsAnyAsync"),
			Verify.Diagnostic().WithLocation(31).WithArguments("Assert.ThrowsAny()", "ThrowsAnyAsync"),
			Verify.Diagnostic().WithLocation(32).WithArguments("Assert.ThrowsAny()", "ThrowsAnyAsync"),
			Verify.Diagnostic().WithLocation(33).WithArguments("Assert.ThrowsAny()", "ThrowsAnyAsync"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
