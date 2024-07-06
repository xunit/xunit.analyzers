using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using VerifyV2 = CSharpVerifier<SerializableClassMustHaveParameterlessConstructorTests.V2Analyzer>;
using VerifyV3 = CSharpVerifier<SerializableClassMustHaveParameterlessConstructorTests.V3Analyzer>;

public class SerializableClassMustHaveParameterlessConstructorTests
{
	static readonly string Template = @"
using {2};

public interface IMySerializer : IXunitSerializable {{ }}
public class Foo : {0}
{{
    {1}
    public void Deserialize(IXunitSerializationInfo info) {{ }}
    public void Serialize(IXunitSerializationInfo info) {{ }}
}}";
	public static TheoryData<string> Interfaces = new()
	{
		"IXunitSerializable",
		"IMySerializer"
	};

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task ImplicitConstructors_NoDiagnostics(string @interface)
	{
		var v2Source = string.Format(Template, @interface, "", "Xunit.Abstractions");

		await VerifyV2.VerifyAnalyzerV2(v2Source);

		var v3Source = string.Format(Template, @interface, "", "Xunit.Sdk");

		await VerifyV3.VerifyAnalyzerV3(v3Source);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task WrongConstructor_ReturnsError(string @interface)
	{
		var v2Source = string.Format(Template, @interface, "public Foo(int x) { }", "Xunit.Abstractions");
		var v2Expected =
			VerifyV2
				.Diagnostic()
				.WithSpan(5, 14, 5, 17)
				.WithArguments("Foo");

		await VerifyV2.VerifyAnalyzerV2(v2Source, v2Expected);

		var v3Source = string.Format(Template, @interface, "public Foo(int x) { }", "Xunit.Sdk");
		var v3Expected =
			VerifyV3
				.Diagnostic()
				.WithSpan(5, 14, 5, 17)
				.WithArguments("Foo");

		await VerifyV3.VerifyAnalyzerV3(v3Source, v3Expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task NonPublicConstructor_ReturnsError(string @interface)
	{
		var v2Source = string.Format(Template, @interface, "protected Foo() { }", "Xunit.Abstractions");
		var v2Expected =
			VerifyV2
				.Diagnostic()
				.WithSpan(5, 14, 5, 17)
				.WithArguments("Foo");

		await VerifyV2.VerifyAnalyzerV2(v2Source, v2Expected);

		var v3Source = string.Format(Template, @interface, "protected Foo() { }", "Xunit.Sdk");
		var v3Expected =
			VerifyV3
				.Diagnostic()
				.WithSpan(5, 14, 5, 17)
				.WithArguments("Foo");

		await VerifyV3.VerifyAnalyzerV3(v3Source, v3Expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task PublicParameterlessConstructor_NoDiagnostics(string @interface)
	{
		var v2Source = string.Format(Template, @interface, "public Foo() { }", "Xunit.Abstractions");

		await VerifyV2.VerifyAnalyzerV2(v2Source);

		var v3Source = string.Format(Template, @interface, "public Foo() { }", "Xunit.Sdk");

		await VerifyV3.VerifyAnalyzerV3(v3Source);
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
