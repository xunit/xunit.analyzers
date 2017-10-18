using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertThrowsShouldNotBeUsedForAsyncThrowsCheck();

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { 
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), ThrowingMethod);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2014", d.Id);
                Assert.Equal(DiagnosticSeverity.Error, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2014", d.Id);
                Assert.Equal(DiagnosticSeverity.Error, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnAsyncThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), async () => await System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2014", d.Id);
                Assert.Equal(DiagnosticSeverity.Error, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { 
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(ThrowingMethod);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use obsolete Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2019", d.Id);
                Assert.Equal(DiagnosticSeverity.Hidden, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use obsolete Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2019", d.Id);
                Assert.Equal(DiagnosticSeverity.Hidden, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnAsyncThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(async () => await System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use obsolete Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2019", d.Id);
                Assert.Equal(DiagnosticSeverity.Hidden, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethodWithParamName()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { 
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", ThrowingMethod);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use obsolete Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2019", d.Id);
                Assert.Equal(DiagnosticSeverity.Hidden, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambdaWithParamName()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", () => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use obsolete Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2019", d.Id);
                Assert.Equal(DiagnosticSeverity.Hidden, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnAsyncThrowingLambdaWithParamName()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", async () => await System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use obsolete Assert.Throws() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2019", d.Id);
                Assert.Equal(DiagnosticSeverity.Hidden, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnThrowingMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { 
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(ThrowingMethod);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.ThrowsAny() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2014", d.Id);
                Assert.Equal(DiagnosticSeverity.Error, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.ThrowsAny() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2014", d.Id);
                Assert.Equal(DiagnosticSeverity.Error, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnAsyncThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(async () => await System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.ThrowsAny() to check for asynchronously thrown exceptions.", d.GetMessage());
                Assert.Equal("xUnit2014", d.Id);
                Assert.Equal(DiagnosticSeverity.Error, d.Severity);
            });
        }

        [Fact]
        public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionParameter_OnNonAsyncThrowingMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { 
void ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), ThrowingMethod);
} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionParameter_OnNonAsyncThrowingLamba()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, true,
                @"class TestClass { void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), () => 1);
} }");

            Assert.Empty(diagnostics);
        }


        [Fact]
        public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionParameter_OnThrowingMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { 
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync(typeof(System.NotImplementedException), ThrowingMethod);
} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionParameter_OnThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
} 

async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync<System.NotImplementedException>(ThrowingMethod);
} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ForThrowsAnyAsyncCheck_WithExceptionTypeArgument_OnThrowingMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
} 

async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAnyAsync<System.NotImplementedException>(ThrowingMethod);
} }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ForThrowsAnyAsyncCheck_WithExceptionTypeArgument_OnThrowingLambda()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAnyAsync<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Empty(diagnostics);
        }
    }
}
