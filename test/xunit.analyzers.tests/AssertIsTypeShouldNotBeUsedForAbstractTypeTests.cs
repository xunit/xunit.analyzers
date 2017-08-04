using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertIsTypeShouldNotBeUsedForAbstractTypeTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new AssertIsTypeShouldNotBeUsedForAbstractType();

        public static TheoryData<string> Methods { get; } = new TheoryData<string> { "IsType", "IsNotType" };

        private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string typeKind, string type)
        {
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not compare an object's exact type to the {typeKind} '{type}'.", d.GetMessage());
                Assert.Equal("xUnit2018", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsError_Interface(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using Xunit;

class TestClass
{
    void TestMethod()
    {
        Assert." + method + @"<IDisposable>(new object());
    }
}");

            AssertHasDiagnostic(diagnostics, "interface", "System.IDisposable");
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsError_AbstractClass(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System.IO;
using Xunit;

class TestClass
{
    void TestMethod()
    {
        Assert." + method + @"<Stream>(new object());
    }
}");

            AssertHasDiagnostic(diagnostics, "abstract class", "System.IO.Stream");
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsError_UsingStatic(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using static Xunit.Assert;

class TestClass
{
    void TestMethod()
    {
        " + method + @"<IDisposable>(new object());
    }
}");

            AssertHasDiagnostic(diagnostics, "interface", "System.IDisposable");
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindError_NonAbstractClass(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using Xunit;

class TestClass
{
    void TestMethod()
    {
        Assert." + method + @"<string>(new object());
    }
}");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("IsAssignableFrom")]
        public async void DoesNotFindError_OtherMethods(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using Xunit;

class TestClass
{
    void TestMethod()
    {
        Assert." + method + @"<IDisposable>(new object());
    }
}");

            Assert.Empty(diagnostics);
        }
    }
}
