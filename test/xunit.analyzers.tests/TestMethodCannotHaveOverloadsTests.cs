using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestMethodCannotHaveOverloadsTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestMethodCannotHaveOverloads();

        [Fact]
        public async void FindsErrors_ForInstanceMethodOverloads_InSameInstanceClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { " +
                "   [Xunit.Fact]" +
                "   public void TestMethod() { }" +
                "   [Xunit.Theory]" +
                "   public void TestMethod(int a) { }" +
                "}");

            Assert.Collection(diagnostics,
                d => VerifyDiagnostic(d, "TestClass"),
                d => VerifyDiagnostic(d, "TestClass"));
        }

        [Fact]
        public async void FindsErrors_ForStaticMethodOverloads_InSameStaticClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public static class TestClass { " +
                "   [Xunit.Fact]" +
                "   public static void TestMethod() { }" +
                "   [Xunit.Theory]" +
                "   public static void TestMethod(int a) { }" +
                "}");

            Assert.Collection(diagnostics,
                d => VerifyDiagnostic(d, "TestClass"),
                d => VerifyDiagnostic(d, "TestClass"));
        }

        [Fact]
        public async void FindsErrors_ForInstanceMethodOverload_InDerivedClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class BaseClass {" +
                "   [Xunit.Fact]" +
                "   public void TestMethod() { }" +
                "}",
                "public class TestClass : BaseClass {" +
                "   [Xunit.Theory]" +
                "   public void TestMethod(int a) { }" +
                "   private void TestMethod(int a, byte c) { }" +
                "}");

            Assert.Collection(diagnostics,
                d => VerifyDiagnostic(d, "BaseClass"),
                d => VerifyDiagnostic(d, "BaseClass"));
        }

        [Fact]
        public async void FindsError_ForStaticAndInstanceMethodOverload()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class BaseClass {" +
                "   [Xunit.Fact]" +
                "   public static void TestMethod() { }" +
                "}",
                "public class TestClass : BaseClass {" +
                "   [Xunit.Theory]" +
                "   public void TestMethod(int a) { }" +
                "}");

            Assert.Collection(diagnostics,
                d => VerifyDiagnostic(d, "BaseClass"));
        }

        [Fact]
        public async void DoesNotFindError_ForMethodOverrides()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class BaseClass {" +
                "   [Xunit.Fact]" +
                "   public virtual void TestMethod() { }" +
                "}",
                "public class TestClass : BaseClass {" +
                "   [Xunit.Fact]" +
                "   public override void TestMethod() { }" +
                "}");

            Assert.Empty(diagnostics);
        }

        private static void VerifyDiagnostic(Diagnostic d, string otherType)
        {
            Assert.Equal($"Test method 'TestMethod' on test class 'TestClass' has the same name as another method declared on class '{otherType}'.", d.GetMessage());
            Assert.Equal("xUnit1024", d.Descriptor.Id);
            Assert.Equal(DiagnosticSeverity.Error, d.Severity);
        }
    }
}
