using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

public class X2015_AssertThrowsShouldUseGenericOverloadCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				Func<Task> ThrowingMethod = () => { throw new NotImplementedException(); };

				void NonGeneric() {
					{|CS0619:{|#0:Assert.Throws(typeof(NotImplementedException), ThrowingMethod)|}|};
					{|CS0619:{|#1:Assert.Throws(typeof(NotImplementedException), () => Task.Delay(0))|}|};

					{|#2:Assert.ThrowsAsync(typeof(NotImplementedException), ThrowingMethod)|};
					{|#3:Assert.ThrowsAsync(typeof(NotImplementedException), () => Task.Delay(0))|};
				}

				void Generic() {
					{|CS0619:Assert.Throws<NotImplementedException>(ThrowingMethod)|};
					{|CS0619:Assert.Throws<NotImplementedException>(() => Task.Delay(0))|};

					Assert.ThrowsAsync<NotImplementedException>(ThrowingMethod);
					Assert.ThrowsAsync<NotImplementedException>(() => Task.Delay(0));
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Throws", "System.NotImplementedException"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Throws", "System.NotImplementedException"),
			Verify.Diagnostic().WithLocation(2).WithArguments("ThrowsAsync", "System.NotImplementedException"),
			Verify.Diagnostic().WithLocation(3).WithArguments("ThrowsAsync", "System.NotImplementedException"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
