using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

public class X3001_SerializableClassMustHaveParameterlessConstructorFixerTests
{
	[Fact]
	public async ValueTask JsonTypeID()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			[JsonTypeID("1")]
			public class [|WithPublicParameteredConstructor_AddsNewConstructor|] {
				public WithPublicParameteredConstructor_AddsNewConstructor(int _) { }
			}

			[JsonTypeID("2")]
			public class [|WithNonPublicParameterlessConstructor_ChangesVisibility|] {
				protected WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			[JsonTypeID("1")]
			public class WithPublicParameteredConstructor_AddsNewConstructor {
				[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
				public WithPublicParameteredConstructor_AddsNewConstructor()
				{
				}

				public WithPublicParameteredConstructor_AddsNewConstructor(int _) { }
			}

			[JsonTypeID("2")]
			public class WithNonPublicParameterlessConstructor_ChangesVisibility {
				[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
				public WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";

		await Verify.VerifyCodeFixV3(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async ValueTask RunnerReporter()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit.Runner.Common;

			public class [|WithPublicParameteredConstructor_AddsNewConstructor|] : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				public WithPublicParameteredConstructor_AddsNewConstructor(int _) { }
			}

			public class [|WithNonPublicParameterlessConstructor_ChangesVisibility|] : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				protected WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit.Runner.Common;

			public class [|WithPublicParameteredConstructor_AddsNewConstructor|] : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				public WithPublicParameteredConstructor_AddsNewConstructor()
				{
				}

				public WithPublicParameteredConstructor_AddsNewConstructor(int _) { }
			}

			public class WithNonPublicParameterlessConstructor_ChangesVisibility : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				public WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";

		await Verify.VerifyCodeFixV3(LanguageVersion.CSharp8, before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async ValueTask XunitSerializable()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			public class [|WithPublicParameteredConstructor_AddsNewConstructor|]: {|CS0535:{|CS0535:IXunitSerializable|}|} {
				public WithPublicParameteredConstructor_AddsNewConstructor(int x) { }
			}

			public class [|WithNonPublicParameterlessConstructor_ChangesVisibility|]: {|CS0535:{|CS0535:IXunitSerializable|}|} {
				protected WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			public class WithPublicParameteredConstructor_AddsNewConstructor: {|CS0535:{|CS0535:IXunitSerializable|}|} {
				[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
				public WithPublicParameteredConstructor_AddsNewConstructor()
				{
				}

				public WithPublicParameteredConstructor_AddsNewConstructor(int x) { }
			}

			public class WithNonPublicParameterlessConstructor_ChangesVisibility: {|CS0535:{|CS0535:IXunitSerializable|}|} {
				[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
				public WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";

		await Verify.VerifyCodeFixV2FixAll(
			before.Replace("Xunit.Sdk", "Xunit.Abstractions"),
			after.Replace("Xunit.Sdk", "Xunit.Abstractions"),
			SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor
		);
		await Verify.VerifyCodeFixV3FixAllNonAot(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async ValueTask XunitSerializer()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			public class [|WithPublicParameteredConstructor_AddsNewConstructor|] : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				public WithPublicParameteredConstructor_AddsNewConstructor(int _) { }
			}

			public class [|WithNonPublicParameterlessConstructor_ChangesVisibility|] : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				protected WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			public class WithPublicParameteredConstructor_AddsNewConstructor : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				public WithPublicParameteredConstructor_AddsNewConstructor()
				{
				}

				public WithPublicParameteredConstructor_AddsNewConstructor(int _) { }
			}

			public class WithNonPublicParameterlessConstructor_ChangesVisibility : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				public WithNonPublicParameterlessConstructor_ChangesVisibility() { }
			}
			""";

		await Verify.VerifyCodeFixV3NonAot(LanguageVersion.CSharp8, before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}
}
