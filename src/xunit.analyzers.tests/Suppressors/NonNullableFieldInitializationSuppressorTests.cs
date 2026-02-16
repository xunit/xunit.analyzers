using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Suppressors.NonNullableFieldInitializationSuppressor>;

public sealed class NonNullableFieldInitializationSuppressorTests
{
	// ----- v2 (Task-based IAsyncLifetime) -----

	[Fact]
	public async Task NonAsyncLifetimeClass_V2_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591

			public class NonTestClass {
				private string {|CS8618:_field|};
			}
			""";

		await Verify.VerifyCompilerWarningSuppressorV2(LanguageVersion.CSharp8, [code]);
	}

	[Fact]
	public async Task FieldInitializedInInitializeAsync_V2_Suppresses()
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
	public async Task PropertyInitializedInInitializeAsync_V2_Suppresses()
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
	public async Task FieldNotInitializedInInitializeAsync_V2_DoesNotSuppress()
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
	public async Task FieldInitializedWithThis_V2_Suppresses()
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
	public async Task MultipleFields_OnlyInitializedOnesSuppressed_V2()
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

	[Fact]
	public async Task FieldAssignedWithExplicitConstructor_V2_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string _field;
				public {|#0:TestClass|}() { }

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
	public async Task FieldNotAssignedWithExplicitConstructor_V2_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string _field;
				public {|CS8618:TestClass|}() { }

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
	public async Task PropertyAssignedWithExplicitConstructor_V2_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				public string MyProp { get; set; }
				public {|#0:TestClass|}() { }

				public Task InitializeAsync() {
					MyProp = "hello";
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

	// ----- v3 (ValueTask-based IAsyncLifetime) -----

	[Fact]
	public async Task NonAsyncLifetimeClass_V3_DoesNotSuppress()
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
	public async Task FieldInitializedInInitializeAsync_V3_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string {|#0:_field|};

				public ValueTask InitializeAsync() {
					_field = "hello";
					return default;
				}

				public ValueTask DisposeAsync() => default;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [code], expected);
	}

	[Fact]
	public async Task PropertyInitializedInInitializeAsync_V3_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				public string {|#0:Prop|} { get; set; }

				public ValueTask InitializeAsync() {
					Prop = "hello";
					return default;
				}

				public ValueTask DisposeAsync() => default;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [code], expected);
	}

	[Fact]
	public async Task FieldNotInitializedInInitializeAsync_V3_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string {|CS8618:_field|};

				public ValueTask InitializeAsync() => default;

				public ValueTask DisposeAsync() => default;

				[Fact]
				public void Test1() { }
			}
			""";

		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [code]);
	}

	[Fact]
	public async Task FieldAssignedWithExplicitConstructor_V3_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string _field;
				public {|#0:TestClass|}() { }

				public ValueTask InitializeAsync() {
					_field = "hello";
					return default;
				}

				public ValueTask DisposeAsync() => default;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [code], expected);
	}

	[Fact]
	public async Task FieldNotAssignedWithExplicitConstructor_V3_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				private string _field;
				public {|CS8618:TestClass|}() { }

				public ValueTask InitializeAsync() => default;

				public ValueTask DisposeAsync() => default;

				[Fact]
				public void Test1() { }
			}
			""";

		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [code]);
	}

	[Fact]
	public async Task PropertyAssignedWithExplicitConstructor_V3_Suppresses()
	{
		var code = /* lang=c#-test */ """
			#nullable enable
			#pragma warning disable CS0414, CS0169, CS1591
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass : IAsyncLifetime {
				public string MyProp { get; set; }
				public {|#0:TestClass|}() { }

				public ValueTask InitializeAsync() {
					MyProp = "hello";
					return default;
				}

				public ValueTask DisposeAsync() => default;

				[Fact]
				public void Test1() { }
			}
			""";
		var expected = DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [code], expected);
	}
}
