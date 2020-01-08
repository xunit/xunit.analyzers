using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

namespace Xunit.Analyzers
{
	public class TestMethodMustNotHaveMultipleFactAttributesTests
	{
		[Theory]
		[InlineData("Fact")]
		[InlineData("Theory")]
		public async void DoesNotFindErrorForMethodWithSingleAttribute(string attribute)
		{
			var source = "public class TestClass { [Xunit." + attribute + "] public void TestMethod() { } }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Fact]
		public async void FindsErrorForMethodWithTheoryAndFact()
		{
			var source = "public class TestClass { [Xunit.Fact, Xunit.Theory] public void TestMethod() { } }";

			var expected = Verify.Diagnostic().WithSpan(1, 65, 1, 75);
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async void FindsErrorForMethodWithCustomFactAttribute()
		{
			await new Verify.Test
			{
				TestState =
				{
					Sources =
					{
						"public class TestClass { [Xunit.Fact, CustomFact] public void TestMethod() { } }",
						"public class CustomFactAttribute : Xunit.FactAttribute { }",
					},
					ExpectedDiagnostics =
					{
						Verify.Diagnostic().WithSpan(1, 63, 1, 73),
					},
				},
			}.RunAsync();
		}
	}
}
