using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataRowArgumentsShouldBeSerializable>;

public sealed class TheoryDataRowArgumentsShouldBeSerializableTests
{
	[Fact]
	public async Task ParamArrayArguments_NotUsingTheoryDataRow_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			public class Foo {
			    public Foo(params object[] args) { }
			}

			public class TestClass {
			    public void TestMethod() {
			        var foo = new Foo(new object());
			    }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("\"String value\"", "string")]
	[InlineData("'a'", "char")]
	[InlineData("(byte)42", "byte")]
	[InlineData("(sbyte)42", "sbyte")]
	[InlineData("(short)42", "short")]
	[InlineData("(ushort)42", "ushort")]
	[InlineData("42", "int")]
	[InlineData("42U", "uint")]
	[InlineData("42L", "long")]
	[InlineData("42UL", "ulong")]
	[InlineData("21.12F", "float")]
	[InlineData("21.12D", "double")]
	[InlineData("21.12M", "decimal")]
	[InlineData("true", "bool")]
	[InlineData("DateTime.Now", "DateTime")]
	[InlineData("DateTimeOffset.Now", "DateTimeOffset")]
	[InlineData("TimeSpan.Zero", "TimeSpan")]
	[InlineData("BigInteger.One", "BigInteger")]
	[InlineData("typeof(TheoryDataRow)", "Type")]
	[InlineData("ConsoleColor.Red", "ConsoleColor")]
	[InlineData("new Dictionary<string, List<string>>()", "Dictionary<string, List<string>>")]
#if NET6_0_OR_GREATER
	[InlineData("DateOnly.MinValue", "DateOnly")]
	[InlineData("TimeOnly.MinValue", "TimeOnly")]
#endif
	public async Task IntrinsicallySerializableValue_DoesNotTrigger(
		string value,
		string type)
	{
		var source = string.Format(/* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using System.Numerics;
			using Xunit;

			public class MyClass {{
			    public IEnumerable<TheoryDataRowBase> MyMethod() {{
			        var value = {0};
			        var defaultValue = default({1});
			        var nullValue = default({1}?);
			        var arrayValue = new {1}[0];

			        yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
			        yield return new TheoryDataRow<{1}, {1}?, {1}?, {1}[]>({0}, default({1}), default({1}?), new {1}[0]);
			    }}
			}}
			""", value, type);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("SerializableClass")]
	[InlineData("SerializableStruct")]
	public async Task IXunitSerializableValue_DoesNotTrigger(string type)
	{
		var source = string.Format(/* lang=c#-test */ """
			#nullable enable

			using System.Collections.Generic;
			using Xunit;
			using Xunit.Sdk;

			public class MyClass {{
			    public IEnumerable<TheoryDataRowBase> MyMethod() {{
			        var value = new {0}();
			        var defaultValue = default({0});
			        var nullValue = default({0}?);
			        var arrayValue = new {0}[0];

			        yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
			        yield return new TheoryDataRow<{0}, {0}?, {0}?, {0}[]>(new {0}(), default({0}), default({0}?), new {0}[0]);
			    }}
			}}

			public interface ISerializableInterface : IXunitSerializable {{ }}

			public class SerializableClass : ISerializableInterface {{
			    public void Deserialize(IXunitSerializationInfo info) {{ }}
			    public void Serialize(IXunitSerializationInfo info) {{ }}
			}}

			public struct SerializableStruct : ISerializableInterface {{
			    public void Deserialize(IXunitSerializationInfo info) {{ }}
			    public void Serialize(IXunitSerializationInfo info) {{ }}
			}}
			""", type);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("CustomSerialized")]
	[InlineData("CustomSerializedDerived")]
	public async Task IXunitSerializerValue_DoesNotTrigger(string type)
	{
		var source = string.Format(/* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using Xunit;
			using Xunit.Sdk;

			[assembly: RegisterXunitSerializer(typeof(CustomSerializer), typeof(ICustomSerialized))]
			
			public class MyClass {{
			    public IEnumerable<TheoryDataRowBase> MyMethod() {{
			        var value = new {0}();
			        var defaultValue = default({0});
			        var nullValue = default({0}?);
			        var arrayValue = new {0}[0];

			        yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
			        yield return new TheoryDataRow<{0}, {0}?, {0}?, {0}[]>(new {0}(), default({0}), default({0}?), new {0}[0]);
			    }}
			}}

			public interface ICustomSerialized {{ }}
			
			public class CustomSerialized : ICustomSerialized {{ }}
			
			public class CustomSerializedDerived : CustomSerialized {{ }}
			
			public class CustomSerializer : IXunitSerializer {{
			    public object Deserialize(Type type, string serializedValue) =>
			        throw new NotImplementedException();
			
			    public bool IsSerializable(Type type, object? value) =>
			        true;
			
			    public string Serialize(object value) =>
			        throw new NotImplementedException();
			}}
			""", type);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("Delegate", "Delegate?", "Delegate?")]
	[InlineData("Func<int>", "Func<int>?", "Func<int>?")]
	[InlineData("NonSerializableSealedClass", "NonSerializableSealedClass?", "NonSerializableSealedClass?")]
	[InlineData("NonSerializableStruct", "NonSerializableStruct", "NonSerializableStruct?")]
	public async Task KnownNonSerializableValue_Triggers1046(
		string type,
		string defaultValueType,
		string nullValueType)
	{
		var source = string.Format(/* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using Xunit;

			public class MyClass {{
			    public IEnumerable<TheoryDataRowBase> MyMethod() {{
			        var defaultValue = default({0});
			        var nullValue = default({0}?);
			        var arrayValue = new {0}[0];

			        yield return new TheoryDataRow({{|#0:defaultValue|}}, {{|#1:nullValue|}}, {{|#2:arrayValue|}});
			        yield return new TheoryDataRow<{1}, {2}, {0}[]>({{|#3:default({0})|}}, {{|#4:default({0}?)|}}, {{|#5:new {0}[0]|}});
			    }}
			}}

			public sealed class NonSerializableSealedClass {{ }}

			public struct NonSerializableStruct {{ }}
			""", type, defaultValueType, nullValueType);
		var expected = new[] {
			Verify.Diagnostic("xUnit1046").WithLocation(0).WithArguments("defaultValue", defaultValueType),
			Verify.Diagnostic("xUnit1046").WithLocation(1).WithArguments("nullValue", nullValueType),
			Verify.Diagnostic("xUnit1046").WithLocation(2).WithArguments("arrayValue", $"{type}[]"),
			Verify.Diagnostic("xUnit1046").WithLocation(3).WithArguments($"default({type})", defaultValueType),
			Verify.Diagnostic("xUnit1046").WithLocation(4).WithArguments($"default({type}?)", nullValueType),
			Verify.Diagnostic("xUnit1046").WithLocation(5).WithArguments($"new {type}[0]", $"{type}[]"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}

	[Theory]
	[InlineData("NonSerializableSealedClass")]
	[InlineData("NonSerializableStruct")]
	public async Task KnownNonSerializableValue_Constructable_Triggers1046(string type)
	{
		var source = string.Format(/* lang=c#-test */ """
			#nullable enable

			using System.Collections.Generic;
			using Xunit;

			public class MyClass {{
			    public IEnumerable<TheoryDataRowBase> MyMethod() {{
			        var value = new {0}();

			        yield return new TheoryDataRow({{|#0:value|}});
			        yield return new TheoryDataRow({{|#1:new {0}()|}});
			        yield return new TheoryDataRow<{0}>({{|#2:value|}});
			        yield return new TheoryDataRow<{0}>({{|#3:new {0}()|}});
			    }}
			}}

			public sealed class NonSerializableSealedClass {{ }}

			public struct NonSerializableStruct {{ }}
			""", type);
		var expected = new[] {
			Verify.Diagnostic("xUnit1046").WithLocation(0).WithArguments("value", type),
			Verify.Diagnostic("xUnit1046").WithLocation(1).WithArguments($"new {type}()", type),
			Verify.Diagnostic("xUnit1046").WithLocation(2).WithArguments("value", type),
			Verify.Diagnostic("xUnit1046").WithLocation(3).WithArguments($"new {type}()", type),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}

	[Theory]
	[InlineData("object")]
	[InlineData("Array")]
	[InlineData("ValueType")]
	[InlineData("IEnumerable")]
	[InlineData("IEnumerable<int>")]
	[InlineData("Dictionary<int, string>")]
	[InlineData("IPossiblySerializableInterface")]
	[InlineData("PossiblySerializableUnsealedClass")]
	public async Task MaybeNonSerializableValue_Triggers1047(string type)
	{
		var source = string.Format(/* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class MyClass {{
			    public IEnumerable<TheoryDataRowBase> MyMethod() {{
			        var defaultValue = default({0});
			        var nullValue = default({0}?);
			        var arrayValue = new {0}[0];

			        yield return new TheoryDataRow({{|#0:defaultValue|}}, {{|#1:nullValue|}}, {{|#2:arrayValue|}});
			        yield return new TheoryDataRow<{0}, {0}, {0}[]>({{|#3:default({0})|}}, {{|#4:default({0}?)|}}, {{|#5:new {0}[0]|}});
			    }}
			}}

			public interface IPossiblySerializableInterface {{ }}

			public class PossiblySerializableUnsealedClass {{ }}
			""", type);
		var expected = new[] {
			Verify.Diagnostic("xUnit1047").WithLocation(0).WithArguments("defaultValue", $"{type}?"),
			Verify.Diagnostic("xUnit1047").WithLocation(1).WithArguments("nullValue", $"{type}?"),
			Verify.Diagnostic("xUnit1047").WithLocation(2).WithArguments("arrayValue", $"{type}[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(3).WithArguments($"default({type})", $"{type}?"),
			Verify.Diagnostic("xUnit1047").WithLocation(4).WithArguments($"default({type}?)", $"{type}?"),
			Verify.Diagnostic("xUnit1047").WithLocation(5).WithArguments($"new {type}[0]", $"{type}[]"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}

	[Theory]
	[InlineData("object")]
	[InlineData("Dictionary<int, string>")]
	[InlineData("PossiblySerializableUnsealedClass")]
	public async Task MaybeNonSerializableValue_Constructable_Triggers1047(string type)
	{
		var source = string.Format(/* lang=c#-test */ """
			#nullable enable

			using System.Collections.Generic;
			using Xunit;

			public class MyClass {{
			    public IEnumerable<TheoryDataRowBase> MyMethod() {{
			        var value = new {0}();

			        yield return new TheoryDataRow({{|#0:value|}});
			        yield return new TheoryDataRow({{|#1:new {0}()|}});
			        yield return new TheoryDataRow<{0}>({{|#2:value|}});
			        yield return new TheoryDataRow<{0}>({{|#3:new {0}()|}});
			    }}
			}}

			public class PossiblySerializableUnsealedClass {{ }}
			""", type);
		var expected = new[] {
			Verify.Diagnostic("xUnit1047").WithLocation(0).WithArguments("value", type),
			Verify.Diagnostic("xUnit1047").WithLocation(1).WithArguments($"new {type}()", type),
			Verify.Diagnostic("xUnit1047").WithLocation(2).WithArguments("value", type),
			Verify.Diagnostic("xUnit1047").WithLocation(3).WithArguments($"new {type}()", type),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}
}
