using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataRowArgumentsShouldBeSerializable>;

public sealed class TheoryDataRowArgumentsShouldBeSerializableTests
{
	[Fact]
	public async Task ParamArrayArguments_NotUsingTheoryDataRow_DoesNotTrigger()
	{
		var source = @"
#nullable enable

public class Foo {
    public Foo(params object[] args) { }
}

public class TestClass {
    public void TestMethod() {
        var foo = new Foo(new object());
    }
}";

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
#if NET6_0_OR_GREATER && ROSLYN_4_4_OR_GREATER
	[InlineData("DateOnly.MinValue", "DateOnly")]
	[InlineData("TimeOnly.MinValue", "TimeOnly")]
#endif
	public async Task IntrinsicallySerializableValue_DoesNotTrigger(
		string value,
		string type)
	{
		var source = $@"
#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit.Sdk;

public class MyClass {{
    public IEnumerable<TheoryDataRow> MyMethod() {{
        var value = {value};
        var defaultValue = default({type});
        var nullValue = default({type}?);
        var arrayValue = new {type}[0];

        yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
        yield return new TheoryDataRow({value}, default({type}), default({type}?), new {type}[0]);
    }}
}}";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Theory]
	[InlineData("SerializableClass")]
	[InlineData("SerializableStruct")]
	public async Task IXunitSerializableValue_DoesNotTrigger(string type)
	{
		var source = $@"
#nullable enable

using System.Collections.Generic;
using Xunit.Sdk;

public class MyClass {{
    public IEnumerable<TheoryDataRow> MyMethod() {{
        var value = new {type}();
        var defaultValue = default({type});
        var nullValue = default({type}?);
        var arrayValue = new {type}[0];

        yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
        yield return new TheoryDataRow(new {type}(), default({type}), default({type}?), new {type}[0]);
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
}}";

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
		var source = $@"
#nullable enable

using System;
using System.Collections.Generic;
using Xunit.Sdk;

public class MyClass {{
    public IEnumerable<TheoryDataRow> MyMethod() {{
        var defaultValue = default({type});
        var nullValue = default({type}?);
        var arrayValue = new {type}[0];

        yield return new TheoryDataRow(defaultValue, nullValue, arrayValue);
        yield return new TheoryDataRow(default({type}), default({type}?), new {type}[0]);
    }}
}}

public sealed class NonSerializableSealedClass {{ }}

public struct NonSerializableStruct {{ }}";

		var expected = new[] {
				Verify
					.Diagnostic("xUnit1046")
					.WithSpan(14, 40, 14, 52)
					.WithArguments("defaultValue", defaultValueType),
				Verify
					.Diagnostic("xUnit1046")
					.WithSpan(14, 54, 14, 63)
					.WithArguments("nullValue", nullValueType),
				Verify
					.Diagnostic("xUnit1046")
					.WithSpan(14, 65, 14, 75)
					.WithArguments("arrayValue", $"{type}[]"),
				Verify
					.Diagnostic("xUnit1046")
					.WithSpan(15, 40, 15, 49 + type.Length)
					.WithArguments($"default({type})", defaultValueType),
				Verify
					.Diagnostic("xUnit1046")
					.WithSpan(15, 51 + type.Length, 15, 61 + type.Length * 2)
					.WithArguments($"default({type}?)", nullValueType),
				Verify
					.Diagnostic("xUnit1046")
					.WithSpan(15, 63 + type.Length * 2, 15, 70 + type.Length * 3)
					.WithArguments($"new {type}[0]", $"{type}[]"),
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
		var source = $@"
#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using Xunit.Sdk;

public class MyClass {{
    public IEnumerable<TheoryDataRow> MyMethod() {{
        var defaultValue = default({type});
        var nullValue = default({type}?);
        var arrayValue = new {type}[0];

        yield return new TheoryDataRow(defaultValue, nullValue, arrayValue);
        yield return new TheoryDataRow(default({type}), default({type}?), new {type}[0]);
    }}
}}

public interface IPossiblySerializableInterface {{ }}

public class PossiblySerializableUnsealedClass {{ }}";

		var expected = new[] {
			Verify
				.Diagnostic("xUnit1047")
				.WithSpan(15, 40, 15, 52)
				.WithArguments("defaultValue", $"{type}?"),
			Verify
				.Diagnostic("xUnit1047")
				.WithSpan(15, 54, 15, 63)
				.WithArguments("nullValue", $"{type}?"),
			Verify
				.Diagnostic("xUnit1047")
				.WithSpan(15, 65, 15, 75)
				.WithArguments("arrayValue", $"{type}[]"),
			Verify
				.Diagnostic("xUnit1047")
				.WithSpan(16, 40, 16, 49 + type.Length)
				.WithArguments($"default({type})", $"{type}?"),
			Verify
				.Diagnostic("xUnit1047")
				.WithSpan(16, 51 + type.Length, 16, 61 + type.Length * 2)
				.WithArguments($"default({type}?)", $"{type}?"),
			Verify
				.Diagnostic("xUnit1047")
				.WithSpan(16, 63 + type.Length * 2, 16, 70 + type.Length * 3)
				.WithArguments($"new {type}[0]", $"{type}[]"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}
}
