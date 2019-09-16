using Microsoft.CodeAnalysis;
using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;
using VerifyVB = Xunit.Analyzers.VisualBasicVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

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

        [Fact]
        public async void DoesNotFindErrorForPublicMethodInNonTestClass_VisualBasic()
        {
            var source = @"
Public Class TestClass
    Public Sub TestMethod()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]
        public async void DoesNotFindErrorForTestMethods_CSharp(string attribute)
        {
            var source = "public class TestClass { [" + attribute + "] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]
        public async void DoesNotFindErrorForTestMethods_VisualBasic(string attribute)
        {
            var source = $@"
Public Class TestClass
    <{attribute}>
    Public Sub TestMethod()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForIDisposableDisposeMethod_VisualBasic()
        {
            var source =
@"Public Class TestClass
    Implements System.IDisposable
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    Public Sub Dispose() Implements System.IDisposable.Dispose
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForPublicAbstractMethod_VisualBasic()
        {
            var source =
@"Public MustInherit Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    Public MustOverride Sub AbstractMethod()
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForPublicAbstractMethodMarkedWithFact_VisualBasic()
        {
            var source =
@"Public MustInherit Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    <Xunit.Fact>
    Public MustOverride Sub AbstractMethod()
End Class";
            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForIDisposableDisposeMethodOverrideFromParentClass_VisualBasic()
        {
            var source =
@"Public Class BaseClass
    Implements System.IDisposable
    Public Overridable Sub Dispose() Implements System.IDisposable.Dispose
    End Sub
End Class
Public Class TestClass
    Inherits BaseClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    Public Overrides Sub Dispose()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForIDisposableDisposeMethodOverrideFromParentClassWithRepeatedInterfaceDeclaration_VisualBasic()
        {
            var source =
@"Public Class BaseClass
    Implements System.IDisposable
    Public Overridable Sub Dispose() Implements System.IDisposable.Dispose
    End Sub
End Class
Public Class TestClass
    Inherits BaseClass
    Implements System.IDisposable
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    Public Overrides Sub Dispose()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForIDisposableDisposeMethodOverrideFromGrandParentClass_VisualBasic()
        {
            var source =
@"Public MustInherit Class BaseClass
    Implements System.IDisposable
    Public MustOverride Sub Dispose() Implements System.IDisposable.Dispose
End Class
Public MustInherit Class IntermediateClass
    Inherits BaseClass
End Class
Public Class TestClass
    Inherits IntermediateClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    Public Overrides Sub Dispose()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForIAsyncLifetimeMethods_VisualBasic()
        {
            var source =
@"Public Class TestClass
    Implements Xunit.IAsyncLifetime
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    Public Function DisposeAsync() As System.Threading.Tasks.Task Implements Xunit.IAsyncLifetime.DisposeAsync
        Throw New System.NotImplementedException()
    End Function
    Public Function InitializeAsync() As System.Threading.Tasks.Task Implements Xunit.IAsyncLifetime.InitializeAsync
        Throw New System.NotImplementedException()
    End Function
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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
        public async void DoesNotFindErrorForPublicMethodMarkedWithAttributeWhichIsMarkedWithIgnoreXunitAnalyzersRule1013_VisualBasic()
        {
            var source =
@"Public Class IgnoreXunitAnalyzersRule1013Attribute
    Inherits System.Attribute
End Class

<IgnoreXunitAnalyzersRule1013>
Public Class CustomTestTypeAttribute
    Inherits System.Attribute
End Class

Public Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    <CustomTestType>
    Public Sub CustomTestMethod()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
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

        [Fact]
        public async void FindsWarningForPublicMethodMarkedWithAttributeWhichInheritsFromAttributeMarkedWithIgnoreXunitAnalyzersRule1013_VisualBasic()
        {
            var source =
@"Public Class IgnoreXunitAnalyzersRule1013Attribute
    Inherits System.Attribute
End Class

<IgnoreXunitAnalyzersRule1013>
Public Class BaseCustomTestTypeAttribute
    Inherits System.Attribute
End Class

Public Class DerivedCustomTestTypeAttribute
    Inherits BaseCustomTestTypeAttribute
End Class

Public Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
    <DerivedCustomTestType>
    Public Sub CustomTestMethod()
    End Sub
End Class";

            var expected = VerifyVB.Diagnostic().WithSpan(19, 16, 19, 32).WithSeverity(DiagnosticSeverity.Warning).WithArguments("CustomTestMethod", "TestClass", "Fact");
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
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

        public async void FindsWarningForPublicMethodWithoutParametersInTestClass_VisualBasic(string attribute)
        {
            var source = $@"
Public Class TestClass
    <{attribute}>
    Public Sub TestMethod()
    End Sub
    Public Sub Method()
    End Sub
End Class";

            var expected = VerifyVB.Diagnostic().WithSpan(6, 16, 6, 22).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Method", "TestClass", "Fact");
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
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

        public async void FindsWarningForPublicMethodWithParametersInTestClass_VisualBasic(string attribute)
        {
            var source = $@"
Public Class TestClass
    <{attribute}>
    Public Sub TestMethod()
    End Sub
    Public Sub Method(a As Integer)
    End Sub
End Class";

            var expected = VerifyVB.Diagnostic().WithSpan(6, 16, 6, 22).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Method", "TestClass", "Theory");
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
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

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]

        public async void DoesNotFindErrorForOverridenMethod_VisualBasic(string attribute)
        {
            var source = $@"
Public Class TestClass
    <{attribute}>
    Public Sub TestMethod()
    End Sub
    Public Overrides Sub Method()
    End Sub
End Class";

            var expected = VerifyVB.CompilerError("BC30284").WithSpan(6, 26, 6, 32).WithMessage("sub 'Method' cannot be declared 'Overrides' because it does not override a sub in a base class.");
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
        }
    }
}
