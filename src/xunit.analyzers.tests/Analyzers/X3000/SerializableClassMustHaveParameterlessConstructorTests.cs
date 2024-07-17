using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using VerifyV2 = CSharpVerifier<SerializableClassMustHaveParameterlessConstructorTests.V2Analyzer>;
using VerifyV3 = CSharpVerifier<SerializableClassMustHaveParameterlessConstructorTests.V3Analyzer>;

public class SerializableClassMustHaveParameterlessConstructorTests
{
	static readonly string Template = /* lang=c#-test */ """
		using {2};

		public interface IMySerializer : IXunitSerializable {{ }}
		public class {{|#0:Foo|}} : {0}
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
		await VerifyV2.VerifyAnalyzerV2(string.Format(Template, @interface, "", "Xunit.Abstractions"));
		await VerifyV3.VerifyAnalyzerV3(string.Format(Template, @interface, "", "Xunit.Sdk"));
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task WrongConstructor_Triggers(string @interface)
	{
		var v2Source = string.Format(Template, @interface, /* lang=c#-test */ "public Foo(int x) { }", "Xunit.Abstractions");
		var v2Expected = VerifyV2.Diagnostic().WithLocation(0).WithArguments("Foo");

		await VerifyV2.VerifyAnalyzerV2(v2Source, v2Expected);

		var v3Source = string.Format(Template, @interface, /* lang=c#-test */ "public Foo(int x) { }", "Xunit.Sdk");
		var v3Expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("Foo");

		await VerifyV3.VerifyAnalyzerV3(v3Source, v3Expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task NonPublicConstructor_Triggers(string @interface)
	{
		var v2Source = string.Format(Template, @interface, /* lang=c#-test */ "protected Foo() { }", "Xunit.Abstractions");
		var v2Expected = VerifyV2.Diagnostic().WithLocation(0).WithArguments("Foo");

		await VerifyV2.VerifyAnalyzerV2(v2Source, v2Expected);

		var v3Source = string.Format(Template, @interface, /* lang=c#-test */ "protected Foo() { }", "Xunit.Sdk");
		var v3Expected = VerifyV3.Diagnostic().WithLocation(0).WithArguments("Foo");

		await VerifyV3.VerifyAnalyzerV3(v3Source, v3Expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task PublicParameterlessConstructor_DoesNotTrigger(string @interface)
	{
		await VerifyV2.VerifyAnalyzerV2(string.Format(Template, @interface, "public Foo() { }", "Xunit.Abstractions"));
		await VerifyV3.VerifyAnalyzerV3(string.Format(Template, @interface, "public Foo() { }", "Xunit.Sdk"));
	}

	public class V2Analyzer : SerializableClassMustHaveParameterlessConstructor
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Abstractions(compilation);
	}

	public class V3Analyzer : SerializableClassMustHaveParameterlessConstructor
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3Core(compilation);
	}
}
