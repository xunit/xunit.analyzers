using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class SerializableClassMustHaveParameterlessConstructorTests
    {
        readonly DiagnosticAnalyzer analyzer = new SerializableClassMustHaveParameterlessConstructor();

        static readonly string Template = @"
using Xunit.Abstractions;

public interface IMySerializer : IXunitSerializable {{ }}
public class Foo : {0}
{{
    {1}
    public void Deserialize(IXunitSerializationInfo info) {{ }}
    public void Serialize(IXunitSerializationInfo info) {{ }}
}}";

        public static TheoryData<string> Interfaces = new TheoryData<string> { "IXunitSerializable", "IMySerializer" };

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void NoConstructors_ReturnsError(string @interface)
        {
            var code = string.Format(Template, @interface, "");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, code);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Class Foo must have a public parameterless constructor to support Xunit.Abstractions.IXunitSerializable", d.GetMessage());
                    Assert.Equal("xUnit3001", d.Descriptor.Id);
                }
            );
        }

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void WrongConstructor_ReturnsError(string @interface)
        {
            var code = string.Format(Template, @interface, "public Foo(int x) { }");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, code);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Class Foo must have a public parameterless constructor to support Xunit.Abstractions.IXunitSerializable", d.GetMessage());
                    Assert.Equal("xUnit3001", d.Descriptor.Id);
                }
            );
        }

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void NonPublicConstructor_ReturnsError(string @interface)
        {
            var code = string.Format(Template, @interface, "protected Foo() { }");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, code);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Class Foo must have a public parameterless constructor to support Xunit.Abstractions.IXunitSerializable", d.GetMessage());
                    Assert.Equal("xUnit3001", d.Descriptor.Id);
                }
            );
        }

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void PublicParameterlessConstructor_NoDiagnostics(string @interface)
        {
            var code = string.Format(Template, @interface, "public Foo() { }");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, code);

            Assert.Empty(diagnostics);
        }
    }
}
