using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Suppressors.NonNullableFieldInitializationSuppressor>;

public sealed class CS8618_NonNullableFieldInitializationSuppressorTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var sourceV2 = /* lang=c#-test */ """
			#pragma warning disable CS0414, CS0169, CS1591
			#nullable enable

			using System.Threading.Tasks;
			using Xunit;

			public class NonAsyncLifetimeClass_DoesNotSuppress {
				private string {|CS8618:field|};
			}

			// Fields

			public class FieldNotInitializedInInitializeAsync_DoesNotSuppress : IAsyncLifetime {
				private string {|CS8618:field|};

				public Task InitializeAsync() {
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			public class FieldInitializedInInitializeAsync_Suppresses : IAsyncLifetime {
				private string {|#0:field|};

				public Task InitializeAsync() {
					field = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			public class FieldInitializedWithThis_Suppresses : IAsyncLifetime {
				private string {|#1:field|};

				public Task InitializeAsync() {
					this.field = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			public class MultipleFields_OnlyInitializedOnesSuppressed : IAsyncLifetime {
				private string {|#2:initialized|};
				private string {|CS8618:notInitialized|};

				public Task InitializeAsync() {
					initialized = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			public class FieldAssignedWithExplicitConstructor_Suppresses : IAsyncLifetime {
				private string field;

				public {|#3:FieldAssignedWithExplicitConstructor_Suppresses|}() { }

				public Task InitializeAsync() {
					field = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			public class FieldNotAssignedWithExplicitConstructor_DoesNotSuppress : IAsyncLifetime {
				private string field;

				public {|CS8618:FieldNotAssignedWithExplicitConstructor_DoesNotSuppress|}() { }

				public Task InitializeAsync() {
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			// Properties

			public class PropertyInitializedInInitializeAsync_Suppresses : IAsyncLifetime {
				public string {|#10:MyProp|} { get; set; }

				public Task InitializeAsync() {
					MyProp = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			public class PropertyAssignedWithExplicitConstructor_Suppresses : IAsyncLifetime {
				public string MyProp { get; set; }

				public {|#11:PropertyAssignedWithExplicitConstructor_Suppresses|}() { }

				public Task InitializeAsync() {
					MyProp = "hello";
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}

			public class PropertyNotAssignedWithExplicitConstructor_DoesNotSuppress : IAsyncLifetime {
				public string MyProp { get; set; }

				public {|CS8618:PropertyNotAssignedWithExplicitConstructor_DoesNotSuppress|}() { }

				public Task InitializeAsync() {
					return Task.CompletedTask;
				}

				public Task DisposeAsync() => Task.CompletedTask;

				[Fact]
				public void Test1() { }
			}
			""";
		var sourceV3 =
			sourceV2
				.Replace("Task.CompletedTask", "default(ValueTask)")
				.Replace("public Task", "public ValueTask");
		var expected = new[] {
			DiagnosticResult.CompilerWarning("CS8618").WithLocation(0).WithIsSuppressed(true).WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
			DiagnosticResult.CompilerWarning("CS8618").WithLocation(1).WithIsSuppressed(true).WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
			DiagnosticResult.CompilerWarning("CS8618").WithLocation(2).WithIsSuppressed(true).WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
			DiagnosticResult.CompilerWarning("CS8618").WithLocation(3).WithIsSuppressed(true).WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),

			DiagnosticResult.CompilerWarning("CS8618").WithLocation(10).WithIsSuppressed(true).WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
			DiagnosticResult.CompilerWarning("CS8618").WithLocation(11).WithIsSuppressed(true).WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
		};

		await Verify.VerifyCompilerWarningSuppressorV2(LanguageVersion.CSharp8, [sourceV2], expected);
		await Verify.VerifyCompilerWarningSuppressorV3(LanguageVersion.CSharp8, [sourceV3], expected);
	}
}
