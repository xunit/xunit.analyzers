using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertNullFirstOrDefaultShouldNotBeUsedFixerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertNullFirstOrDefaultShouldNotBeUsed();
        private readonly CodeFixProvider fixer = new AssertNullFirstOrDefaultShouldNotBeUsedFixer();

        private static string Template(string expression) => $@"using System.Linq;
using System.Collections.Generic;
using Xunit;

class TestClass
{{
    void TestMethod()
    {{
        var collection = new List<string>();
        {expression};
    }}
}}";

        [Fact]
        public async Task ShouldConvertAssertNullEmptyFirstOrDefaultToAssertEmpty()
        {
            var initial = Template("Assert.Null(collection.FirstOrDefault())");

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, initial);

            var expected = Template("Assert.Empty(collection)");

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ShouldConvertNullFirstOrDefaultToDoesNotContain()
        {
            var initial = Template("Assert.Null(collection.FirstOrDefault(x => x == \"test\"))");

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, initial);

            var expected = Template("Assert.DoesNotContain(collection, x => x == \"test\")");

            Assert.Equal(expected, result);
        }
    }
}
