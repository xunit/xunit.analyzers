namespace Xunit.Analyzers
{
    using System.Linq;
    using Microsoft.CodeAnalysis.Testing;
    using Verify = CSharpVerifier<TestCaseMustBeLongLivedMarshalByRefObject>;

    public class TestCaseMustBeLongLivedMarshalByRefObjectTests
    {
        readonly static string Template = @"
public class Foo {{ }}
public class MyLLMBRO: Xunit.LongLivedMarshalByRefObject {{ }}
public class MyTestCase: {0} {{ }}
";

        public static TheoryData<string> Interfaces =
            new TheoryData<string>
            {
                "{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|}",
                "{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Sdk.IXunitTestCase|}|}|}|}|}|}|}|}|}|}|}|}|}",
            };

        public static TheoryData<string, string> InterfacesWithBaseClasses
        {
            get
            {
                var result = new TheoryData<string, string>();

                foreach (var @interface in Interfaces.Select(x => (string)x[0]))
                {
                    result.Add(@interface, "MyLLMBRO");
                    result.Add(@interface, "Xunit.LongLivedMarshalByRefObject");
                }

                return result;
            }
        }

        [Fact]
        public async void NonTestCase_NoDiagnostics()
        {
            var source = "public class Foo { }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void XunitTestCase_NoDiagnostics()
        {
            var source = "public class MyTestCase : Xunit.Sdk.XunitTestCase { }";

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [MemberData(nameof(InterfacesWithBaseClasses))]
        public async void InterfaceWithProperBaseClass_NoDiagnostics(string @interface, string baseClass)
        {
            var source = string.Format(Template, $"{baseClass}, {@interface}");
            await new Verify.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void InterfaceWithoutBaseClass_ReturnsError(string @interface)
        {
            var source = string.Format(Template, @interface);

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
                    ExpectedDiagnostics = { Verify.Diagnostic().WithLocation(4, 14).WithArguments("MyTestCase") },
                },
            }.RunAsync();
        }

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void InterfaceWithBadBaseClass_ReturnsError(string @interface)
        {
            var source = string.Format(Template, $"Foo, {@interface}");

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
                    ExpectedDiagnostics = { Verify.Diagnostic().WithLocation(4, 14).WithArguments("MyTestCase") },
                },
            }.RunAsync();
        }
    }
}
