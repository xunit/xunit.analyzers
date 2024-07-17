using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.SerializableClassMustHaveParameterlessConstructor>;

public class SerializableClassMustHaveParameterlessConstructorFixerTests
{
	[Fact]
	public async Task WithPublicParameteredConstructor_AddsNewConstructor()
	{
		var beforeTemplate = /* lang=c#-test */ """
			public class [|MyTestCase|]: {0}.IXunitSerializable {{
			    public MyTestCase(int x) {{ }}

			    void {0}.IXunitSerializable.Deserialize({0}.IXunitSerializationInfo _) {{ }}
			    void {0}.IXunitSerializable.Serialize({0}.IXunitSerializationInfo _) {{ }}
			}}
			""";
		var afterTemplate = /* lang=c#-test */ """
			public class MyTestCase: {0}.IXunitSerializable {{
			    [System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
			    public MyTestCase()
			    {{
			    }}

			    public MyTestCase(int x) {{ }}

			    void {0}.IXunitSerializable.Deserialize({0}.IXunitSerializationInfo _) {{ }}
			    void {0}.IXunitSerializable.Serialize({0}.IXunitSerializationInfo _) {{ }}
			}}
			""";

		var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
		var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

		await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

		var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
		var v3After = string.Format(afterTemplate, "Xunit.Sdk");

		await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async Task WithNonPublicParameterlessConstructor_ChangesVisibility_WithoutUsing()
	{
		var beforeTemplate = /* lang=c#-test */ """
			using {0};

			public class [|MyTestCase|]: IXunitSerializable {{
			    protected MyTestCase() {{ throw new System.DivideByZeroException(); }}

			    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
			    void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
			}}
			""";
		var afterTemplate = /* lang=c#-test */ """
			using {0};

			public class MyTestCase: IXunitSerializable {{
			    [System.Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
			    public MyTestCase() {{ throw new System.DivideByZeroException(); }}

			    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
			    void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
			}}
			""";

		var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
		var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

		await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

		var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
		var v3After = string.Format(afterTemplate, "Xunit.Sdk");

		await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async Task WithNonPublicParameterlessConstructor_ChangesVisibility_WithUsing()
	{
		var beforeTemplate = /* lang=c#-test */ """
			using System;
			using {0};

			public class [|MyTestCase|]: IXunitSerializable {{
			    protected MyTestCase() {{ throw new DivideByZeroException(); }}

			    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
			    void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
			}}
			""";
		var afterTemplate = /* lang=c#-test */ """
			using System;
			using {0};

			public class MyTestCase: IXunitSerializable {{
			    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
			    public MyTestCase() {{ throw new DivideByZeroException(); }}

			    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
			    void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
			}}
			""";

		var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
		var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

		await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

		var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
		var v3After = string.Format(afterTemplate, "Xunit.Sdk");

		await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}

	[Fact]
	public async Task PreservesExistingObsoleteAttribute()
	{
		var beforeTemplate = /* lang=c#-test */ """
			using {0};
			using obo = System.ObsoleteAttribute;

			public class [|MyTestCase|]: IXunitSerializable {{
			    [obo("This is my custom obsolete message")]
			    protected MyTestCase() {{ throw new System.DivideByZeroException(); }}

			    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
			    void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
			}}
			""";
		var afterTemplate = /* lang=c#-test */ """
			using {0};
			using obo = System.ObsoleteAttribute;

			public class MyTestCase: IXunitSerializable {{
			    [obo("This is my custom obsolete message")]
			    public MyTestCase() {{ throw new System.DivideByZeroException(); }}

			    void IXunitSerializable.Deserialize(IXunitSerializationInfo _) {{ }}
			    void IXunitSerializable.Serialize(IXunitSerializationInfo _) {{ }}
			}}
			""";

		var v2Before = string.Format(beforeTemplate, "Xunit.Abstractions");
		var v2After = string.Format(afterTemplate, "Xunit.Abstractions");

		await Verify.VerifyCodeFixV2(v2Before, v2After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);

		var v3Before = string.Format(beforeTemplate, "Xunit.Sdk");
		var v3After = string.Format(afterTemplate, "Xunit.Sdk");

		await Verify.VerifyCodeFixV3(v3Before, v3After, SerializableClassMustHaveParameterlessConstructorFixer.Key_GenerateOrUpdateConstructor);
	}
}
