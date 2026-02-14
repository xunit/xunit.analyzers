using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Suppressors.NonNullableFieldInitializationSuppressor>;

public sealed class NonNullableFieldInitializationSuppressorTests
{
	[Fact]
	public async Task NonAsyncLifetimeClass_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591

			public class NonTestClass {
				private string {|CS8618:_field|};
			}
			""";

		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [code]);
	}

	[Fact]
	public async Task FieldInitializedInInitializeAsync_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string {|#0:_field|};

				public Task InitializeAsync() {
					_field = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV2(LanguageVersion.CSharp8, [code], expected);
	}

	[Fact]
	public async Task PropertyInitializedInInitializeAsync_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				public string {|#0:Prop|} { get; set; }

				public Task InitializeAsync() {
					Prop = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV2(LanguageVersion.CSharp8, [code], expected);
	}

	[Fact]
	public async Task FieldNotInitializedInInitializeAsync_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string {|CS8618:_field|};

				public Task InitializeAsync() {
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}
			""";

		await Verify.VerifyCompilerWarningSuppressorV2(LanguageVersion.CSharp8, [code]);
	}

	[Fact]
	public async Task FieldInitializedWithThis_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string {|#0:_field|};

				public Task InitializeAsync() {
					this._field = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV2(LanguageVersion.CSharp8, [code], expected);
	}

	[Fact]
	public async Task MultipleFields_OnlyInitializedOnesSuppressed()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string {|#0:_initialized|};
				private string {|CS8618:_notInitialized|};

				public Task InitializeAsync() {
					_initialized = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV2(LanguageVersion.CSharp8, [code], expected);
	}
}
