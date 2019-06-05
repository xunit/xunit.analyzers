using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertNullFirstOrDefaultShouldNotBeUsedTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertNullFirstOrDefaultShouldNotBeUsed();

        [Fact]
        public async void FindsWarningForFirstOrDefaultInsideAssertNullWithoutArguments()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                          @"
using System.Linq;
using System.Collections.Generic;
using Xunit;

class TestClass
{
	void TestMethod()
	{ 
        var collection = new List<string>();
    	Assert.Null(collection.FirstOrDefault());
	}
}");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use FirstOrDefault within Assert.Null or Assert.NotNull. Use Empty/Contains instead.", d.GetMessage());
                Assert.Equal("xUnit2020", d.Id);
                Assert.Equal(DiagnosticSeverity.Info, d.Severity);
            });
        }
    }
}
