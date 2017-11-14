using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class SerializableClassMustHaveParameterlessConstructorFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new SerializableClassMustHaveParameterlessConstructor();
        readonly CodeFixProvider fixer = new SerializableClassMustHaveParameterlessConstructorFixer();

        [Fact]
        public async void WithNoParameterlessConstructor_AddsConstructor()
        {
            var code =
@"public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
}";
            var expected =
@"public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
    [System.Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase()
    {
    }
}";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, CompilationReporting.IgnoreErrors, code);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async void WithNonPublicParameterlessConstructor_ChangesVisibility()
        {
            var code =
@"public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
    protected MyTestCase() { throw new System.DivideByZeroException(); }
}";
            var expected =
@"public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
    [System.Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase() { throw new System.DivideByZeroException(); }
}";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, CompilationReporting.IgnoreErrors, code);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async void PreservesExistingObsoleteAttribute()
        {
            var code =
@"using obo = System.ObsoleteAttribute;

public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
    [obo(""This is my custom obsolete message"")]
    protected MyTestCase() { throw new System.DivideByZeroException(); }
}";
            var expected =
@"using obo = System.ObsoleteAttribute;

public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
    [obo(""This is my custom obsolete message"")]
    public MyTestCase() { throw new System.DivideByZeroException(); }
}";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, CompilationReporting.IgnoreErrors, code);

            Assert.Equal(expected, result);
        }
    }
}
