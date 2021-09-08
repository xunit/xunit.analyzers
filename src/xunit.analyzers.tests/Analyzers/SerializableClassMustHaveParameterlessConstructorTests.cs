using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

namespace Xunit.Analyzers
{
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

		public static TheoryData<string> Interfaces
			= new TheoryData<string> { "IXunitSerializable", "IMySerializer" };

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async void ImplicitConstructors_NoDiagnostics(string @interface)
		{
			var source = string.Format(Template, @interface, "");
			await Verify.VerifyAnalyzerAsync(source);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async void WrongConstructor_ReturnsError(string @interface)
		{
			var source = string.Format(Template, @interface, "public Foo(int x) { }");
			var expected = Verify.Diagnostic().WithSpan(5, 14, 5, 17).WithArguments("Foo");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async void NonPublicConstructor_ReturnsError(string @interface)
		{
			var source = string.Format(Template, @interface, "protected Foo() { }");
			var expected = Verify.Diagnostic().WithSpan(5, 14, 5, 17).WithArguments("Foo");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async void PublicParameterlessConstructor_NoDiagnostics(string @interface)
		{
			var source = string.Format(Template, @interface, "public Foo() { }");
			await Verify.VerifyAnalyzerAsync(source);
		}
	}
}
