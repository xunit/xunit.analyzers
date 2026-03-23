using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

public class X3001_SerializableClassMustHaveParameterlessConstructorTests
{
	[Fact]
	public async ValueTask JsonTypeID()
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
			Verify.Diagnostic().WithLocation(0).WithArguments("SerializedWithNoMatchingCtor", "Xunit.Sdk.JsonTypeIDAttribute"),
			Verify.Diagnostic().WithLocation(1).WithArguments("SerializedWithNonPublicCtor", "Xunit.Sdk.JsonTypeIDAttribute"),
		};

		await Verify.VerifyAnalyzerV3NonAot(source, expected);
	}

	[Fact]
	public async ValueTask RunnerReporter()
	{
		var source = /* lang=c#-test */ """
			using Xunit.Runner.Common;

			public class ImplicitConstructor_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{ }

			public class {|#0:WrongConstructor_Triggers|} : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				public WrongConstructor_Triggers(int _) { }
			}

			public abstract class WrongConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				public WrongConstructor_AbstractClass_DoesNotTrigger(int _) { }
			}

			public class {|#1:NonPublicConstructor_Triggers|} : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				protected NonPublicConstructor_Triggers() { }
			}

			public abstract class NonPublicConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				protected NonPublicConstructor_AbstractClass_DoesNotTrigger() { }
			}

			public class PublicParameterlessConstructor_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{
				public PublicParameterlessConstructor_DoesNotTrigger() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("WrongConstructor_Triggers", "Xunit.Runner.Common.IRunnerReporter"),
			Verify.Diagnostic().WithLocation(1).WithArguments("NonPublicConstructor_Triggers", "Xunit.Runner.Common.IRunnerReporter"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async ValueTask XunitSerializable()
	{
		var source = /* lang=c#-test */ """
			public interface IMySerializable : IXunitSerializable { }

			public class IXunitSerializable_ImplicitConstructor_DoesNotTrigger : {|CS0535:{|CS0535:IXunitSerializable|}|}
			{ }

			public class IMySerializable_ImplicitConstructor_DoesNotTrigger : {|CS0535:{|CS0535:IMySerializable|}|}
			{ }

			public class {|#0:IXunitSerializable_WrongConstructor_Triggers|} : {|CS0535:{|CS0535:IXunitSerializable|}|}
			{
				public IXunitSerializable_WrongConstructor_Triggers(int _) { }
			}

			public class {|#1:IMySerializable_WrongConstructor_Triggers|} : {|CS0535:{|CS0535:IMySerializable|}|}
			{
				public IMySerializable_WrongConstructor_Triggers(int _) { }
			}

			public abstract class IXunitSerializable_WrongConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:IXunitSerializable|}|}
			{
				public IXunitSerializable_WrongConstructor_AbstractClass_DoesNotTrigger(int _) { }
			}

			public abstract class IMySerializable_WrongConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:IMySerializable|}|}
			{
				public IMySerializable_WrongConstructor_AbstractClass_DoesNotTrigger(int _) { }
			}

			public class {|#2:IXunitSerializable_NonPublicConstructor_Triggers|} : {|CS0535:{|CS0535:IXunitSerializable|}|}
			{
				protected IXunitSerializable_NonPublicConstructor_Triggers() { }
			}

			public class {|#3:IMySerializable_NonPublicConstructor_Triggers|} : {|CS0535:{|CS0535:IMySerializable|}|}
			{
				protected IMySerializable_NonPublicConstructor_Triggers() { }
			}

			public abstract class IXunitSerializable_NonPublicConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:IXunitSerializable|}|}
			{
				protected IXunitSerializable_NonPublicConstructor_AbstractClass_DoesNotTrigger() { }
			}

			public abstract class IMySerializable_NonPublicConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:IMySerializable|}|}
			{
				protected IMySerializable_NonPublicConstructor_AbstractClass_DoesNotTrigger() { }
			}

			public class IXunitSerializable_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:IXunitSerializable|}|}
			{
				public IXunitSerializable_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger() { }
			}

			public class IMySerializable_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:IMySerializable|}|}
			{
				public IMySerializable_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger() { }
			}
			""";
		var expectedV2 = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("IXunitSerializable_WrongConstructor_Triggers", "Xunit.Abstractions.IXunitSerializable"),
			Verify.Diagnostic().WithLocation(1).WithArguments("IMySerializable_WrongConstructor_Triggers", "Xunit.Abstractions.IXunitSerializable"),
			Verify.Diagnostic().WithLocation(2).WithArguments("IXunitSerializable_NonPublicConstructor_Triggers", "Xunit.Abstractions.IXunitSerializable"),
			Verify.Diagnostic().WithLocation(3).WithArguments("IMySerializable_NonPublicConstructor_Triggers", "Xunit.Abstractions.IXunitSerializable"),
		};
		var expectedV3 = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("IXunitSerializable_WrongConstructor_Triggers", "Xunit.Sdk.IXunitSerializable"),
			Verify.Diagnostic().WithLocation(1).WithArguments("IMySerializable_WrongConstructor_Triggers", "Xunit.Sdk.IXunitSerializable"),
			Verify.Diagnostic().WithLocation(2).WithArguments("IXunitSerializable_NonPublicConstructor_Triggers", "Xunit.Sdk.IXunitSerializable"),
			Verify.Diagnostic().WithLocation(3).WithArguments("IMySerializable_NonPublicConstructor_Triggers", "Xunit.Sdk.IXunitSerializable"),
		};

		await Verify.VerifyAnalyzerV2("using Xunit.Abstractions; " + source, expectedV2);
		await Verify.VerifyAnalyzerV3NonAot("using Xunit.Sdk; " + source, expectedV3);
	}

	[Fact]
	public async ValueTask XunitSerializer()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			public interface IMySerializer : IXunitSerializer { }

			public class IXunitSerializer_ImplicitConstructor_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{ }

			public class IMySerializer_ImplicitConstructor_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:IMySerializer|}|}|}
			{ }

			public class {|#0:IXunitSerializer_WrongConstructor_Triggers|} : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				public IXunitSerializer_WrongConstructor_Triggers(int _) { }
			}

			public class {|#1:IMySerializer_WrongConstructor_Triggers|} : {|CS0535:{|CS0535:{|CS0535:IMySerializer|}|}|}
			{
				public IMySerializer_WrongConstructor_Triggers(int _) { }
			}

			public abstract class IXunitSerializer_WrongConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				public IXunitSerializer_WrongConstructor_AbstractClass_DoesNotTrigger(int _) { }
			}

			public abstract class IMySerializer_WrongConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:IMySerializer|}|}|}
			{
				public IMySerializer_WrongConstructor_AbstractClass_DoesNotTrigger(int _) { }
			}

			public class {|#2:IXunitSerializer_NonPublicConstructor_Triggers|} : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				protected IXunitSerializer_NonPublicConstructor_Triggers() { }
			}

			public class {|#3:IMySerializer_NonPublicConstructor_Triggers|} : {|CS0535:{|CS0535:{|CS0535:IMySerializer|}|}|}
			{
				protected IMySerializer_NonPublicConstructor_Triggers() { }
			}

			public abstract class IXunitSerializer_NonPublicConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				protected IXunitSerializer_NonPublicConstructor_AbstractClass_DoesNotTrigger() { }
			}

			public abstract class IMySerializer_NonPublicConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:IMySerializer|}|}|}
			{
				protected IMySerializer_NonPublicConstructor_AbstractClass_DoesNotTrigger() { }
			}

			public class IXunitSerializer_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{
				public IXunitSerializer_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger() { }
			}

			public class IMySerializer_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger :{|CS0535:{|CS0535:{|CS0535:IMySerializer|}|}|}
			{
				public IMySerializer_PublicParameterlessConstructor_AbstractClass_DoesNotTrigger() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("IXunitSerializer_WrongConstructor_Triggers", "Xunit.Sdk.IXunitSerializer"),
			Verify.Diagnostic().WithLocation(1).WithArguments("IMySerializer_WrongConstructor_Triggers", "Xunit.Sdk.IXunitSerializer"),
			Verify.Diagnostic().WithLocation(2).WithArguments("IXunitSerializer_NonPublicConstructor_Triggers", "Xunit.Sdk.IXunitSerializer"),
			Verify.Diagnostic().WithLocation(3).WithArguments("IMySerializer_NonPublicConstructor_Triggers", "Xunit.Sdk.IXunitSerializer"),
		};

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source, expected);
	}
}
