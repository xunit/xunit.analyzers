using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

// 2.1.0 does not support params arrays
using Verify_2_1_CS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotHaveParamsArrayTests.Analyzer_2_1_0>;
using Verify_2_1_VB = Xunit.Analyzers.VisualBasicVerifier<Xunit.Analyzers.TheoryMethodCannotHaveParamsArrayTests.Analyzer_2_1_0>;

// 2.2.0 does support params arrays
using Verify_2_2_CS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotHaveParamsArrayTests.Analyzer_2_2_0>;
using Verify_2_2_VB = Xunit.Analyzers.VisualBasicVerifier<Xunit.Analyzers.TheoryMethodCannotHaveParamsArrayTests.Analyzer_2_2_0>;

namespace Xunit.Analyzers
{
    public class TheoryMethodCannotHaveParamsArrayTests
    {
        [Fact]
        public async Task FindsErrorForTheoryWithParamsArrayAsync_WhenParamsArrayNotSupported_CSharp()
        {
            var source =
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, params string[] c) { }" +
                "}";

            var expected = Verify_2_1_CS.Diagnostic().WithSpan(1, 76, 1, 93).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "c");
            await Verify_2_1_CS.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task FindsErrorForTheoryWithParamsArrayAsync_WhenParamsArrayNotSupported_VisualBasic()
        {
            var source = @"
Class TestClass
    <Xunit.Theory>
    Public Sub TestMethod(a As Integer, b As String, ParamArray c As String())
    End Sub
End Class";

            var expected = Verify_2_1_VB.Diagnostic().WithSpan(4, 54, 4, 78).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "c");
            await Verify_2_1_VB.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithParamsArrayAsync_WhenParamsArraySupported_CSharp()
        {
            var source =
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, params string[] c) { }" +
                "}";

            await Verify_2_2_CS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithParamsArrayAsync_WhenParamsArraySupported_VisualBasic()
        {
            var source = @"
Class TestClass
    <Xunit.Theory>
    Public Sub TestMethod(a As Integer, b As String, ParamArray c As String())
    End Sub
End Class";

            await Verify_2_2_VB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithNonParamsArrayAsync_WhenParamsArrayNotSupported_CSharp()
        {
            var source =
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string[] c) { }" +
                "}";

            await Verify_2_1_CS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithNonParamsArrayAsync_WhenParamsArrayNotSupported_VisualBasic()
        {
            var source = @"
Class TestClass
    <Xunit.Theory>
    Public Sub TestMethod(a As Integer, b As String, c As String())
    End Sub
End Class";

            await Verify_2_1_VB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithNonParamsArrayAsync_WhenParamsArraySupported_CSharp()
        {
            var source =
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string[] c) { }" +
                "}";

            await Verify_2_2_CS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithNonParamsArrayAsync_WhenParamsArraySupported_VisualBasic()
        {
            var source = @"
Class TestClass
    <Xunit.Theory>
    Public Sub TestMethod(a As Integer, b As String, c As String())
    End Sub
End Class";

            await Verify_2_2_VB.VerifyAnalyzerAsync(source);
        }

        internal class Analyzer_2_1_0
            : TheoryMethodCannotHaveParamsArray
        {
            public Analyzer_2_1_0()
                : base("2.1.0")
            {
            }
        }

        internal class Analyzer_2_2_0
            : TheoryMethodCannotHaveParamsArray
        {
            public Analyzer_2_2_0()
                : base("2.2.0")
            {
            }
        }
    }
}
