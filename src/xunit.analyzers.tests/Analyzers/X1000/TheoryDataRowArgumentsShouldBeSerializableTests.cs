using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataRowArgumentsShouldBeSerializable>;
using Verify_v3_Pre301 = CSharpVerifier<TheoryDataRowArgumentsShouldBeSerializableTests.Analyzer_v3_Pre301>;

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
	[InlineData("Index.Start", "Index")]
	[InlineData("Range.All", "Range")]
	[InlineData("TimeOnly.MinValue", "TimeOnly")]
#endif
	[InlineData("Guid.Empty", "Guid")]
	[InlineData("new Uri(\"https://xunit.net/\")", "Uri")]
	[InlineData("new Version(\"1.2.3\")", "Version")]
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

				public bool IsSerializable(Type type, object? value, out string? failureReason)
				{{
					failureReason = null;
					return true;
				}}

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

	[Fact]
	public async Task IFormattableAndIParseable_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using System.Diagnostics.CodeAnalysis;
			using Xunit;

			public class Formattable : IFormattable
			{
				public string ToString(string? format, IFormatProvider? formatProvider) => string.Empty;
			}

			public class Parsable : IParsable<Parsable>
			{
				public static Parsable Parse(string s, IFormatProvider? provider) => new();
				public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Parsable result)
				{
					result = new();
					return true;
				}
			}

			public class FormattableAndParsable : IFormattable, IParsable<FormattableAndParsable>
			{
				public static FormattableAndParsable Parse(string s, IFormatProvider? provider) => new();
				public string ToString(string? format, IFormatProvider? formatProvider) => string.Empty;
				public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FormattableAndParsable result)
				{
					result = new();
					return true;
				}
			}

			public class FormattableData {
				public IEnumerable<TheoryDataRowBase> MyMethod() {
					var defaultValue = default(Formattable);
					var nullValue = default(Formattable?);
					var arrayValue = new Formattable[0];

					yield return new TheoryDataRow({|#0:defaultValue|}, {|#1:nullValue|}, {|#2:arrayValue|});
					yield return new TheoryDataRow<Formattable, Formattable, Formattable[]>({|#3:default(Formattable)|}, {|#4:default(Formattable?)|}, {|#5:new Formattable[0]|});
				}
			}

			public class ParsableData {
				public IEnumerable<TheoryDataRowBase> MyMethod() {
					var defaultValue = default(Parsable);
					var nullValue = default(Parsable?);
					var arrayValue = new Parsable[0];

					yield return new TheoryDataRow({|#10:defaultValue|}, {|#11:nullValue|}, {|#12:arrayValue|});
					yield return new TheoryDataRow<Parsable, Parsable, Parsable[]>({|#13:default(Parsable)|}, {|#14:default(Parsable?)|}, {|#15:new Parsable[0]|});
				}
			}

			public class FormattableAndParsableData {
				public IEnumerable<TheoryDataRowBase> MyMethod() {
					var defaultValue = default(FormattableAndParsable);
					var nullValue = default(FormattableAndParsable?);
					var arrayValue = new FormattableAndParsable[0];

					yield return new TheoryDataRow(defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<FormattableAndParsable, FormattableAndParsable, FormattableAndParsable[]>(default(FormattableAndParsable), default(FormattableAndParsable?), new FormattableAndParsable[0]);
				}
			}
			""";
#if ROSLYN_LATEST && NET8_0_OR_GREATER
		var expected = new[] {
			Verify.Diagnostic("xUnit1047").WithLocation(0).WithArguments("defaultValue", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(1).WithArguments("nullValue", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(2).WithArguments("arrayValue", "Formattable[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(3).WithArguments("default(Formattable)", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(4).WithArguments("default(Formattable?)", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(5).WithArguments("new Formattable[0]", "Formattable[]"),

			Verify.Diagnostic("xUnit1047").WithLocation(10).WithArguments("defaultValue", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(11).WithArguments("nullValue", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(12).WithArguments("arrayValue", "Parsable[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(13).WithArguments("default(Parsable)", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(14).WithArguments("default(Parsable?)", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(15).WithArguments("new Parsable[0]", "Parsable[]"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
#else
		// For some reason, 'dotnet format' complains about the indenting of #nullable enable in the source code line
		// above if the #if statement surrounds the whole method, so we use this "workaround" to do nothing in that case.
		Assert.NotEqual(string.Empty, source);
		await Task.Yield();
#endif
	}

	[Fact]
	public async Task Tuples_OnlySupportedByV3_3_0_1()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using Xunit;

			public class MyClass {
				public IEnumerable<TheoryDataRowBase> MyTupleMethod() {
					var value = Tuple.Create("Hello world", 42);

					yield return new TheoryDataRow({|#0:value|});
					yield return new TheoryDataRow({|#1:Tuple.Create("Hello world", 42)|});
					yield return new TheoryDataRow<Tuple<string, int>>({|#2:value|});
					yield return new TheoryDataRow<Tuple<string, int>>({|#3:Tuple.Create("Hello world", 42)|});
				}

				public IEnumerable<TheoryDataRowBase> MyValueTupleMethod() {
					var value = ValueTuple.Create("Hello world", 42);

					yield return new TheoryDataRow({|#10:value|});
					yield return new TheoryDataRow({|#11:ValueTuple.Create("Hello world", 42)|});
					yield return new TheoryDataRow<ValueTuple<string, int>>({|#12:value|});
					yield return new TheoryDataRow<ValueTuple<string, int>>({|#13:ValueTuple.Create("Hello world", 42)|});
				}
			}

			public class PossiblySerializableUnsealedClass { }
			""";
		var expectedUnsupported = new[] {
			Verify.Diagnostic("xUnit1047").WithLocation(0).WithArguments("value", "Tuple<string, int>"),
			Verify.Diagnostic("xUnit1047").WithLocation(1).WithArguments("Tuple.Create(\"Hello world\", 42)", "Tuple<string, int>"),
			Verify.Diagnostic("xUnit1047").WithLocation(2).WithArguments("value", "Tuple<string, int>"),
			Verify.Diagnostic("xUnit1047").WithLocation(3).WithArguments("Tuple.Create(\"Hello world\", 42)", "Tuple<string, int>"),

			Verify.Diagnostic("xUnit1046").WithLocation(10).WithArguments("value", "(string, int)"),
			Verify.Diagnostic("xUnit1046").WithLocation(11).WithArguments("ValueTuple.Create(\"Hello world\", 42)", "(string, int)"),
			Verify.Diagnostic("xUnit1046").WithLocation(12).WithArguments("value", "(string, int)"),
			Verify.Diagnostic("xUnit1046").WithLocation(13).WithArguments("ValueTuple.Create(\"Hello world\", 42)", "(string, int)"),
		};

		await Verify_v3_Pre301.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expectedUnsupported);
		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	internal class Analyzer_v3_Pre301 : TheoryDataRowArgumentsShouldBeSerializable
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(3, 0, 0));
	}
}
