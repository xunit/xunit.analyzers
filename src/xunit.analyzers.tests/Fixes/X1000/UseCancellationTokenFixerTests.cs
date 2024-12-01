using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.UseCancellationToken>;

public class UseCancellationTokenFixerTests
{
	[Fact]
	public async Task UseCancellationTokenArgument()
	{
		var before = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod()
				{
					[|FunctionWithOverload(42)|];
					[|FunctionWithOverload(42, default(CancellationToken))|];
			
					[|FunctionWithDefaults()|];
					[|FunctionWithDefaults(42)|];
					[|FunctionWithDefaults(cancellationToken: default(CancellationToken))|];
					[|FunctionWithDefaults(42, cancellationToken: default(CancellationToken))|];
				}
			
				void FunctionWithOverload(int _) { }
				void FunctionWithOverload(int _1, CancellationToken _2) { }

				void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default(CancellationToken)) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod()
				{
					FunctionWithOverload(42, TestContext.Current.CancellationToken);
					FunctionWithOverload(42, TestContext.Current.CancellationToken);
			
					FunctionWithDefaults(cancellationToken: TestContext.Current.CancellationToken);
					FunctionWithDefaults(42, TestContext.Current.CancellationToken);
					FunctionWithDefaults(cancellationToken: TestContext.Current.CancellationToken);
					FunctionWithDefaults(42, cancellationToken: TestContext.Current.CancellationToken);
				}
			
				void FunctionWithOverload(int _) { }
				void FunctionWithOverload(int _1, CancellationToken _2) { }

				void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default(CancellationToken)) { }
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, UseCancellationTokenFixer.Key_UseCancellationTokenArgument);
	}

	[Fact]
	public async Task UseCancellationTokenArgument_AliasTestContext()
	{
		var before = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					[|Function()|];
				}
			
				void Function(CancellationToken token = default(CancellationToken)) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					Function(MyContext.Current.CancellationToken);
				}
			
				void Function(CancellationToken token = default(CancellationToken)) { }
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, UseCancellationTokenFixer.Key_UseCancellationTokenArgument);
	}

	[Fact]
	public async Task UseCancellationTokenArgument_ParamsArgument()
	{
		var before = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					[|Function(1, 2, 3)|];
				}

				void Function(params int[] integers) { }

				void Function(int[] integers, CancellationToken token = default(CancellationToken)) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					Function(new int[] { 1, 2, 3 }, MyContext.Current.CancellationToken);
				}
			
				void Function(params int[] integers) { }

				void Function(int[] integers, CancellationToken token = default(CancellationToken)) { }
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, UseCancellationTokenFixer.Key_UseCancellationTokenArgument);
	}

	[Fact]
	public async Task UseCancellationTokenArgument_ParamsArgumentAfterRegularArguments()
	{
		var before = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					[|Function("hello", System.Guid.NewGuid(), System.Guid.NewGuid())|];
				}
			
				void Function(string str, params System.Guid[] guids) { }
			
				void Function(string str, System.Guid[] guids, CancellationToken token = default(CancellationToken)) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					Function("hello", new System.Guid[] { System.Guid.NewGuid(), System.Guid.NewGuid() }, MyContext.Current.CancellationToken);
				}
			
				void Function(string str, params System.Guid[] guids) { }
			
				void Function(string str, System.Guid[] guids, CancellationToken token = default(CancellationToken)) { }
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, UseCancellationTokenFixer.Key_UseCancellationTokenArgument);
	}

	[Fact]
	public async Task UseCancellationTokenArgument_ParamsArgumentWithNoValues()
	{
		var before = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					[|Function()|];
				}

				void Function(params int[] integers) { }

				void Function(int[] integers, CancellationToken token = default(CancellationToken)) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using MyContext = Xunit.TestContext;

			public class TestClass {
				[Xunit.Fact]
				public void TestMethod()
				{
					Function(new int[] { }, MyContext.Current.CancellationToken);
				}
			
				void Function(params int[] integers) { }

				void Function(int[] integers, CancellationToken token = default(CancellationToken)) { }
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, UseCancellationTokenFixer.Key_UseCancellationTokenArgument);
	}
}
