using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

public class SerializableClassMustHaveParameterlessConstructorFixerTests
{
	public class JsonTypeID
	{
		[Fact]
		public async Task WithPublicParameteredConstructor_AddsNewConstructor()
		{
			var before = /* lang=c#-test */ """
				using Xunit.Sdk;

				[JsonTypeID("1")]
				public class [|MyJsonObject|] {
					public MyJsonObject(int _) { }
				}
				""";
			var after = /* lang=c#-test */ """
				using Xunit.Sdk;

				[JsonTypeID("1")]
				public class MyJsonObject {
					[System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
					public MyJsonObject()
					{
					}
				
					public MyJsonObject(int _) { }
				}
				""";

			await Verify.VerifyCodeFixV3(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}

		[Fact]
		public async Task WithNonPublicParameterlessConstructor_ChangesVisibility()
		{
			var before = /* lang=c#-test */ """
				using Xunit.Sdk;

				[JsonTypeID("1")]
				public class [|MyJsonObject|] {
					protected MyJsonObject() { }
				}
				""";
			var after = /* lang=c#-test */ """
				using Xunit.Sdk;

				[JsonTypeID("1")]
				public class MyJsonObject {
					[System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
					public MyJsonObject() { }
				}
				""";

			await Verify.VerifyCodeFixV3(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}
	}

	public class RunnerReporter
	{
		[Fact]
		public async Task WithPublicParameteredConstructor_AddsNewConstructor()
		{
			var before = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks;
				using Xunit.Runner.Common;
				using Xunit.Sdk;
				
				public class [|MyRunnerReporter|] : IRunnerReporter
				{
					public MyRunnerReporter(int _) { }
				
					public bool CanBeEnvironmentallyEnabled => false;
					public string Description => string.Empty;
					public bool ForceNoLogo => false;
					public bool IsEnvironmentallyEnabled => false;
					public string? RunnerSwitch => "unused";
				
					public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
						IRunnerLogger logger,
						IMessageSink? diagnosticMessageSink) =>
							throw new NotImplementedException();
				}
				""";
			var after = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks;
				using Xunit.Runner.Common;
				using Xunit.Sdk;
				
				public class MyRunnerReporter : IRunnerReporter
				{
					public MyRunnerReporter()
					{
					}

					public MyRunnerReporter(int _) { }
				
					public bool CanBeEnvironmentallyEnabled => false;
					public string Description => string.Empty;
					public bool ForceNoLogo => false;
					public bool IsEnvironmentallyEnabled => false;
					public string? RunnerSwitch => "unused";
				
					public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
						IRunnerLogger logger,
						IMessageSink? diagnosticMessageSink) =>
							throw new NotImplementedException();
				}
				""";

			await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}

		[Fact]
		public async Task WithNonPublicParameterlessConstructor_ChangesVisibility()
		{
			var before = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks;
				using Xunit.Runner.Common;
				using Xunit.Sdk;
				
				public class [|MyRunnerReporter|] : IRunnerReporter
				{
					protected MyRunnerReporter() { }
				
					public bool CanBeEnvironmentallyEnabled => false;
					public string Description => string.Empty;
					public bool ForceNoLogo => false;
					public bool IsEnvironmentallyEnabled => false;
					public string? RunnerSwitch => "unused";
				
					public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
						IRunnerLogger logger,
						IMessageSink? diagnosticMessageSink) =>
							throw new NotImplementedException();
				}
				""";
			var after = /* lang=c#-test */ """
				using System;
				using System.Threading.Tasks;
				using Xunit.Runner.Common;
				using Xunit.Sdk;
				
				public class MyRunnerReporter : IRunnerReporter
				{
					public MyRunnerReporter() { }
				
					public bool CanBeEnvironmentallyEnabled => false;
					public string Description => string.Empty;
					public bool ForceNoLogo => false;
					public bool IsEnvironmentallyEnabled => false;
					public string? RunnerSwitch => "unused";
				
					public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
						IRunnerLogger logger,
						IMessageSink? diagnosticMessageSink) =>
							throw new NotImplementedException();
				}
				""";

			await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}
	}

	public class XunitSerializable
	{
		[Fact]
		public async Task FixAll_AddsConstructorsToMultipleClasses()
		{
			var before = /* lang=c#-test */ """
				using Xunit.Sdk;

				public class [|MyTestCase1|]: IXunitSerializable {
					public MyTestCase1(int x) { }

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
				}

				public class [|MyTestCase2|]: IXunitSerializable {
					public MyTestCase2(string s) { }

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
				}
				""";
			var after = /* lang=c#-test */ """
				using Xunit.Sdk;

				public class MyTestCase1: IXunitSerializable {
					[System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
					public MyTestCase1()
					{
					}

					public MyTestCase1(int x) { }

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
				}

				public class MyTestCase2: IXunitSerializable {
					[System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
					public MyTestCase2()
					{
					}

					public MyTestCase2(string s) { }

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
				}
				""";

			await Verify.VerifyCodeFixV3FixAll(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}

		[Fact]
		public async Task WithPublicParameteredConstructor_AddsNewConstructor()
		{
			var beforeTemplate = /* lang=c#-test */ """
				public class [|MyTestCase|]: {0}.IXunitSerializable {{
					public MyTestCase(int x) {{ }}

