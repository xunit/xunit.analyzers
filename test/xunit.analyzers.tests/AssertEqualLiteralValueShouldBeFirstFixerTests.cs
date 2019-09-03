using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

namespace Xunit.Analyzers
{
    public class AssertEqualLiteralValueShouldBeFirstFixerTests
    {
        static readonly string Template = @"
public class TestClass
{{
    [Xunit.Fact]
    public void TestMethod()
    {{
        var i = 0;
        Xunit.{0};
    }}
}}";

        [Fact]
        public async void SwapArguments()
        {
            var source = string.Format(Template, "Assert.Equal(i, 0)");
            var fixedSource = string.Format(Template, "Assert.Equal(0, i)");

            var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }

        [Fact]
        public async void NamedArgumentsOnlySwapsArgumentValues()
        {
            var source = string.Format(Template, "Assert.Equal(actual: 0, expected: i)");
            var fixedSource = string.Format(Template, "Assert.Equal(actual: i, expected: 0)");

            var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }

        [Fact]
        public async void NamedArgumentsInCorrectPositionOnlySwapsArgumentValues()
        {
            var source = string.Format(Template, "Assert.Equal(expected: i, actual: 0)");
            var fixedSource = string.Format(Template, "Assert.Equal(expected: 0, actual: i)");

            var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }

        [Fact]
        public async void NamedArgumentsTakePossibleThirdParameterIntoAccount()
        {
            var source = string.Format(Template, "Assert.Equal(comparer: null, actual: 0, expected: i)");
            var fixedSource = string.Format(Template, "Assert.Equal(comparer: null, actual: i, expected: 0)");

            var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual, comparer)", "TestMethod", "TestClass");
            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }

        [Fact]
        public async void PartiallyNamedArgumentsInCorrectPositionOnlySwapsArgumentValues()
        {
            var source = string.Format(Template, "Assert.Equal(expected: i, 0)");
            var fixedSource = string.Format(Template, "Assert.Equal(expected: 0, i)");

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { source },
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass"),
                    },
                },
                FixedState = { Sources = { fixedSource } },
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                        return solution.WithProjectParseOptions(
                            projectId,
                            ((CSharpParseOptions)solution.GetProject(projectId).ParseOptions).WithLanguageVersion(LanguageVersion.CSharp7_2));
                    },
                },
            }.RunAsync();
        }
    }
}
