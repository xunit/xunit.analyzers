using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

public class SerializableClassMustHaveParameterlessConstructorFixerTests
{
	[Fact]
	public async Task WithPublicParameteredConstructor_AddsNewConstructor()
	{
		var before = @"
public class [|MyTestCase|]: Xunit.Abstractions.IXunitSerializable {
    public MyTestCase(int x) { }

    void Xunit.Abstractions.IXunitSerializable.Deserialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
    void Xunit.Abstractions.IXunitSerializable.Serialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
}";

		var after = @"
public class MyTestCase: Xunit.Abstractions.IXunitSerializable {
    [System.Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase()
    {
    }

    public MyTestCase(int x) { }

    void Xunit.Abstractions.IXunitSerializable.Deserialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
    void Xunit.Abstractions.IXunitSerializable.Serialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
}";

		await Verify.VerifyCodeFixV2(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async Task WithNonPublicParameterlessConstructor_ChangesVisibility_WithoutUsing()
	{
		var before = @"
public class [|MyTestCase|]: Xunit.Abstractions.IXunitSerializable {
    protected MyTestCase() { throw new System.DivideByZeroException(); }

    void Xunit.Abstractions.IXunitSerializable.Deserialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
    void Xunit.Abstractions.IXunitSerializable.Serialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
}";

		var after = @"
public class MyTestCase: Xunit.Abstractions.IXunitSerializable {
    [System.Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase() { throw new System.DivideByZeroException(); }

    void Xunit.Abstractions.IXunitSerializable.Deserialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
    void Xunit.Abstractions.IXunitSerializable.Serialize(Xunit.Abstractions.IXunitSerializationInfo _) { }
}";

		await Verify.VerifyCodeFixV2(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async Task WithNonPublicParameterlessConstructor_ChangesVisibility_WithUsing()
	{
		var before = @"
using System;
using Xunit.Abstractions;

public class [|MyTestCase|]: IXunitSerializable {
    protected MyTestCase() { throw new DivideByZeroException(); }

    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
    void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
}";

		var after = @"
using System;
using Xunit.Abstractions;

public class MyTestCase: IXunitSerializable {
    [Obsolete(""Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"")]
    public MyTestCase() { throw new DivideByZeroException(); }

    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
    void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
}";

		await Verify.VerifyCodeFixV2(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async Task PreservesExistingObsoleteAttribute()
	{
		var before = @"
using Xunit.Abstractions;
using obo = System.ObsoleteAttribute;

public class [|MyTestCase|]: IXunitSerializable {
    [obo(""This is my custom obsolete message"")]
    protected MyTestCase() { throw new System.DivideByZeroException(); }

    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
    void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
}";

		var after = @"
using Xunit.Abstractions;
using obo = System.ObsoleteAttribute;

public class MyTestCase: IXunitSerializable {
    [obo(""This is my custom obsolete message"")]
    public MyTestCase() { throw new System.DivideByZeroException(); }

    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) { }
    void IXunitSerializable.Serialize(IXunitSerializationInfo _) { }
}";

		await Verify.VerifyCodeFixV2(before, after, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}
}
