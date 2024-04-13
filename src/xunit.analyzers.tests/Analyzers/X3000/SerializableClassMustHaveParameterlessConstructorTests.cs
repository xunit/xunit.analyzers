using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<SerializableClassMustHaveParameterlessConstructorTests.Analyzer>;

public class SerializableClassMustHaveParameterlessConstructorTests
{
	static readonly string Template = @"
using Xunit.Abstractions;

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
		var source = string.Format(Template, @interface, "");

		await Verify.VerifyAnalyzerV2(source);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task WrongConstructor_ReturnsError(string @interface)
	{
		var source = string.Format(Template, @interface, "public Foo(int x) { }");
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 14, 5, 17)
				.WithArguments("Foo");

		await Verify.VerifyAnalyzerV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task NonPublicConstructor_ReturnsError(string @interface)
	{
		var source = string.Format(Template, @interface, "protected Foo() { }");
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 14, 5, 17)
				.WithArguments("Foo");

		await Verify.VerifyAnalyzerV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async Task PublicParameterlessConstructor_NoDiagnostics(string @interface)
	{
		var source = string.Format(Template, @interface, "public Foo() { }");

		await Verify.VerifyAnalyzerV2(source);
	}

	public class Analyzer : SerializableClassMustHaveParameterlessConstructor
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Abstractions(compilation);
	}
}
