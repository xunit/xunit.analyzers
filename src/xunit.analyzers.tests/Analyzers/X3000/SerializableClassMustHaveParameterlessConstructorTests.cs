using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using VerifyV2 = CSharpVerifier<SerializableClassMustHaveParameterlessConstructorTests.V2Analyzer>;
using VerifyV3 = CSharpVerifier<SerializableClassMustHaveParameterlessConstructorTests.V3Analyzer>;

public class SerializableClassMustHaveParameterlessConstructorTests
{
	public class JsonTypeID
	{
		[Fact]
		public async Task JsonTypeIDAcceptanceTest()
		{
			var source = /* lang=c#-test */ """
				using Xunit.Sdk;

				public class NonSerializedClass { }

				[JsonTypeID("1")]
				public class SerializedWithImplicitCtor { }

				[JsonTypeID("2")]
				public class SerializedWithExplicitCtor {
					public SerializedWithExplicitCtor() { }
				}

				[JsonTypeID("3")]
				public class {|#0:SerializedWithNoMatchingCtor|} {
					public SerializedWithNoMatchingCtor(int _) { }
				}

				[JsonTypeID("4")]
				public class {|#1:SerializedWithNonPublicCtor|} {
					protected SerializedWithNonPublicCtor() { }
				}
				""";
			var expected = new[] {
				VerifyV3.Diagnostic().WithLocation(0).WithArguments("SerializedWithNoMatchingCtor", "Xunit.Sdk.JsonTypeIDAttribute"),
				VerifyV3.Diagnostic().WithLocation(1).WithArguments("SerializedWithNonPublicCtor", "Xunit.Sdk.JsonTypeIDAttribute"),
			};

			await VerifyV3.VerifyAnalyzerV3(source, expected);
		}
	}

	public class RunnerReporter
	{
		static readonly string Template = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit.Runner.Common;
			using Xunit.Sdk;

