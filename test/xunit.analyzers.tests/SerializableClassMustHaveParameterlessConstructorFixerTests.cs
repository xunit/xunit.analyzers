using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

namespace Xunit.Analyzers
{
    public class SerializableClassMustHaveParameterlessConstructorFixerTests
    {
        [Fact]
        public async void WithNonPublicParameterlessConstructor_ChangesVisibility_WithoutUsing()
        {
            var code =
@"public class [|MyTestCase|] : {|CS0535:{|CS0535:Xunit.Abstractions.IXunitSerializable|}|}
{
    protected MyTestCase() { throw new System.DivideByZeroException(); }
}";
            var expected =
@"public class MyTestCase : {|CS0535:{|CS0535:Xunit.Abstractions.IXunitSerializable|}|}
{
    [System.Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase() { throw new System.DivideByZeroException(); }
}";

            await Verify.VerifyCodeFixAsync(code, expected);
        }

        [Fact]
        public async void WithNonPublicParameterlessConstructor_ChangesVisibility_WithUsing()
        {
            var code =
@"using System;
using Xunit.Abstractions;

public class [|MyTestCase|] : {|CS0535:{|CS0535:IXunitSerializable|}|}
{
    protected MyTestCase() { throw new DivideByZeroException(); }
}";
            var expected =
@"using System;
using Xunit.Abstractions;

public class MyTestCase : {|CS0535:{|CS0535:IXunitSerializable|}|}
{
    [Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase() { throw new DivideByZeroException(); }
}";

            await Verify.VerifyCodeFixAsync(code, expected);
        }

        [Fact]
        public async void PreservesExistingObsoleteAttribute()
        {
            var code =
@"using obo = System.ObsoleteAttribute;

public class [|MyTestCase|] : {|CS0535:{|CS0535:Xunit.Abstractions.IXunitSerializable|}|}
{
    [obo(""This is my custom obsolete message"")]
    protected MyTestCase() { throw new System.DivideByZeroException(); }
}";
            var expected =
@"using obo = System.ObsoleteAttribute;

public class MyTestCase : {|CS0535:{|CS0535:Xunit.Abstractions.IXunitSerializable|}|}
{
    [obo(""This is my custom obsolete message"")]
    public MyTestCase() { throw new System.DivideByZeroException(); }
}";

            await Verify.VerifyCodeFixAsync(code, expected);
        }
    }
}