					void {0}.IXunitSerializable.Deserialize({0}.IXunitSerializationInfo _) {{ }}
					void {0}.IXunitSerializable.Serialize({0}.IXunitSerializationInfo _) {{ }}
				}}
				""";
			var afterTemplate = /* lang=c#-test */ """
				public class MyTestCase: {0}.IXunitSerializable {{
					[System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
					public MyTestCase()
					{{
					}}

					public MyTestCase(int x) {{ }}

					void {0}.IXunitSerializable.Deserialize({0}.IXunitSerializationInfo _) {{ }}
					void {0}.IXunitSerializable.Serialize({0}.IXunitSerializationInfo _) {{ }}
				}}
				""";

			var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
			var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

			await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

			var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
			var v3After = string.Format(afterTemplate, "Xunit.Sdk");

			await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}

		[Fact]
		public async Task WithNonPublicParameterlessConstructor_ChangesVisibility_WithoutUsing()
		{
			var beforeTemplate = /* lang=c#-test */ """
				using {0};

				public class [|MyTestCase|]: IXunitSerializable {{
					protected MyTestCase() {{ throw new System.DivideByZeroException(); }}

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
				}}
				""";
			var afterTemplate = /* lang=c#-test */ """
				using {0};

				public class MyTestCase: IXunitSerializable {{
					[System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
					public MyTestCase() {{ throw new System.DivideByZeroException(); }}

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
				}}
				""";

			var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
			var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

			await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

			var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
			var v3After = string.Format(afterTemplate, "Xunit.Sdk");

			await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}

		[Fact]
		public async Task WithNonPublicParameterlessConstructor_ChangesVisibility_WithUsing()
		{
			var beforeTemplate = /* lang=c#-test */ """
				using System;
				using {0};

				public class [|MyTestCase|]: IXunitSerializable {{
					protected MyTestCase() {{ throw new DivideByZeroException(); }}

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
				}}
				""";
			var afterTemplate = /* lang=c#-test */ """
				using System;
				using {0};

				public class MyTestCase: IXunitSerializable {{
					[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
					public MyTestCase() {{ throw new DivideByZeroException(); }}

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
				}}
				""";

			var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
			var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

			await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

			var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
			var v3After = string.Format(afterTemplate, "Xunit.Sdk");

			await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}

		[Fact]
		public async Task PreservesExistingObsoleteAttribute()
		{
			var beforeTemplate = /* lang=c#-test */ """
				using {0};
				using obo = System.ObsoleteAttribute;

				public class [|MyTestCase|]: IXunitSerializable {{
					[obo("This is my custom obsolete message")]
					protected MyTestCase() {{ throw new System.DivideByZeroException(); }}

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
				}}
				""";
			var afterTemplate = /* lang=c#-test */ """
				using {0};
				using obo = System.ObsoleteAttribute;

				public class MyTestCase: IXunitSerializable {{
					[obo("This is my custom obsolete message")]
					public MyTestCase() {{ throw new System.DivideByZeroException(); }}

					void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
					void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
				}}
				""";

			var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
			var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

			await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

			var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
			var v3After = string.Format(afterTemplate, "Xunit.Sdk");

			await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}
	}

	public class XunitSerializer
	{
		[Fact]
		public async Task WithPublicParameteredConstructor_AddsNewConstructor()
		{
			var before = /* lang=c#-test */ """
				using System;
				using Xunit.Sdk;
				
				public class [|MySerializer|] : IXunitSerializer
				{
					public MySerializer(int _) { }

					public object Deserialize(Type type, string serializedValue) => null!;
					public bool IsSerializable(Type type, object? value, out string? failureReason)
					{
						failureReason = null;
						return true;
					}
					public string Serialize(object value) => string.Empty;
				}
				""";
			var after = /* lang=c#-test */ """
				using System;
				using Xunit.Sdk;
				
				public class MySerializer : IXunitSerializer
				{
					public MySerializer()
					{
					}

					public MySerializer(int _) { }
				
					public object Deserialize(Type type, string serializedValue) => null!;
					public bool IsSerializable(Type type, object? value, out string? failureReason)
					{
						failureReason = null;
						return true;
					}
					public string Serialize(object value) => string.Empty;
				}
				""";

			await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}

		[Fact]
		public async Task WithNonPublicParameterlessConstructor_ChangesVisibility()
		{
			var before = /* lang=c#-test */ """
				using System;
				using Xunit.Sdk;
				
				public class [|MySerializer|] : IXunitSerializer
				{
					protected MySerializer() { }

					public object Deserialize(Type type, string serializedValue) => null!;
					public bool IsSerializable(Type type, object? value, out string? failureReason)
					{
						failureReason = null;
						return true;
					}
					public string Serialize(object value) => string.Empty;
				}
				""";
			var after = /* lang=c#-test */ """
				using System;
				using Xunit.Sdk;
				
				public class MySerializer : IXunitSerializer
				{
					public MySerializer() { }
				
					public object Deserialize(Type type, string serializedValue) => null!;
					public bool IsSerializable(Type type, object? value, out string? failureReason)
					{
						failureReason = null;
						return true;
					}
					public string Serialize(object value) => string.Empty;
				}
				""";

			await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
		}
	}
}