			public {1} {{|#0:MyRunnerReporter|}} : IRunnerReporter
			{{
				{0}

				public bool CanBeEnvironmentallyEnabled => false;
				public string Description => string.Empty;
				public bool ForceNoLogo => false;
				public bool IsEnvironmentallyEnabled => false;
				public string? RunnerSwitch => "unused";

				public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
					IRunnerLogger logger,
					IMessageSink? diagnosticMessageSink) =>
						throw new NotImplementedException();
			}}
			""";

		[Fact]
		public async Task ImplicitConstructor_DoesNotTrigger()
		{
			var source = string.Format(Template, string.Empty, "class");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task WrongConstructor_Triggers()
		{
			var source = string.Format(Template, /* lang=c#-test */ "public MyRunnerReporter(int x) { }", "class");
			var expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("MyRunnerReporter", "Xunit.Runner.Common.IRunnerReporter");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
		}

		[Fact]
		public async Task WrongConstructorOnAbstractClass_DoesNotTrigger()
		{
			var source = string.Format(Template, /* lang=c#-test */ "public MyRunnerReporter(int x) { }", "abstract class");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task NonPublicConstructor_Triggers()
		{
			var source = string.Format(Template, /* lang=c#-test */ "protected MyRunnerReporter() { }", "class");
			var expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("MyRunnerReporter", "Xunit.Runner.Common.IRunnerReporter");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
		}

		[Fact]
		public async Task NonPublicConstructorOnAbstractClass_DoesNotTrigger()
		{
			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, string.Format(Template, /* lang=c#-test */ "protected MyRunnerReporter() { }", "abstract class"));
		}

		[Fact]
		public async Task PublicParameterlessConstructor_DoesNotTrigger()
		{
			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, string.Format(Template, /* lang=c#-test */ "public MyRunnerReporter() { }", "class"));
		}
	}

	public class XunitSerializable
	{
		static readonly string Template = /* lang=c#-test */ """
			using {2};

			public interface IMySerializer : IXunitSerializable {{ }}
			public {3} {{|#0:Foo|}} : {0}
			{{
				{1}
				public void Deserialize(IXunitSerializationInfo info) {{ }}
				public void Serialize(IXunitSerializationInfo info) {{ }}
			}}
			""";
		public static TheoryData<string> Interfaces =
		[
			"IXunitSerializable",
			"IMySerializer"
		];

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task ImplicitConstructors_DoesNotTrigger(string @interface)
		{
			await VerifyV2.VerifyAnalyzerV2(string.Format(Template, @interface, "", "Xunit.Abstractions", "class"));
			await VerifyV3.VerifyAnalyzerV3(string.Format(Template, @interface, "", "Xunit.Sdk", "class"));
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task WrongConstructor_Triggers(string @interface)
		{
			var v2Source = string.Format(Template, @interface, /* lang=c#-test */ "public Foo(int x) { }", "Xunit.Abstractions", "class");
			var v2Expected = VerifyV2.Diagnostic().WithLocation(0).WithArguments("Foo", "Xunit.Abstractions.IXunitSerializable");

			await VerifyV2.VerifyAnalyzerV2(v2Source, v2Expected);

			var v3Source = string.Format(Template, @interface, /* lang=c#-test */ "public Foo(int x) { }", "Xunit.Sdk", "class");
			var v3Expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("Foo", "Xunit.Sdk.IXunitSerializable");

			await VerifyV3.VerifyAnalyzerV3(v3Source, v3Expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task WrongConstructorOnAbstractClass_DoesNotTrigger(string @interface)
		{
			await VerifyV2.VerifyAnalyzerV2(string.Format(Template, @interface, /* lang=c#-test */ "public Foo(int x) { }", "Xunit.Abstractions", "abstract class"));
			await VerifyV3.VerifyAnalyzerV3(string.Format(Template, @interface, /* lang=c#-test */ "public Foo(int x) { }", "Xunit.Sdk", "abstract class"));
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task NonPublicConstructor_Triggers(string @interface)
		{
			var v2Source = string.Format(Template, @interface, /* lang=c#-test */ "protected Foo() { }", "Xunit.Abstractions", "class");
			var v2Expected = VerifyV2.Diagnostic().WithLocation(0).WithArguments("Foo", "Xunit.Abstractions.IXunitSerializable");

			await VerifyV2.VerifyAnalyzerV2(v2Source, v2Expected);

			var v3Source = string.Format(Template, @interface, /* lang=c#-test */ "protected Foo() { }", "Xunit.Sdk", "class");
			var v3Expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("Foo", "Xunit.Sdk.IXunitSerializable");

			await VerifyV3.VerifyAnalyzerV3(v3Source, v3Expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task NonPublicConstructorOnAbstractClass_DoesNotTrigger(string @interface)
		{
			await VerifyV2.VerifyAnalyzerV2(string.Format(Template, @interface, /* lang=c#-test */ "protected Foo() { }", "Xunit.Abstractions", "abstract class"));
			await VerifyV3.VerifyAnalyzerV3(string.Format(Template, @interface, /* lang=c#-test */ "protected Foo() { }", "Xunit.Sdk", "abstract class"));
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task PublicParameterlessConstructor_DoesNotTrigger(string @interface)
		{
			await VerifyV2.VerifyAnalyzerV2(string.Format(Template, @interface, /* lang=c#-test */ "public Foo() { }", "Xunit.Abstractions", "class"));
			await VerifyV3.VerifyAnalyzerV3(string.Format(Template, @interface, /* lang=c#-test */ "public Foo() { }", "Xunit.Sdk", "class"));
		}
	}

	public class XunitSerializer
	{
		static readonly string Template = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			public {1} {{|#0:MySerializer|}} : IXunitSerializer
			{{
				{0}

				public object Deserialize(Type type, string serializedValue) => null!;
				public bool IsSerializable(Type type, object? value, out string? failureReason)
				{{
					failureReason = null;
					return true;
				}}
				public string Serialize(object value) => string.Empty;
			}}
			""";

		[Fact]
		public async Task ImplicitConstructor_DoesNotTrigger()
		{
			var source = string.Format(Template, string.Empty, "class");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task WrongConstructor_Triggers()
		{
			var source = string.Format(Template, /* lang=c#-test */ "public MySerializer(int x) { }", "class");
			var expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("MySerializer", "Xunit.Sdk.IXunitSerializer");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
		}

		[Fact]
		public async Task WrongConstructorOnAbstractClass_DoesNotTrigger()
		{
			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, string.Format(Template, /* lang=c#-test */ "public MySerializer(int x) { }", "abstract class"));
		}

		[Fact]
		public async Task NonPublicConstructor_Triggers()
		{
			var source = string.Format(Template, /* lang=c#-test */ "protected MySerializer() { }", "class");
			var expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("MySerializer", "Xunit.Sdk.IXunitSerializer");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
		}

		[Fact]
		public async Task NonPublicConstructorOnAbstractClass_DoesNotTrigger()
		{
			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, string.Format(Template, /* lang=c#-test */ "protected MySerializer() { }", "abstract class"));
		}

		[Fact]
		public async Task PublicParameterlessConstructor_DoesNotTrigger()
		{
			var source = string.Format(Template, /* lang=c#-test */ "public MySerializer() { }", "class");

			await VerifyV3.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}
	}

	public class V2Analyzer : SerializableClassMustHaveParameterlessConstructor
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Abstractions(compilation);
	}

	public class V3Analyzer : SerializableClassMustHaveParameterlessConstructor
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation);
	}
}
