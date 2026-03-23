using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				void AssertInMethod() {
					{|CS0619:[|Assert.Throws(typeof(Exception), AsyncThrowingMethod)|]|};
					{|CS0619:[|Assert.Throws(typeof(Exception), () => Task.Delay(0))|]|};
					{|CS0619:[|Assert.Throws(typeof(Exception), async () => await Task.Delay(0))|]|};
					{|CS0619:[|Assert.Throws(typeof(Exception), async () => await Task.Delay(0).ConfigureAwait(false))|]|};

					{|CS0619:[|Assert.Throws<Exception>(AsyncThrowingMethod)|]|};
					{|CS0619:[|Assert.Throws<Exception>(() => Task.Delay(0))|]|};
					{|CS0619:[|Assert.Throws<Exception>(async () => await Task.Delay(0))|]|};
					{|CS0619:[|Assert.Throws<Exception>(async () => await Task.Delay(0).ConfigureAwait(false))|]|};

					{|CS0619:[|Assert.Throws<ArgumentException>("param", AsyncThrowingMethod)|]|};
					{|CS0619:[|Assert.Throws<ArgumentException>("param", () => Task.Delay(0))|]|};
					{|CS0619:[|Assert.Throws<ArgumentException>("param", async () => await Task.Delay(0))|]|};
					{|CS0619:[|Assert.Throws<ArgumentException>("param", async () => await Task.Delay(0).ConfigureAwait(false))|]|};

					{|CS0619:[|Assert.ThrowsAny<Exception>(AsyncThrowingMethod)|]|};
					{|CS0619:[|Assert.ThrowsAny<Exception>(() => Task.Delay(0))|]|};
					{|CS0619:[|Assert.ThrowsAny<Exception>(async () => await Task.Delay(0))|]|};
					{|CS0619:[|Assert.ThrowsAny<Exception>(async () => await Task.Delay(0).ConfigureAwait(false))|]|};
				}

				void AssertInLambda() {
					Func<int> function = () => {
						{|CS0619:[|Assert.Throws(typeof(Exception), AsyncThrowingMethod)|]|};
						return 0;
					};

					int number = function();
					function();
				}

				void AssertInLambda_Uninvoked() {
					Action<int> function = (int x) => {
						{|CS0619:[|Assert.Throws(typeof(Exception), AsyncThrowingMethod)|]|};
					};
				}

				void AssertInLocalFunction() {
					int function() {
						{|CS0619:[|Assert.Throws(typeof(Exception), AsyncThrowingMethod)|]|};
						return 0;
					};

					int number = function();
					function();
				}

				void AssertInLocalFunction_Uninvoked() {
					void function(int x) {
						{|CS0619:[|Assert.Throws(typeof(Exception), AsyncThrowingMethod)|]|};
					};
				}

				Task AsyncThrowingMethod() { throw new NotImplementedException(); }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				async Task AssertInMethod() {
					await Assert.ThrowsAsync(typeof(Exception), AsyncThrowingMethod);
					await Assert.ThrowsAsync(typeof(Exception), () => Task.Delay(0));
					await Assert.ThrowsAsync(typeof(Exception), async () => await Task.Delay(0));
					await Assert.ThrowsAsync(typeof(Exception), async () => await Task.Delay(0).ConfigureAwait(false));

					await Assert.ThrowsAsync<Exception>(AsyncThrowingMethod);
					await Assert.ThrowsAsync<Exception>(() => Task.Delay(0));
					await Assert.ThrowsAsync<Exception>(async () => await Task.Delay(0));
					await Assert.ThrowsAsync<Exception>(async () => await Task.Delay(0).ConfigureAwait(false));

					await Assert.ThrowsAsync<ArgumentException>("param", AsyncThrowingMethod);
					await Assert.ThrowsAsync<ArgumentException>("param", () => Task.Delay(0));
					await Assert.ThrowsAsync<ArgumentException>("param", async () => await Task.Delay(0));
					await Assert.ThrowsAsync<ArgumentException>("param", async () => await Task.Delay(0).ConfigureAwait(false));

					await Assert.ThrowsAnyAsync<Exception>(AsyncThrowingMethod);
					await Assert.ThrowsAnyAsync<Exception>(() => Task.Delay(0));
					await Assert.ThrowsAnyAsync<Exception>(async () => await Task.Delay(0));
					await Assert.ThrowsAnyAsync<Exception>(async () => await Task.Delay(0).ConfigureAwait(false));
				}

				async Task AssertInLambda() {
					Func<Task<int>> function = async () => {
						await Assert.ThrowsAsync(typeof(Exception), AsyncThrowingMethod);
						return 0;
					};

					int number = {|CS0029:function()|};
					function();
				}

				void AssertInLambda_Uninvoked() {
					Func<int, Task> function = async (int x) => {
						await Assert.ThrowsAsync(typeof(Exception), AsyncThrowingMethod);
					};
				}

				async Task AssertInLocalFunction() {
					async Task<int> function() {
						await Assert.ThrowsAsync(typeof(Exception), AsyncThrowingMethod);
						return 0;
					};

					int number = {|CS0029:function()|};
					function();
				}

				void AssertInLocalFunction_Uninvoked() {
					async Task function(int x) {
						await Assert.ThrowsAsync(typeof(Exception), AsyncThrowingMethod);
					};
				}

				Task AsyncThrowingMethod() { throw new NotImplementedException(); }
			}
			""";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp7, before, after, AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer.Key_UseAlternateAssert);
	}
}
