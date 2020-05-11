using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestMethodCannotHaveOverloads>;

namespace Xunit.Analyzers
{
	public class TestMethodCannotHaveOverloadsTests
	{
		[Fact]
		public async void FindsErrors_ForInstanceMethodOverloads_InSameInstanceClass()
		{
			var source =
				"public class TestClass { " +
				"   [Xunit.Fact]" +
				"   public void TestMethod() { }" +
				"   [Xunit.Theory]" +
				"   public void TestMethod(int a) { }" +
				"}";

			DiagnosticResult[] expected =
			{
				Verify.Diagnostic().WithSpan(1, 56, 1, 66).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "TestClass"),
				Verify.Diagnostic().WithSpan(1, 104, 1, 114).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "TestClass"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async void FindsErrors_ForStaticMethodOverloads_InSameStaticClass()
		{
			var source =
				"public static class TestClass { " +
				"   [Xunit.Fact]" +
				"   public static void TestMethod() { }" +
				"   [Xunit.Theory]" +
				"   public static void TestMethod(int a) { }" +
				"}";

			DiagnosticResult[] expected =
			{
				Verify.Diagnostic().WithSpan(1, 70, 1, 80).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "TestClass"),
				Verify.Diagnostic().WithSpan(1, 125, 1, 135).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "TestClass"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async void FindsErrors_ForInstanceMethodOverload_InDerivedClass()
		{
			await new Verify.Test
			{
				TestState =
				{
					Sources =
					{
						"public class TestClass : BaseClass {" +
						"   [Xunit.Theory]" +
						"   public void TestMethod(int a) { }" +
						"   private void TestMethod(int a, byte c) { }" +
						"}",
						"public class BaseClass {" +
						"   [Xunit.Fact]" +
						"   public void TestMethod() { }" +
						"}",
					},
					ExpectedDiagnostics =
					{
						Verify.Diagnostic().WithSpan(1, 69, 1, 79).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "BaseClass"),
						Verify.Diagnostic().WithSpan(1, 106, 1, 116).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "BaseClass"),
					},
				},
			}.RunAsync();
		}

		[Fact]
		public async void FindsError_ForStaticAndInstanceMethodOverload()
		{
			await new Verify.Test
			{
				TestState =
				{
					Sources =
					{
						"public class TestClass : BaseClass {" +
						"   [Xunit.Theory]" +
						"   public void TestMethod(int a) { }" +
						"}",
						"public class BaseClass {" +
						"   [Xunit.Fact]" +
						"   public static void TestMethod() { }" +
						"}",
					},
					ExpectedDiagnostics =
					{
						Verify.Diagnostic().WithSpan(1, 69, 1, 79).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "BaseClass"),
					},
				},
			}.RunAsync();
		}

		[Fact]
		public async void DoesNotFindError_ForMethodOverrides()
		{
			await new Verify.Test
			{
				TestState =
				{
					Sources =
					{
						"public class BaseClass {" +
						"   [Xunit.Fact]" +
						"   public virtual void TestMethod() { }" +
						"}",
						"public class TestClass : BaseClass {" +
						"   [Xunit.Fact]" +
						"   public override void TestMethod() { }" +
						"}",
					},
				},
			}.RunAsync();
		}
	}
}
