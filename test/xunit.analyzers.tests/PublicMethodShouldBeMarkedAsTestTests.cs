using Microsoft.CodeAnalysis;
using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

namespace Xunit.Analyzers
{
    public class PublicMethodShouldBeMarkedAsTestTests
    {
        [Fact]
        public async void DoesNotFindErrorForPublicMethodInNonTestClass_CSharp()
        {
            var source = "public class TestClass { public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]
        public async void DoesNotFindErrorForTestMethods_CSharp(string attribute)
        {
            var source = "public class TestClass { [" + attribute + "] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForIDisposableDisposeMethod_CSharp()
        {
            var source =
@"public class TestClass : System.IDisposable {
    [Xunit.Fact] public void TestMethod() { }
    public void Dispose() { }
}";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForPublicAbstractMethod_CSharp()
        {
            var source =
@"public abstract class TestClass {
    [Xunit.Fact] public void TestMethod() { }
    public abstract void AbstractMethod();
}";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForPublicAbstractMethodMarkedWithFact_CSharp()
        {
            var source =
@"public abstract class TestClass {
    [Xunit.Fact] public void TestMethod() { }
    [Xunit.Fact] public abstract void AbstractMethod();
}";
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForIDisposableDisposeMethodOverrideFromParentClass_CSharp()
        {
            var source =
@"public class BaseClass : System.IDisposable {
    public virtual void Dispose() { }
}
public class TestClass : BaseClass {
    [Xunit.Fact] public void TestMethod() { }
    public override void Dispose() { }
}";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForIDisposableDisposeMethodOverrideFromParentClassWithRepeatedInterfaceDeclaration_CSharp()
        {
            var source =
@"public class BaseClass : System.IDisposable {
    public virtual void Dispose() { }
}
public class TestClass : BaseClass, System.IDisposable {
    [Xunit.Fact] public void TestMethod() { }
    public override void Dispose() { }
}";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForIDisposableDisposeMethodOverrideFromGrandParentClass_CSharp()
        {
            var source =
@"public abstract class BaseClass : System.IDisposable {
    public abstract void Dispose();
}
public abstract class IntermediateClass : BaseClass {
}
public class TestClass : IntermediateClass {
    [Xunit.Fact] public void TestMethod() { }
    public override void Dispose() { }
}";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForIAsyncLifetimeMethods_CSharp()
        {
            var source =
@"public class TestClass : Xunit.IAsyncLifetime {
    [Xunit.Fact] public void TestMethod() { }
    public System.Threading.Tasks.Task DisposeAsync()
    {
        throw new System.NotImplementedException();
    }
    public System.Threading.Tasks.Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForPublicMethodMarkedWithAttributeWhichIsMarkedWithIgnoreXunitAnalyzersRule1013_CSharp()
        {
            var source =
@"public class IgnoreXunitAnalyzersRule1013Attribute : System.Attribute { }

[IgnoreXunitAnalyzersRule1013]
public class CustomTestTypeAttribute : System.Attribute { }

public class TestClass { [Xunit.Fact] public void TestMethod() { } [CustomTestType] public void CustomTestMethod() {} }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsWarningForPublicMethodMarkedWithAttributeWhichInheritsFromAttributeMarkedWithIgnoreXunitAnalyzersRule1013_CSharp()
        {
            var source =
@"public class IgnoreXunitAnalyzersRule1013Attribute : System.Attribute { }

[IgnoreXunitAnalyzersRule1013]
public class BaseCustomTestTypeAttribute : System.Attribute { }

public class DerivedCustomTestTypeAttribute : BaseCustomTestTypeAttribute { }

public class TestClass { [Xunit.Fact] public void TestMethod() { } [DerivedCustomTestType] public void CustomTestMethod() {} }";

            var expected = VerifyCS.Diagnostic().WithSpan(8, 104, 8, 120).WithSeverity(DiagnosticSeverity.Warning).WithArguments("CustomTestMethod", "TestClass", "Fact");
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]

        public async void FindsWarningForPublicMethodWithoutParametersInTestClass_CSharp(string attribute)
        {
            var source =
                "public class TestClass { [" + attribute + "] public void TestMethod() { } public void Method() {} }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 70 + attribute.Length, 1, 76 + attribute.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Method", "TestClass", "Fact");
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]

        public async void FindsWarningForPublicMethodWithParametersInTestClass_CSharp(string attribute)
        {
            var source =
                "public class TestClass { [" + attribute + "] public void TestMethod() { } public void Method(int a) {} }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 70 + attribute.Length, 1, 76 + attribute.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Method", "TestClass", "Theory");
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]

        public async void DoesNotFindErrorForOverridenMethod_CSharp(string attribute)
        {
            var source =
                "public class TestClass { [" + attribute + "] public void TestMethod() { } public override void Method() {} }";

            var expected = VerifyCS.CompilerError("CS0115").WithSpan(1, 79 + attribute.Length, 1, 85 + attribute.Length).WithMessage("'TestClass.Method()': no suitable method found to override");
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }
    }
}
