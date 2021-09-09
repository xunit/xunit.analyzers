using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

public class SerializableClassMustHaveParameterlessConstructorFixerTests
{
	[Fact]
	public async void WithNonPublicParameterlessConstructor_ChangesVisibility_WithoutUsing()
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

		await Verify.VerifyCodeFixAsync(before, after);
	}

	[Fact]
	public async void WithNonPublicParameterlessConstructor_ChangesVisibility_WithUsing()
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

		await Verify.VerifyCodeFixAsync(before, after);
	}

	[Fact]
	public async void PreservesExistingObsoleteAttribute()
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

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
