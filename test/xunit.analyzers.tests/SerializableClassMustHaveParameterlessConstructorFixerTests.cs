using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class SerializableClassMustHaveParameterlessConstructorFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new SerializableClassMustHaveParameterlessConstructor();
        readonly CodeFixProvider fixer = new SerializableClassMustHaveParameterlessConstructorFixer();

        [Fact]
        public async void WithNoParameterlessConstructor_AddsConstructor_WithoutUsing()
        {
            var code =
@"public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
    public void Foo() { }
}";
            var expected =
@"public class MyTestCase : Xunit.Abstractions.IXunitSerializable
{
    [System.Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase()
    {
    }

    public void Foo() { }
}";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors);

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async void WithNoParameterlessConstructor_AddsConstructor_WithUsing()
        {
            var code =
@"using System;
using Xunit.Abstractions;

public class MyTestCase : IXunitSerializable
{
    public void Foo() { }
}";
            var expected =
@"using System;
using Xunit.Abstractions;

public class MyTestCase : IXunitSerializable
{
    [Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase()
    {
    }

    public void Foo() { }
}";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors);

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async void WithNonPublicParameterlessConstructor_ChangesVisibility_WithoutUsing()
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

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors);

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async void WithNonPublicParameterlessConstructor_ChangesVisibility_WithUsing()
        {
            var code =
@"using System;
using Xunit.Abstractions;

public class MyTestCase : IXunitSerializable
{
    protected MyTestCase() { throw new DivideByZeroException(); }
}";
            var expected =
@"using System;
using Xunit.Abstractions;

public class MyTestCase : IXunitSerializable
{
    [Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase() { throw new DivideByZeroException(); }
}";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors);

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
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

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, code, CompilationReporting.IgnoreErrors);

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
    }
}
